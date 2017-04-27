using UnityEngine;
using Battle.Damage;
using System.Collections.Generic;
using Enums;
using System.Linq;

namespace Battle.Skills
{
public class Yeong_2_r_SkillLogic : BasePassiveSkillLogic
{
	public override void TriggerActiveSkillDamageApplied(Unit yeong)
	{
		UnitManager unitManager = MonoBehaviour.FindObjectOfType<UnitManager>();
		TileManager tileManager = MonoBehaviour.FindObjectOfType<TileManager>();

		Vector2 unitPosition = yeong.GetPosition();
		List<Tile> nearbyTiles = tileManager.GetTilesInRange(RangeForm.Diamond, unitPosition, 1, 3, Direction.LeftUp);

		int numberOfNearbyEnemies = nearbyTiles.Count(x => x.GetUnitOnTile().GetSide() == Side.Enemy);
		List<StatusEffect> statusEffectList = yeong.GetStatusEffectList();

		if (numberOfNearbyEnemies > 0)
		{
			statusEffectList = statusEffectList.FindAll(x => x.GetOriginSkillName() != "신속 반응");
            yeong.SetStatusEffectList(statusEffectList);
		}
		else
		{
			StatusEffector.AttachStatusEffect(yeong, this.passiveSkill, yeong);
		}
	}

	public override void SetAmountToEachStatusEffect(List<StatusEffect> statusEffects, Unit caster, Unit target) 
	{
		UnitManager unitManager = MonoBehaviour.FindObjectOfType<UnitManager>();
		TileManager tileManager = MonoBehaviour.FindObjectOfType<TileManager>();

		Vector2 unitPosition = caster.GetPosition();
		List<Tile> nearbyTiles = tileManager.GetTilesInRange(RangeForm.Diamond, unitPosition, 1, 3, Direction.LeftUp);

		int numberOfNearbyEnemies = nearbyTiles.Count(x => x.GetUnitOnTile().GetSide() == Side.Enemy);
		int amount = 5 * numberOfNearbyEnemies;

    	statusEffects[0].SetAmount(amount);
    }
}
}
