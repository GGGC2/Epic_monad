using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;
using System.Linq;
using Battle.Damage;

namespace Battle.Skills
{
public class Eren_0_1_SkillLogic : BasePassiveSkillLogic {
	public override void ApplyStatusEffectByKill(Unit eren)
	{
		StatusEffector.AttachStatusEffect(eren, this.passiveSkill, eren);
	}

	public override void SetAmountToEachStatusEffect(List<StatusEffect> statusEffects)
	{
		statusEffects[0].SetRemainStack(1);
		statusEffects[0].SetRemainPhase(3);
	}
}
}