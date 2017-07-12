using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace Battle.Skills
{
public class Reina_3_l_SkillLogic : BasePassiveSkillLogic {

	public override float ApplyIgnoreResistanceAbsoluteValueByEachPassive(Skill appliedSkill, Unit target, Unit caster, float resistance)	
	{
		int numberOfBuffFromOtherUnits = caster.GetStatusEffectList().Count(
						x => x.GetIsBuff() && (x.GetCaster() != caster));

		// 갯수 * {40 + (레벨 * 0.5)} %
		float ignoreAmount = numberOfBuffFromOtherUnits * (40 + (GameData.level * 0.5f));

		return resistance - ignoreAmount;
	}
}
}
