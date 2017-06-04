using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;
using System.Linq;

namespace Battle.Skills
{
public class Eren_1_r_SkillLogic : BaseSkillLogic {
        public override bool TriggerStatusEffectApplied(StatusEffect statusEffect, Unit caster, Unit target) {
            StatusEffect AbsorptionStatusEffect = caster.GetStatusEffectList().Find(se => se.GetDisplayName() == "흡수");
            int stack = 0;
            if (AbsorptionStatusEffect != null)
                stack = AbsorptionStatusEffect.GetRemainStack();
            float power = caster.GetActualStat(Stat.Power);

            statusEffect.CalculateAmount(stack);
            statusEffect.SetAmount(statusEffect.GetAmount() * power);
            return true;
        }
    }
}