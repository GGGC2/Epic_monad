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
	public static void AttachStatusEffect(Unit caster, ActiveSkill appliedSkill, Unit target, List<Tile> targetTiles)
	{
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
            if(ignoreStatusEffect == false)
                newStatusEffects.Add(statusEffect);
        }
        AttachStatusEffect(caster, newStatusEffects, target);
	}

	public static void AttachStatusEffect(Unit caster, PassiveSkill appliedSkill, Unit target)
	{
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
            if (ignoreStatusEffect == false)
                newStatusEffects.Add(statusEffect);
        }
        AttachStatusEffect(caster, newStatusEffects, target);
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

	public static void AttachStatusEffect(Unit caster, List<UnitStatusEffect> statusEffects, Unit target)
	{
		List<UnitStatusEffect> validStatusEffects = new List<UnitStatusEffect>();
		foreach (var statusEffect in statusEffects)
		{
			if (IsValid(statusEffect))
				validStatusEffects.Add(statusEffect);
		}

		foreach (var statusEffect in validStatusEffects)
		{
            List<PassiveSkill> targetPassiveSkills = target.GetLearnedPassiveSkillList();
            if(SkillLogicFactory.Get(targetPassiveSkills).TriggerStatusEffectAppliedToOwner(statusEffect, caster, target) == false) {
                Debug.Log(statusEffect.GetDisplayName() + " ignored by passiveSkills of " + target.GetName());
                continue;
            }
            List<UnitStatusEffect> targetStatusEffectList = target.GetStatusEffectList();
            foreach (var targetStatusEffect in targetStatusEffectList) {
                Skill originSkill = targetStatusEffect.GetOriginSkill();
                if (originSkill.GetType() == typeof(ActiveSkill)) {
                    if(((ActiveSkill)originSkill).SkillLogic.TriggerStatusEffectWhenStatusEffectApplied(target, targetStatusEffect, statusEffect) == false) {
                        Debug.Log(statusEffect.GetDisplayName() + " ignored by " + targetStatusEffect.GetOriginSkillName() + " of " + target.GetName());
                        continue;
                    }
                }
            }

            var alreadyAppliedSameEffect = target.GetStatusEffectList().Find(
				alreadyAppliedEffect => statusEffect.IsSameStatusEffect(alreadyAppliedEffect)
			);

			// 동일한 효과가 있고 스택 불가능 -> 최신것으로 대체
			if (alreadyAppliedSameEffect != null  && !statusEffect.GetIsStackable())
			{
				Debug.Log("Update SE : " + statusEffect.GetDisplayName() + " to " + target.GetName() + target.GetPosition());
                List<UnitStatusEffect> newStatusEffectList = target.GetStatusEffectList().FindAll(se => se != alreadyAppliedSameEffect);
                newStatusEffectList.Add(statusEffect);
                target.SetStatusEffectList(newStatusEffectList);
                target.updateStats(alreadyAppliedSameEffect, false, true);
                target.updateStats(statusEffect, true, false);
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
			else{
				Debug.Log("Apply new SE : " + statusEffect.GetDisplayName() + " to " + target.GetName() + target.GetPosition());
                List<UnitStatusEffect> newStatusEffectList = target.GetStatusEffectList().FindAll(se => true);
                newStatusEffectList.Add(statusEffect);
                target.SetStatusEffectList(newStatusEffectList);
                target.updateStats(statusEffect, true, false);
                target.UpdateSpriteByStealth();
            }
		}
	}


    public static void AttachStatusEffect(Unit caster, ActiveSkill appliedSkill, Tile targetTile) {
        List<TileStatusEffect.FixedElement> fixedStatusEffects = appliedSkill.GetTileStatusEffectList();
        List<TileStatusEffect> statusEffects = fixedStatusEffects
            .Select(fixedElem => new TileStatusEffect(fixedElem, caster, targetTile, appliedSkill))
            .ToList();
            List<TileStatusEffect> newStatusEffects = new List<TileStatusEffect>();
            foreach (var statusEffect in statusEffects) {
                bool ignoreStatusEffect = false;
                if (SkillLogicFactory.Get(appliedSkill).TriggerTileStatusEffectApplied(statusEffect, caster, targetTile) == false) {
                    ignoreStatusEffect = true;
                    Debug.Log(statusEffect.GetDisplayName() + " ignored by " + statusEffect.GetOriginSkillName());
                }
                if (ignoreStatusEffect == false)
                    newStatusEffects.Add(statusEffect);
            }
            AttachStatusEffect(caster, newStatusEffects, targetTile);
        }
    public static void AttachStatusEffect(Unit caster, List<TileStatusEffect> statusEffects, Tile targetTile) {
        Vector2 tilePos = targetTile.GetTilePos();
        foreach (var statusEffect in statusEffects) {
            var alreadyAppliedSameEffect = targetTile.GetStatusEffectList().Find(
                alreadyAppliedEffect => statusEffect.IsSameStatusEffect(alreadyAppliedEffect)
            );

            // 동일한 효과가 있고 스택 불가능 -> 최신것으로 대체
            if (alreadyAppliedSameEffect != null && !statusEffect.GetIsStackable()) {
                Debug.Log("Update SE : " + statusEffect.GetDisplayName() + " to tile (" + tilePos.x + ", " + tilePos.y + ")");
                List<TileStatusEffect> newStatusEffectList = targetTile.GetStatusEffectList().FindAll(se => se != alreadyAppliedSameEffect);
                newStatusEffectList.Add(statusEffect);
                targetTile.SetStatusEffectList(newStatusEffectList);
            }
            // 동일한 효과가 있지만 스택 가능 -> 지속시간, 수치 초기화. 1스택 추가
            else if (alreadyAppliedSameEffect != null && statusEffect.GetIsStackable()) {
                Debug.Log("Add same SE : " + statusEffect.GetDisplayName() + " to tile (" + tilePos.x + ", " + tilePos.y + ")");
                int num = alreadyAppliedSameEffect.fixedElem.actuals.Count;
                for (int i = 0; i < num; i++) {
                    alreadyAppliedSameEffect.SetAmount(i, statusEffect.GetAmount(i));
                    alreadyAppliedSameEffect.SetRemainAmount(i, statusEffect.GetAmount(i));
                    alreadyAppliedSameEffect.SetRemainPhase(statusEffect.GetRemainPhase());
                }
                alreadyAppliedSameEffect.AddRemainStack(1);
            }
            // 동일한 효과가 없음 -> 새로 넣음
            else {
                Debug.Log("Apply new SE : " + statusEffect.GetDisplayName() + " to tile (" + tilePos.x + ", " + tilePos.y + ")");
                List<TileStatusEffect> newStatusEffectList = targetTile.GetStatusEffectList().FindAll(se => true);
                newStatusEffectList.Add(statusEffect);
                targetTile.SetStatusEffectList(newStatusEffectList);
            }
            if(targetTile.IsUnitOnTile())   targetTile.GetUnitOnTile().updateStats();
        }
    }
}
}
