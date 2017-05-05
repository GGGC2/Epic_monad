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
			.Select(fixedElem => new StatusEffect(fixedElem, caster))
			.ToList();

		// SkillLogicFactory.Get(appliedSkill).SetAmountToEachStatusEffect(statusEffects, caster);

		AttachStatusEffect(caster, statusEffects, target);
	}

	public static void AttachStatusEffect(Unit caster, PassiveSkill appliedSkill, Unit target)
	{
		List<StatusEffect.FixedElement> fixedStatusEffects = appliedSkill.GetStatusEffectList();
		List<StatusEffect> statusEffects = fixedStatusEffects
			.Select(fixedElem => new StatusEffect(fixedElem, caster))
			.ToList();

		// SkillLogicFactory.Get(appliedSkill).SetAmountToEachStatusEffect(statusEffects, caster, target);

		AttachStatusEffect(caster, statusEffects, target);
	}

	private static void AttachStatusEffect(Unit caster, List<StatusEffect> statusEffects, Unit target)
	{
		foreach (var statusEffect in statusEffects)
		{
			var alreadyAppliedSameEffect = target.GetStatusEffectList().Find(
				alreadyAppliedEffect => statusEffect.IsSameStatusEffect(alreadyAppliedEffect)
			);

			// 동일한 효과가 있고 스택 불가능 -> 최신것으로 대체
			if (alreadyAppliedSameEffect != null  && !statusEffect.GetIsStackable())
			{
				var statusEffectListOfTarget = target.GetStatusEffectList();
				statusEffectListOfTarget.Remove(alreadyAppliedSameEffect);
				statusEffectListOfTarget.Add(statusEffect);
			}
			// 동일한 효과가 있지만 스택 가능 -> 지속시간, 수치 초기화. 1스택 추가
			else if (alreadyAppliedSameEffect != null && statusEffect.GetIsStackable())
			{
				int num = alreadyAppliedSameEffect.fixedElem.actuals.Count;
				for (int i = 0; i < num; i++)
				{
					alreadyAppliedSameEffect.SetAmount(i, statusEffect.GetAmount(i));
					alreadyAppliedSameEffect.SetRemainAmount(i, statusEffect.GetAmount(i));
					alreadyAppliedSameEffect.SetRemainPhase(statusEffect.GetRemainPhase()); 
				}
				alreadyAppliedSameEffect.AddRemainStack(1);
			}
			// 동일한 효과가 없음 -> 새로 넣음
			else
			{
				target.GetStatusEffectList().Add(statusEffect);
			}

			Debug.Log("Apply " + statusEffect.GetDisplayName() + " effect to " + target.name);
		}
	}
}
}
