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
}
}
