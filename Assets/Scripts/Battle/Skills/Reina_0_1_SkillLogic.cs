using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Reina_0_1_SkillLogic : BasePassiveSkillLogic {

	public override float GetAdditionalPowerBonus(Unit caster)
	{
		float powerBonusPerBuff = 0.05f;
		int numberOfBuffFromOtherUnits = caster.GetStatusEffectList().Count(
						x => x.GetIsBuff() && (x.GetCaster() != caster.gameObject));
		return numberOfBuffFromOtherUnits * powerBonusPerBuff + 1.0f;
	}
}
}
