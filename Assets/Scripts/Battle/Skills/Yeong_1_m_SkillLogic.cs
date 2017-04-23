using UnityEngine;
using Enums;

namespace Battle.Skills
{
public class Yeong_1_2_SkillLogic : BasePassiveSkillLogic {

	// public override void triggerEvasionEvent(BattleData battleData, Unit yeong)
	// {
	// 	var battleManager = battleData.battleManager;
	// 	int dexturity = yeong.GetActualStat(Stat.Dexturity);
	// 	int recoverAmount = (int)(dexturity * 0.3f);
	// 	battleManager.StartCoroutine(yeong.RecoverAP(recoverAmount));
	// 	Debug.LogError("Recover ap " + recoverAmount);
	// }
}
}
