using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;

namespace Battle.Skills
{
public class BaseSkillLogic
{
	public virtual int CalculateAP(int originAP, Unit caster)
	{
		return originAP;
	}

	public virtual void OnKill(HitInfo hitInfo)
	{		
	}

	public virtual void SetAmountToEachStatusEffect(List<StatusEffect> statusEffects, Unit caster)
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
		return skillInstanceData.getDamage();
	}

	public virtual void ActionInDamageRoutine(BattleData battleData, Skill appliedSkill, Unit unitInChain, Tile targetTile, List<Tile> selectedTiles)
	{
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