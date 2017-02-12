using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Kashyasty_1_r_SkillLogic : BasePassiveSkillLogic {

	public override DamageCalculator.AttackDamage ApplyTacticalBonusFromEachPassive(DamageCalculator.AttackDamage attackDamage, Unit caster)
	{
		// fixme : '2배' 표현 어떻게 할것인지
		return attackDamage;
	}
}
}