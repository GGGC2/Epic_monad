using UnityEngine;
using Battle.Damage;
using System.Collections.Generic;
using Enums;
using System.Linq;

namespace Battle.Skills
{
public class Yeong_2_r_SkillLogic : BasePassiveSkillLogic
{
	public override void TriggerActionEnd(Unit yeong)
	{
		UnitManager unitManager = MonoBehaviour.FindObjectOfType<UnitManager>();
		TileManager tileManager = MonoBehaviour.FindObjectOfType<TileManager>();

		Vector2 unitPosition = yeong.GetPosition();
		List<Tile> nearbyTiles = tileManager.GetTilesInRange(RangeForm.Diamond, unitPosition, 1, 3, Direction.LeftUp);

		List<Unit> nearbyUnits = new List<Unit>();
		foreach (var tile in nearbyTiles)
		{
			if (tile.IsUnitOnTile())
				nearbyUnits.Add(tile.GetUnitOnTile());
		}

		int numberOfNearbyEnemies = nearbyUnits.Count(x => x.GetSide() == Side.Enemy);

		Debug.Log("numberOfNearbyEnemies : " + numberOfNearbyEnemies);

		if (numberOfNearbyEnemies == 0)
		{
			List<StatusEffect> statusEffectList = yeong.GetStatusEffectList();
			statusEffectList = statusEffectList.FindAll(x => x.GetOriginSkillName() != "기척 감지");
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

		List<Unit> nearbyUnits = new List<Unit>();
		foreach (var tile in nearbyTiles)
		{
			if (tile.IsUnitOnTile())
				nearbyUnits.Add(tile.GetUnitOnTile());
		}

		int numberOfNearbyEnemies = nearbyUnits.Count(x => x.GetSide() == Side.Enemy);

		statusEffects[0].SetRemainStack(numberOfNearbyEnemies);
    	statusEffects[0].SetAmount(5);
    }
}
}
