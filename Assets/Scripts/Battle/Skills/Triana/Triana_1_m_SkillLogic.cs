using Enums;
using System.Collections.Generic;

namespace Battle.Skills {
    class Triana_1_m_SkillLogic : BaseSkillLogic {
        public override bool TriggerStatusEffectApplied(UnitStatusEffect statusEffect, Unit caster, Unit target, List<Tile> targetTiles) {
            if(caster.myInfo.element == Element.Water) return true;
            return false;
        }
    }
}
