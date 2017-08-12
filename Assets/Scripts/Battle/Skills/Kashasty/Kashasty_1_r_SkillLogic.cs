using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Kashasty_1_r_SkillLogic : BasePassiveSkillLogic {

	public override void ApplyTacticalBonusFromEachPassive(CastingApply castingApply)
	{
		DamageCalculator.AttackDamage attackDamage = castingApply.GetDamage();
		//'보너스'에만 2배
		float additionalDireationBonus = 2;

		if (attackDamage.directionBonus >= 1)
			attackDamage.directionBonus = ((attackDamage.directionBonus -1)
											* additionalDireationBonus) +1;

	}
}
}