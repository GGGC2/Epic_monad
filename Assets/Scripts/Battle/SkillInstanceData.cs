using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillInstanceData {
    Battle.DamageCalculator.AttackDamage damage;
    ActiveSkill skill;
    Unit caster;
    List<Tile> tiles;
    Unit mainTarget;    //ApplyDamage나 CalculateAttackDamage 등을 호출할 때 쓰일 변수. 스킬 자체의 주 대상이 아님.
    int targetCount;
    public SkillInstanceData(Battle.DamageCalculator.AttackDamage damage,
        ActiveSkill appliedSkill, Unit caster, List<Tile> tiles, Unit mainTarget, int targetCount) {
        this.damage = damage;
        this.skill = appliedSkill;
        this.caster = caster;
        this.tiles = tiles;
        this.mainTarget = mainTarget;
        this.targetCount = targetCount;
    }

    public Battle.DamageCalculator.AttackDamage GetDamage() { return damage; }
    public ActiveSkill GetSkill() { return skill; }
    public Unit GetCaster() { return caster; }
    public List<Tile> GetTiles() { return tiles; }
    public List<Unit> GetTargets() {
        List<Unit> targets = new List<Unit>();
        foreach(var tile in tiles) {
            targets.Add(tile.GetUnitOnTile());
        }
        return targets;
    }
    public Unit GetMainTarget() { return mainTarget; }
    public int GetTargetCount() { return targetCount; }
}
