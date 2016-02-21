﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UnitManager : MonoBehaviour {

	int standardActionPoint;

	public GameObject unitPrefab;
	List<GameObject> units = new List<GameObject>();
	List<GameObject> readiedUnits = new List<GameObject>();

    public List<GameObject> GetAllUnits()
    {
        return units;
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
            unit.GetComponent<Unit>().DecreaseRemainPhaseBuffAndDebuff();
        
        foreach (var unit in units)
            unit.GetComponent<Unit>().RegenerateActionPoint();
	}
    
    int CompareByActionPoint(GameObject unit, GameObject anotherUnit)
    {
        int compareResultByCurrentActionPoint = anotherUnit.GetComponent<Unit>().GetCurrentActivityPoint().CompareTo(unit.GetComponent<Unit>().GetCurrentActivityPoint());
        if (compareResultByCurrentActionPoint == 0)
        {
            int compareResultByTrueDexturity = anotherUnit.GetComponent<Unit>().GetTrueDexturity().CompareTo(unit.GetComponent<Unit>().GetTrueDexturity());
            if (compareResultByTrueDexturity == 0)
                return anotherUnit.GetInstanceID().CompareTo(unit.GetInstanceID());
            else
                return compareResultByTrueDexturity;
        }
        else
            return compareResultByCurrentActionPoint;
    }

	// Use this for initialization
	void Start () {
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
