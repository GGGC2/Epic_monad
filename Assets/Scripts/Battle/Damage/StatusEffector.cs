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
			.Select(fixedElem => new StatusEffect(fixedElem, caster, appliedSkill, null))
			.ToList();
        bool ignoreStatusEffect = false;
        foreach(var statusEffect in statusEffects) {
            List<StatusEffect> targetStatusEffectList = target.GetStatusEffectList();
            foreach(StatusEffect targetStatusEffect in targetStatusEffectList) {
                if((targetStatusEffect.GetOriginSkill() != null && SkillLogicFactory.Get(targetStatusEffect.GetOriginSkill()).
                        TriggerStatusEffectWhenStatusEffectApplied(target, targetStatusEffect, statusEffect) == false)) {
                    ignoreStatusEffect = true; 
                    Debug.Log(statusEffect.GetDisplayName()+ " ignored by "+targetStatusEffect.GetOriginSkillName()+" of "+target.GetName());
                }
            }
            if (SkillLogicFactory.Get(appliedSkill).TriggerStatusEffectApplied(statusEffect, caster, target) == false) {
                ignoreStatusEffect = true;
                Debug.Log(statusEffect.GetDisplayName() + " ignored by "+statusEffect.GetOriginSkillName());
            }
        }
        if(ignoreStatusEffect == false) {
            AttachStatusEffect(caster, statusEffects, target);
        }
	}

	public static void AttachStatusEffect(Unit caster, PassiveSkill appliedSkill, Unit target)
	{
		List<StatusEffect.FixedElement> fixedStatusEffects = appliedSkill.GetStatusEffectList();
		List<StatusEffect> statusEffects = fixedStatusEffects
			.Select(fixedElem => new StatusEffect(fixedElem, caster, null, appliedSkill))
			.ToList();
		AttachStatusEffect(caster, statusEffects, target);
	}

	private static bool IsValidAtZero(StatusEffectType seType)
	{
		if (seType == StatusEffectType.Silence || 
			seType == StatusEffectType.Bind || 
			seType == StatusEffectType.Confused || 
			seType == StatusEffectType.Faint || 
			seType == StatusEffectType.Retire || 
			seType == StatusEffectType.Taunt || 
			seType == StatusEffectType.MeleeImmune || 
			seType == StatusEffectType.MagicImmune || 
			seType == StatusEffectType.AllImmune || 
			seType == StatusEffectType.Etc)
			return true;
		else
			return false; 
	}

	private static bool IsValid(StatusEffect se)
	{
		int elems = se.fixedElem.actuals.Count;
		/*for (int i = 0; i < elems; i++)
		{
			if (!IsValidAtZero(se.GetStatusEffectType(i)) && se.GetAmount(i) == 0)
				return false;
		}*/
		return true;
	}

	private static void AttachStatusEffect(Unit caster, List<StatusEffect> statusEffects, Unit target)
	{
		List<StatusEffect> validStatusEffects = new List<StatusEffect>();
		foreach (var statusEffect in statusEffects)
		{
			if (IsValid(statusEffect))
				validStatusEffects.Add(statusEffect);
		}

		foreach (var statusEffect in validStatusEffects)
		{
            List<PassiveSkill> targetPassiveSkills = target.GetLearnedPassiveSkillList();
            if(SkillLogicFactory.Get(targetPassiveSkills).TriggerStatusEffectApplied(statusEffect, caster, target) == false) {
                Debug.Log(statusEffect.GetDisplayName() + " ignored by passiveSkills of " + target.GetName());
                continue;
            }

			var alreadyAppliedSameEffect = target.GetStatusEffectList().Find(
				alreadyAppliedEffect => statusEffect.IsSameStatusEffect(alreadyAppliedEffect)
			);

			// 동일한 효과가 있고 스택 불가능 -> 최신것으로 대체
			if (alreadyAppliedSameEffect != null  && !statusEffect.GetIsStackable())
			{
				Debug.Log("Update SE : " + statusEffect.GetDisplayName() + " to " + target.GetName() + target.GetPosition());
				var statusEffectListOfTarget = target.GetStatusEffectList();
				statusEffectListOfTarget.Remove(alreadyAppliedSameEffect);
				statusEffectListOfTarget.Add(statusEffect);
			}
			// 동일한 효과가 있지만 스택 가능 -> 지속시간, 수치 초기화. 1스택 추가
			else if (alreadyAppliedSameEffect != null && statusEffect.GetIsStackable())
			{
				Debug.Log("Add same SE : " + statusEffect.GetDisplayName() + " to " + target.GetName() + target.GetPosition());
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
				Debug.Log("Apply new SE : " + statusEffect.GetDisplayName() + " to " + target.GetName() + target.GetPosition());
				target.GetStatusEffectList().Add(statusEffect);
			}
		}
	}
}
}
