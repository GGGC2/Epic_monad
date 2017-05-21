using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Reina_2_m_SkillLogic : BasePassiveSkillLogic {

	public override void ApplyBonusDamageFromEachPassive(SkillInstanceData skillInstanceData) 
    {
		float damageBonusForPlaneTypeUnit = 1.15f;
		
		if (skillInstanceData.GetMainTarget().GetElement() == Enums.Element.Plant)
			skillInstanceData.GetDamage().relativeDamageBonus *= damageBonusForPlaneTypeUnit;
	}
}
}
