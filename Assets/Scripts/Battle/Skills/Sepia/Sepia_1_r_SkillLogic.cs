using Enums;
using System.Collections;
using System.Collections.Generic;
using Battle.Damage;

namespace Battle.Skills {
    class Sepia_1_r_SkillLogic : BaseSkillLogic {
        public override IEnumerator ActionInDamageRoutine(SkillInstanceData skillInstanceData) {
            Unit caster = skillInstanceData.GetCaster();
            StatusEffector.AttachStatusEffect(caster, skill, caster, skillInstanceData.GetTiles());
            caster.UpdateHealthViewer();
            yield return null;
        }
        public override bool TriggerStatusEffectApplied(StatusEffect statusEffect, Unit caster, Unit target, List<Tile> targetTiles) {
            if(caster != target)    return false;
            int targetCount = 0;
            foreach(var tile in targetTiles) {
                if(tile.IsUnitOnTile()) targetCount++;
            }

            float power = caster.GetStat(Stat.Power);
            statusEffect.CalculateAmount(0, targetCount);
            statusEffect.SetAmount(0, statusEffect.GetAmountOfType(StatusEffectType.Shield) * power);
            statusEffect.SetRemainAmount(0, statusEffect.GetAmount(0));
            return true;
        }
    }
}
