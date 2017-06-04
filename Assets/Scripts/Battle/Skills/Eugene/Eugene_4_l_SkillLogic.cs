
namespace Battle.Skills {
    class Eugene_4_l_SkillLogic : BaseSkillLogic{
        public override void TriggerStatusEffectAtReflection(Unit target, StatusEffect statusEffect, Unit reflectTarget) {
            float maxHealth = target.GetMaxHealth();
            float percentage = statusEffect.GetAmount(1)/100;
            target.RecoverHealth(maxHealth * percentage);
        }
    }
}
