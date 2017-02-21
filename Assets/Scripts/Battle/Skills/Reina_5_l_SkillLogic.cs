using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Reina_5_l_SkillLogic : BasePassiveSkillLogic {

	public override DamageCalculator.AttackDamage ApplyBonusDamageFromEachPassive(DamageCalculator.AttackDamage attackDamage, Unit caster, Skill appliedSkill, Unit target, int targetCount)
	{
		float damageBonus = 1.3f;

		if ((appliedSkill.GetName() == "화염 폭발") && (targetCount >= 3))
			attackDamage.relativeDamageBonus *= damageBonus;

		return attackDamage;
	}
}
}
