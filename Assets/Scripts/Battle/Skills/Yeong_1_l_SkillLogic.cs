using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

namespace Battle.Skills
{
public class Yeong_1_l_SkillLogic : BaseSkillLogic {
	public override DamageCalculator.AttackDamage ApplyAdditionalDamage(DamageCalculator.AttackDamage attackDamage, Skill appliedSkill, BattleData battleData, Unit casterUnit, Unit targetUnit, int targetCount)
	{
		float damageBonusToMeleeUnit = 1.2f;

		if (targetUnit.GetUnitClass() == UnitClass.Melee)
			attackDamage.relativeDamageBonus += damageBonusToMeleeUnit;
		
		return attackDamage;
	}
}
}