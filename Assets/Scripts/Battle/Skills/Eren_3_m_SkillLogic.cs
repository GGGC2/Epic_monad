using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Eren_3_m_SkillLogic : BasePassiveSkillLogic {

	public override DamageCalculator.AttackDamage ApplyBonusDamageFromEachPassive(DamageCalculator.AttackDamage attackDamage, Unit caster, Skill appliedSkill, Unit target, int targetCount)
	{
		float damageBonus = 1.2f;
		
		if (target.GetMaxHealth() < caster.GetMaxHealth())
			attackDamage.ratioDamageBonus *= damageBonus;
		
		return attackDamage;
	}
}
}
