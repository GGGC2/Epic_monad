using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Reina_5_m_SkillLogic : BasePassiveSkillLogic {

	public override void ApplyBonusDamageFromEachPassive(CastingApply castingApply) 
    {
		float damageBonus = 1.25f;

		if ((castingApply.GetSkill().GetName() == "화염구") && (castingApply.GetTargetCount() == 1))
			castingApply.GetDamage().relativeDamageBonus *= damageBonus;
	}
}
}
