using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;

namespace Battle.Skills
{
public class BaseSkillLogic
{
    public Skill skill;
	public virtual int CalculateAP(int originAP, Unit caster)
	{
		return originAP;
	}

	public virtual void OnKill(HitInfo hitInfo)
	{		
	}

	public virtual void SetAmountToEachStatusEffect(List<StatusEffect> statusEffects, Unit caster, Unit target)
	{
	}

	public virtual float ApplyIgnoreDefenceRelativeValueBySkill(float defense, Unit caster, Unit target)
	{
		return defense;
	}

	public virtual float ApplyIgnoreDefenceAbsoluteValueBySkill(float defense, Unit caster, Unit target)
	{
		return defense;
	}

	public virtual float ApplyIgnoreResistanceRelativeValueBySkill(float resistance, Unit caster, Unit target)
	{
		return resistance;
	}

	public virtual float ApplyIgnoreResistanceAbsoluteValueBySkill(float resistance, Unit caster, Unit target)
	{
		return resistance;
	}

	public virtual int CalculateAP(BattleData battleData, List<Tile> selectedTiles)
	{
		int requireAP = battleData.selectedUnit.GetActualRequireSkillAP(battleData.SelectedSkill);
		return requireAP;
	}

	public virtual void ApplyAdditionalDamage(SkillInstanceData skillInstanceData)
	{
	}

	public virtual DamageCalculator.AttackDamage GetAdditionalSkillOption(SkillInstanceData skillInstanceData)
	{
		return skillInstanceData.GetDamage();
	}

	public virtual void ActionInDamageRoutine(BattleData battleData, Skill appliedSkill, Unit unitInChain, List<Tile> selectedTiles)
	{
	}

    public virtual bool TriggerStatusEffectApplied(StatusEffect statusEffect, Unit caster, Unit target) //StatusEffect가 적용될 때 발동. false를 반환할 경우 해당 StatusEffect가 적용되지 않음
    {
        return true;
    }
    public virtual bool TriggerStatusEffectRemoved(StatusEffect statusEffect, Unit target) //unit의 StatusEffect가 사라질 때 발동. false를 리턴할 시 해당 StatusEffect가 사라지지 않음.
    {
        return true;
    }

    public virtual void TriggerStatusEffectsAtPhaseStart(Unit target, StatusEffect statusEffect) {

    }
    public virtual IEnumerator TriggerStatusEffectsAtActionEnd(Unit target, StatusEffect statusEffect) {
        return null;
    }

    public static List<Tile> GetTilesInFirstRange(BattleData battleData, Direction? direction = null)
	{
		Direction realDirection;
		if (direction.HasValue) {
			realDirection = direction.Value;
		} else {
			realDirection = battleData.selectedUnit.GetDirection();
		}
		var firstRange = battleData.tileManager.GetTilesInRange(battleData.SelectedSkill.GetFirstRangeForm(),
															battleData.selectedUnit.GetPosition(),
															battleData.SelectedSkill.GetFirstMinReach(),
															battleData.SelectedSkill.GetFirstMaxReach(),
															realDirection);

		return firstRange;
	}
}
}