using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enums;
using System;

interface BattleEndCondition
{
	string successMessage { get; }
	bool Check(BattleEndChecker checker);
}

static class BattleEndConditionExtension
{
	public static bool CheckAndPrint(this BattleEndCondition condition, BattleEndChecker checker)
	{
		bool result = condition.Check(checker);
		if (result) {
			Debug.Log(condition.successMessage);
		}
		return result;
	}
}

class PhaseChecker : BattleEndCondition
{
    public string successMessage { get { return "Win by phase"; } }

    public bool Check(BattleEndChecker checker)
	{
		if (checker.isBattleEnd) return false;
		int currentPhase = checker.BattleData.currentPhase;
		if (currentPhase > checker.MaxPhase) {
			Debug.Log("Win by phase");
			return true;
		}
		return false;
	}
}

class TargetUnitAllDieChecker : BattleEndCondition
{
	public string successMessage { get { return "Win by all target die"; } }

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
    public string successMessage { get { return "target unit some die"; } }

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
    public string successMessage { get { return "target unit at least one die"; } }

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
    public string successMessage { get { return "ally all die"; } }

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
    public string successMessage { get { return "ally some die"; } }

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
    public string successMessage { get { return "ally at least one die"; } }

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
    public string successMessage { get { return "enemy all die"; } }

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
    public string successMessage { get { return "enemy some die"; } }

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
    public string successMessage { get { return "enemy atleast one die"; } }

    public bool Check(BattleEndChecker checker)
	{
		if (checker.isBattleEnd) return false;
		bool dieAnyEnemy = checker.BattleData.unitManager.GetDeadUnitsInfo().Any(x => x.unitSide == Enums.Side.Enemy);
		if (dieAnyEnemy) return true;
		return false;
	}
}

class TargetUnitAllReachedChecker : BattleEndCondition
{
    public string successMessage { get { return "target unit all reached"; } }

    public bool Check(BattleEndChecker checker)
	{
		if (checker.isBattleEnd) return false;
		var targetUnits = checker.BattleData.unitManager.GetAllUnits().FindAll(unit => checker.ReachedTargetUnitNames.Contains(unit.GetComponent<Unit>().GetName()));
		var reachedTargetUnits = targetUnits.Count(targetUnit => checker.TargetTiles.Contains(targetUnit.GetComponent<Unit>().GetPosition()));
		if (reachedTargetUnits == targetUnits.Count) return true;
		return false;
	}
}

class TargetUnitSomeReachedChecker : BattleEndCondition
{
    public string successMessage { get { return "target unit some reached"; } }

    public bool Check(BattleEndChecker checker)
	{
		if (checker.isBattleEnd) return false;
		var targetUnits = checker.BattleData.unitManager.GetAllUnits().FindAll(unit => checker.ReachedTargetUnitNames.Contains(unit.GetComponent<Unit>().GetName()));
		var reachedTargetUnits = targetUnits.Count(targetUnit => checker.TargetTiles.Contains(targetUnit.GetComponent<Unit>().GetPosition()));
		if (reachedTargetUnits >= checker.MinNumberOfReachedTargetUnit) return true;
		return false;
	}
}

class TargetUnitAtLeastOneReachedChecker : BattleEndCondition
{
    public string successMessage { get { return "target unit at least one reached"; } }

    public bool Check(BattleEndChecker checker)
	{
		if (checker.isBattleEnd) return false;
		var targetUnits = checker.BattleData.unitManager.GetAllUnits().FindAll(unit => checker.ReachedTargetUnitNames.Contains(unit.GetComponent<Unit>().GetName()));
		var reachedTargetUnits = targetUnits.Count(targetUnit => checker.TargetTiles.Contains(targetUnit.GetComponent<Unit>().GetPosition()));
		if (reachedTargetUnits > 0) return true;
		return false;
	}
}

class AllyAllReachedChecker : BattleEndCondition
{
    public string successMessage { get { return "ally all reached"; } }

    public bool Check(BattleEndChecker checker)
	{
		if (checker.isBattleEnd) return false;
		var allies = checker.BattleData.unitManager.GetAllUnits().FindAll(unit => unit.GetComponent<Unit>().GetSide() == Side.Ally);
		var reachedAllies = allies.Count(ally => checker.TargetTiles.Contains(ally.GetComponent<Unit>().GetPosition()));
		if (reachedAllies == allies.Count) return true;
		return false;
	}
}

