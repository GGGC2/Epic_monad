using UnityEngine;
using Enums;
using System.Collections;
using System.Collections.Generic;

public class ChainInfo {

	// 체인에 필요한 정보
	// 시전자, 시전스킬, 위치정보
	Casting casting;

	public ChainInfo (Casting casting)
	{
		this.casting = casting;
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

	public Casting Casting { get { return casting; } }
	public Unit Caster { get { return casting.Caster; } }
	public ActiveSkill Skill { get { return casting.Skill; } }
	public SkillLocation Location { get { return casting.Location; } }
	public List<Tile> GetSecondRange() {
		//투사체 스킬은 타일 위 유닛 배치에 따라 targetTile이 변하므로 새로 갱신
		Skill.SetRealTargetTileForSkillLocation(Location);
		//체인 걸어둔 투사체 스킬이 아무것도 안 때리고 사라지면 발동 안 되므로 GetTilesInSecondRange가 아니라 GetTilesInRealEffectRange 호출
		return Skill.GetTilesInRealEffectRange (Location);
	}
}
