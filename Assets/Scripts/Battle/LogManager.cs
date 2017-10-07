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
        LogDisplay logDisplay = Instantiate(logDisplayPrefab).GetComponent<LogDisplay>();
        int numLog = BattleData.logDisplayList.Count;

        if (log is EffectLog) {
            LogDisplay LastEventLogDisplay = GetLastEventLogDisplay();
            if (LastEventLogDisplay == null)
                Debug.LogError("EffectLog appears faster than EventLog");
            else {
                EventLog lastEventLog = (EventLog)GetLastEventLogDisplay().log;
                lastEventLog.getEffectLogList().Add((EffectLog)log);
            }
        }

        logDisplay.log = log;
        BattleData.logDisplayList.Add(logDisplay);
        logDisplayPanel.AddLogDisplay(logDisplay, numLog + 1);
    }
    public IEnumerator ExecuteLastEventLogAndConsequences() {
        do {
            do {
                yield return ExecuteLastEventLog();
                GenerateConsequentEffectLogs();
            }while(IsThereNewEffect());

            yield return ExecuteLastEventLog();
            GenerateConsequentEventLogs();
        } while (IsThereNewEvent());
        BattleManager.Instance.CheckBattleTriggers();
    }
    void GenerateConsequentEffectLogs() { // lastEvent에 의해 발생하는 Effect들을 발생시킴
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
    bool IsThereNewEvent() {
        return !GetLastEventLogDisplay().log.executed;
    }
    bool IsThereNewEffect() {
        LogDisplay lastEffectLogDisplay = GetLastEffectLogDisplay();
        return !(lastEffectLogDisplay == null || lastEffectLogDisplay.log.executed);
    }
    public EventLog PopLastEventLog() {
        int numLog = BattleData.logDisplayList.Count;
        int lastEventLogIndex = BattleData.logDisplayList.FindLastIndex(logDisplay => logDisplay.log is EventLog);

        LogDisplay lastEventLogDisplay = BattleData.logDisplayList[lastEventLogIndex];
        BattleData.logDisplayList.RemoveRange(lastEventLogIndex, numLog - lastEventLogIndex); //그 EventLog와, 그 Event로부터 발생한 모든 EffectLog를 삭제

        return (EventLog)BattleData.logDisplayList[lastEventLogIndex].log;
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
}