class AllySomeReachedChecker : BattleEndCondition
{
    public string successMessage { get { return "ally some reached"; } }

    public bool Check(BattleEndChecker checker)
	{
		if (checker.isBattleEnd) return false;
		var allies = checker.BattleData.unitManager.GetAllUnits().FindAll(unit => unit.GetComponent<Unit>().GetSide() == Side.Ally);
		var reachedAllies = allies.Count(ally => checker.TargetTiles.Contains(ally.GetComponent<Unit>().GetPosition()));
		if (reachedAllies >= checker.MinNumberOfReachedAlly) return true;
		return false;
	}
}

class AllyAtLeastOneReachedChecker : BattleEndCondition
{
    public string successMessage { get { return "ally at least one reached"; } }

    public bool Check(BattleEndChecker checker)
	{
		if (checker.isBattleEnd) return false;
		var allies = checker.BattleData.unitManager.GetAllUnits().FindAll(unit => unit.GetComponent<Unit>().GetSide() == Side.Ally);
		var reachedAllies = allies.Count(ally => checker.TargetTiles.Contains(ally.GetComponent<Unit>().GetPosition()));
		if (reachedAllies > 0) return true;
		return false;
	}
}

class EnemyAllReachedChecker : BattleEndCondition
{
    public string successMessage { get { return "enemy all reached"; } }

    public bool Check(BattleEndChecker checker)
	{
		if (checker.isBattleEnd) return false;
		var enemies = checker.BattleData.unitManager.GetAllUnits().FindAll(unit => unit.GetComponent<Unit>().GetSide() == Side.Enemy);
		var reachedEnemies = enemies.Count(enemy => checker.TargetTiles.Contains(enemy.GetComponent<Unit>().GetPosition()));
		if (reachedEnemies == enemies.Count) return true;
		return false;
	}
}

class EnemySomeReachedChecker : BattleEndCondition
{
    public string successMessage { get { return "Enemy some reached"; } }

    public bool Check(BattleEndChecker checker)
	{
		if (checker.isBattleEnd) return false;
		var enemies = checker.BattleData.unitManager.GetAllUnits().FindAll(unit => unit.GetComponent<Unit>().GetSide() == Side.Enemy);
		var reachedEnemies = enemies.Count(enemy => checker.TargetTiles.Contains(enemy.GetComponent<Unit>().GetPosition()));
		if (reachedEnemies >= checker.MinNumberOfReachedEnemy) return true;
		return false;
	}
}

