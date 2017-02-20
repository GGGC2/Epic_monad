using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Reina_2_m_SkillLogic : BasePassiveSkillLogic {

	public override DamageCalculator.AttackDamage ApplyBonusDamageFromEachPassive(DamageCalculator.AttackDamage attackDamage, Unit caster, Skill appliedSkill, Unit target, int targetCount)
	{
		float damageBonusForPlaneTypeUnit = 1.15f;
		
		if (target.GetElement() == Enums.Element.Plant)
			attackDamage.ratioDamageBonus *= damageBonusForPlaneTypeUnit;
		
		return attackDamage;
	}
}
}
