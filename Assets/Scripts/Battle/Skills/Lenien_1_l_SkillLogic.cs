using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Lenien_1_l_SkillLogic : BasePassiveSkillLogic {

	public override DamageCalculator.AttackDamage ApplyTacticalBonusFromEachPassive(DamageCalculator.AttackDamage attackDamage, Unit caster, Unit target)
	{
		int casterHeight = caster.GetHeight();
		int targetHeight = target.GetHeight();

		// 피해량이 높이차 * 15% 만큼 상승
		if (casterHeight > targetHeight)
		{
			float additionalHeightBonus = (casterHeight - targetHeight) * 0.15f;

			attackDamage.heightBonus += additionalHeightBonus;
		}

		return attackDamage;
	}
}
}