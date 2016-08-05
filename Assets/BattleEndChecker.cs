﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enums;

interface BattleEndCondition
{
	bool Check(BattleEndChecker checker);
}

class PhaseChecker : BattleEndCondition
{
	public bool Check(BattleEndChecker checker)
	{
		if (checker.isBattleEnd) return false;
		int currentPhase = checker.BattleData.currentPhase;
		if (currentPhase > checker.MaxPhase) return true;
		return false;
	}
}

class TargetUnitAllDieChecker : BattleEndCondition
{
	public bool Check(BattleEndChecker checker)
	{
		if (checker.isBattleEnd) return false;
		var targetUnitNames = checker.TargetUnitNames;
		var allUnits = checker.BattleData.unitManager.GetAllUnits();
		var notDiedUnitNames =
			from unitName in targetUnitNames
			from unit in allUnits
			where unit.GetComponent<Unit>().GetName() == unitName
			select unitName;
		bool allDied = notDiedUnitNames.Count() == 0; 
		return allDied;

		// bool remainAnyTargetUnit = checker.TargetUnitNames.Any(x => checker.BattleData.unitManager.GetAllUnits().Any(y => Equals(y.GetComponent<Unit>().GetName(), x)));
		// if (!remainAnyTargetUnit) return true;
		// return false;
	}
}

// '몇 명이 죽었는지'가 아니라 '몇 명이 남았는지'를 체크함.
class TargetUnitSomeDieChecker : BattleEndCondition
{
	public bool Check(BattleEndChecker checker)
	{
		if (checker.isBattleEnd) return false;
		int remainNumberOfTargetUnit = checker.BattleData.unitManager.GetAllUnits().Count(x => checker.TargetUnitNames.Contains(x.GetComponent<Unit>().GetName()));
		if (remainNumberOfTargetUnit < checker.MinNumberOfTargetUnit) return true;
		return false;
	}
}

class TargetUnitAtLeastOneDieChecker : BattleEndCondition
{
	public bool Check(BattleEndChecker checker)
	{
		if (checker.isBattleEnd) return false;
		bool dieAnyTargetUnit = checker.TargetUnitNames.Any(x => checker.BattleData.unitManager.GetDeadUnitsInfo().Any(y => Equals(y.unitName, x)));
		if (dieAnyTargetUnit) return true;
		return false;
	}
}		

class AllyAllDieChecker : BattleEndCondition
{
	public bool Check(BattleEndChecker checker)
	{
		if (checker.isBattleEnd) return false;
		bool remainAnyAlly = checker.BattleData.unitManager.GetAllUnits().Any(x => x.GetComponent<Unit>().GetSide() == Enums.Side.Ally);
		if (!remainAnyAlly) return true;
		return false;
	}
}

// '몇 명이 죽었는지'가 아니라 '몇 명이 남았는지'를 체크함.
class AllySomeDieChecker : BattleEndCondition
{
	public bool Check(BattleEndChecker checker)
	{
		if (checker.isBattleEnd) return false;
		int remainNumberOfAlly = checker.BattleData.unitManager.GetAllUnits().Count(x => x.GetComponent<Unit>().GetSide() == Enums.Side.Ally);
		if (remainNumberOfAlly < checker.MinNumberOfAlly) return true;
		return false;
	}
}

class AllyAtLeastOneDieChecker : BattleEndCondition
{
	public bool Check(BattleEndChecker checker)
	{
		if (checker.isBattleEnd) return false;
		bool dieAnyAlly = checker.BattleData.unitManager.GetDeadUnitsInfo().Any(x => x.unitSide == Enums.Side.Ally);
		if (dieAnyAlly) return true;
		return false;
	}
}

class EnemyAllDieChecker : BattleEndCondition
{
	public bool Check(BattleEndChecker checker)
	{
		if (checker.isBattleEnd) return false;
		bool remainAnyAlly = checker.BattleData.unitManager.GetAllUnits().Any(x => x.GetComponent<Unit>().GetSide() == Enums.Side.Enemy);
		if (!remainAnyAlly) return true;
		return false;
	}
}

