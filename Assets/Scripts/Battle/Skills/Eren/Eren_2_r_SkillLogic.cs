using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;
using System.Linq;
using Battle.Damage;

namespace Battle.Skills
{
public class Eren_2_r_SkillLogic : BasePassiveSkillLogic {
	public override IEnumerator TriggerOnKill(HitInfo hitInfo, Unit deadUnit)
	{
		Debug.Log("AP before : " + hitInfo.caster.GetCurrentActivityPoint());
		// 민첩성의 0.1만큼 행동력을 회복
		Unit eren = hitInfo.caster;
		int dexterity = eren.GetStat(Stat.Agility);
		int amount = (int)(dexterity * 0.1f);
		Debug.Log("Amount : " + amount);
		BattleData.battleManager.StartCoroutine(eren.RecoverActionPoint(amount));
		Debug.Log("AP before : " + hitInfo.caster.GetCurrentActivityPoint());
        yield return null;
	}
}
}