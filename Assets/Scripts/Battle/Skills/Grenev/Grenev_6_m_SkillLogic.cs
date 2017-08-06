using Enums;

namespace Battle.Skills {
    class Grenev_6_m_SkillLogic : BasePassiveSkillLogic {
        public override void ApplyBonusDamageFromEachPassive(SkillInstanceData skillInstanceData) {
            Unit caster = skillInstanceData.GetCaster();
            if(caster.HasStatusEffect(StatusEffectType.Stealth)) {
                skillInstanceData.GetDamage().relativeDamageBonus *= 1.5f;
            }
        }
    }
}
