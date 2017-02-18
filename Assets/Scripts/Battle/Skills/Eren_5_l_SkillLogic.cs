using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Eren_5_l_SkillLogic : BasePassiveSkillLogic {

	public override float GetAdditionalPowerBouns(Unit caster)
	{
		float powerBounsPerBuff = 0.02f;
		UnitManager unitManager = MonoBehaviour.FindObjectOfType<UnitManager>();
		int numberOfRemainEnemies = unitManager.GetAllUnits().Count(
						x => x.GetComponent<Unit>().GetSide() == Enums.Side.Enemy);
		return numberOfRemainEnemies * powerBounsPerBuff + 1;
	}
}
}
