using Battle.Damage;

namespace Battle.Skills {
    class Grenev_0_1_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerOnStart(Unit caster) {
            StatusEffector.AttachStatusEffect(caster, passiveSkill, caster);
        }
        public override void ApplyBonusDamageFromEachPassive(CastingApply castingApply) {
            Unit caster = castingApply.GetCaster();
            Unit target = castingApply.GetTarget();
            DamageCalculator.AttackDamage attackDamage = castingApply.GetDamage();

            int distance = Utility.GetDistance(caster.GetPosition(), target.GetPosition());
            attackDamage.relativeDamageBonus *= (float)(1 + (0.02 * distance));
        }

    }
}
