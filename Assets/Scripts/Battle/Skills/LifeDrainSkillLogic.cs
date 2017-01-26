using System;
using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;

namespace Battle.Skills
{
public class LifeDrainSkillLogic : BaseSkillLogic {
	public override void ActionInDamageRoutine(BattleData battleData, Skill appliedSkill, Unit unitInChain, Tile targetTile, List<GameObject> selectedTiles)
	{
		Unit target = targetTile.GetUnitOnTile().GetComponent<Unit>();
		int finalDamage = (int)DamageCalculator.CalculateTotalDamage(battleData, targetTile, selectedTiles, GetTilesInFirstRange(battleData))[target.gameObject];
		int targetCurrentHealth = target.GetCurrentHealth();
		float[] recoverFactor = new float[5] {0.3f, 0.35f, 0.4f, 0.45f, 0.5f};
		int recoverAmount = (int) ((float) finalDamage * recoverFactor[appliedSkill.GetLevel()-1]);
		if (targetCurrentHealth == 0) recoverAmount *= 2;
		var recoverCoroutine = unitInChain.RecoverHealth(recoverAmount);
		battleData.battleManager.StartCoroutine(recoverCoroutine);
	}

	private static List<GameObject> GetTilesInFirstRange(BattleData battleData, Direction? direction = null)
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