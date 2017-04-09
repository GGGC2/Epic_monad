using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

namespace Battle.Skills
{
public class Yeong_1_l_SkillLogic : BaseSkillLogic {
	public override void ApplyAdditionalDamage(SkillInstanceData skillInstanceData) 
    {
		float damageBonusToMeleeUnit = 1.2f;

		if (skillInstanceData.getTarget().GetUnitClass() == UnitClass.Melee)
			skillInstanceData.getDamage().relativeDamageBonus += damageBonusToMeleeUnit;
	}
}
}