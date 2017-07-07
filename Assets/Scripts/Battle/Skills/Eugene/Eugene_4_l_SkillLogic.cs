using UnityEngine;
using System.Collections;

namespace Battle.Skills {
    class Eugene_4_l_SkillLogic : BaseSkillLogic{
        public override IEnumerator TriggerStatusEffectAtReflection(Unit target, StatusEffect statusEffect, Unit reflectTarget) {
            BattleManager battleManager = MonoBehaviour.FindObjectOfType<BattleManager>();
            float maxHealth = target.GetMaxHealth();
            float percentage = statusEffect.GetAmount(1)/100;
            yield return battleManager.StartCoroutine(target.RecoverHealth(maxHealth * percentage));
        }
    }
}
