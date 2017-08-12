
namespace Battle.Skills {
    class Grenev_3_m_SkillLogic : BaseSkillLogic {
        public override void ApplyAdditionalDamageFromTargetStatusEffect(CastingApply castingApply, UnitStatusEffect statusEffect) {
            Unit caster = castingApply.GetCaster();
            Unit target = castingApply.GetTarget();

            if(statusEffect.GetOriginSkillName() == "암살표식" && statusEffect.GetCaster() == caster) {
                castingApply.GetDamage().relativeDamageBonus *= 1.5f;
            }
        }
    }
}
