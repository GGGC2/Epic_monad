using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enums;
using System;

public class BattleTriggerChecker : MonoBehaviour {
	UnitManager unitManager;
	public BattleData battleData;
	public SceneLoader sceneLoader;
	ResultPanel resultPanel;

	public List<BattleTrigger> battleTriggers = new List<BattleTrigger>();
	List<Vector2> targetTiles = new List<Vector2>();
	List<string> reachedTargetUnitNames = new List<string>();

	public string nextScriptName;

	public BattleData BattleData
	{
		get { return battleData; }
	}

	public SceneLoader SceneLoader
	{
		get { return sceneLoader; }
	}

	public List<Vector2> TargetTiles
	{
		get { return targetTiles; }
	}

	public List<string> ReachedTargetUnitNames	
	{
		get { return reachedTargetUnitNames; }
	}

	public void CountBattleTrigger(BattleTrigger trigger){
		trigger.countDown -= 1;
		if(trigger.countDown <= 0 && !trigger.acquired){
			if(trigger.resultType == BattleTrigger.ResultType.Bonus){
				trigger.acquired = true;
				battleData.rewardPoint += trigger.reward;
			}
			else if(trigger.resultType == BattleTrigger.ResultType.Win){
				battleData.rewardPoint += trigger.reward;
				resultPanel.gameObject.SetActive(true);
			}
			else if(trigger.resultType == BattleTrigger.ResultType.Lose){
				Debug.Log(trigger.actionType + " " + trigger.unitType);
				sceneLoader.LoadNextDialogueScene("Title");
			}
		}
	}
	void Start () {
		battleData = FindObjectOfType<BattleManager>().battleData;
		unitManager = battleData.unitManager;
		sceneLoader = FindObjectOfType<SceneLoader>();

		resultPanel = FindObjectOfType<ResultPanel>();
		resultPanel.Checker = this;
		resultPanel.gameObject.SetActive(false);

		battleTriggers = Parser.GetParsedBattleTriggerData();
		nextScriptName = battleTriggers.Find(x => x.resultType == BattleTrigger.ResultType.End).nextSceneIndex;
	}

	public static void CountBattleCondition(Unit unit){
		BattleTriggerChecker Checker = FindObjectOfType<BattleTriggerChecker>();
		Debug.Log("TriggerCount : "+Checker.battleTriggers.Count);
		foreach(BattleTrigger trigger in Checker.battleTriggers){
			if(trigger.resultType == BattleTrigger.ResultType.End)
				continue;
			else if(trigger.actionType == BattleTrigger.ActionType.Neutralize && Checker.CheckUnitType(trigger, unit))
				Checker.CountBattleTrigger(trigger);
		}
	}

	public static void CountBattleCondition(){
		BattleTriggerChecker Checker = FindObjectOfType<BattleTriggerChecker>();
		foreach(BattleTrigger trigger in Checker.battleTriggers){
			if(trigger.resultType == BattleTrigger.ResultType.End)
				continue;
			else if(trigger.actionType == BattleTrigger.ActionType.Phase)
				Checker.CountBattleTrigger(trigger);
		}
	}

	public static void CountBattleCondition(Unit unit, Tile destination){
		BattleTriggerChecker Checker = FindObjectOfType<BattleTriggerChecker>();
		foreach(BattleTrigger trigger in Checker.battleTriggers){
			if(trigger.resultType == BattleTrigger.ResultType.End)
				continue;
			else if(trigger.actionType == BattleTrigger.ActionType.Reach && trigger.targetTiles.Any(x => x == destination.position) && Checker.CheckUnitType(trigger, unit)){
				Checker.CountBattleTrigger(trigger);
			}	
		}
	}

	public bool CheckUnitType(BattleTrigger trigger, Unit unit){
		Debug.Log("UnitType : "+trigger.unitType+", Name : "+unit.name);
		if(trigger.unitType == BattleTrigger.UnitType.Target && trigger.targetUnitNames.Any(x => x == unit.name))
			return true;
		else if(trigger.unitType == BattleTrigger.UnitType.Ally && unit.side == Side.Ally)
			return true;
		else if(trigger.unitType == BattleTrigger.UnitType.Enemy && unit.side == Side.Enemy)
			return true;
		else
			return false;
	}
}