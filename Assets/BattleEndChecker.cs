using UnityEngine;
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
		bool remainAnyTargetUnit = checker.TargetUnitNames.Any(x => checker.BattleData.unitManager.GetAllUnits().Any(y => Equals(y.GetComponent<Unit>().GetName(), x)));
		if (!remainAnyTargetUnit) return true;
		return false;
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
		// bool dieAnyTargetUnit = checker.BattleData.unitManager.GetDeadUnits().Any(x => checker.TargetUnitNames.Contains(x.GetComponent<Unit>().GetName()));
		bool dieAnyTargetUnit = checker.TargetUnitNames.Any(x => checker.BattleData.unitManager.GetDeadUnits().Any(y => Equals(y.GetComponent<Unit>().GetName(), x)));
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
		bool dieAnyAlly = checker.BattleData.unitManager.GetDeadUnits().Any(x => x.GetComponent<Unit>().GetSide() == Enums.Side.Ally);
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
		bool dieAnyEnemy = checker.BattleData.unitManager.GetDeadUnits().Any(x => x.GetComponent<Unit>().GetSide() == Enums.Side.Enemy);
		if (dieAnyEnemy) return true;
		return false;
	}
}

public class BattleEndChecker : MonoBehaviour {

	UnitManager unitManager;
	BattleData battleData;
	SceneLoader sceneLoader;

	public bool isBattleEnd;

	List<int> battleEndTriggers = new List<int>();
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

		// battleEndTriggers.Add(4);
		// battleEndTriggers.Add(10);

		battleEndConditions = BattleEndConditionFactory(battleEndTriggers);

		isBattleEnd = false;

		maxPhase = 5; // Using test.
		targetUnitNames.Add("루키어스");
		targetUnitNames.Add("레이나");
		minNumberOfAlly = 2;
		minNumberOfEnemy = 4;

	}

	// Update is called once per frame
	void Update () {
		CheckPushedButton();
		
		isBattleEnd = battleEndConditions.Any(x => x.Check(this));
		if (isBattleEnd)
		{
			sceneLoader.LoadNextDialogueScene("pintos#3");
		}
	}

	List<BattleEndCondition> BattleEndConditionFactory(List<int> battleEndTriggers)
	{
		List<BattleEndCondition> battleEndConditions = new List<BattleEndCondition>();

		if (battleEndTriggers.Contains(1))
		{
			PhaseChecker phaseChecker = new PhaseChecker();
			battleEndConditions.Add(phaseChecker);
		}
		if (battleEndTriggers.Contains(2))
		{
			TargetUnitAllDieChecker targetUnitAllDieChecker = new TargetUnitAllDieChecker();
			battleEndConditions.Add(targetUnitAllDieChecker);
		}
		if (battleEndTriggers.Contains(3))
		{
			TargetUnitSomeDieChecker targetUnitSomeDieChecker = new TargetUnitSomeDieChecker();
			battleEndConditions.Add(targetUnitSomeDieChecker);
		}
		if (battleEndTriggers.Contains(4))
		{
			TargetUnitAtLeastOneDieChecker targetUnitAtLeastOneDieChecker = new TargetUnitAtLeastOneDieChecker();
			battleEndConditions.Add(targetUnitAtLeastOneDieChecker);
		}
		if (battleEndTriggers.Contains(5))
		{
			AllyAllDieChecker allyAllDieChecker = new AllyAllDieChecker();
			battleEndConditions.Add(allyAllDieChecker);
		}
		if (battleEndTriggers.Contains(6))
		{
			EnemyAllDieChecker enemyAllDieChecker = new EnemyAllDieChecker();
			battleEndConditions.Add(enemyAllDieChecker);
		}
		if (battleEndTriggers.Contains(7))
		{
			AllySomeDieChecker allySomeDieChecker = new AllySomeDieChecker();
			battleEndConditions.Add(allySomeDieChecker);
		}
		if (battleEndTriggers.Contains(8))
		{
			EnemySomeDieChecker enemySomeDieChecker = new EnemySomeDieChecker();
			battleEndConditions.Add(enemySomeDieChecker);
		}
		if (battleEndTriggers.Contains(9))
		{
			AllyAtLeastOneDieChecker allyAtLeastOneDieChecker = new AllyAtLeastOneDieChecker();
			battleEndConditions.Add(allyAtLeastOneDieChecker);
		}
		if (battleEndTriggers.Contains(10))
		{
			EnemyAtLeastOneDieChecker enemyAtLeastOneDieChecker = new EnemyAtLeastOneDieChecker();
			battleEndConditions.Add(enemyAtLeastOneDieChecker);
		}

		return battleEndConditions;
	}

	void CheckPushedButton()
	{
		if (isBattleEnd) return;

		if (Input.GetKeyDown(KeyCode.S))
		{
			sceneLoader.LoadNextDialogueScene("pintos#3");
		}
	}

	void CheckRemainPhase()
	{	
		if (isBattleEnd) return;

		int currentPhase = battleData.currentPhase;

		if (currentPhase > maxPhase)
		{
			sceneLoader.LoadNextDialogueScene("pintos#3");
		}
	}
}
