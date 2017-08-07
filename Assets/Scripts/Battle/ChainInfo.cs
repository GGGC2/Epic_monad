using UnityEngine;
using Enums;
using System.Collections;
using System.Collections.Generic;

public class ChainInfo {

	// 체인에 필요한 정보
	// 시전자, 시전스킬, 위치정보
	Unit unit;
	ActiveSkill skill;
	SkillLocation skillLocation;

	public ChainInfo (Unit unit, ActiveSkill skill, SkillLocation skillLocation)
	{
		this.unit = unit;
		this.skill = skill;
		this.skillLocation = skillLocation;
	}

	public SkillLocation GetSkillLocation(){
		return skillLocation;
	}
	public Unit GetUnit() {	 return unit;	}
	public List<Tile> GetFirstRange() {	
		return skill.GetTilesInFirstRange (skillLocation.CasterPos, skillLocation.Direction);
	}
	public List<Tile> GetSecondRange() {
		return skill.GetTilesInSecondRange (skillLocation.TargetTile, skillLocation.Direction);	
	}
	public ActiveSkill GetSkill() {	return skill;	}
	public bool IsRouteType() {	return skill.GetSkillType() == SkillType.Route;	}
	public List<Tile> GetRouteArea()
	{
		if (!IsRouteType ()) {
			Debug.LogError ("Invaild access - not route type skill");
			return new List<Tile> ();
		}
		else
			return GetFirstRange();
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
		foreach (var targetTile in GetSecondRange())
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
