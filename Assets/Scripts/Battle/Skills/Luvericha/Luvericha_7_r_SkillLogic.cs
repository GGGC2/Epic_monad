
namespace Battle.Skills {
    class Luvericha_7_r_SkillLogic : BasePassiveSkillLogic{
        public override void ApplyBonusDamageFromEachPassive(CastingApply castingApply) {
            Unit target = castingApply.GetTarget();
            float LostHpPercent = 1 - (float)target.GetCurrentHealth() / (float)target.GetMaxHealth();
            castingApply.GetDamage().relativeDamageBonus *= 1 + LostHpPercent * 0.3f;
        }
    }
}
