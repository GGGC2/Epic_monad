using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Reina_0_1_SkillLogic : BasePassiveSkillLogic {

	public override float GetAdditionalPowerBouns(Unit caster)
	{
		float damageBounsPerBuff = 5;
		int numberOfBuffFromOtherUnits = caster.GetStatusEffectList().Count(
						x => x.GetIsBuff() && (x.GetCaster() != caster.gameObject));
		return numberOfBuffFromOtherUnits * damageBounsPerBuff;
	}
}
}
