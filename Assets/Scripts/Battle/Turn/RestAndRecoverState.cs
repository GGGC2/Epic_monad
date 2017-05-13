using UnityEngine;
using System.Collections;
using Enums;
using System.Collections.Generic;
using Battle.Skills;

public class RestAndRecover {
	public static int GetRestCostAP(BattleData battleData)
	{
		return (int)(battleData.selectedUnit.GetCurrentActivityPoint() * 0.5f);
	}

	public static IEnumerator Run(BattleData battleData)
	{
		int usingActivityPointToRest = GetRestCostAP(battleData);
		int maxHealthOfUnit = battleData.selectedUnit.GetStat(Stat.MaxHealth);
		int level = battleData.partyLevel;
		Debug.Log("Float : " + ((0.9f + maxHealthOfUnit * 0.0006f + level * 0.04f) * usingActivityPointToRest));
		Debug.Log("Int : " + (int)((0.9f + maxHealthOfUnit * 0.0006f + level * 0.04f) * usingActivityPointToRest));
		int recoverHealthDuringRest = (int)((0.9f + maxHealthOfUnit * 0.0006f + level * 0.04f) * usingActivityPointToRest);
		battleData.selectedUnit.UseActivityPoint(usingActivityPointToRest);
		IEnumerator recoverHealthCoroutine = battleData.selectedUnit.RecoverHealth(recoverHealthDuringRest);

		BattleManager battleManager = battleData.battleManager;
		yield return battleManager.StartCoroutine(recoverHealthCoroutine);

		// 휴식시 발동되는 특성
		List<PassiveSkill> passiveSkillList = battleData.selectedUnit.GetLearnedPassiveSkillList();
		SkillLogicFactory.Get(passiveSkillList).TriggerRest(battleData.selectedUnit);

		Debug.Log("Rest. Using " + usingActivityPointToRest + "AP and recover " + recoverHealthDuringRest + " HP");

		yield return new WaitForSeconds(0.1f);
	}
}
