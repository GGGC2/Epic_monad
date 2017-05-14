using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Eren_3_m_SkillLogic : BasePassiveSkillLogic {

	public override void ApplyBonusDamageFromEachPassive(SkillInstanceData skillInstanceData) 
    {
		float damageBonus = 1.2f;
		
		if (skillInstanceData.getTarget().GetMaxHealth() < skillInstanceData.GetCaster().GetMaxHealth())
			skillInstanceData.getDamage().relativeDamageBonus *= damageBonus;
	}
}
}
