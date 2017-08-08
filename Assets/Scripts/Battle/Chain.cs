using UnityEngine;
using Enums;
using System.Collections;
using System.Collections.Generic;

public class Chain {
	// 체인에 필요한 정보
	// 시전자, 시전스킬, 위치정보를 담은 Casting 인스턴스
	Casting casting;

	public Chain (Casting casting)
	{
		this.casting = casting;
	}

	private Casting Casting { get { return casting; } }
	public Unit Caster { get { return casting.Caster; } }
	public ActiveSkill Skill { get { return casting.Skill; } }
	private SkillLocation Location { get { return casting.Location; } }
	public List<Unit> CurrentTargets{
		get { return casting.CurrentTargets; }
	}
	public List<Tile> SecondRange {
		get {
			//투사체 스킬은 타일 위 유닛 배치에 따라 targetTile이 변하므로 새로 갱신
			Skill.SetRealTargetTileForSkillLocation (Location);
			return Skill.GetTilesInSecondRange (Location);
		}
	}
	public List<Tile> RealEffectRange {
		get {
			//투사체 스킬은 타일 위 유닛 배치에 따라 targetTile이 변하므로 새로 갱신
			Skill.SetRealTargetTileForSkillLocation (Location);
			return Skill.GetTilesInRealEffectRange (Location);
		}
	}

	public bool Overlapped(Chain allyChain)
	{
		List<Unit> myTargets = CurrentTargets;
		List<Unit> allyTargets = allyChain.CurrentTargets;
		foreach (Unit allyTarget in allyTargets)
		{
			if (myTargets.Contains(allyTarget))
				return true;
		}
		return false;
	}

	public IEnumerator Cast(int chainCombo){
		yield return casting.Cast (chainCombo);
	}
}