using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace Battle.Skills
{
public class Eren_2_l_SkillLogic : BasePassiveSkillLogic {

	public override float ApplyIgnoreDefenceAbsoluteValueByEachPassive(SkillInstanceData skillInstanceData, float defense)
	{
        Unit caster = skillInstanceData.getCaster();
		// 27 + (lv * 0.3 * stack)
		List<StatusEffect> statusEffectsOfCaster = caster.GetStatusEffectList();
		StatusEffect uniqueStatusEffect = statusEffectsOfCaster.Find(se => se.GetDisplayName() == "흡수");
		
		int stack = 0;
		if (uniqueStatusEffect != null)
			stack = uniqueStatusEffect.GetRemainStack();
		
		float ignoreAmountPerStack = 0.3f;
		int partyLevel = MonoBehaviour.FindObjectOfType<BattleManager>().GetPartyLevel();
		float baseAmountPerLevel = 27;

		defense -= baseAmountPerLevel + (partyLevel * ignoreAmountPerStack * stack);

		return defense;
	}
}
}
