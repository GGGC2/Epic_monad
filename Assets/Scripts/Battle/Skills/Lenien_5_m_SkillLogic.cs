using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Lenien_5_m_SkillLogic : BasePassiveSkillLogic {

	public override DamageCalculator.AttackDamage ApplyBonusDamageFromEachPassive(DamageCalculator.AttackDamage attackDamage, Unit caster, Skill appliedSkill, Unit target, int targetCount)
	{
		float damageBonusPerTarget = 0.05f;
		
		float totalDamageBonus = 1.0f + targetCount * damageBonusPerTarget;
		attackDamage.relativeDamageBonus *= totalDamageBonus;
		
		return attackDamage;
	}
}
}
