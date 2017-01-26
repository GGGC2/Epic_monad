using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Skills
{
public class BaseSkillLogic
{
	public virtual int CalculateAP(BattleData battleData, List<GameObject> selectedTiles)
	{
		int requireAP = battleData.selectedUnitObject.GetComponent<Unit>().GetActualRequireSkillAP(battleData.SelectedSkill);
		return requireAP;
	}
}
}