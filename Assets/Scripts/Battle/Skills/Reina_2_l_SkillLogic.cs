using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Battle.Damage;

namespace Battle.Skills
{
public class Reina_2_l_SkillLogic : BasePassiveSkillLogic {

	public override void TriggerActiveSkillDamageApplied(Unit caster, Unit target)
	{
		StatusEffector.AttachStatusEffect(caster, this.passiveSkill, target);
	}

	public override void SetAmountToEachStatusEffect(List<StatusEffect> statusEffects, Unit caster, Unit target)	
	{
		float bonusPerBuff = 10;
		int numberOfBuffFromOtherUnits = caster.GetStatusEffectList().Count(
						x => x.GetIsBuff() && (x.GetCaster() != caster.gameObject));
		float amount = numberOfBuffFromOtherUnits * bonusPerBuff;
		if (amount > 100) amount = 100;

		// 10 * 갯수 %
		var statusEffect1st = statusEffects.Find(se => se.GetOriginSkillName() == "내상");
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
