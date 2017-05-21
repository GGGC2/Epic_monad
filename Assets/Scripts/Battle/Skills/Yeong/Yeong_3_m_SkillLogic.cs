using UnityEngine;
using Enums;
using Battle.Damage;
using System.Collections.Generic;

namespace Battle.Skills
{
public class Yeong_3_m_SkillLogic : BasePassiveSkillLogic {

	public override void TriggerEvasionEvent(BattleData battleData, Unit caster, Unit yeong)
	{
		StatusEffector.AttachStatusEffect(yeong, this.passiveSkill, yeong);
	}

	public override void SetAmountToEachStatusEffect(List<StatusEffect> statusEffects, Unit caster, Unit target)
	{
		int partyLevel = MonoBehaviour.FindObjectOfType<BattleManager>().GetPartyLevel();		
		float amount = 8 + (0.1f * partyLevel);
		
		var statusEffect1st = statusEffects.Find(se => se.GetOriginSkillName() == "질풍노도");
		statusEffect1st.SetRemainPhase(999);
		statusEffect1st.SetAmount(0, amount);
		statusEffect1st.SetAmount(1, amount);
	}
}
}
