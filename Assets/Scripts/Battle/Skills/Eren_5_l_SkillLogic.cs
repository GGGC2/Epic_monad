using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Eren_5_l_SkillLogic : BasePassiveSkillLogic {

	public override float GetAdditionalPowerBonus(Unit caster)
	{
		float powerBonusPerBuff = 0.02f;
		UnitManager unitManager = MonoBehaviour.FindObjectOfType<UnitManager>();
		int numberOfRemainEnemies = unitManager.GetAllUnits().Count(
						x => x.GetComponent<Unit>().GetSide() == Enums.Side.Enemy);
		return numberOfRemainEnemies * powerBonusPerBuff + 1;
	}
}
}
