using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Kashyasty_2_r_SkillLogic : BasePassiveSkillLogic {

	public override void ApplyTacticalBonusFromEachPassive(SkillInstanceData skillInstanceData)
	{
		DamageCalculator.AttackDamage attackDamage = skillInstanceData.GetDamage();
		//'보너스'에만 2배
		float additionalCelestialBonus = 2;

		if (attackDamage.celestialBonus >= 1)
			attackDamage.celestialBonus = ((attackDamage.directionBonus -1)
											* additionalCelestialBonus) +1;
	}
}
}