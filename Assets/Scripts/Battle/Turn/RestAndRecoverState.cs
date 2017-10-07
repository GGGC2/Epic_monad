using UnityEngine;
using System.Collections;
using Enums;
using System.Collections.Generic;
using Battle.Skills;

public class RestAndRecover {
	public static int GetRestCostAP()
	{
		return (int)(BattleData.selectedUnit.GetCurrentActivityPoint() * 0.5f);
	}

	public static void Run()
	{
        Unit unit = BattleData.selectedUnit;
        List<PassiveSkill> passiveSkillList = unit.GetLearnedPassiveSkillList();
        int usingActivityPointToRest = GetRestCostAP();
		int maxHealthOfUnit = unit.GetStat(Stat.MaxHealth);
		int level = GameData.PartyData.level;
		//Debug.Log("Float : " + ((0.9f + maxHealthOfUnit * 0.0006f + level * 0.04f) * usingActivityPointToRest));
		//Debug.Log("Int : " + (int)((0.9f + maxHealthOfUnit * 0.0006f + level * 0.04f) * usingActivityPointToRest));
		int recoverHealthDuringRest = (int)((0.9f + maxHealthOfUnit * 0.0006f + level * 0.04f) * usingActivityPointToRest);
        LogManager logManager = LogManager.Instance;
        
        recoverHealthDuringRest = (int)SkillLogicFactory.Get(passiveSkillList).ApplyAdditionalRecoverHealthDuringRest(unit, recoverHealthDuringRest);
		unit.UseActivityPoint(usingActivityPointToRest);
		unit.RecoverHealth(recoverHealthDuringRest);

        // 휴식시 발동되는 특성 및 statusEffect
        SkillLogicFactory.Get(passiveSkillList).TriggerOnRest(unit);
        List<UnitStatusEffect> statusEffectList = unit.StatusEffectList;
        foreach(UnitStatusEffect statusEffect in statusEffectList) {
            Skill passiveSkill = statusEffect.GetOriginSkill();
            if(passiveSkill != null && passiveSkill.GetType() == typeof(PassiveSkill)) {
                ((PassiveSkill)passiveSkill).SkillLogic.TriggerStatusEffectsOnRest(unit, statusEffect);
            }
        }

		Debug.Log("Rest. Using " + usingActivityPointToRest + "AP and recover " + recoverHealthDuringRest + " HP");
        
        logManager.Record(new WaitForSecondsLog(0.1f));
	}
}
