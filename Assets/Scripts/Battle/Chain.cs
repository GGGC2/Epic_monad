using UnityEngine;
using Enums;
using System.Collections;
using System.Collections.Generic;

public class Chain {
	Casting casting;
	public Chain (Casting casting){
		this.casting = casting;
	}
	public Unit Caster { get { return casting.Caster; } }
	public ActiveSkill Skill { get { return casting.Skill; } }
	SkillLocation Location { get { return casting.Location; } }
	void UpdateLocation(){
		Skill.SetRealTargetTileForSkillLocation (Location);
	}
	public List<Unit> CurrentTargets{
		get {
			List<Unit> targets = new List<Unit> ();
			foreach (Tile targetTile in RealEffectRange) {
				if (targetTile.IsUnitOnTile ())
					targets.Add (targetTile.GetUnitOnTile ());
			}
			return targets;
		}
	}
	public List<Tile> SecondRange {
		get {
			//투사체 스킬은 타일 위 유닛 배치에 따라 targetTile이 변하므로 새로 갱신
			UpdateLocation();
			return casting.SecondRange;
		}
	}
	public List<Tile> RealEffectRange {
		get {
			UpdateLocation();
			return casting.RealEffectRange;
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
		UpdateLocation();
		yield return casting.Cast (chainCombo);
	}
}