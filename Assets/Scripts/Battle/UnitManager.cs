using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;
using Util;

public class DeadUnitInfo
{
	public readonly string unitName;
	public readonly Side unitSide;

	public DeadUnitInfo(GameObject unitObject)
	{
		Unit unit = unitObject.GetComponent<Unit>();
		unitName = unit.GetName();
		unitSide = unit.GetSide();
	}
}

public class RetreatUnitInfo
{
	public readonly string unitName;
	public readonly Side unitSide;

	public RetreatUnitInfo(GameObject unitObject)
	{
		Unit unit = unitObject.GetComponent<Unit>();
		unitName = unit.GetName();
		unitSide = unit.GetSide();
	}
}

public class UnitManager : MonoBehaviour {

	int standardActionPoint;

	List<SkillInfo> skillInfoList = new List<SkillInfo>();
    List<StatusEffectInfo> statusEffectInfoList = new List<StatusEffectInfo>();

	public GameObject unitPrefab;
	List<GameObject> units = new List<GameObject>();
	List<GameObject> readiedUnits = new List<GameObject>();
	List<GameObject> deadUnits = new List<GameObject>();
	List<GameObject> retreatUnits = new List<GameObject>();
    List<GameObject> enemyUnits = new List<GameObject>();
	List<DeadUnitInfo> deadUnitsInfo = new List<DeadUnitInfo>();
	List<RetreatUnitInfo> retreatUnitsInfo = new List<RetreatUnitInfo>();

	public List<GameObject> GetAllUnits()
	{
		return units;
	}
	
	public List<GameObject> GetRetreatUnits()
	{
		retreatUnits.Clear();
		foreach (var unit in units)
		{
			float percentHealth = 100f * (float)unit.GetComponent<Unit>().GetCurrentHealth() / (float)unit.GetComponent<Unit>().GetMaxHealth();
			if (((percentHealth <= 10) && (unit.GetComponent<Unit>().GetCurrentHealth() > 0)) || 
				(retreatUnits.Contains(unit)))
				retreatUnits.Add(unit);		
		}

		return retreatUnits;
	}

	public void	MakeRetreatUnitInfo()
	{		
		foreach (var retreatUnit in retreatUnits)
		{
			RetreatUnitInfo retreatUnitInfo = new RetreatUnitInfo(retreatUnit);
			retreatUnitsInfo.Add(retreatUnitInfo);
		}
	}

	public List<RetreatUnitInfo> GetRetreatUnitsInfo()
	{
		return retreatUnitsInfo;
	}

	public List<GameObject> GetDeadUnits()
	{
		// 죽은 유닛들을 체크.
		deadUnits.Clear();
		foreach (var unit in units)
		{
			if ((unit.GetComponent<Unit>().GetCurrentHealth() <= 0) || (deadUnits.Contains(unit)))
				deadUnits.Add(unit);		
		}

		return deadUnits;
	}

	void MakeDeadUnitInfo()
	{		
		foreach (var deadUnit in deadUnits)
		{
			DeadUnitInfo deadUnitInfo = new DeadUnitInfo(deadUnit);
			deadUnitsInfo.Add(deadUnitInfo);
		}
	}

	public List<DeadUnitInfo> GetDeadUnitsInfo()
	{
		MakeDeadUnitInfo();
		return deadUnitsInfo;
	}
	
	public int GetStandardActionPoint()
	{
		return standardActionPoint;
	}
	
	public void SetStandardActionPoint(int partyLevel)
	{
		standardActionPoint = partyLevel + 60;
	}

	void GenerateUnits ()
	{
		// TileManager tileManager = GetComponent<TileManager>(); 
		float tileWidth = 0.5f*200/100;
		float tileHeight = 0.5f*100/100;
		
		List<UnitInfo> unitInfoList = Parser.GetParsedUnitInfo();
		
		foreach (var unitInfo in unitInfoList)
		{
			GameObject unit = Instantiate(unitPrefab) as GameObject;
			
			unit.GetComponent<Unit>().ApplyUnitInfo(unitInfo);
			unit.GetComponent<Unit>().ApplySkillList(skillInfoList, statusEffectInfoList);
			
			Vector2 initPosition = unit.GetComponent<Unit>().GetInitPosition();
			// Vector3 tilePosition = tileManager.GetTilePos(initPosition);
			// Vector3 respawnPos = tilePosition + new Vector3(0,0,5f);
			Vector3 respawnPos = new Vector3(tileWidth * (initPosition.y + initPosition.x) * 0.5f, 
											 tileHeight * (initPosition.y - initPosition.x) * 0.5f, 
											 (initPosition.y - initPosition.x) * 0.1f - 5f);
			unit.transform.position = respawnPos;
			
			GameObject tileUnderUnit = FindObjectOfType<TileManager>().GetTile((int)initPosition.x, (int)initPosition.y);
			tileUnderUnit.GetComponent<Tile>().SetUnitOnTile(unit);
			
			units.Add(unit);
		}
		
		Debug.Log("Generate units complete");
	}
	
