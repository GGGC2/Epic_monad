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
		float apDamage = 32.0f;
		Dictionary<GameObject, float> finalDamage = DamageCalculator.CalculateTotalDamage(battleData, targetTile, selectedTiles, GetTilesInFirstRange(battleData));
		IEnumerator damageAPCoroutine = targetTile.GetUnitOnTile().GetComponent<Unit>().Damaged(appliedSkill, unitInChain, apDamage, appliedSkill.GetPenetration(), isDot: false, isHealth: false);
		battleData.battleManager.StartCoroutine(damageAPCoroutine);
	}
}
}