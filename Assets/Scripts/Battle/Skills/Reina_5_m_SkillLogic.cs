using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Reina_5_m_SkillLogic : BasePassiveSkillLogic {

	public override DamageCalculator.AttackDamage ApplyBonusDamageFromEachPassive(DamageCalculator.AttackDamage attackDamage, Skill appliedSkill, Unit target, int targetCount)
	{
		float damageBonus = 1.25f;

		if ((appliedSkill.GetName() == "화염구") && (targetCount == 1))
			attackDamage.ratioDamageBonus *= damageBonus;

		return attackDamage;
	}
}
}
