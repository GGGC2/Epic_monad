using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Lenien_1_l_SkillLogic : BasePassiveSkillLogic {

	public override void ApplyTacticalBonusFromEachPassive(SkillInstanceData skillInstanceData)
	{
		int casterHeight = skillInstanceData.GetCaster().GetHeight();
		int targetHeight = skillInstanceData.getTarget().GetHeight();
        DamageCalculator.AttackDamage attackDamage = skillInstanceData.getDamage();
		// 피해량이 높이차 * 15% 만큼 상승
		if (casterHeight > targetHeight)
		{
			float additionalHeightBonus = (casterHeight - targetHeight) * 0.15f;

			attackDamage.heightBonus += additionalHeightBonus;
		}
	}
}
}