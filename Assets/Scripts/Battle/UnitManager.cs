using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Enums;

public class UnitManager : MonoBehaviour {

	int standardActionPoint;

	List<SkillInfo> skillInfoList = new List<SkillInfo>();
    List<StatusEffectInfo> statusEffectInfoList = new List<StatusEffectInfo>();

	public GameObject unitPrefab;
	List<GameObject> units = new List<GameObject>();
	List<GameObject> readiedUnits = new List<GameObject>();
	List<GameObject> deadUnits = new List<GameObject>();

	public List<GameObject> GetAllUnits()
	{
		return units;
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
		readiedUnits.Sort(delegate(GameObject x, GameObject y)
		{
			if (x.GetComponent<Unit>() == null && y.GetComponent<Unit>() == null) return 0;
			else if (y.GetComponent<Unit>() == null) return -1;
			else if (x.GetComponent<Unit>() == null) return 1;
			else return CompareByActionPoint(x, y);
		});
		
		return readiedUnits;
	}

	public void EndPhase()
	{
		// Decrease each buff & debuff phase
		foreach (var unit in units)
			unit.GetComponent<Unit>().DecreaseRemainPhaseStatusEffect();
		
		foreach (var unit in units)
			unit.GetComponent<Unit>().RegenerateActionPoint();
	}
	
	int CompareByActionPoint(GameObject unitGO, GameObject anotherUnitGO)
	{
		Unit unit = unitGO.GetComponent<Unit>();
		Unit anotherUnit = anotherUnitGO.GetComponent<Unit>();

		int unitActionPoint = 0;
		int anotherUnitActionPoint = 0;
		if (unit.GetCurrentActivityPoint() < GetStandardActionPoint() && anotherUnit.GetCurrentActivityPoint() < GetStandardActionPoint())
		{
			unitActionPoint = unit.GetCurrentActivityPoint() + unit.GetActualStat(Stat.Dexturity);
			anotherUnitActionPoint = anotherUnit.GetCurrentActivityPoint() + anotherUnit.GetActualStat(Stat.Dexturity);
		}
		else
		{
			unitActionPoint = unit.GetCurrentActivityPoint();
			anotherUnitActionPoint = anotherUnit.GetCurrentActivityPoint();
		}

		int compareResultByActionPoint = anotherUnitActionPoint.CompareTo(unitActionPoint);
		if (compareResultByActionPoint != 0)
		{
			return compareResultByActionPoint;
		}

		int compareResultByTrueDexturity = anotherUnit.GetActualStat(Stat.Dexturity).CompareTo(unit.GetActualStat(Stat.Dexturity));
		if (compareResultByTrueDexturity != 0)
		{
			return compareResultByTrueDexturity;
		}

		return anotherUnit.GetInstanceID().CompareTo(unit.GetInstanceID());
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
	}
	
	// Update is called once per frame
	void Update () {
	   // 유닛 전체에 대해서도 소팅. 변경점이 있을때마다 반영된다.
		units.Sort(delegate(GameObject x, GameObject y)
		{
			if (x.GetComponent<Unit>() == null && y.GetComponent<Unit>() == null) return 0;
			else if (y.GetComponent<Unit>() == null) return -1;
			else if (x.GetComponent<Unit>() == null) return 1;
			else return CompareByActionPoint(x, y);
		});
	}
}
