using UnityEngine;
using System.Linq;
using Battle.Damage;

namespace Battle.Skills
{
public class Lenien_3_r_SkillLogic : BasePassiveSkillLogic {

	public override void TriggerDamaged(Unit lenien, int damage, Unit attacker)
	{
		StatusEffector.AttachStatusEffect(lenien, this.passiveSkill, attacker);
	}
}
}
