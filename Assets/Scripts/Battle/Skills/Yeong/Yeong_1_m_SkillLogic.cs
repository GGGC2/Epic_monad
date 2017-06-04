using UnityEngine;
using Enums;
using Battle.Damage;

namespace Battle.Skills
{
public class Yeong_1_m_SkillLogic : BasePassiveSkillLogic {

	public override void TriggerOnEvasionEvent(BattleData battleData, Unit caster, Unit yeong)
	{
		StatusEffector.AttachStatusEffect(yeong, this.passiveSkill, caster);
	}
}
}
