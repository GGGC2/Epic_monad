using UnityEngine;
using Enums;
using Battle.Damage;
using System.Collections.Generic;

namespace Battle.Skills
{
public class Yeong_6_r_SkillLogic : BasePassiveSkillLogic {

	public override void TriggerDamaged(Unit yeong, int finalDamage)
	{
		StatusEffector.AttachStatusEffect(yeong, this.passiveSkill, yeong);
	}
}
}
