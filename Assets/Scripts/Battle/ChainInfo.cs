using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChainInfo {

	// 체인에 필요한 정보?
	// 시전자, 중심, 영역, 시전스킬 + 경로형 여부, 경로범위
	GameObject unit;
	Tile centerTile;
	List<GameObject> targetArea;
	List<GameObject> firstRange;
	Skill skill;

	List<GameObject> routeArea;

	public ChainInfo (GameObject unit, Tile centerTile, List<GameObject> secondRange, Skill skill, List<GameObject> firstRange)
	{
		this.unit = unit;
		this.centerTile = centerTile;
		this.targetArea = secondRange;
		this.skill = skill;

		this.firstRange = firstRange;
	}
	
	public GameObject GetUnit() {	return unit;	}
	public Tile GetCenterTile() {	return centerTile;	}
	public List<GameObject> GetTargetArea() {	return targetArea;	}
	public Skill GetSkill() {	return skill;	}
	public bool IsRouteType() {	return skill.GetSkillType() == Enums.SkillType.Route;	}
	public List<GameObject> GetRouteArea()
	{
		if (!IsRouteType())
		{
			Debug.LogError("Invaild access - not route type skill");
			return new List<GameObject>();
		}
		else
			return firstRange;
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
