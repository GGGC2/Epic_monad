using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Reina_5_l_SkillLogic : BasePassiveSkillLogic {

	public override DamageCalculator.AttackDamage ApplyBonusDamageFromEachPassive(DamageCalculator.AttackDamage attackDamage, Unit target, int targetCount)
	{
		float damageBonus = 1.3f;

		if (targetCount >= 3)
			attackDamage.ratioDamageBonus *= damageBonus;

		return attackDamage;
	}
}
}
