﻿using System.Collections.Generic;
using Enums;

namespace Battle.Skills
{
public class Eren_1_r_SkillLogic : BaseSkillLogic {
        public override bool TriggerStatusEffectApplied(StatusEffect statusEffect, Unit caster, Unit target, List<Tile> targetTiles) {
            StatusEffect AbsorptionStatusEffect = caster.GetStatusEffectList().Find(se => se.GetDisplayName() == "흡수");
            int stack = 0;
            if (AbsorptionStatusEffect != null)
                stack = AbsorptionStatusEffect.GetRemainStack();
            float power = caster.GetStat(Stat.Power);

            statusEffect.CalculateAmount(stack);
            statusEffect.SetAmount(0, statusEffect.GetAmount() * power);
            return true;
        }
    }
}