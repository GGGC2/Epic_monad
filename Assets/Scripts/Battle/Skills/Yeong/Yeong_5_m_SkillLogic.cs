using UnityEngine;
using Enums;
using Battle.Damage;
using System.Collections.Generic;

namespace Battle.Skills
{
public class Yeong_5_m_SkillLogic : BasePassiveSkillLogic {

	public override void TriggerOnEvasionEvent(BattleData battleData, Unit caster, Unit yeong)
	{
		int amount = (int)(yeong.GetStat(Stat.Dexturity) * 0.3f);
		yeong.RecoverActionPoint(amount);
	}
}
}
