﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Enums;

namespace Battle.Skills
{
public class Yeong_2_l_SkillLogic : BaseSkillLogic {
	public override void ApplyAdditionalDamage(CastingApply castingApply) 
    {
		int totalEvasion = 0;

		List<UnitStatusEffect> statusEffects = castingApply.GetCaster().StatusEffectList;
		foreach (var statusEffect in statusEffects)
		{
			int num = statusEffect.fixedElem.actuals.Count;
			for (int i = 0; i < num; i++)
			{
				if (statusEffect.IsOfType(i, StatusEffectType.EvasionChange))
				{
					totalEvasion += (int)statusEffect.GetAmount(i);
				}
			}
		}

		float damageBonus = (float)(100 + totalEvasion) / 100;
		castingApply.GetDamage().relativeDamageBonus *= damageBonus;

		Debug.Log("Evasion : " + totalEvasion + '\n' + "damage bonus : " + damageBonus);
	}
}
}