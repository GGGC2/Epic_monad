using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace Battle.Skills
{
public class Eren_6_m_SkillLogic : BasePassiveSkillLogic {

	public override DamageCalculator.AttackDamage ApplyBonusDamageFromEachPassive(DamageCalculator.AttackDamage attackDamage, Unit caster, Skill appliedSkill, Unit target, int targetCount)
	{
		float damageBonusPerEachEnemyUnit = 0.05f;
		
		TileManager tileManager = MonoBehaviour.FindObjectOfType<TileManager>();
		List<GameObject> nearByTilesFromTarget = tileManager.GetTilesInRange(Enums.RangeForm.Diamond, target.GetPosition(), 1, 2, target.GetDirection());

		int numberOfEnemyUnitsInRange = nearByTilesFromTarget.Count(x => x.GetComponent<Tile>().GetUnitOnTile().GetComponent<Unit>().GetSide() == Enums.Side.Enemy);
		float totalDamageBonus = damageBonusPerEachEnemyUnit * numberOfEnemyUnitsInRange + 1.0f;

		attackDamage.relativeDamageBonus *= totalDamageBonus;
		
		return attackDamage;
	}
}
}