// '몇 명이 죽었는지'가 아니라 '몇 명이 남았는지'를 체크함.
class EnemySomeDieChecker : BattleEndCondition
{
	public bool Check(BattleEndChecker checker)
	{
		if (checker.isBattleEnd) return false;
		int remainNumberOfEnemy = checker.BattleData.unitManager.GetAllUnits().Count(x => x.GetComponent<Unit>().GetSide() == Enums.Side.Enemy);
		if (remainNumberOfEnemy < checker.MinNumberOfEnemy) return true;
		return false;
	}
}

class EnemyAtLeastOneDieChecker : BattleEndCondition
{
	public bool Check(BattleEndChecker checker)
	{
		if (checker.isBattleEnd) return false;
		bool dieAnyEnemy = checker.BattleData.unitManager.GetDeadUnitsInfo().Any(x => x.unitSide == Enums.Side.Enemy);
		if (dieAnyEnemy) return true;
		return false;
	}
}

public class BattleEndChecker : MonoBehaviour {

	UnitManager unitManager;
	BattleData battleData;
	SceneLoader sceneLoader;

	public bool isBattleEnd;
	public bool isBattleWin;
	public bool isBattleLose;

	List<BattleEndTrigger> battleEndTriggers = new List<BattleEndTrigger>();
	List<int> battleWinTriggers = new List<int>();
	List<int> battleLoseTriggers = new List<int>();
	List<BattleEndCondition> battleWinConditions = new List<BattleEndCondition>();
	List<BattleEndCondition> battleLoseConditions = new List<BattleEndCondition>();
	// List<int> battleEndTriggers_int = new List<int>();
	List<BattleEndCondition> battleEndConditions = new List<BattleEndCondition>();

	int maxPhase;
	List<string> targetUnitNames = new List<string>();
	public int minNumberOfTargetUnit;
	public int minNumberOfAlly;
	public int minNumberOfEnemy;

	public BattleData BattleData
	{
		get { return battleData; }
	}

	public SceneLoader SceneLoader
	{
		get { return sceneLoader; }
	}

	public int MaxPhase
	{
		get { return maxPhase; }
		set { maxPhase = value; }
	}

	public List<string> TargetUnitNames
	{
		get { return targetUnitNames; }
	}

	public int MinNumberOfTargetUnit
	{
		get { return minNumberOfTargetUnit; }
	}

	public int MinNumberOfAlly
	{
		get { return minNumberOfAlly; }
	}

	public int MinNumberOfEnemy
	{
		get { return minNumberOfEnemy; }
	}

	// Use this for initialization
	void Start () {
		battleData = FindObjectOfType<BattleManager>().battleData;
		unitManager = battleData.unitManager;
		sceneLoader = FindObjectOfType<SceneLoader>();

		// battleEndTriggers_int.Add(4);
		// battleEndTriggers.Add(10);
		battleEndTriggers = Parser.GetParsedBattleEndConditionData();
		// Debug.Log("BET : " + battleEndTriggers.Count);
		// battleEndTriggers.ForEach(trigger => Debug.Log(trigger.result + ", " + trigger.triggerNumber));

		battleWinConditions = BattleEndConditionFactory(battleEndTriggers, Enums.BattleResult.Win);
		battleLoseConditions = BattleEndConditionFactory(battleEndTriggers, Enums.BattleResult.Lose);

		// Debug.Log("BET_win : " + battleWinConditions.Count);
		// Debug.Log("BET_lose : " + battleLoseConditions.Count);
		
		// battleEndConditions = BattleEndConditionFactory(battleEndTriggers_int);

		isBattleEnd = false;
		isBattleWin = false;
		isBattleLose = false;

		// maxPhase = 5; // Using test.
		// targetUnitNames.Add("루키어스");
		// targetUnitNames.Add("레이나");
		// minNumberOfTargetUnit = 2;
		// minNumberOfAlly = 2;
		// minNumberOfEnemy = 4;
	}

