using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Reina_0_1_SkillLogic : BasePassiveSkillLogic {

	public override float GetAdditionalRelativePowerBonus(Unit caster)
	{
		float powerBonusPerBuff = 0.05f;
		int numberOfBuffFromOtherUnits = caster.StatusEffectList.Count(
						x => x.GetIsBuff() && (x.GetCaster() != caster));
		return 1 + numberOfBuffFromOtherUnits * powerBonusPerBuff;
	}
}
}
