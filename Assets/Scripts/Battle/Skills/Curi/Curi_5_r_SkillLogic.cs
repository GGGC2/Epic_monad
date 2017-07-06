using Enums;
using System.Collections.Generic;

namespace Battle.Skills {
    public class Curi_5_r_SkillLogic : BaseSkillLogic {
        public override bool TriggerStatusEffectApplied(StatusEffect statusEffect, Unit caster, Unit target, List<Tile> targetTiles) {
            if(target.GetTileUnderUnit().GetTileElement() == Element.Metal) {
                statusEffect.flexibleElem.display.remainPhase += 1;
            }
            return true;
        }
    }
}
