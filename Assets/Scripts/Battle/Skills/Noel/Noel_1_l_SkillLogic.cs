using Enums;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Skills {
    class Noel_1_l_SkillLogic : BaseSkillLogic {
        public override bool TriggerStatusEffectApplied(UnitStatusEffect statusEffect, Unit caster, Unit target, List<Tile> targetTiles) {
            TileManager tileManager = MonoBehaviour.FindObjectOfType<TileManager>();
            List<Tile> tilesInRange = tileManager.GetTilesInRange(RangeForm.Diamond, target.GetPosition(), 0, 2, 0, Direction.Down);
            foreach(var tile in tilesInRange) {
                if(tile.IsUnitOnTile()) {
                    Unit unit = tile.GetUnitOnTile();
                    if(unit.GetSide() == Side.Ally && unit.HasStatusEffect(StatusEffectType.Shield)) {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
