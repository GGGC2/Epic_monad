using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

namespace Battle.Skills
{
public class Reina_4_m_SkillLogic : BaseSkillLogic {
	public override DamageCalculator.AttackDamage ApplyIndividualAdditionalDamage(DamageCalculator.AttackDamage attackDamage, Skill appliedSkill, BattleData battleData, Unit casterUnit, Unit targetUnit, int targetCount)
	{
		// 공버프가 총 30% 면 이 값은 1.3이 된다
		float damageBonusToAttackBuff = casterUnit.GetActualStat(Stat.Power) / casterUnit.GetStat(Stat.Power);
		
		float finalDamage = (attackDamage.baseDamage
							* damageBonusToAttackBuff
							+ attackDamage.smiteAmount) 
							* attackDamage.directionBonus
							* attackDamage.celestialBonus
							* attackDamage.chainBonus;

		attackDamage.resultDamage = finalDamage;

		return attackDamage;
	}
}
}