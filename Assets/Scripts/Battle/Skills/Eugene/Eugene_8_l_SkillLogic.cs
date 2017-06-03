
namespace Battle.Skills {
    class Eugene_8_l_SkillLogic : BaseSkillLogic {
        public override void TriggerStatusEffectAtReflection(Unit target, StatusEffect statusEffect, Unit reflectTarget) {
            float maxHealth = target.GetMaxHealth();
            float percentage = statusEffect.GetAmount(1) / 100;
            target.RecoverHealth(maxHealth * percentage);
        }
        public override bool TriggerStatusEffectWhenStatusEffectApplied(Unit target, StatusEffect statusEffect, StatusEffect appliedStatusEffect) {
            appliedStatusEffect.DecreaseRemainPhase((int)statusEffect.GetAmount(2));
            if (appliedStatusEffect.GetRemainPhase() <= 0) {
                return false;
            } else return true;
        }
    }
}
