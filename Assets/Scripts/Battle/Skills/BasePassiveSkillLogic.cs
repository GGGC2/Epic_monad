using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;

namespace Battle.Skills
{
public class BasePassiveSkillLogic
{
	public PassiveSkill passiveSkill = null;

	public virtual float GetAdditionalPowerBonus(Unit caster)
	{
		return 1.0f;
	}

	public virtual DamageCalculator.AttackDamage ApplyBonusDamageFromEachPassive(DamageCalculator.AttackDamage attackDamage, Unit caster, Skill appliedSkill, Unit target, int targetCount)
	{
		return attackDamage;
	}

	public virtual DamageCalculator.AttackDamage ApplyTacticalBonusFromEachPassive(DamageCalculator.AttackDamage attackDamage, Unit caster)
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

	public virtual void triggerActiveSkillDamageApplied(Unit yeong)
	{
	}
}
}
