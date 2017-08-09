using System.Collections.Generic;
using Enums;

namespace Battle.Skills
{
public class Eren_1_r_SkillLogic : BaseSkillLogic {
        public override bool TriggerStatusEffectApplied(UnitStatusEffect statusEffect, Unit caster, Unit target, List<Tile> targetTiles) {
            UnitStatusEffect AbsorptionStatusEffect = caster.GetStatusEffectList().Find(se => se.GetDisplayName() == "흡수");
            int stack = 0;
            if (AbsorptionStatusEffect != null)
                stack = AbsorptionStatusEffect.GetRemainStack();
            float power = caster.GetStat(Stat.Power);

            statusEffect.CalculateAmount(0, stack);
            statusEffect.SetAmount(0, statusEffect.GetAmountOfType(StatusEffectType.Smite) * power);
            return true;
        }
    }
}