	// Update is called once per frame
	void Update () {
		if (!isBattleEnd)
		{
			isBattleWin = battleWinConditions.Any(x => x.Check(this));
			isBattleLose = battleLoseConditions.Any(x => x.Check(this));
			isBattleEnd = isBattleWin || isBattleLose;
		
			CheckPushedButton();
		
			if (isBattleEnd)
			{
				if (isBattleWin)
				{
					Debug.Log("Win");
					sceneLoader.LoadNextDialogueScene("pintos#3");
				}
				else
					Debug.Log("Lose");
			}
		}
	}

	List<BattleEndCondition> BattleEndConditionFactory(List<BattleEndTrigger> battleEndTriggers, BattleResult result)
	{
		List<BattleEndCondition> battleEndConditions = new List<BattleEndCondition>();

		List<BattleEndTrigger> battleEndTriggersFilteredByResult = battleEndTriggers.FindAll(trigger => trigger.result == result);

		// Debug.Log("BET_factory : " + battleEndTriggersFilteredByResult.Count + ", " + result);

		foreach (var trigger in battleEndTriggersFilteredByResult)
		{
			if (trigger.triggerNumber == 1)
			{
				PhaseChecker phaseChecker = new PhaseChecker();
				maxPhase = trigger.maxPhase;
				battleEndConditions.Add(phaseChecker);
			}
			if (trigger.triggerNumber == 2)
			{
				TargetUnitAllDieChecker targetUnitAllDieChecker = new TargetUnitAllDieChecker();
				targetUnitNames = trigger.targetUnitNames;
				battleEndConditions.Add(targetUnitAllDieChecker);
			}
			if (trigger.triggerNumber == 3)
			{
				TargetUnitSomeDieChecker targetUnitSomeDieChecker = new TargetUnitSomeDieChecker();
				targetUnitNames = trigger.targetUnitNames;
				minNumberOfTargetUnit = trigger.minNumberOfTargetUnit;
				battleEndConditions.Add(targetUnitSomeDieChecker);
			}
			if (trigger.triggerNumber == 4)
			{
				TargetUnitAtLeastOneDieChecker targetUnitAtLeastOneDieChecker = new TargetUnitAtLeastOneDieChecker();
				targetUnitNames = trigger.targetUnitNames;
				battleEndConditions.Add(targetUnitAtLeastOneDieChecker);
			}
			if (trigger.triggerNumber == 5)
			{
				AllyAllDieChecker allyAllDieChecker = new AllyAllDieChecker();
				battleEndConditions.Add(allyAllDieChecker);
			}
			if (trigger.triggerNumber == 6)
			{
				EnemyAllDieChecker enemyAllDieChecker = new EnemyAllDieChecker();
				battleEndConditions.Add(enemyAllDieChecker);
			}
			if (trigger.triggerNumber == 7)
			{
				AllySomeDieChecker allySomeDieChecker = new AllySomeDieChecker();
				minNumberOfAlly = trigger.minNumberOfAlly;
				battleEndConditions.Add(allySomeDieChecker);
			}
			if (trigger.triggerNumber == 8)
			{
				EnemySomeDieChecker enemySomeDieChecker = new EnemySomeDieChecker();
				minNumberOfEnemy = trigger.minNumberOfEnemy;
				battleEndConditions.Add(enemySomeDieChecker);
			}
			if (trigger.triggerNumber == 9)
			{
				AllyAtLeastOneDieChecker allyAtLeastOneDieChecker = new AllyAtLeastOneDieChecker();
				battleEndConditions.Add(allyAtLeastOneDieChecker);
			}
			if (trigger.triggerNumber == 10)
			{
				EnemyAtLeastOneDieChecker enemyAtLeastOneDieChecker = new EnemyAtLeastOneDieChecker();
				battleEndConditions.Add(enemyAtLeastOneDieChecker);
			}
		}

		return battleEndConditions;
	}

	void CheckPushedButton()
	{
		if (isBattleEnd) return;

		if (Input.GetKeyDown(KeyCode.S))
		{
			isBattleEnd = true;
			isBattleWin = true;
		}
		else if (Input.GetKeyDown(KeyCode.D))
		{
			isBattleEnd = true;
			isBattleLose = true;
		}
	}
}
