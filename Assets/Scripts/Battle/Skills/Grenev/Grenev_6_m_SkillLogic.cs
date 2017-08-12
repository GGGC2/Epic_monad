using Enums;

namespace Battle.Skills {
    class Grenev_6_m_SkillLogic : BasePassiveSkillLogic {
        public override void ApplyBonusDamageFromEachPassive(CastingApply castingApply) {
            Unit caster = castingApply.GetCaster();
            if(caster.HasStatusEffect(StatusEffectType.Stealth)) {
                castingApply.GetDamage().relativeDamageBonus *= 1.5f;
            }
        }
    }
}
