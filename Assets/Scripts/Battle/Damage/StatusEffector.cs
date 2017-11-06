using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Enums;
using Battle.Skills;

namespace Battle.Damage
{
public static class StatusEffector{
	public static void AttachStatusEffect(Unit caster, ActiveSkill appliedSkill, Unit target, List<Tile> targetTiles){
        List<UnitStatusEffect.FixedElement> fixedStatusEffects = appliedSkill.GetUnitStatusEffectList();
		List<UnitStatusEffect> statusEffects = fixedStatusEffects
			.Select(fixedElem => new UnitStatusEffect(fixedElem, caster, target, appliedSkill))
			.ToList();
        List<UnitStatusEffect> newStatusEffects = new List<UnitStatusEffect>();
        foreach(var statusEffect in statusEffects) {
            bool ignoreStatusEffect = false;
            if (SkillLogicFactory.Get(appliedSkill).TriggerStatusEffectApplied(statusEffect, caster, target, targetTiles) == false) {
                ignoreStatusEffect = true;
                Debug.Log(statusEffect.GetOriginSkillName() + "의 " + statusEffect.GetDisplayName() + " 효과는 적용되지 않음");
            }
            if(ignoreStatusEffect == false) {newStatusEffects.Add(statusEffect);}
        }
        AttachStatusEffect(caster, newStatusEffects, target);
	}

	public static List<UnitStatusEffect> AttachStatusEffect(Unit caster, PassiveSkill appliedSkill, Unit target){
        //실제로 적용된 statusEffect들을 리턴함
		List<UnitStatusEffect.FixedElement> fixedStatusEffects = appliedSkill.GetUnitStatusEffectList();
		List<UnitStatusEffect> statusEffects = fixedStatusEffects
			.Select(fixedElem => new UnitStatusEffect(fixedElem, caster, target, appliedSkill))
			.ToList();
        List<UnitStatusEffect> newStatusEffects = new List<UnitStatusEffect>();
        foreach (var statusEffect in statusEffects) {
            bool ignoreStatusEffect = false;
            if (SkillLogicFactory.Get(appliedSkill).TriggerStatusEffectApplied(statusEffect, caster, target) == false) {
                ignoreStatusEffect = true;
                Debug.Log(statusEffect.GetDisplayName() + " ignored by " + statusEffect.GetOriginSkillName());
            }
            if (ignoreStatusEffect == false) {newStatusEffects.Add(statusEffect);}
        }
        return AttachStatusEffect(caster, newStatusEffects, target);
	}

