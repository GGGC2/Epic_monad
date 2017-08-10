using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Eren_2_l_SkillLogic : BasePassiveSkillLogic {

	public override float ApplyIgnoreResistanceAbsoluteValueByEachPassive(ActiveSkill appliedSkill, Unit target, Unit caster, float resistance)
	{
		// 27 + (lv * 0.3 * stack)
		UnitStatusEffect uniqueStatusEffect = caster.GetStatusEffectList().Find(se => se.GetDisplayName() == "흡수");		
		int stack = 0;
		if (uniqueStatusEffect != null)
			stack = uniqueStatusEffect.GetRemainStack();
		float ignoreAmountPerStack = 0.3f;
		float baseAmountPerLevel = 27;

		resistance -= baseAmountPerLevel + (GameData.PartyData.level * ignoreAmountPerStack * stack);
		return resistance;
	}
}
}
