using UnityEngine;
using Enums;
using Battle.Damage;
using System.Collections.Generic;

namespace Battle.Skills
{
public class Yeong_7_r_SkillLogic : BasePassiveSkillLogic {

	public override void TriggerRest(Unit caster)
	{
		StatusEffector.AttachStatusEffect(caster, this.passiveSkill, caster);
	}
}
}
