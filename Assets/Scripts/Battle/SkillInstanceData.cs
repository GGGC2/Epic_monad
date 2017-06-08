using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillInstanceData {
    Battle.DamageCalculator.AttackDamage damage;
    Skill skill;
    Unit caster;
    List<Unit> targets;
    Unit mainTarget;    //ApplyDamage나 CalculateAttackDamage 등을 호출할 때 쓰일 변수. 스킬 자체의 주 대상이 아님.
    int targetCount;
    public SkillInstanceData(Battle.DamageCalculator.AttackDamage damage,
        Skill appliedSkill, Unit caster, List<Unit> targets, Unit mainTarget, int targetCount) {
        this.damage = damage;
        this.skill = appliedSkill;
        this.caster = caster;
        this.targets = targets;
        this.mainTarget = mainTarget;
        this.targetCount = targetCount;
    }

    public Battle.DamageCalculator.AttackDamage GetDamage() { return damage; }
    public Skill GetSkill() { return skill; }
    public Unit GetCaster() { return caster; }
    public List<Unit> GetTargets() { return targets;}
    public Unit GetMainTarget() { return mainTarget; }
    public int GetTargetCount() { return targetCount; }
}
