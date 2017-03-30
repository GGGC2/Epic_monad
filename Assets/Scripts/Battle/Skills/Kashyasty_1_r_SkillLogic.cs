using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Kashyasty_1_r_SkillLogic : BasePassiveSkillLogic {

	public override DamageCalculator.AttackDamage ApplyTacticalBonusFromEachPassive(DamageCalculator.AttackDamage attackDamage, Unit caster, Unit target)
	{
		//'보너스'에만 2배
		float additionalDireationBonus = 2;

		if (attackDamage.directionBonus >= 1)
			attackDamage.directionBonus = ((attackDamage.directionBonus -1)
											* additionalDireationBonus) +1;

		return attackDamage;
	}
}
}