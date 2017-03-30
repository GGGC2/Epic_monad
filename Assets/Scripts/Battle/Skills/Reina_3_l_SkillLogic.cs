using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace Battle.Skills
{
public class Reina_3_l_SkillLogic : BasePassiveSkillLogic {

	public override float ApplyIgnoreResistanceAbsoluteValueByEachPassive(float resistance, Unit caster, Unit target)	
	{
		int numberOfBuffFromOtherUnits = caster.GetStatusEffectList().Count(
						x => x.GetIsBuff() && (x.GetCaster() != caster.gameObject));

		// 갯수 * {40 + (레벨 * 0.5)} %
		int partyLevel = MonoBehaviour.FindObjectOfType<BattleManager>().GetPartyLevel();
		float ignoreAmount = numberOfBuffFromOtherUnits * (40 + (partyLevel * 0.5f));

		return resistance - ignoreAmount;
	}
}
}
