using Enums;
using System.Collections.Generic;

namespace Battle.Skills {
    public class Curi_7_r_SkillLogic : BaseSkillLogic {
        public override bool TriggerStatusEffectApplied(StatusEffect statusEffect, Unit caster, Unit target, List<Tile> targetTiles) {
            if ((target.currentHealth <= (int)(target.GetMaxHealth()) * 0.4f) && (target.GetUnitClass() == UnitClass.Magic)){
                return true;
            }
            return false;
        }
        public override void ApplyAdditionalDamage(SkillInstanceData skillInstanceData) {
            if(skillInstanceData.GetMainTarget().GetUnitClass() != UnitClass.Magic) {
                skillInstanceData.GetDamage().relativeDamageBonus = 0;
            }
        }
    }
}