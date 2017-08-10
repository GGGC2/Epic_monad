using Enums;
using System.Collections.Generic;

namespace Battle.Skills {
    public class Curi_4_m_SkillLogic : BaseSkillLogic {
        public override bool TriggerStatusEffectApplied(UnitStatusEffect statusEffect, Unit caster, Unit target, List<Tile> targetTiles) {
            if(target.GetTileUnderUnit().GetTileElement() != Element.Metal) {
                return false;
            }
            return true;
        }
    }
}
