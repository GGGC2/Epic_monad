using UnityEngine;
using System.Collections;
using Enums;

public class RestAndRecover {
	public static int GetRestCostAP(BattleData battleData)
	{
		return (int)(battleData.selectedUnitObject.GetComponent<Unit>().GetCurrentActivityPoint() * 0.9f);
	}

	public static IEnumerator Run(BattleData battleData)
	{
		int usingActivityPointToRest = GetRestCostAP(battleData);
		int maxHealthOfUnit = battleData.selectedUnitObject.GetComponent<Unit>().GetStat(Stat.MaxHealth);
		int level = 30;
		int recoverHealthDuringRest = (int)(0.9f + maxHealthOfUnit*0.0006f + level*0.04f)*usingActivityPointToRest;
		battleData.selectedUnitObject.GetComponent<Unit>().UseActivityPoint(usingActivityPointToRest);
		IEnumerator recoverHealthCoroutine = battleData.selectedUnitObject.GetComponent<Unit>().RecoverHealth(recoverHealthDuringRest);

		BattleManager battleManager = battleData.battleManager;
		yield return battleManager.StartCoroutine(recoverHealthCoroutine);

		Debug.Log("Rest. Using " + usingActivityPointToRest + "AP and recover " + recoverHealthDuringRest + " HP");

		yield return new WaitForSeconds(0.1f);
	}
}
