using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Reina_5_m_SkillLogic : BasePassiveSkillLogic {

	public override void ApplyBonusDamageFromEachPassive(SkillInstanceData skillInstanceData) 
    {
		float damageBonus = 1.25f;

		if ((skillInstanceData.GetSkill().GetName() == "화염구") && (skillInstanceData.GetTargetCount() == 1))
			skillInstanceData.GetDamage().relativeDamageBonus *= damageBonus;
	}
}
}
