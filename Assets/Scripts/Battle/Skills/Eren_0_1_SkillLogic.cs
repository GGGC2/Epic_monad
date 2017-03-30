using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;
using System.Linq;
using Battle.Damage;

namespace Battle.Skills
{
public class Eren_0_1_SkillLogic : BasePassiveSkillLogic {
	public override void ApplyStatusEffectByKill(HitInfo hitInfo, Unit deadUnit)
	{
		StatusEffector.AttachStatusEffect(hitInfo.caster, this.passiveSkill, hitInfo.caster);
	}

	public override void SetAmountToEachStatusEffect(List<StatusEffect> statusEffects, Unit caster, Unit target)
	{
		statusEffects[0].SetRemainStack(1);
		statusEffects[0].SetRemainPhase(3);
	}
}
}