using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace Battle.Skills
{
public class Eren_5_l_SkillLogic : BasePassiveSkillLogic {

	public override float GetAdditionalRelativePowerBonus(Unit caster)
	{
		float powerBonusPerBuff = 0.02f;
		UnitManager unitManager = MonoBehaviour.FindObjectOfType<UnitManager>();
		List<Unit> allUnitsInStage = unitManager.GetAllUnits(); 
		int numberOfRemainEnemies = allUnitsInStage.Count(x => x.GetSide() == Enums.Side.Enemy);
		return numberOfRemainEnemies * powerBonusPerBuff + 1;
	}
}
}
