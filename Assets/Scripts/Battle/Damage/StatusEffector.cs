using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
			}
			else
			{
				target.GetStatusEffectList().Add(statusEffect);
			}

			Debug.Log("Apply " + statusEffect.GetName() + " effect to " + target.name);
		}
	}
}
}
