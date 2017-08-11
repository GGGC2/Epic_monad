using System;
using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;

namespace Battle.Skills
{
public class ImagineBulletSkillLogic : BaseSkillLogic {
	/*public override void ActionInDamageRoutine(BattleData battleData, Skill appliedSkill, Unit unitInChain, Tile targetTile, List<GameObject> selectedTiles)
	{
		Debug.Log("Casting Imagine bullet");
		float apDamage = 32.0f;
		Dictionary<GameObject, DamageCalculator.DamageInfo> finalDamage = DamageCalculator.CalculateTotalDamage(targetTile, selectedTiles, GetTilesInFirstRange());
		IEnumerator damageAPCoroutine = targetTile.GetUnitOnTile().GetComponent<Unit>().Damaged(skillInstanceData, isDot: false, isHealth: false);
		BattleData.battleManager.StartCoroutine(damageAPCoroutine);
	}*/
}
}