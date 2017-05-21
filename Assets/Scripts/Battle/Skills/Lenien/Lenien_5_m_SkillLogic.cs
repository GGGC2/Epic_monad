using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Lenien_5_m_SkillLogic : BasePassiveSkillLogic {

	public override void ApplyBonusDamageFromEachPassive(SkillInstanceData skillInstanceData) 
    {
		float damageBonusPerTarget = 0.05f;
		
		float totalDamageBonus = 1.0f + skillInstanceData.GetTargetCount() * damageBonusPerTarget;
		skillInstanceData.GetDamage().relativeDamageBonus *= totalDamageBonus;
	}
}
}
