using UnityEngine;
using Battle.Damage;
using System.Collections.Generic;

namespace Battle.Skills
{
public class Yeong_1_r_SkillLogic : BasePassiveSkillLogic
{
	public override void TriggerUsingSkill(Unit yeong, List<Unit> targets)
	{
		StatusEffector.AttachStatusEffect(yeong, this.passiveSkill, yeong);
	}
}
}
