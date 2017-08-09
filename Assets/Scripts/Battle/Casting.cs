﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Casting {
	Unit caster;
	ActiveSkill skill;
	SkillLocation location;
	public Casting(Unit caster, ActiveSkill skill, SkillLocation location){
		this.caster = caster;
		this.skill = skill;
		this.location = location;
	}
	public Unit Caster{
		get { return caster; }
	}
	public ActiveSkill Skill{
		get { return skill; }
	}
	public SkillLocation Location{
		get { return location; }
	}
	public List<Tile> FirstRange{
		get { return Skill.GetTilesInFirstRange (Location); }
	}
	public List<Tile> SecondRange{
		get { return Skill.GetTilesInSecondRange (Location); }
	}
	public List<Tile> RealEffectRange{
		get { return Skill.GetTilesInRealEffectRange (Location); }
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
	public int RequireAP{
		get { return Caster.GetActualRequireSkillAP(Skill); }
	}
	public IEnumerator Cast(int chainCombo){
		yield return Skill.Apply(this, chainCombo);
	}
}