using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Reina_2_m_SkillLogic : BasePassiveSkillLogic {

	public override DamageCalculator.AttackDamage ApplyBonusDamageFromEachPassive(DamageCalculator.AttackDamage attackDamage, Unit caster, Skill appliedSkill, Unit target, int targetCount)
	{
		float damageBounsForPlaneTypeUnit = 1.15f;
		
		if (target.GetElement() == Enums.Element.Plant)
			attackDamage.ratioDamageBonus *= damageBounsForPlaneTypeUnit;
		
		return attackDamage;
	}
}
}
