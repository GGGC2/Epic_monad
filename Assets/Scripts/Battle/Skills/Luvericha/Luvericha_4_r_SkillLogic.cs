using System.Collections.Generic;

namespace Battle.Skills {
    class Luvericha_4_r_SkillLogic : BaseSkillLogic {
        public override bool TriggerStatusEffectApplied(UnitStatusEffect statusEffect, Unit caster, Unit target, List<Tile> targetTiles) {
            if(caster == target)
                return false;
            return true;
        }
    }
}
