
namespace Battle.Skills {
    class Grenev_3_m_SkillLogic : BaseSkillLogic {
        public override void ApplyAdditionalDamageFromTargetStatusEffect(SkillInstanceData skillInstanceData, UnitStatusEffect statusEffect) {
            Unit caster = skillInstanceData.GetCaster();
            Unit target = skillInstanceData.GetMainTarget();

            if(statusEffect.GetOriginSkillName() == "암살표식" && statusEffect.GetCaster() == caster) {
                skillInstanceData.GetDamage().relativeDamageBonus *= 1.5f;
            }
        }
    }
}
