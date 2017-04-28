using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace Battle.Skills
{
public class Reina_7_m_SkillLogic : BasePassiveSkillLogic {

	public override void SetAmountToEachStatusEffect(List<StatusEffect> statusEffects, Unit caster, Unit target)	
	{
		float bonusPerBuff = 10;
		int numberOfBuffFromOtherUnits = caster.GetStatusEffectList().Count(
						x => x.GetIsBuff() && (x.GetCaster() != caster.gameObject));
		float amount = numberOfBuffFromOtherUnits * bonusPerBuff;
		
		// 10 * 갯수 %
		var statusEffect1st = statusEffects.Find(se => se.GetOriginSkillName() == "화속성 약점");
		if (amount <= 0)
			statusEffects.Remove(statusEffect1st);
		else
		{
			statusEffect1st.SetRemainPhase(2);
			statusEffect1st.SetAmount(0, amount);
		}
	}
}
}
