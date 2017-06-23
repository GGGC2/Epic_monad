
namespace Battle.Skills {
    class Luvericha_7_m_SkillLogic : BasePassiveSkillLogic {
        public override void ApplyBonusDamageFromEachPassive(SkillInstanceData skillInstanceData) {
            Unit caster = skillInstanceData.GetCaster();
            Unit target = skillInstanceData.GetMainTarget();
            if(caster != target) {
                float LostHpPercent = 1 - (float)caster.GetCurrentHealth() / (float)caster.GetMaxHealth();
                skillInstanceData.GetDamage().relativeDamageBonus *= 1 + LostHpPercent * 0.5f;
            }
        }
    }
}
