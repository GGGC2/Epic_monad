using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace Battle.Skills
{
public class Eren_6_m_SkillLogic : BasePassiveSkillLogic {

	public override void ApplyBonusDamageFromEachPassive(SkillInstanceData skillInstanceData) 
    {
		float damageBonusPerEachEnemyUnit = 0.05f;
		Unit target = skillInstanceData.GetMainTarget();
		TileManager tileManager = MonoBehaviour.FindObjectOfType<TileManager>();
		List<Tile> nearByTilesFromTarget = tileManager.GetTilesInRange(Enums.RangeForm.Diamond, target.GetPosition(), 1, 2, target.GetDirection());

		int numberOfEnemyUnitsInRange = nearByTilesFromTarget.Count(x => x.GetUnitOnTile().GetSide() == Enums.Side.Enemy);
		float totalDamageBonus = damageBonusPerEachEnemyUnit * numberOfEnemyUnitsInRange + 1.0f;

		skillInstanceData.GetDamage().relativeDamageBonus *= totalDamageBonus;
	}
}
}
