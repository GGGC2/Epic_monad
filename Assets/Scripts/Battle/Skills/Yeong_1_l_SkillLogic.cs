using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

namespace Battle.Skills
{
public class Yeong_1_l_SkillLogic : BaseSkillLogic {
	public override float ApplyIndividualAdditionalDamage(float previousDamage, Skill appliedSkill, BattleData battleData, Unit targetUnit, int targetCount)
	{
		float damageBonusToMeleeUnit = 1.2f;
		if (targetUnit.GetUnitClass() == UnitClass.Melee)
			previousDamage *= damageBonusToMeleeUnit;
		return previousDamage;
	}
}
}