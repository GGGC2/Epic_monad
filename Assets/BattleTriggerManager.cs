using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;
using Enums;
using GameData;

public class BattleTriggerManager : MonoBehaviour {
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

	public void CountBattleTrigger(BattleTrigger trigger){
		if(trigger.actionType == TrigActionType.UnderCount && CountUnitOfCondition(trigger) <= trigger.targetCount){
			AcquireTrigger(trigger);
		}else{
			trigger.count += 1;
			Debug.Log("Trigger counting : " + trigger.korName + ", " + trigger.count);
			if (trigger.count == trigger.targetCount) {
				AcquireTrigger(trigger);
			} else if (trigger.repeatable && trigger.acquired){
				BattleData.rewardPoint += trigger.reward;
			}
		}
	}

	void AcquireTrigger(BattleTrigger trigger){
		if(!trigger.acquired){
			trigger.acquired = true;
			Debug.Log ("Trigger acquired : " + trigger.korName);
			if (trigger.resultType == TrigResultType.Bonus)
				BattleData.rewardPoint += trigger.reward;
			else if (trigger.resultType == TrigResultType.Win)
				BattleData.rewardPoint += trigger.reward;
			else if (trigger.resultType == TrigResultType.Lose) {
				Debug.Log ("Mission FAIL : " + trigger.korName);
				sceneLoader.LoadNextDialogueScene ("Title");
			}
		}else{
			Debug.Log("This trigger is already Acquired.");
		}
	}

	public void InitializeResultPanel(){
		resultPanel.gameObject.SetActive(true);
		resultPanel.Activate();
		resultPanel.UpdatePanel(0);
	}

	void Awake(){
		if (!SceneData.isTestMode && !SceneData.isStageMode){
            GameDataManager.Load();
		}
		resultPanel = FindObjectOfType<ResultPanel>();
		resultPanel.Checker = this;
		resultPanel.gameObject.SetActive(false);

		triggers = Parser.GetParsedData<BattleTrigger>();
		nextScriptName = triggers.Find(x => x.resultType == TrigResultType.End).nextSceneIndex;
		
		if(FindObjectOfType<ConditionPanel>() != null)
			FindObjectOfType<ConditionPanel>().Initialize(triggers);
	}
	void Start () {
		unitManager = BattleData.unitManager;
		sceneLoader = FindObjectOfType<SceneLoader>();
	}

	public static IEnumerator CheckBattleTrigger(Unit unit, TrigActionType actionType){
		Debug.Log("Count BattleTrigger : " + unit.name + "'s " + actionType);
		BattleTriggerManager Checker = FindObjectOfType<BattleTriggerManager>();
		foreach(BattleTrigger trigger in Checker.ActiveTriggers){
			if(actionType == TrigActionType.Kill && unit.IsObject){
				continue;
			}else{
				if(Checker.CheckUnitType(trigger, unit) && Checker.CheckActionType(trigger, actionType)){
					Checker.CountBattleTrigger(trigger);
				}
			}
		}
		return null;
	}

	public static void CheckBattleTrigger(){
		BattleTriggerManager Checker = FindObjectOfType<BattleTriggerManager>();
		foreach(BattleTrigger trigger in Checker.ActiveTriggers){
			if(trigger.actionType == TrigActionType.Phase){
				Checker.CountBattleTrigger(trigger);
			}
		}
	}

	public static void CheckBattleTrigger(Unit unit, Tile destination){
		BattleTriggerManager Checker = FindObjectOfType<BattleTriggerManager>();
		foreach(BattleTrigger trigger in Checker.ActiveTriggers){
			if(trigger.actionType == TrigActionType.Reach && destination.IsReachPoint && Checker.CheckUnitType(trigger, unit)){
				Checker.CountBattleTrigger(trigger);
			}
		}
	}

	//트리거가 현재 활성화되어있는지 여부를 확인. relation == Sequence이고 앞번째 트리거가 달성되지 않은 경우만 false, 그 외에 전부 true.
	public bool isTriggerActive(BattleTrigger trigger){
		if(trigger.resultType == TrigResultType.End){
			return false;
		}else if(trigger.resultType == TrigResultType.Bonus){
			return true;
		}else{
			List<BattleTrigger> checkList;
			BattleTrigger.TriggerRelation relation;
			if(trigger.resultType == TrigResultType.Win){
				checkList = triggers.FindAll(trig => trig.resultType == TrigResultType.Win);
				relation = triggers.Find(trig => trig.resultType == TrigResultType.End).winTriggerRelation;
			}else{
				checkList = triggers.FindAll(trig => trig.resultType == TrigResultType.Lose);
				relation = triggers.Find(trig => trig.resultType == TrigResultType.End).loseTriggerRelation;
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