﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastingApply {
	Casting casting;
	Unit target;
	Battle.DamageCalculator.AttackDamage damage = new Battle.DamageCalculator.AttackDamage ();

    public CastingApply(Casting casting, Unit target) {
		this.casting = casting;
        this.target = target;
    }

    public Battle.DamageCalculator.AttackDamage GetDamage() { return damage; }
    public ActiveSkill GetSkill() { return casting.Skill; }
    public Unit GetCaster() { return casting.Caster; }
	public List<Tile> GetRealEffectRange() { return casting.RealEffectRange; }
    public List<Unit> GetTargets() {
        List<Unit> targets = new List<Unit>();
		foreach(var tile in GetRealEffectRange()) {
            targets.Add(tile.GetUnitOnTile());
        }
        return targets;
    }
    public Unit GetTarget() { return target; }
	public int GetTargetCount() { return GetTargets().Count; }
    public bool IsLast() {
        List<Unit> targets = casting.Targets;
        return target == targets[targets.Count - 1];
    }
}