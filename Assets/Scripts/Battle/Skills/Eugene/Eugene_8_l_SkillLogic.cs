using System.Collections;
using UnityEngine;

namespace Battle.Skills {
    class Eugene_8_l_SkillLogic : BaseSkillLogic {
        public override void TriggerStatusEffectAtReflection(Unit target, UnitStatusEffect statusEffect, Unit reflectTarget) {
			BattleManager battleManager = BattleData.battleManager;
            float maxHealth = target.GetMaxHealth();
            float percentage = statusEffect.GetAmount(1) / 100;
            target.RecoverHealth(maxHealth * percentage);
        }
        public override bool TriggerStatusEffectWhenStatusEffectApplied(Unit target, UnitStatusEffect statusEffect, UnitStatusEffect appliedStatusEffect) {
            appliedStatusEffect.DecreaseRemainPhase((int)statusEffect.GetAmount(2));
            if (appliedStatusEffect.GetRemainPhase() <= 0) {
                return false;
            } else return true;
        }
    }
}
