using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Lenien_1_l_SkillLogic : BasePassiveSkillLogic {

	public override void ApplyTacticalBonusFromEachPassive(CastingApply castingApply)
	{
		int casterHeight = castingApply.GetCaster().GetHeight();
		int targetHeight = castingApply.GetTarget().GetHeight();
        DamageCalculator.AttackDamage attackDamage = castingApply.GetDamage();
		// 피해량이 높이차 * 15% 만큼 상승
		if (casterHeight > targetHeight)
		{
			float additionalHeightBonus = (casterHeight - targetHeight) * 0.15f;

			attackDamage.heightBonus += additionalHeightBonus;
		}
	}
}
}