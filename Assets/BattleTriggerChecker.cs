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

	public IEnumerator CountBattleTrigger(BattleTrigger trigger){
		trigger.count += 1;
		if(trigger.count == trigger.targetCount && !trigger.acquired){
			trigger.acquired = true;
			Debug.Log("TriggerName : " + trigger.korName);
			if(trigger.resultType == BattleTrigger.ResultType.Bonus)
				battleData.rewardPoint += trigger.reward;
			else if(trigger.resultType == BattleTrigger.ResultType.Win){
				battleData.rewardPoint += trigger.reward;
				yield return new WaitForSeconds(1.0f);
				resultPanel.gameObject.SetActive(true);
				resultPanel.UpdatePanel(0);
			}
			else if(trigger.resultType == BattleTrigger.ResultType.Lose){
				Debug.Log("Mission FAIL : "+trigger.korName);
				sceneLoader.LoadNextDialogueScene("Title");
			}
		}
		else if(trigger.repeatable)
			battleData.rewardPoint += trigger.reward;
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

	public static void CountBattleCondition(Unit unit, BattleTrigger.ActionType actionType){
		BattleTriggerChecker Checker = FindObjectOfType<BattleTriggerChecker>();
		foreach(BattleTrigger trigger in Checker.battleTriggers){
			if(trigger.resultType == BattleTrigger.ResultType.End)
				continue;
			else if(Checker.CheckUnitType(trigger, unit) && Checker.CheckActionType(trigger, actionType))
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
		if(trigger.unitType == BattleTrigger.UnitType.Target && trigger.targetUnitNames.Any(x => x == unit.name))
			return true;
		else if(trigger.unitType == BattleTrigger.UnitType.Ally && unit.side == Side.Ally)
			return true;
		else if(trigger.unitType == BattleTrigger.UnitType.Enemy && unit.side == Side.Enemy)
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