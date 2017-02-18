using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Lenian_5_m_SkillLogic : BasePassiveSkillLogic {

	public override DamageCalculator.AttackDamage ApplyBonusDamageFromEachPassive(DamageCalculator.AttackDamage attackDamage, Unit caster, Skill appliedSkill, Unit target, int targetCount)
	{
		float damageBounsPerTarget = 0.05f;
		
		float totalDamageBonus = 1.0f + targetCount * damageBounsPerTarget;
		attackDamage.ratioDamageBonus *= totalDamageBonus;
		
		return attackDamage;
	}
}
}
