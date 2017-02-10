using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

namespace Battle.Skills
{
public class Yeong_1_l_SkillLogic : BaseSkillLogic {
	public override DamageCalculator.AttackDamage ApplyIndividualAdditionalDamage(DamageCalculator.AttackDamage attackDamage, Skill appliedSkill, BattleData battleData, Unit casterUnit, Unit targetUnit, int targetCount)
	{
		float damageBonusToMeleeUnit = 0.2f;

		float additionalDamage = attackDamage.baseDamage 
								* attackDamage.directionBonus
								* attackDamage.celestialBonus
								* attackDamage.chainBonus
								* damageBonusToMeleeUnit;
		if (targetUnit.GetUnitClass() == UnitClass.Melee)
			attackDamage.resultDamage += additionalDamage;
		return attackDamage;
	}
}
}