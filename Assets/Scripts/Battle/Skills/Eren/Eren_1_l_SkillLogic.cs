using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;
using System.Linq;

namespace Battle.Skills
{
public class Eren_1_l_SkillLogic : BaseSkillLogic {
	public override void ApplyAdditionalDamage(SkillInstanceData skillInstanceData) 
    {
		UnitStatusEffect uniqueStatusEffect = skillInstanceData.GetCaster().GetStatusEffectList().Find(se => se.GetDisplayName() == "흡수");
		
		if (uniqueStatusEffect != null)
		{
			int stack = uniqueStatusEffect.GetRemainStack();
			skillInstanceData.GetDamage().baseDamage *= (1.0f + (0.2f * stack));
		}
	}
}
}