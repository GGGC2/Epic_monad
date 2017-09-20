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

	public void CountBattleTrigger(BattleTrigger trigger){
		if(trigger.actionType == BattleTrigger.ActionType.UnderCount && CountUnitOfCondition(trigger) <= trigger.targetCount){
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
			if (trigger.resultType == BattleTrigger.ResultType.Bonus)
				BattleData.rewardPoint += trigger.reward;
			else if (trigger.resultType == BattleTrigger.ResultType.Win)
				BattleData.rewardPoint += trigger.reward;
			else if (trigger.resultType == BattleTrigger.ResultType.Lose) {
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
		nextScriptName = triggers.Find(x => x.resultType == BattleTrigger.ResultType.End).nextSceneIndex;
		
		if(FindObjectOfType<ConditionPanel>() != null)
			FindObjectOfType<ConditionPanel>().Initialize(triggers);
	}
	void Start () {
		unitManager = BattleData.unitManager;
		sceneLoader = FindObjectOfType<SceneLoader>();
	}

	public static IEnumerator CheckBattleTrigger(Unit unit, BattleTrigger.ActionType actionType){
		Debug.Log("Count BattleTrigger : " + unit.name + "'s " + actionType);
		BattleTriggerManager Checker = FindObjectOfType<BattleTriggerManager>();
		foreach(BattleTrigger trigger in Checker.triggers){
			if(trigger.resultType == BattleTrigger.ResultType.End)
				continue;
			else if(actionType == BattleTrigger.ActionType.Kill && unit.IsObject)
				continue;
			else{
				if(Checker.CheckUnitType(trigger, unit) && Checker.CheckActionType(trigger, actionType)){
					Checker.CountBattleTrigger(trigger);
				}
			}
		}
		return null;
	}

	public static void CheckBattleTrigger(){
		BattleTriggerManager Checker = FindObjectOfType<BattleTriggerManager>();
		foreach(BattleTrigger trigger in Checker.triggers){
			if(trigger.resultType == BattleTrigger.ResultType.End)
				continue;
			else if(trigger.actionType == BattleTrigger.ActionType.Phase)
				Checker.CountBattleTrigger(trigger);
		}
	}

	public static void CheckBattleTrigger(Unit unit, Tile destination){
		BattleTriggerManager Checker = FindObjectOfType<BattleTriggerManager>();
		foreach(BattleTrigger trigger in Checker.triggers){
			if(trigger.resultType == BattleTrigger.ResultType.End)
				continue;
			else if(trigger.actionType == BattleTrigger.ActionType.Reach && trigger.targetTiles.Any(x => x == destination.position) && Checker.CheckUnitType(trigger, unit)){
				Checker.CountBattleTrigger(trigger);
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
		if (trigger.unitType == BattleTrigger.UnitType.Target && trigger.targetUnitNames.Any (x => x.Equals(unit.GetNameEng())))
			return true;
		else if(trigger.unitType == BattleTrigger.UnitType.Ally && unit.GetSide() == Side.Ally)
			return true;
		else if(trigger.unitType == BattleTrigger.UnitType.Enemy && unit.GetSide() == Side.Enemy)
			return true;
		else if(trigger.unitType == BattleTrigger.UnitType.PC && unit.IsPC == true && unit.GetSide() == Side.Ally)
			return true;
		else
			return false;
	}

	public bool CheckActionType(BattleTrigger trigger, BattleTrigger.ActionType actionType){
		if(trigger.actionType == actionType)
			return true;
		else
			return false;
	}
}