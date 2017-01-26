using System;
using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;

namespace Battle.Skills
{
public class ImagineBulletSkillLogic : BaseSkillLogic {
	public override void ActionInDamageRoutine(BattleData battleData, Skill appliedSkill, Unit unitInChain, Tile targetTile, List<GameObject> selectedTiles)
	{
		Debug.Log("Casting Imagine bullet");
		float[] apDamage = new float[5] {32.0f, 44.0f, 57.0f, 69.0f, 81.0f};
		Dictionary<GameObject, float> finalDamage = DamageCalculator.CalculateTotalDamage(battleData, targetTile, selectedTiles, GetTilesInFirstRange(battleData));
		IEnumerator damageAPCoroutine = targetTile.GetUnitOnTile().GetComponent<Unit>().Damaged(UnitClass.None, apDamage[appliedSkill.GetLevel()-1], appliedSkill.GetPenetration(appliedSkill.GetLevel()), isDot: false, isHealth: false);
		battleData.battleManager.StartCoroutine(damageAPCoroutine);
	}
}
}