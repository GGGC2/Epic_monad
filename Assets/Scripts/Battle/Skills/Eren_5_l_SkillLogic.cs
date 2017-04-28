using UnityEngine;
using System.Linq;
using Battle.Damage;
using System.Collections.Generic;

namespace Battle.Skills
{
public class Eren_5_l_SkillLogic : BasePassiveSkillLogic {

	public override void TriggerActionEnd(Unit eren)
	{
		StatusEffector.AttachStatusEffect(eren, this.passiveSkill, eren);
	} 

	public override void SetAmountToEachStatusEffect(List<StatusEffect> statusEffects, Unit eren, Unit target) 
	{
		float powerBonusPerBuff = 0.02f;
		UnitManager unitManager = MonoBehaviour.FindObjectOfType<UnitManager>();
		int numberOfRemainEnemies = unitManager.GetAllUnits().Count(x => x.GetSide() == Enums.Side.Enemy);
		float amount = numberOfRemainEnemies * powerBonusPerBuff + 1;

		statusEffects[0].SetAmount(amount);
	}
}
}
