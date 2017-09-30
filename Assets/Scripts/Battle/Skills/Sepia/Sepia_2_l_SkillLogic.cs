using System;
using System.Collections;
using Enums;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Battle.Skills{
    class Sepia_2_l_SkillLogic : BaseSkillLogic {
        public override bool TriggerStatusEffectApplied(UnitStatusEffect statusEffect, Unit caster, Unit target, List<Tile> targetTiles) {
            TileManager tileManager = TileManager.Instance;
            Tile frontTile = tileManager.GetTile(caster.GetPosition() + Utility.ToVector2(caster.GetDirection()));
            if (frontTile.IsUnitOnTile() && frontTile.GetUnitOnTile().GetSide() == Side.Ally)
                return true;
            return false;
        }
    }
}
