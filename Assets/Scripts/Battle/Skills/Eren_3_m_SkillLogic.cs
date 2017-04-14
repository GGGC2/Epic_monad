using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Eren_3_m_SkillLogic : BasePassiveSkillLogic {

	public override void ApplyBonusDamageFromEachPassive(SkillInstanceData skillInstanceData) 
    {
		float damageBonus = 1.2f;
		
		Unit target = skillInstanceData.getTarget();
		Unit caster = skillInstanceData.getCaster();

		int maxHealthOfTarget = target.GetMaxHealth();
		int maxHealthOfCaster = caster.GetMaxHealth();

		Battle.DamageCalculator.AttackDamage attackDamage = skillInstanceData.getDamage();

		if (maxHealthOfTarget < maxHealthOfCaster)
			attackDamage.relativeDamageBonus *= damageBonus;
	}
}
}
