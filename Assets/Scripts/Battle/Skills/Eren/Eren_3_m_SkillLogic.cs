using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Eren_3_m_SkillLogic : BasePassiveSkillLogic {

	public override void ApplyBonusDamageFromEachPassive(CastingApply castingApply) 
    {
		float damageBonus = 1.2f;
		
		if (castingApply.GetTarget().GetMaxHealth() < castingApply.GetCaster().GetMaxHealth())
			castingApply.GetDamage().relativeDamageBonus *= damageBonus;
	}
}
}
