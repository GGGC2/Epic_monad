using UnityEngine;
using System.Linq;
using Battle.Damage;
using System.Collections.Generic;

namespace Battle.Skills
{
public class Eren_5_l_SkillLogic : BasePassiveSkillLogic {

	public override void TriggerActionEnd(Unit eren)
	{
		UnitManager unitManager = MonoBehaviour.FindObjectOfType<UnitManager>();
		int numberOfRemainEnemies = unitManager.GetAllUnits().Count(x => x.GetSide() == Enums.Side.Enemy);

		if (numberOfRemainEnemies > 0)
			StatusEffector.AttachStatusEffect(eren, this.passiveSkill, eren);
		else
		{
			List<StatusEffect> statusEffectList = eren.GetStatusEffectList();
			statusEffectList = statusEffectList.FindAll(x => x.GetOriginSkillName() != "배척받는 자");
			eren.SetStatusEffectList(statusEffectList);
		}
	} 

	public override void SetAmountToEachStatusEffect(List<StatusEffect> statusEffects, Unit eren, Unit target) 
	{
		float powerBonusPerBuff = 0.02f;
		UnitManager unitManager = MonoBehaviour.FindObjectOfType<UnitManager>();
		int numberOfRemainEnemies = unitManager.GetAllUnits().Count(x => x.GetSide() == Enums.Side.Enemy);
		// float amount = numberOfRemainEnemies * powerBonusPerBuff + 1;

		statusEffects[0].SetRemainStack(numberOfRemainEnemies);
		statusEffects[0].SetAmount(1 + powerBonusPerBuff);
	}
}
}
