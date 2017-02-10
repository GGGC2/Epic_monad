using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Reina_0_1_SkillLogic : BasePassiveSkillLogic {

	public override float GetAdditionalPowerBouns(Unit unit)
	{
		float damageBounsPerBuff = 5;
		int numberOfBuffFromOtherUnits = unit.GetStatusEffectList().Count(
						x => x.GetIsBuff() && (x.GetCaster() != unit.gameObject));
		return numberOfBuffFromOtherUnits * damageBounsPerBuff;
	}
}
}
