using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillInstanceData : MonoBehaviour {
    Battle.DamageCalculator.AttackDamage damage;
    Skill skill;
    Unit caster;
    Unit target;
    int targetCount;
    public SkillInstanceData(Battle.DamageCalculator.AttackDamage damage,
        Skill appliedSkill, Unit caster, Unit target, int targetCount) {
        this.damage = damage;
        this.skill = appliedSkill;
        this.caster = caster;
        this.target = target;
        this.targetCount = targetCount;
    }

    public Battle.DamageCalculator.AttackDamage getDamage() { return damage; }
    public Skill getSkill() { return skill; }
    public Unit getCaster() { return caster; }
    public Unit getTarget() { return target; }
    public int getTargetCount() { return targetCount; }
}
