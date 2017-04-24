using UnityEngine;
using Battle.Damage;

namespace Battle.Skills
{
public class Yeong_1_r_SkillLogic : BasePassiveSkillLogic
{
	public override void TriggerActiveSkillDamageApplied(Unit yeong)
	{
		StatusEffector.AttachStatusEffect(yeong, this.passiveSkill, yeong);
	}
}
}
