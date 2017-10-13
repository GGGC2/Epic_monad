using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Enums;

public class LogManager : MonoBehaviour {
    public LogDisplayPanel logDisplayPanel;
    public GameObject logDisplayPrefab;
    private static LogManager instance = null;
    public static LogManager Instance { get { return instance; } }
    public static void SetInstance() { instance = FindObjectOfType<LogManager>(); }

    public void Awake() {
        logDisplayPanel = GameObject.Find("LogDisplayPanel").GetComponentInChildren<LogDisplayPanel>();
    }

    public void Record(Log log) {
        if (log is EffectLog) {
            if(((EffectLog)log).isMeaningless())   return;
            if(log is StatusEffectLog && !CheckToOffset(((StatusEffectLog)log)))    return;

            if(GetLastEventLogDisplay() != null){
                EventLog lastEventLog = (EventLog)GetLastEventLogDisplay().log;
                lastEventLog.getEffectLogList().Add((EffectLog)log);
                ((EffectLog)log).parentEvent = lastEventLog;
            } else
                Debug.LogError(log.GetText() + "appears faster than event log");
        }

        LogDisplay logDisplay = Instantiate(logDisplayPrefab).GetComponent<LogDisplay>();
        int numLog = BattleData.logDisplayList.Count;

        logDisplay.log = log;
        log.logDisplay = logDisplay;
        BattleData.logDisplayList.Add(logDisplay);
        if(DisplayThisLog(log)) logDisplayPanel.AddLogDisplay(logDisplay, numLog + 1);
    }
    public IEnumerator ExecuteLastEventLogAndConsequences() {   // 마지막 Event와 그로 인해 발생하는 모든 새로운 Effect와 Event를 실행한다.
        do {
            do {
                yield return ExecuteLastEventLog();         // 마지막 Event와 그로 인해 발생했던 Effect들을 실행한다.
                GenerateConsequentEffectLogs();             // 이 Event로 인해 발생한 Effect들이 더 있는지 확인한다.
                RemoveInvalidEffectLogs();                  // 무의미한 Effect들을 없앤다. (Attach한 후 Stack을 0으로 한다던가)
            } while(ThereIsNewEffect());                    // 유의미한 Effect가 있다면 있다면 다시 실행한다.
            yield return ExecuteLastEventLog();             
            GenerateConsequentEventLogs();                  // 새로 발생한 Event가 있는지 확인한다.
        } while (ThereIsNewEvent());                        // 있다면 그 Event를 실행한다.
        BattleManager.Instance.CheckBattleTriggers();
    }
    public EventLog PopLastEventLog() {                     // 마지막 Event를 실행하지 않고 없앤 후 리턴한다(Preview에 쓰임)
        int numLog = BattleData.logDisplayList.Count;
        int lastEventLogIndex = BattleData.logDisplayList.FindLastIndex(logDisplay => logDisplay.log is EventLog);

        LogDisplay lastEventLogDisplay = BattleData.logDisplayList[lastEventLogIndex];
        BattleData.logDisplayList.RemoveRange(lastEventLogIndex, numLog - lastEventLogIndex); //그 EventLog와, 그 Event로부터 발생한 모든 EffectLog를 삭제
        logDisplayPanel.RemoveLogsFrom(lastEventLogIndex);
        return (EventLog)lastEventLogDisplay.log;
    }
    void GenerateConsequentEffectLogs() {
        //Debug.Log("Generating ConsequentEffectLogs");
        UnitManager unitManager = UnitManager.Instance;
        TileManager tileManager = TileManager.Instance;
        unitManager.TriggerPassiveSkillsAtActionEnd();
        unitManager.ApplyTileBuffsAtActionEnd();
        unitManager.TriggerStatusEffectsAtActionEnd();
        unitManager.UpdateStatusEffectsAtActionEnd();
        tileManager.UpdateTileStatusEffectsAtActionEnd();
        unitManager.UpdateStatsAtActionEnd();
        unitManager.UpdateHealthViewersAtActionEnd();
        UnitManager.Instance.UpdateUnitOrder();
        if (BattleData.selectedUnit != null)
            FindObjectOfType<UIManager>().UpdateSelectedUnitViewerUI(BattleData.selectedUnit);
    }
    IEnumerator ExecuteLastEventLog() {
        LogDisplay logDisplay = GetLastEventLogDisplay();
        Log log = logDisplay.log;
        Log lastEventLog = GetLastEventLogDisplay().log;

        lastEventLog.executed = true;
        yield return lastEventLog.Execute();
        if (log is CastLog) yield return HandleAfterCastLog();
        else if (log is ChainLog) HandleAfterChainLog();
        else if (log is MoveLog) HandleAfterMoveLog();
        else if (log is StandbyLog) HandleAfterStandbyLog();
        else if (log is MoveCancelLog) HandleAfterMoveCancelLog();
        yield return null;
    }
    void GenerateConsequentEventLogs() {   // 새로운 Event를 발생시킴
        //Debug.Log("Generating ConsequentEventLogs");
        TileManager.Instance.CheckAllTraps();
        BattleManager.Instance.UpdateUnitsForDestroy();
        BattleData.unitManager.ResetLatelyHitUnits();
    }
    bool ThereIsNewEvent() {
        return !GetLastEventLogDisplay().log.executed;
    }
    bool ThereIsNewEffect() {
        LogDisplay lastEventLogDisplay = GetLastEventLogDisplay();
        foreach(var effectLog in ((EventLog)lastEventLogDisplay.log).getEffectLogList()) {
            if(!effectLog.executed) 
                return true;
        }
        return false;
    }
    IEnumerator HandleAfterCastLog() {
        BattleData.currentState = CurrentState.FocusToUnit;
        BattleData.selectedSkill = null;
        yield return new WaitForSeconds(0.5f);
        BattleData.alreadyMoved = false;
    }
    void HandleAfterChainLog() {
        BattleData.currentState = CurrentState.Standby;
        BattleData.selectedSkill = null;
        BattleData.alreadyMoved = false;
    }
    void HandleAfterMoveLog() {
        BattleData.currentState = CurrentState.FocusToUnit;
        BattleData.previewAPAction = null;
        BattleData.alreadyMoved = true;
    }
    void HandleAfterStandbyLog() {
        BattleData.currentState = CurrentState.Standby;
        BattleData.alreadyMoved = false;
    }
    void HandleAfterMoveCancelLog() {
        BattleData.currentState = CurrentState.FocusToUnit;
        BattleData.alreadyMoved = false;
    }
    LogDisplay GetLastEventLogDisplay() {
        int lastEventLogIndex = BattleData.logDisplayList.FindLastIndex(logDisplay => logDisplay.log is EventLog);

        if (lastEventLogIndex == -1)    return null;
        else return BattleData.logDisplayList[lastEventLogIndex];
    }
    LogDisplay GetLastEffectLogDisplay() {
        int numLog = BattleData.logDisplayList.Count;
        return BattleData.logDisplayList[numLog - 1];
    }
    bool DisplayThisLog(Log log) {
        if(log is CameraMoveLog || log is PrintBonusTextLog || log is SoundEffectLog || log is VisualEffectLog
            || log is DisplayDamageOrHealTextLog || log is AddLatelyHitInfoLog || log is WaitForSecondsLog
            || log is PaintTilesLog || log is DepaintTilesLog)   return false;
        return true;
    }
    void RemoveLog(Log log) {
        LogDisplay logDisplay = log.logDisplay;
        BattleData.logDisplayList.Remove(logDisplay);
        if(logDisplay != null)  logDisplay.transform.SetParent(null);
        if(log is EffectLog && ((EffectLog)log).parentEvent != null)
            ((EffectLog)log).parentEvent.getEffectLogList().Remove((EffectLog)log); 
    }


