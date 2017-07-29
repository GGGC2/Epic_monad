using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChainInfo {

	// 체인에 필요한 정보?
	// 시전자, 중심, 영역, 시전스킬 + 경로형 여부, 경로범위
	Unit unit;
	Tile centerTile;
	List<Tile> targetArea;
	List<Tile> firstRange;
	ActiveSkill skill;

	List<Tile> routeArea;

	public ChainInfo (Unit unit, Tile centerTile, List<Tile> secondRange, ActiveSkill skill, List<Tile> firstRange)
	{
		this.unit = unit;
		this.centerTile = centerTile;
		this.targetArea = secondRange;
		this.skill = skill;

		this.firstRange = firstRange;
	}
	
	public Unit GetUnit() {	return unit;	}
	public Tile GetCenterTile() {	return centerTile;	}
	public List<Tile> GetTargetArea() {	return targetArea;	}
	public ActiveSkill GetSkill() {	return skill;	}
	public bool IsRouteType() {	return skill.GetSkillType() == Enums.SkillType.Route;	}
	public List<Tile> GetRouteArea()
	{
		if (!IsRouteType())
		{
			Debug.LogError("Invaild access - not route type skill");
			return new List<Tile>();
		}
		else
			return firstRange;
	}
	
	public bool Overlapped(List<Tile> anotherTargetArea)
	{
		List<Unit> anotherTargets = new List<Unit>();
		foreach (var anotherTargetTile in anotherTargetArea)
		{
			if (anotherTargetTile.IsUnitOnTile())
				anotherTargets.Add(anotherTargetTile.GetUnitOnTile());
		}
		
		List<Unit> targets = new List<Unit>();
		foreach (var targetTile in targetArea)
		{
			if (targetTile.IsUnitOnTile())
				targets.Add(targetTile.GetUnitOnTile());
		}

		foreach (var anotherTarget in anotherTargets)
		{
			if (targets.Contains(anotherTarget))
				return true;
		}
		
		return false;
	}
}
