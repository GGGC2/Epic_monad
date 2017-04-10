﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Skills
{
public class HarmonySkillLogic : BaseSkillLogic {
	public override int CalculateAP(BattleData battleData, List<Tile> selectedTiles)
	{
		Debug.Assert(selectedTiles.Count == 1, "조화 진동은 범위가 1일거야");
		int enemyCurrentAP = selectedTiles[0].GetUnitOnTile().GetCurrentActivityPoint();
		int requireAP = Math.Min(enemyCurrentAP, battleData.selectedUnit.GetActualRequireSkillAP(battleData.SelectedSkill));
		return requireAP;
	}
}
}