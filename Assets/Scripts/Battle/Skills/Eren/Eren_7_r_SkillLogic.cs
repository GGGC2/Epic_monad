using UnityEngine;
using System.Linq;
using Battle.Damage;
using System.Collections.Generic;

namespace Battle.Skills
{
public class Eren_7_r_SkillLogic : BasePassiveSkillLogic {

	public override void TriggerActionEnd(Unit eren)
	{
		StatusEffect uniqueStatusEffect = eren.GetStatusEffectList().Find(se => se.GetDisplayName() == "흡수");

		if (uniqueStatusEffect != null)
			StatusEffector.AttachStatusEffect(eren, this.passiveSkill, eren);
		else
		{
			List<StatusEffect> statusEffectList = eren.GetStatusEffectList();
			statusEffectList = statusEffectList.FindAll(x => x.GetOriginSkillName() != "천상의 전령");
			eren.SetStatusEffectList(statusEffectList);
		}
	} 

	public override void SetAmountToEachStatusEffect(List<StatusEffect> statusEffects, Unit eren, Unit target) 
	{
		StatusEffect uniqueStatusEffect = eren.GetStatusEffectList().Find(se => se.GetDisplayName() == "흡수");		
		int stack = 0;
		if (uniqueStatusEffect != null)
			stack = uniqueStatusEffect.GetRemainStack();
		int speedBonusPerBuff = 7;
		UnitManager unitManager = MonoBehaviour.FindObjectOfType<UnitManager>();
		
		// int amount = stack * speedBonusPerBuff;
		
		statusEffects[0].SetRemainStack(stack);
		statusEffects[0].SetAmount(speedBonusPerBuff);
	}
}
}
