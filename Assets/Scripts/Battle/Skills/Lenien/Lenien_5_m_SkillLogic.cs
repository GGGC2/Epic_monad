using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Lenien_5_m_SkillLogic : BasePassiveSkillLogic {

	public override void ApplyBonusDamageFromEachPassive(CastingApply castingApply) 
    {
		float damageBonusPerTarget = 0.05f;
		
		float totalDamageBonus = 1.0f + castingApply.GetTargetCount() * damageBonusPerTarget;
		castingApply.GetDamage().relativeDamageBonus *= totalDamageBonus;
	}
}
}