class EnemyAtLeastOneReachedChecker : BattleEndCondition
{
    public string successMessage { get { return "Enemy at least one reached"; } }
	public bool Check(BattleEndChecker checker)
	{
		if (checker.isBattleEnd) return false;
		var enemies = checker.BattleData.unitManager.GetAllUnits().FindAll(unit => unit.GetComponent<Unit>().GetSide() == Side.Enemy);
		var reachedEnemies = enemies.Count(enemy => checker.TargetTiles.Contains(enemy.GetComponent<Unit>().GetPosition()));
		if (reachedEnemies > 0) return true;
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
	List<BattleEndCondition> battleEndConditions = new List<BattleEndCondition>();

	int maxPhase;
	List<string> targetUnitNames = new List<string>();
	int minNumberOfTargetUnit;
	int minNumberOfAlly;
	int minNumberOfEnemy;
	List<Vector2> targetTiles = new List<Vector2>();
	List<string> reachedTargetUnitNames = new List<string>();
	int minNumberOfReachedTargetUnit;
	int minNumberOfReachedAlly;
	int minNumberOfReachedEnemy;

	string nextScriptName;

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

	public List<Vector2> TargetTiles
	{
		get { return targetTiles; }
	}

	public List<string> ReachedTargetUnitNames
	{
		get { return reachedTargetUnitNames; }
	}

	public int MinNumberOfReachedTargetUnit
	{
		get { return minNumberOfReachedTargetUnit; }
	}

	public int MinNumberOfReachedAlly
	{
		get { return minNumberOfReachedAlly; }
	}

	public int MinNumberOfReachedEnemy
	{
		get { return minNumberOfReachedEnemy; }
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

		nextScriptName = battleEndTriggers.Find(x => x.result == Enums.BattleResult.End).nextSceneIndex;

		if (nextScriptName == "pintos#16")
			FindObjectOfType<SoundManager>().PlayBgm("Boss_Orchid");

		// Debug.Log("BET_win : " + battleWinConditions.Count);
		// Debug.Log("BET_lose : " + battleLoseConditions.Count);
		
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
			isBattleWin = battleWinConditions.Any(x => x.CheckAndPrint(this));
			isBattleLose = battleLoseConditions.Any(x => x.CheckAndPrint(this));
			isBattleEnd = isBattleWin || isBattleLose;
		
			CheckPushedButton();
		
			if (isBattleEnd)
			{
				if (isBattleWin)
				{
					Debug.Log("Win");
					FindObjectOfType<SceneLoader>().LoadNextDialogueScene(nextScriptName);
				}
				else
					Debug.Log("Lose");
					FindObjectOfType<SceneLoader>().LoadNextDialogueScene(nextScriptName);
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
			if (trigger.triggerNumber == 11)
			{
				TargetUnitAllReachedChecker targetUnitAllReachedChecker = new TargetUnitAllReachedChecker();
				targetTiles = trigger.targetTiles;
				reachedTargetUnitNames = trigger.reachedTargetUnitNames;
				battleEndConditions.Add(targetUnitAllReachedChecker);
			}
			if (trigger.triggerNumber == 12)
			{
				TargetUnitSomeReachedChecker targetUnitSomeReachedChecker = new TargetUnitSomeReachedChecker();
				targetTiles = trigger.targetTiles;
				reachedTargetUnitNames = trigger.reachedTargetUnitNames;
				battleEndConditions.Add(targetUnitSomeReachedChecker);
			}
			if (trigger.triggerNumber == 13)
			{
				TargetUnitAtLeastOneReachedChecker targetUnitAtLeastOneReachedChecker = new TargetUnitAtLeastOneReachedChecker();
				targetTiles = trigger.targetTiles;
				reachedTargetUnitNames = trigger.reachedTargetUnitNames;
				battleEndConditions.Add(targetUnitAtLeastOneReachedChecker);
			}
			if (trigger.triggerNumber == 14)
			{
				AllyAllReachedChecker allyAllReachedChecker = new AllyAllReachedChecker();
				targetTiles = trigger.targetTiles;
				battleEndConditions.Add(allyAllReachedChecker);
			}
			if (trigger.triggerNumber == 15)
			{
				EnemyAllReachedChecker enemyAllReachedChecker = new EnemyAllReachedChecker();
				targetTiles = trigger.targetTiles;
				battleEndConditions.Add(enemyAllReachedChecker);
			}
			if (trigger.triggerNumber == 16)
			{
				AllySomeReachedChecker allySomeReachedChecker = new AllySomeReachedChecker();
				targetTiles = trigger.targetTiles;
				minNumberOfReachedAlly = trigger.minNumberOfReachedAlly;
				battleEndConditions.Add(allySomeReachedChecker);
			}
			if (trigger.triggerNumber == 17)
			{
				EnemySomeReachedChecker enemySomeReachedChecker = new EnemySomeReachedChecker();
				targetTiles = trigger.targetTiles;
				minNumberOfReachedEnemy = trigger.minNumberOfReachedEnemy;
				battleEndConditions.Add(enemySomeReachedChecker);
			}
			if (trigger.triggerNumber == 18)
			{
				AllyAtLeastOneReachedChecker allyAtLeastOneReachedChecker = new AllyAtLeastOneReachedChecker();
				targetTiles = trigger.targetTiles;
				battleEndConditions.Add(allyAtLeastOneReachedChecker);
			}
			if (trigger.triggerNumber == 19)
			{
				EnemyAtLeastOneReachedChecker enemyAtLeastOneReachedChecker = new EnemyAtLeastOneReachedChecker();
				targetTiles = trigger.targetTiles;
				battleEndConditions.Add(enemyAtLeastOneReachedChecker);
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
