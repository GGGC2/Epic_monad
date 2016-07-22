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
		int recoverHealthDuringRest = (int)(battleData.selectedUnitObject.GetComponent<Unit>().GetStat(Stat.MaxHealth) * (usingActivityPointToRest / 100f));
		battleData.selectedUnitObject.GetComponent<Unit>().UseActionPoint(usingActivityPointToRest);
		IEnumerator recoverHealthCoroutine = battleData.selectedUnitObject.GetComponent<Unit>().RecoverHealth(recoverHealthDuringRest);

		BattleManager battleManager = battleData.battleManager;
		yield return battleManager.StartCoroutine(recoverHealthCoroutine);

		Debug.Log("Rest. Using " + usingActivityPointToRest + "AP and recover " + recoverHealthDuringRest + " HP");

		yield return new WaitForSeconds(0.5f);
	}
}
