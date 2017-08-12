
namespace Battle.Skills {
    class Luvericha_7_m_SkillLogic : BasePassiveSkillLogic {
        public override void ApplyBonusDamageFromEachPassive(CastingApply castingApply) {
            Unit caster = castingApply.GetCaster();
            Unit target = castingApply.GetTarget();
            if(caster != target) {
                float LostHpPercent = 1 - (float)caster.GetCurrentHealth() / (float)caster.GetMaxHealth();
                castingApply.GetDamage().relativeDamageBonus *= 1 + LostHpPercent * 0.5f;
            }
        }
    }
}
