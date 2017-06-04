using UnityEngine;
using Battle.Damage;
using System.Collections.Generic;
using Enums;
using System.Linq;

namespace Battle.Skills
{
    public class Yeong_2_r_SkillLogic : BasePassiveSkillLogic
    {
        public override void TriggerStart(Unit caster) {
            StatusEffector.AttachStatusEffect(caster, passiveSkill, caster);
        }
        public override float GetStatusEffectVar(StatusEffect statusEffect, int i, Unit unit) {
            UnitManager unitManager = MonoBehaviour.FindObjectOfType<UnitManager>();
            TileManager tileManager = MonoBehaviour.FindObjectOfType<TileManager>();
            Vector2 unitPosition = unit.GetPosition();
            List<Tile> nearbyTiles = tileManager.GetTilesInRange(RangeForm.Diamond, unitPosition, 1, 3, Direction.LeftUp);

            List<Unit> nearbyUnits = new List<Unit>();
            foreach (var tile in nearbyTiles) {
                if (tile.IsUnitOnTile())
                    nearbyUnits.Add(tile.GetUnitOnTile());
            }
            return nearbyUnits.Count(x => x.GetSide() == Side.Enemy);
        }

	    /*public override void SetAmountToEachStatusEffect(List<StatusEffect> statusEffects, Unit caster, Unit target) 
	    {
		    UnitManager unitManager = MonoBehaviour.FindObjectOfType<UnitManager>();
		    TileManager tileManager = MonoBehaviour.FindObjectOfType<TileManager>();

		    Vector2 unitPosition = caster.GetPosition();
		    List<Tile> nearbyTiles = tileManager.GetTilesInRange(RangeForm.Diamond, unitPosition, 1, 3, Direction.LeftUp);

		    List<Unit> nearbyUnits = new List<Unit>();
		    foreach (var tile in nearbyTiles)
		    {
			    if (tile.IsUnitOnTile())
				    nearbyUnits.Add(tile.GetUnitOnTile());
		    }

		    int numberOfNearbyEnemies = nearbyUnits.Count(x => x.GetSide() == Side.Enemy);

		    statusEffects[0].SetRemainStack(numberOfNearbyEnemies);
    	    statusEffects[0].SetAmount(5);
        }*/
    }
}
