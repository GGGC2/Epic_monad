using System.Collections;
using UnityEngine;

namespace Battle.Skills {
    class Eugene_8_l_SkillLogic : BaseSkillLogic {
        public override IEnumerator TriggerStatusEffectAtReflection(Unit target, StatusEffect statusEffect, Unit reflectTarget) {
            BattleManager battleManager = MonoBehaviour.FindObjectOfType<BattleManager>();
            float maxHealth = target.GetMaxHealth();
            float percentage = statusEffect.GetAmount(1) / 100;
            yield return battleManager.StartCoroutine(target.RecoverHealth(maxHealth * percentage));
        }
        public override bool TriggerStatusEffectWhenStatusEffectApplied(Unit target, StatusEffect statusEffect, StatusEffect appliedStatusEffect) {
            appliedStatusEffect.DecreaseRemainPhase((int)statusEffect.GetAmount(2));
            if (appliedStatusEffect.GetRemainPhase() <= 0) {
                return false;
            } else return true;
        }
    }
}
