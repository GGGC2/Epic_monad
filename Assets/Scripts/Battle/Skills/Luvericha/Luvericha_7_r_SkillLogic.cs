
namespace Battle.Skills {
    class Luvericha_7_r_SkillLogic : BasePassiveSkillLogic{
        public override void ApplyBonusDamageFromEachPassive(SkillInstanceData skillInstanceData) {
            Unit target = skillInstanceData.GetTarget();
            float LostHpPercent = 1 - (float)target.GetCurrentHealth() / (float)target.GetMaxHealth();
            skillInstanceData.GetDamage().relativeDamageBonus *= 1 + LostHpPercent * 0.3f;
        }
    }
}
