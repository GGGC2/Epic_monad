using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Reina_5_m_SkillLogic : BasePassiveSkillLogic {

	public override void ApplyBonusDamageFromEachPassive(SkillInstanceData skillInstanceData) 
    {
		float damageBonus = 1.25f;

		if ((skillInstanceData.getSkill().GetName() == "화염구") && (skillInstanceData.getTargetCount() == 1))
			skillInstanceData.getDamage().relativeDamageBonus *= damageBonus;
	}
}
}
