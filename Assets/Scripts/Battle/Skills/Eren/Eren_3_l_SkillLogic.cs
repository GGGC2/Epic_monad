using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;
using System.Linq;

namespace Battle.Skills
{
public class Eren_3_l_SkillLogic : BaseSkillLogic {
	public override void ApplyAdditionalDamage(SkillInstanceData skillInstanceData)
	{
		Unit target = skillInstanceData.GetTarget();
		float damagedRatio = 1.0f - ((float)target.GetCurrentHealth()/(float)target.GetMaxHealth());
		skillInstanceData.GetDamage().baseDamage *= (1.0f + damagedRatio);
	}

	public override void OnKill(HitInfo hitInfo)
	{
		string skillName = hitInfo.skill.GetName();
		if (hitInfo.caster.GetUsedSkillDict().ContainsKey(skillName))
			hitInfo.caster.GetUsedSkillDict().Remove(skillName);
	}

	public override int CalculateAP(int originAP, Unit caster)
	{
		UnitStatusEffect uniqueStatusEffect = caster.GetStatusEffectList().Find(se => se.GetDisplayName() == "흡수");
		int stack = 0;

		if (uniqueStatusEffect != null)
			stack = uniqueStatusEffect.GetRemainStack();
		
		int result = originAP - (stack * 10);
		if (result < 0) result = 0;

		return result;
	}
}
}