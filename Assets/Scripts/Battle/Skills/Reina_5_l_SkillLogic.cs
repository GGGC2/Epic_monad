using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Reina_5_l_SkillLogic : BasePassiveSkillLogic {

	public override void ApplyBonusDamageFromEachPassive(SkillInstanceData skillInstanceData) 
    {
		float damageBonus = 1.3f;

		if ((skillInstanceData.getSkill().GetName() == "화염 폭발") && (skillInstanceData.getTargetCount() >= 3))
			skillInstanceData.getDamage().relativeDamageBonus *= damageBonus;
	}
}
}
