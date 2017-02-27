using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;

namespace Battle.Skills
{
public class BaseSkillLogic
{
	public virtual void SetAmountToEachStatusEffect(List<StatusEffect> statusEffects)
	{
	}

	public virtual int CalculateAP(BattleData battleData, List<GameObject> selectedTiles)
	{
		int requireAP = battleData.selectedUnitObject.GetComponent<Unit>().GetActualRequireSkillAP(battleData.SelectedSkill);
		return requireAP;
	}

	public virtual DamageCalculator.AttackDamage ApplyAdditionalDamage(DamageCalculator.AttackDamage attackDamage, Skill appliedSkill, BattleData battleData, Unit casterUnit, Unit targetUnit, int targetCount)
	{
		return attackDamage;
	}

	public virtual DamageCalculator.AttackDamage GetAdditionalSkillOption(DamageCalculator.AttackDamage attackDamage, Unit casterUnit, int targetCount)
	{
		return attackDamage;
	}

	public virtual void ActionInDamageRoutine(BattleData battleData, Skill appliedSkill, Unit unitInChain, Tile targetTile, List<GameObject> selectedTiles)
	{
	}

	public static List<GameObject> GetTilesInFirstRange(BattleData battleData, Direction? direction = null)
	{
		Direction realDirection;
		if (direction.HasValue) {
			realDirection = direction.Value;
		} else {
			realDirection = battleData.SelectedUnit.GetDirection();
		}
		var firstRange = battleData.tileManager.GetTilesInRange(battleData.SelectedSkill.GetFirstRangeForm(),
															battleData.SelectedUnit.GetPosition(),
															battleData.SelectedSkill.GetFirstMinReach(),
															battleData.SelectedSkill.GetFirstMaxReach(),
															realDirection);

		return firstRange;
	}
}
}