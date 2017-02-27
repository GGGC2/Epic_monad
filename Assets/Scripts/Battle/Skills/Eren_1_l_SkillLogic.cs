using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;
using System.Linq;

namespace Battle.Skills
{
public class Eren_1_l_SkillLogic : BaseSkillLogic {
	public override DamageCalculator.AttackDamage ApplyAdditionalDamage(DamageCalculator.AttackDamage attackDamage, Skill appliedSkill, BattleData battleData, Unit casterUnit, Unit targetUnit, int targetCount)
	{
		StatusEffect uniqueStatusEffect = casterUnit.GetStatusEffectList().Find(se => se.GetDisplayName() == "흡수");
		
		if (uniqueStatusEffect != null)
		{
			int stack = uniqueStatusEffect.GetRemainStack();
			attackDamage.baseDamage *= (1.0f + (0.2f * stack));
		}

		return attackDamage;
	}
}
}