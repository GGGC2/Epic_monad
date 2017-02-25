using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;
using System.Linq;

namespace Battle.Skills
{
public class Eren_1_r_SkillLogic : BaseSkillLogic {
	public override void SetAmountToEachStatusEffect(List<StatusEffect> statusEffects)
	{
		var statusEffect1st = statusEffects.Find(se => se.GetDisplayName() == "강타");
		statusEffect1st.SetRemainPhase(2);
		statusEffect1st.SetAmount(0, 10);
		statusEffect1st.SetAmount(1, 46);

		var statusEffect2nd = statusEffects.Find(se => se.GetDisplayName() == "저항력 감소");
		statusEffect2nd.SetRemainPhase(5);
		statusEffect2nd.SetAmount(0, -100);
	}
}
}