using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;
using System.Linq;
using Battle.Damage;

namespace Battle.Skills
{
public class Eren_2_r_SkillLogic : BasePassiveSkillLogic {
	public override void ApplyStatusEffectByKill(HitInfo hitInfo, Unit deadUnit)
	{
		// 민첩성의 0.1만큼 행동력을 회복
		Unit eren = hitInfo.caster;
		int dexterity = eren.GetActualStat(Stat.Dexturity);
		int amount = (int)(dexterity * 0.1f);

		eren.RecoverActionPoint(amount);
	}
}
}