	public void DeleteDeadUnit(GameObject unitObject)
	{
		units.Remove(unitObject);
		readiedUnits.Remove(unitObject);
	}

	public void DeleteRetreatUnit(GameObject unitObject)
	{
		units.Remove(unitObject);
		readiedUnits.Remove(unitObject);
	}

	public List<GameObject> GetUpdatedReadiedUnits()
	{
		readiedUnits.Clear();
		// check each unit and add all readied units.
		foreach (var unit in units)
		{
			if (unit.GetComponent<Unit>().GetCurrentActivityPoint() >= standardActionPoint)
			{
				readiedUnits.Add(unit);
				Debug.Log(unit.GetComponent<Unit>().GetName() + " is readied");
			}
		}
		
		// AP가 큰 순서대로 소팅.
		readiedUnits.Sort(SortHelper.Chain(new List<Comparison<GameObject>>
		{
			SortHelper.CompareBy<GameObject>(go => go.GetComponent<Unit>().GetCurrentActivityPoint()),
			SortHelper.CompareBy<GameObject>(go => go.GetComponent<Unit>().GetActualStat(Stat.Dexturity)),
			SortHelper.CompareBy<GameObject>(go => go.GetInstanceID())
		}, reverse:true));
		
		return readiedUnits;
	}

    public List<GameObject> GetEnemyUnits()
    {
        foreach (var unit in units)
        {
            if (unit.GetComponent<Unit>().GetSide() == Side.Enemy)
            {
                enemyUnits.Add(unit);
                Debug.Log(unit.GetComponent<Unit>().GetName() + " is enemy");
            }
        }
        return enemyUnits;
    }

	public void EndPhase()
	{
		// Decrease each buff & debuff phase
		foreach (var unit in units)
		{
			unit.GetComponent<Unit>().DecreaseRemainPhaseStatusEffect();
			unit.GetComponent<Unit>().UpdateStatusEffect();
		}
		
		foreach (var unit in units)
			unit.GetComponent<Unit>().RegenerateActionPoint();
	}
	
	void LoadSkills()
	{
		skillInfoList = Parser.GetParsedSkillInfo();
	}
    
    void LoadStatusEffects()
    {
        statusEffectInfoList = Parser.GetParsedStatusEffectInfo();
    }

	// Use this for initialization
	void Start () {
		LoadSkills();
        LoadStatusEffects();
		GenerateUnits ();
        GetEnemyUnits();
	}
	
	// Update is called once per frame
	void Update () {

		int standardActionPoint = GetStandardActionPoint();
		List<GameObject> currentTurnUnits =
			units.FindAll(go => go.GetComponent<Unit>().GetCurrentActivityPoint() >= standardActionPoint);
		List<GameObject> nextTurnUnits =
			units.FindAll(go => go.GetComponent<Unit>().GetCurrentActivityPoint() < standardActionPoint);

		currentTurnUnits.Sort(SortHelper.Chain(new List<Comparison<GameObject>>
		{
			SortHelper.CompareBy<GameObject>(go => go.GetComponent<Unit>().GetCurrentActivityPoint()),
			SortHelper.CompareBy<GameObject>(go => go.GetComponent<Unit>().GetActualStat(Stat.Dexturity)),
			SortHelper.CompareBy<GameObject>(go => go.GetInstanceID())
		}, reverse:true));

		nextTurnUnits.Sort(SortHelper.Chain(new List<Comparison<GameObject>>
		{
			SortHelper.CompareBy<GameObject>(go => {
					int currentAP = go.GetComponent<Unit>().GetCurrentActivityPoint();
					int recover = go.GetComponent<Unit>().GetActualStat(Stat.Dexturity);
					return currentAP + recover;
			}),
			SortHelper.CompareBy<GameObject>(go => go.GetComponent<Unit>().GetActualStat(Stat.Dexturity)),
			SortHelper.CompareBy<GameObject>(go => go.GetInstanceID())
		}, reverse:true));

	   // 유닛 전체에 대해서도 소팅. 변경점이 있을때마다 반영된다.
		units.Clear();
		units.AddRange(currentTurnUnits);
		units.AddRange(nextTurnUnits);
	}
}
