using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

namespace Battle.Skills
{
public class Reina_4_m_SkillLogic : BaseSkillLogic {
	public override DamageCalculator.AttackDamage ApplyAdditionalDamage(DamageCalculator.AttackDamage attackDamage, Skill appliedSkill, BattleData battleData, Unit casterUnit, Unit targetUnit, int targetCount)
	{
		// 공버프가 총 30% 면 이 값은 1.3이 된다
		float damageBonusToAttackBuff = casterUnit.GetActualStat(Stat.Power) / casterUnit.GetStat(Stat.Power);
		
		if (damageBonusToAttackBuff < 1.0f)
			damageBonusToAttackBuff = 1.0f;

		attackDamage.relativeDamageBonus *= damageBonusToAttackBuff;

		return attackDamage;
	}
}
}