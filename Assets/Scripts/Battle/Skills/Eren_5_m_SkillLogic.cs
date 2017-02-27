using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Eren_5_m_SkillLogic : BasePassiveSkillLogic {

	public override float GetAdditionalRelativePowerBonus(Unit caster)
	{
		StatusEffect uniqueStatusEffect = caster.GetStatusEffectList().Find(se => se.GetDisplayName() == "흡수");		
		int stack = 0;
		if (uniqueStatusEffect != null)
			stack = uniqueStatusEffect.GetRemainStack();
		float powerBonusPerBuff = 0.07f;
		UnitManager unitManager = MonoBehaviour.FindObjectOfType<UnitManager>();
		
		return stack * powerBonusPerBuff + 1;
	}
}
}
