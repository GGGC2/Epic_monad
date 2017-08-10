using UnityEngine;
using Battle.Damage;
using System.Collections.Generic;
using Enums;
using System.Linq;

namespace Battle.Skills
{
    public class Yeong_2_r_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerOnStart(Unit caster) {
            StatusEffector.AttachStatusEffect(caster, passiveSkill, caster);
        }
        public override float GetStatusEffectVar(UnitStatusEffect statusEffect, int i, Unit caster, Unit owner) {
            UnitManager unitManager = MonoBehaviour.FindObjectOfType<UnitManager>();
            TileManager tileManager = MonoBehaviour.FindObjectOfType<TileManager>();
            Vector2 unitPosition = owner.GetPosition();
            List<Tile> nearbyTiles = tileManager.GetTilesInRange(RangeForm.Diamond, unitPosition, 1, 3, 0, Direction.LeftUp);

            List<Unit> nearbyUnits = new List<Unit>();
            foreach (var tile in nearbyTiles) {
                if (tile.IsUnitOnTile())
                    nearbyUnits.Add(tile.GetUnitOnTile());
            }
            return nearbyUnits.Count(x => x.GetSide() == Side.Enemy);
        }
    }
}
