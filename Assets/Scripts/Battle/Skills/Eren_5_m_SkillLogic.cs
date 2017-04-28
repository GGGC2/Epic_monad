using UnityEngine;
using System.Linq;
using Battle.Damage;
using System.Collections.Generic;

namespace Battle.Skills
{
public class Eren_5_m_SkillLogic : BasePassiveSkillLogic {

	public override void TriggerActionEnd(Unit eren)
	{
		StatusEffector.AttachStatusEffect(eren, this.passiveSkill, eren);
	} 

	public override void SetAmountToEachStatusEffect(List<StatusEffect> statusEffects, Unit eren, Unit target) 
	{
		StatusEffect uniqueStatusEffect = eren.GetStatusEffectList().Find(se => se.GetDisplayName() == "흡수");		
		int stack = 0;
		if (uniqueStatusEffect != null)
			stack = uniqueStatusEffect.GetRemainStack();
		float powerBonusPerBuff = 0.07f;
		UnitManager unitManager = MonoBehaviour.FindObjectOfType<UnitManager>();
		
		float amount = stack * powerBonusPerBuff + 1;
		
		statusEffects[0].SetAmount(amount);
	}
}
}
