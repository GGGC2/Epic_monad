using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Enums;

namespace Battle.Damage
{
public static class StatusEffector
{
	public static void AttachStatusEffect(Skill appliedSkill, Unit target)
	{
		foreach (var statusEffect in appliedSkill.GetStatusEffectList())
		{

			var alreadyAppliedSameEffect = target.GetStatusEffectList().Find(
				alreadyAppliedEffect => statusEffect.IsSameStatusEffect(alreadyAppliedEffect)
			);

			if (alreadyAppliedSameEffect != null  && !statusEffect.GetIsStackable())
			{
				alreadyAppliedSameEffect.SetRemainPhase(statusEffect.GetRemainPhase());
				alreadyAppliedSameEffect.SetRemainStack(statusEffect.GetRemainStack());
				if (statusEffect.IsOfType(StatusEffectType.Shield))
					alreadyAppliedSameEffect.SetRemainAmount((int)(target.GetActualStat(statusEffect.GetAmountStat())*statusEffect.GetAmount(statusEffect.GetLevel())));
			}
			else
			{
				target.GetStatusEffectList().Add(statusEffect);
				if (statusEffect.IsOfType(StatusEffectType.Shield))
				{
					target.GetStatusEffectList()[target.GetStatusEffectList().Count].SetRemainAmount
					((int)(target.GetActualStat(statusEffect.GetAmountStat())*statusEffect.GetAmount(statusEffect.GetLevel())));
				}
			}

			Debug.Log("Apply " + statusEffect.GetName() + " effect to " + target.name);
		}
	}
}
}
