using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Eren_2_l_SkillLogic : BasePassiveSkillLogic {

	public override float ApplyIgnoreDefenceAbsoluteValueByEachPassive(float defense, Unit caster, Unit target)
	{
		// 27 + (lv * 0.3 * stack)
		StatusEffect uniqueStatusEffect = caster.GetStatusEffectList().Find(se => se.GetDisplayName() == "흡수");		
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
