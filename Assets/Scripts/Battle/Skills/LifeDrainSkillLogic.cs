using System;
using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;

namespace Battle.Skills
{
public class LifeDrainSkillLogic : BaseSkillLogic {
	public override void ActionInDamageRoutine(BattleData battleData, Skill appliedSkill, Unit unitInChain, Tile targetTile, List<Tile> selectedTiles)
	{
		Unit target = targetTile.GetUnitOnTile();
		int finalDamage = (int)DamageCalculator.CalculateTotalDamage(battleData, targetTile, selectedTiles, GetTilesInFirstRange(battleData))[target].damage;
		int targetCurrentHealth = target.GetCurrentHealth();
		float recoverFactor = 0.3f;
		int recoverAmount = (int) ((float) finalDamage * recoverFactor);
		if (targetCurrentHealth == 0) recoverAmount *= 2;
		var recoverCoroutine = unitInChain.RecoverHealth(recoverAmount);
		battleData.battleManager.StartCoroutine(recoverCoroutine);
	}
}
}