    Dictionary<StatusEffect, StatusEffectChange> SEChangeDict = new Dictionary<StatusEffect, StatusEffectChange>();
    // 매 event의 발생마다, 변하는 statusEffect들의 change를 기억한다. 
    
    bool CheckToOffset(StatusEffectLog log) {
        // AmountChange 2 -> 2나, AmountChange 2->3 직후에 3 -> 2 하는 것과 같이 의미없는 log들을 상쇄한다.
        //Debug.Log("checking offset : " + log.GetText());
        StatusEffect statusEffect = log.statusEffect;
        if (!SEChangeDict.ContainsKey(statusEffect))
            SEChangeDict.Add(statusEffect, new StatusEffectChange(log));
        else SEChangeDict[statusEffect].Update(log);
        StatusEffectChange change = SEChangeDict[statusEffect];
        foreach (var logToOffset in change.logsOffsetBy(log)) {
            //Debug.Log("offset log : " + logToOffset.GetText());
            if(logToOffset != log)
                RemoveLog(logToOffset);
            return false;
        }
        return true;
    }

    void RemoveInvalidEffectLogs() {
        //Debug.Log("Removing Invalid EffectLogs");
        foreach (var kv in SEChangeDict) {
            foreach (var log in kv.Value.logsNotValid()) {
                //Debug.Log("Invalid log : " + log.GetText());
                RemoveLog(log);
            }
        }
        SEChangeDict.Clear();
    }

    class StatusEffectChange {
        Dictionary<StatusEffectChangeType, List<StatusEffectLog>> changes = new Dictionary<StatusEffectChangeType, List<StatusEffectLog>>();
        public StatusEffectChange(StatusEffectLog log) {
            Update(log);
        }
        public void Update(StatusEffectLog log) {
            if(changes.ContainsKey(log.type))
                changes[log.type].Add(log);
            else changes.Add(log.type, new List<StatusEffectLog> { log });
        }
        public List<StatusEffectLog> logsOffsetBy(StatusEffectLog log) {    // log에 의해 상쇄되는 것들
            if(CheckOffsetForNewLog(log)) {
                List<StatusEffectLog> logsToOffset = changes[log.type];
                changes.Remove(log.type);
                return logsToOffset;
            }
            return new List<StatusEffectLog>();
        }
        public List<StatusEffectLog> logsNotValid() {   // statusEffect를 Attatch하고 바로 Stack을 0으로 하는 것 등
            if(!isValid())
                return AllLogs();
            return new List<StatusEffectLog>();
        }
        bool CheckOffsetForNewLog(StatusEffectLog log) {
            StatusEffectChangeType type = log.type;
            if(type != StatusEffectChangeType.Attach && type != StatusEffectChangeType.Remove && 
                changes.ContainsKey(log.type) &&    changes[log.type][0].beforeAmount == log.afterAmount)
                return true;
            return false;
        }
        bool isValid() {
            if (changes.ContainsKey(StatusEffectChangeType.Attach) && 
                !changes[StatusEffectChangeType.Attach][0].executed &&
                changes.ContainsKey(StatusEffectChangeType.RemainStackChange)){
                    List<StatusEffectLog> stackChangeLogs = changes[StatusEffectChangeType.RemainStackChange];
                    if(stackChangeLogs[stackChangeLogs.Count - 1].afterAmount == 0)
                        return false;
                }
                return true;
        }
        List<StatusEffectLog> AllLogs() {
            List<StatusEffectLog> allLogs = new List<StatusEffectLog>();
            foreach(var kv in changes)
                allLogs.AddRange(kv.Value);
            return allLogs;
        }
    }
}