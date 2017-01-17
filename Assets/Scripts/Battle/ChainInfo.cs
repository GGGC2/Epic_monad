using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChainInfo {

	// 체인에 필요한 정보?
	// 시전자, 중심, 영역, 시전스킬
	GameObject unit;
	Tile centerTile;
	List<GameObject> targetArea;
	int skillIndex;

	public ChainInfo (GameObject unit, Tile centerTile, List<GameObject> targetArea, int skillIndex)
	{
		this.unit = unit;
		this.centerTile = centerTile;
		this.targetArea = targetArea;
		this.skillIndex = skillIndex;
	}
	
	public GameObject GetUnit()
	{
		return unit;
	}
	
	public Tile GetCenterTile()
	{
		return centerTile;
	}

	public List<GameObject> GetTargetArea()
	{
		return targetArea;
	}
	
	public int GetSkillIndex()
	{
		return skillIndex;
	}
	
	public bool Overlapped(List<GameObject> anotherTargetArea)
	{
		List<GameObject> anotherTargets = new List<GameObject>();
		foreach (var anotherTargetTile in anotherTargetArea)
		{
			if (anotherTargetTile.GetComponent<Tile>().IsUnitOnTile())
				anotherTargets.Add(anotherTargetTile.GetComponent<Tile>().GetUnitOnTile());
		}
		
		List<GameObject> targets = new List<GameObject>();
		foreach (var targetTile in targetArea)
		{
			if (targetTile.GetComponent<Tile>().IsUnitOnTile())
				targets.Add(targetTile.GetComponent<Tile>().GetUnitOnTile());
		}

		foreach (var anotherTarget in anotherTargets)
		{
			if (targets.Contains(anotherTarget))
				return true;
		}
		
		return false;
	}
}
