using UnityEngine;
using System.Collections;

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
            if(!((EffectLog)log).isValid())
                return;
            LogDisplay LastEventLogDisplay = GetLastEventLogDisplay();
            if (LastEventLogDisplay == null)
                Debug.LogError("EffectLog appears faster than EventLog");
            else {
                EventLog lastEventLog = (EventLog)GetLastEventLogDisplay().log;
                lastEventLog.getEffectLogList().Add((EffectLog)log);
            }
        }

        LogDisplay logDisplay = Instantiate(logDisplayPrefab).GetComponent<LogDisplay>();
        int numLog = BattleData.logDisplayList.Count;

        logDisplay.log = log;
        BattleData.logDisplayList.Add(logDisplay);
        if(DisplayThisLog(log)) logDisplayPanel.AddLogDisplay(logDisplay, numLog + 1);
    }
    public IEnumerator ExecuteLastEventLogAndConsequences() {   // 마지막 Event와 그로 인해 발생하는 모든 새로운 Effect와 Event를 실행한다.
        do {
            do {
                yield return ExecuteLastEventLog();         // 마지막 Event와 그로 인해 발생했던 Effect들을 실행한다.
                GenerateConsequentEffectLogs();             // 이 Event로 인해 발생한 Effect들이 더 있는지 확인한다.
            } while(ThereIsNewEffect());                    // 있다면 다시 실행한다.
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

        if (log is CastLog)             yield return HandleAfterCastLog();
        else if (log is ChainLog)       HandleAfterChainLog();
        else if (log is MoveLog)        HandleAfterMoveLog();
        else if (log is StandbyLog)     HandleAfterStandbyLog();
        else if (log is MoveCancelLog)  HandleAfterMoveCancelLog();
    }
    void GenerateConsequentEventLogs() {   // 새로운 Event를 발생시킴
        TileManager.Instance.CheckAllTrpas();
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

        if (lastEventLogIndex == -1)
            return null;
        else return BattleData.logDisplayList[lastEventLogIndex];
    }
    LogDisplay GetLastEffectLogDisplay() {
        int numLog = BattleData.logDisplayList.Count;
        return BattleData.logDisplayList[numLog - 1];
    }
    bool DisplayThisLog(Log log) {
        if(log is CameraMoveLog || log is PrintBonusTextLog || log is SoundEffectLog || log is VisualEffectLog
            || log is DisplayDamageOrHealTextLog || log is AddLatelyHitInfoLog || log is WaitForSecondsLog)   return false;
        return true;
    }
}