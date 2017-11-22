using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;
using Enums;
using GameData;

public class BattleTriggerManager : MonoBehaviour {
    private static BattleTriggerManager instance;
    public static BattleTriggerManager Instance {
        get { return instance; }
    }
    UnitManager unitManager;
	public SceneLoader sceneLoader;
	ResultPanel resultPanel;

	public List<BattleTrigger> triggers = new List<BattleTrigger>();
	public List<Vector2> targetTiles = new List<Vector2>();
	List<string> reachedTargetUnitNames = new List<string>();

	public string nextScriptName;

	public SceneLoader SceneLoader { get { return sceneLoader; } }
	public List<Vector2> TargetTiles { get { return targetTiles; } }
	public List<string> ReachedTargetUnitNames { get { return reachedTargetUnitNames; } }

	public List<BattleTrigger> ActiveTriggers{
		get{
			return triggers.FindAll(trig => isTriggerActive(trig));
		}
	}

	public void CountBattleTrigger(BattleTrigger trigger) {
        if (trigger.resultType == TrigResultType.Trigger) {
            trigger.Trigger();
        }

		if(trigger.actionType == TrigActionType.UnderCount){
			if(CountUnitOfCondition(trigger) <= trigger.reqCount){
				AcquireTrigger(trigger);
			}
		}else{
			trigger.count += 1;
			Debug.Log("Trigger counting : " + trigger.korName + ", " + trigger.count + " / " + trigger.reqCount);
			if (trigger.count == trigger.reqCount) {
				AcquireTrigger(trigger);
			} else if (trigger.repeatable && trigger.acquired){
				BattleData.rewardPoint += trigger.reward;
			}
		}
	}

	void AcquireTrigger(BattleTrigger trigger){
		Debug.Log ("Trigger Applied : " + trigger.korName);
		if(trigger.acquired == trigger.reverse){
			trigger.acquired = !trigger.reverse;
            if (trigger.resultType == TrigResultType.Lose) {
                Debug.Log("Mission FAIL : " + trigger.korName);
                LoadLoseScene();
            }
		}else{
			Debug.Log("This trigger is already Applied.");
		}
	}

	public void WinGame(){
		List<BattleTrigger> scoreTriggers = triggers.FindAll(trig =>
			(trig.resultType == TrigResultType.Win || trig.resultType == TrigResultType.Bonus) && trig.acquired);
		Debug.Log("count of scoreTriggers : " + scoreTriggers.Count);
		
		scoreTriggers.ForEach(trig => BattleData.rewardPoint += trig.reward);
		InitializeResultPanel();
	}

	public void InitializeResultPanel(){
		resultPanel.gameObject.SetActive(true);
		resultPanel.Activate();
		resultPanel.UpdatePanel(0);
	}
	public void LoadLoseScene(){
		sceneLoader.LoadNextDialogueScene ("Scene_Lose" + SceneData.stageNumber);
	}

	void Awake(){
        instance = this;
		if (!SceneData.isTestMode && !SceneData.isStageMode){
            GameDataManager.Load();
		}
		resultPanel = FindObjectOfType<ResultPanel>();
		resultPanel.Checker = this;
		resultPanel.gameObject.SetActive(false);

		triggers = Parser.GetParsedData<BattleTrigger>();
		nextScriptName = triggers.Find(x => x.resultType == TrigResultType.Info).nextSceneIndex;

        if (FindObjectOfType<ConditionPanel>() != null) {
            FindObjectOfType<CameraMover>().SetMovable(false);
            FindObjectOfType<ConditionPanel>().Initialize(triggers);
        }
	}
	void Start () {
		unitManager = BattleData.unitManager;
		sceneLoader = FindObjectOfType<SceneLoader>();
	}

	public static void CheckPhaseTriggers(){
		List<BattleTrigger> trigger = Instance.ActiveTriggers.FindAll(trig => trig.actionType == TrigActionType.Phase);
		trigger.ForEach(trig => {
			Instance.CountBattleTrigger(trig);
		});
	}

	public static void CheckBattleTrigger(Unit unit, TrigActionType actionType){
		Debug.Log("Count BattleTrigger : " + unit.name + "'s " + actionType);
		foreach(BattleTrigger trigger in Instance.ActiveTriggers){
			if(actionType == TrigActionType.Kill && unit.IsObject){
				continue;
			}else{
				if(Instance.CheckUnitType(trigger, unit) && Instance.CheckActionType(trigger, actionType)){
                    trigger.units.Add(unit);
					Instance.CountBattleTrigger(trigger);
				}
			}
		}
	}

	public static void CheckBattleTrigger(Unit unit, Tile destination){
		foreach(BattleTrigger trigger in Instance.ActiveTriggers){
			if(trigger.actionType == TrigActionType.Reach && destination.IsReachPoint && Instance.CheckUnitType(trigger, unit)){
                trigger.units.Add(unit);
				Instance.CountBattleTrigger(trigger);
			}
		}
	}

	//트리거가 현재 활성화되어있는지 여부를 확인. relation == Sequence이고 앞번째 트리거가 달성되지 않은 경우만 false, 그 외에 전부 true.
	public bool isTriggerActive(BattleTrigger trigger){
		if(trigger.resultType == TrigResultType.Info){
			return false;
		}else if(trigger.resultType == TrigResultType.Bonus){
			return true;
		}else{
			List<BattleTrigger> checkList;
			BattleTrigger.TriggerRelation relation;
			if(trigger.resultType == TrigResultType.Win){
				checkList = triggers.FindAll(trig => trig.resultType == TrigResultType.Win);
				relation = triggers.Find(trig => trig.resultType == TrigResultType.Info).winTriggerRelation;
			}else{
				checkList = triggers.FindAll(trig => trig.resultType == TrigResultType.Lose);
				relation = triggers.Find(trig => trig.resultType == TrigResultType.Info).loseTriggerRelation;
			}

			if(relation == BattleTrigger.TriggerRelation.Sequence){
				for(int i = 0; i < checkList.Count; i++){
					if(checkList[i] == trigger){
						if(i == 0){
							return true;
						}else{
							return checkList[i-1].acquired;
						}
					}
				}
				Debug.LogError("checkList에 trigger가 없습니다!");
				return false;
			}else{
				return true;
			}
		}
	}

	int CountUnitOfCondition(BattleTrigger trigger){
		int result = 0;
		foreach(var unit in unitManager.units){
			if(CheckUnitType(trigger, unit)){
				result += 1;
			}
		}
		return result;
	}
	public bool CheckUnitType(BattleTrigger trigger, Unit unit){
		if (trigger.unitType == TrigUnitType.Target && trigger.targetUnitNames.Any (x => x.Equals(unit.GetNameEng())))
			return true;
		else if(trigger.unitType == TrigUnitType.Ally && unit.GetSide() == Side.Ally)
			return true;
		else if(trigger.unitType == TrigUnitType.Enemy && unit.GetSide() == Side.Enemy)
			return true;
		else if(trigger.unitType == TrigUnitType.PC && unit.IsPC == true && unit.GetSide() == Side.Ally)
			return true;
		else if(trigger.unitType == TrigUnitType.NeutralChar && unit.IsObject == false && unit.GetSide() == Side.Neutral)
			return true;
		else
			return false;
	}

	public bool CheckActionType(BattleTrigger trigger, TrigActionType actionType){
		if(trigger.actionType == actionType)
			return true;
		else
			return false;
	}
}