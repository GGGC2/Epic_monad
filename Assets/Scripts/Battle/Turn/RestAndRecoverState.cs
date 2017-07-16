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
        Unit unit = battleData.selectedUnit;
        List<PassiveSkill> passiveSkillList = unit.GetLearnedPassiveSkillList();
        int usingActivityPointToRest = GetRestCostAP(battleData);
		int maxHealthOfUnit = unit.GetStat(Stat.MaxHealth);
		int level = GameData.PartyData.level;
		//Debug.Log("Float : " + ((0.9f + maxHealthOfUnit * 0.0006f + level * 0.04f) * usingActivityPointToRest));
		//Debug.Log("Int : " + (int)((0.9f + maxHealthOfUnit * 0.0006f + level * 0.04f) * usingActivityPointToRest));
		int recoverHealthDuringRest = (int)((0.9f + maxHealthOfUnit * 0.0006f + level * 0.04f) * usingActivityPointToRest);
        recoverHealthDuringRest = (int)SkillLogicFactory.Get(passiveSkillList).ApplyAdditionalRecoverHealthDuringRest(unit, recoverHealthDuringRest);
		unit.UseActivityPoint(usingActivityPointToRest);
		IEnumerator recoverHealthCoroutine = unit.RecoverHealth(recoverHealthDuringRest);

		BattleManager battleManager = battleData.battleManager;
		yield return battleManager.StartCoroutine(recoverHealthCoroutine);

		// 휴식시 발동되는 특성 및 statusEffect
		SkillLogicFactory.Get(passiveSkillList).TriggerOnRest(unit);
        List<StatusEffect> statusEffectList = unit.GetStatusEffectList();
        foreach(StatusEffect statusEffect in statusEffectList) {
            PassiveSkill passiveSkill = statusEffect.GetOriginPassiveSkill();
            if(passiveSkill != null) {
                SkillLogicFactory.Get(passiveSkill).TriggerStatusEffectsOnRest(unit, statusEffect);
            }
        }

		Debug.Log("Rest. Using " + usingActivityPointToRest + "AP and recover " + recoverHealthDuringRest + " HP");

		yield return new WaitForSeconds(0.1f);
	}
}
