using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

namespace Battle.Skills{
    class Luvericha_3_r_SkillLogic : BasePassiveSkillLogic {
        public override IEnumerator TriggerOnPhaseStart(Unit caster) {
            TileManager tileManager = MonoBehaviour.FindObjectOfType<TileManager>();
            List<Tile> tilesInRange = tileManager.GetTilesInRange(RangeForm.Diamond, caster.position, 1, 3, 0, Direction.Down);
            foreach(var tile in tilesInRange) {
                if(tile.IsUnitOnTile()) {
                    Unit unit = tile.GetUnitOnTile();
                    yield return unit.RecoverHealth(unit.GetMaxHealth() * 0.05f);
                }
            }
        }
    }
}
