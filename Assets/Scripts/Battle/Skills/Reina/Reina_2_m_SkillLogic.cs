using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Reina_2_m_SkillLogic : BasePassiveSkillLogic {

	public override void ApplyBonusDamageFromEachPassive(CastingApply castingApply) 
    {
		float damageBonusForPlaneTypeUnit = 1.15f;
		
		if (castingApply.GetTarget().GetElement() == Enums.Element.Plant)
			castingApply.GetDamage().relativeDamageBonus *= damageBonusForPlaneTypeUnit;
	}
}
}
