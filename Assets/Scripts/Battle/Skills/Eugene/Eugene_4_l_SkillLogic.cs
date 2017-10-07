using UnityEngine;
using System.Collections;

namespace Battle.Skills {
    class Eugene_4_l_SkillLogic : BaseSkillLogic{
        public override void TriggerStatusEffectAtReflection(Unit target, UnitStatusEffect statusEffect, Unit reflectTarget) {
			BattleManager battleManager = BattleData.battleManager;
            float maxHealth = target.GetMaxHealth();
            float percentage = statusEffect.GetAmount(1)/100;
            target.RecoverHealth(maxHealth * percentage);
        }
    }
}