	private static bool IsValidAtZero(StatusEffectType seType){
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

	private static bool IsValid(UnitStatusEffect se)
	{
		int elems = se.fixedElem.actuals.Count;
		/*for (int i = 0; i < elems; i++)
		{
			if (!IsValidAtZero(se.GetStatusEffectType(i)) && se.GetAmount(i) == 0)
				return false;
		}*/
		return true;
	}

	public static List<UnitStatusEffect> AttachStatusEffect(Unit caster, List<UnitStatusEffect> statusEffects, Unit target){
        //실제로 적용된 statusEffect들을 리턴함
        LogManager logManager = LogManager.Instance;
		List<UnitStatusEffect> validStatusEffects = new List<UnitStatusEffect>();
        List<UnitStatusEffect> actuallyAppliedStatusEffects = new List<UnitStatusEffect>();
		foreach (var statusEffect in statusEffects)
		{
			if (IsValid(statusEffect))
				validStatusEffects.Add(statusEffect);
		}

		foreach (var statusEffect in validStatusEffects){
            List<PassiveSkill> targetPassiveSkills = target.GetLearnedPassiveSkillList();
            if(SkillLogicFactory.Get(targetPassiveSkills).TriggerStatusEffectAppliedToOwner(statusEffect, caster, target) == false) {
                Debug.Log(statusEffect.GetDisplayName() + " ignored by passiveSkills of " + target.GetNameKor());
                continue;
            }
            List<UnitStatusEffect> targetStatusEffectList = target.StatusEffectList;
            foreach (var targetStatusEffect in targetStatusEffectList) {
                Skill originSkill = targetStatusEffect.GetOriginSkill();
                if (originSkill is ActiveSkill) {
                    if(((ActiveSkill)originSkill).SkillLogic.TriggerStatusEffectWhenStatusEffectApplied(target, targetStatusEffect, statusEffect) == false) {
                        Debug.Log(statusEffect.GetDisplayName() + " ignored by " + targetStatusEffect.GetOriginSkillName() + " of " + target.GetNameKor());
                        continue;
                    }
                }
            }

            var alreadyAppliedSameEffect = target.StatusEffectList.Find(
				alreadyAppliedEffect => statusEffect.IsSameStatusEffect(alreadyAppliedEffect)
			);

			// 동일한 효과가 있을 경우 -> 지속시간, 수치 초기화. 스택 가능할 경우 1스택 추가
			if (alreadyAppliedSameEffect != null) {
                actuallyAppliedStatusEffects.Add(alreadyAppliedSameEffect);
                int num = alreadyAppliedSameEffect.fixedElem.actuals.Count;
				for (int i = 0; i < num; i++) {
                    float amount = statusEffect.GetAmount(i);
					alreadyAppliedSameEffect.SetAmount(i, amount);
					alreadyAppliedSameEffect.SetRemainAmount(i, amount);
                }
                alreadyAppliedSameEffect.SetRemainPhase(statusEffect.GetRemainPhase());
                if(statusEffect.GetIsStackable())
                    alreadyAppliedSameEffect.AddRemainStack(1);
			}
			// 동일한 효과가 없음 -> 새로 넣음
			else{
                actuallyAppliedStatusEffects.Add(statusEffect);
                logManager.Record(new StatusEffectLog(statusEffect, StatusEffectChangeType.Attach, 0, 0, 0));
                //target.UpdateStats(statusEffect, true, false);
            }
		}

        // 특수 상태이상(기절, 속박, 침묵)이 갱신되었는지 체크
        target.UpdateStatusEffectIcon();

        return actuallyAppliedStatusEffects;
	}


    public static void AttachStatusEffect(Unit caster, ActiveSkill appliedSkill, Tile targetTile) {
        List<TileStatusEffect.FixedElement> fixedStatusEffects = appliedSkill.GetTileStatusEffectList();
        List<TileStatusEffect> statusEffects = fixedStatusEffects
            .Select(fixedElem => new TileStatusEffect(fixedElem, caster, targetTile, appliedSkill))
            .ToList();
        List<TileStatusEffect> newStatusEffects = new List<TileStatusEffect>();
        foreach (var statusEffect in statusEffects) {
            bool ignoreStatusEffect = false;
            if (SkillLogicFactory.Get(appliedSkill).TriggerTileStatusEffectApplied(statusEffect) == false) {
                ignoreStatusEffect = true;
                Debug.Log(statusEffect.GetDisplayName() + " ignored by " + statusEffect.GetOriginSkillName());
            }
            if (ignoreStatusEffect == false)
                newStatusEffects.Add(statusEffect);
        }
        AttachStatusEffect(caster, newStatusEffects, targetTile);
    }
    public static void AttachStatusEffect(Unit caster, List<TileStatusEffect> statusEffects, Tile targetTile) {
        LogManager logManager = LogManager.Instance;
        Vector2 tilePos = targetTile.GetTilePos();
        foreach (var statusEffect in statusEffects) {
            var alreadyAppliedSameEffect = targetTile.GetStatusEffectList().Find(
                alreadyAppliedEffect => statusEffect.IsSameStatusEffect(alreadyAppliedEffect)
            );

            // 동일한 효과가 있을 경우 -> 지속시간, 수치 초기화. 스택 가능할 경우 1스택 추가
            if (alreadyAppliedSameEffect != null) {
                int num = alreadyAppliedSameEffect.fixedElem.actuals.Count;
                for (int i = 0; i < num; i++) {
                    float amount = statusEffect.GetAmount(i);
                    alreadyAppliedSameEffect.SetAmount(i, amount);
                    alreadyAppliedSameEffect.SetRemainAmount(i, amount);
                }
                alreadyAppliedSameEffect.SetRemainPhase(statusEffect.GetRemainPhase());
                if (statusEffect.GetIsStackable())
                    alreadyAppliedSameEffect.AddRemainStack(1);
            }
            // 동일한 효과가 없음 -> 새로 넣음
            else logManager.Record(new StatusEffectLog(statusEffect, StatusEffectChangeType.Attach, 0, 0, 0));
        }
    }
}
}
