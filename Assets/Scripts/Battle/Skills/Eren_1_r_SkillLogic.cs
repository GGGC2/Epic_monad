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
		var statusEffect = statusEffects.Find(se => se.GetDisplayName() == "강타");
		statusEffect.SetRemainPhase(2);
		statusEffect.SetAmount(0, 10);
	}
}
}