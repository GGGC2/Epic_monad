using UnityEngine;
using System.Linq;
using Battle.Damage;
using System.Collections.Generic;

namespace Battle.Skills
{
public class Eren_7_r_SkillLogic : BasePassiveSkillLogic {

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
		int speedBonusPerBuff = 7;
		UnitManager unitManager = MonoBehaviour.FindObjectOfType<UnitManager>();
		
		int amount = stack * speedBonusPerBuff;
		
		statusEffects[0].SetAmount(amount);
	}
}
}
