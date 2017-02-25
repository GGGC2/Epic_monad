using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Enums;
using Battle.Skills;

namespace Battle.Damage
{
public static class StatusEffector
{
	public static void AttachStatusEffect(Unit caster, Skill appliedSkill, Unit target)
	{
		List<StatusEffect.FixedElement> fixedStatusEffects = appliedSkill.GetStatusEffectList();
		List<StatusEffect> statusEffects = fixedStatusEffects
			.Select(fixedElem => new StatusEffect(fixedElem, caster.gameObject))
			.ToList();

		SkillLogicFactory.Get(appliedSkill).SetAmountToEachStatusEffect(statusEffects);

		AttachStatusEffect(caster, statusEffects, target);
	}

	public static void AttachStatusEffect(Unit caster, PassiveSkill appliedSkill, Unit target)
	{
		List<StatusEffect.FixedElement> fixedStatusEffects = appliedSkill.GetStatusEffectList();
		List<StatusEffect> statusEffects = fixedStatusEffects
			.Select(fixedElem => new StatusEffect(fixedElem, caster.gameObject))
			.ToList();

		// SkillLogicFactory.Get(appliedSkill).SetAmountToEachStatusEffect(statusEffects);

		AttachStatusEffect(caster, statusEffects, target);
	}

	private static void AttachStatusEffect(Unit caster, List<StatusEffect> statusEffects, Unit target)
	{
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
						(target.GetActualStat(statusEffect.GetAmountStat())*statusEffect.GetAmount()));
			}
			else
			{
				target.GetStatusEffectList().Add(statusEffect);
				if (statusEffect.IsOfType(StatusEffectType.Shield))
				{
					target.GetStatusEffectList()[target.GetStatusEffectList().Count].SetRemainAmount
					((target.GetActualStat(statusEffect.GetAmountStat())*statusEffect.GetAmount()));
				}
			}

			Debug.Log("Apply " + statusEffect.GetDisplayName() + " effect to " + target.name);
		}
	}
}
}
