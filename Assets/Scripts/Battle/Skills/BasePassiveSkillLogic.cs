using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;

namespace Battle.Skills
{
public class BasePassiveSkillLogic
{
	public virtual float GetAdditionalPowerBouns(Unit unit)
	{
		return 0;
	}

	public virtual DamageCalculator.AttackDamage ApplyBonusDamageFromEachPassive(DamageCalculator.AttackDamage attackDamage, Unit target)
	{
		return attackDamage;
	}

	public virtual bool checkEvade()
	{
		return false;
	}

	public virtual void triggerEvasionEvent(BattleData battleData, Unit unit)
	{
	}
}
}
