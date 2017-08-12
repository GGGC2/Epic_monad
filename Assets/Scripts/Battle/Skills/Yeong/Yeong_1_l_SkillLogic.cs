using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Enums;

namespace Battle.Skills
{
public class Yeong_1_l_SkillLogic : BaseSkillLogic {
	public override void ApplyAdditionalDamage(CastingApply castingApply) 
    {
		float damageBonus = 1.2f;

		List<UnitStatusEffect> statusEffects = castingApply.GetCaster().GetStatusEffectList();
		bool isUniquePassiveActive = statusEffects.Any(x => x.GetOriginSkillName() == "방랑자");
		statusEffects.ForEach(x => Debug.Log("origin : " + x.GetOriginSkillName()));
		Debug.LogWarning("isUniquePassiveActive : " + isUniquePassiveActive);
		if (isUniquePassiveActive)
			castingApply.GetDamage().relativeDamageBonus *= damageBonus;

		Debug.Log("Total relative bonus : " + castingApply.GetDamage().relativeDamageBonus);
	}
}
}