using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Enums;

namespace Battle.Damage
{
public static class StatusEffector
{
	public static void AttachStatusEffect(Unit caster, Skill appliedSkill, Unit target)
	{
		AttachStatusEffect(caster, appliedSkill.GetStatusEffectList(), target);
	}

	public static void AttachStatusEffect(Unit caster, PassiveSkill appliedSkill, Unit target)
	{
		AttachStatusEffect(caster, appliedSkill.GetStatusEffectList(), target);
	}

	private static void AttachStatusEffect(Unit caster, List<StatusEffect.FixedElement> fixedStatusEffects, Unit target)
	{
		List<StatusEffect> statusEffects = fixedStatusEffects
			.Select(fixedElem => new StatusEffect(fixedElem, caster.gameObject))
			.ToList();

		foreach (var statusEffect in statusEffects)
		{
			var alreadyAppliedSameEffect = target.GetStatusEffectList().Find(
				alreadyAppliedEffect => statusEffect.IsSameStatusEffect(alreadyAppliedEffect)
			);

			if (alreadyAppliedSameEffect != null  && !statusEffect.GetIsStackable())
			{
				alreadyAppliedSameEffect.SetRemainPhase(statusEffect.GetRemainPhase());
				alreadyAppliedSameEffect.SetRemainStack(statusEffect.GetRemainStack());
				if (statusEffect.IsOfType(StatusEffectType.Shield))
					alreadyAppliedSameEffect.SetRemainAmount(
						(int)(target.GetActualStat(statusEffect.GetAmountStat())*statusEffect.GetAmount()));
			}
			else
			{
				target.GetStatusEffectList().Add(statusEffect);
				if (statusEffect.IsOfType(StatusEffectType.Shield))
				{
					target.GetStatusEffectList()[target.GetStatusEffectList().Count].SetRemainAmount
					((int)(target.GetActualStat(statusEffect.GetAmountStat())*statusEffect.GetAmount()));
				}
			}

			Debug.Log("Apply " + statusEffect.GetOriginSkillName() + " effect to " + target.name);
		}
	}
}
}
