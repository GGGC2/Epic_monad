using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;

namespace Battle.Skills
{
public class BaseSkillLogic
{
	public virtual int CalculateAP(BattleData battleData, List<GameObject> selectedTiles)
	{
		int requireAP = battleData.selectedUnitObject.GetComponent<Unit>().GetActualRequireSkillAP(battleData.SelectedSkill);
		return requireAP;
	}

	public virtual float ApplyIndividualAdditionalDamage(float previousDamage, Skill appliedSkill, BattleData battleData, Unit targetUnit, int targetCount)
	{
		return previousDamage;
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