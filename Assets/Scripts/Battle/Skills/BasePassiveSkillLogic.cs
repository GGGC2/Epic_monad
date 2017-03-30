using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;

namespace Battle.Skills
{
public class BasePassiveSkillLogic
{
	public PassiveSkill passiveSkill = null;

	public virtual void ApplyStatusEffectByKill(HitInfo hitInfo, Unit deadUnit)
	{
	}

	public virtual void SetAmountToEachStatusEffect(List<StatusEffect> statusEffects, Unit caster, Unit target)
	{
	}

	public virtual float GetAdditionalRelativePowerBonus(Unit caster)
	{
		return 1.0f;
	}

	public virtual float GetAdditionalAbsoluteDefenseBonus(Unit caster)
	{
		return 0;
	}

	public virtual float ApplyIgnoreDefenceRelativeValueByEachPassive(float defense, Unit caster, Unit target)
	{
		return defense;
	}

	public virtual float ApplyIgnoreDefenceAbsoluteValueByEachPassive(float defense, Unit caster, Unit target)
	{
		return defense;
	}

	public virtual float ApplyIgnoreResistanceRelativeValueByEachPassive(float resistance, Unit caster, Unit target)
	{
		return resistance;
	}

	public virtual float ApplyIgnoreResistanceAbsoluteValueByEachPassive(float resistance, Unit caster, Unit target)
	{
		return resistance;
	}

	public virtual DamageCalculator.AttackDamage ApplyBonusDamageFromEachPassive(DamageCalculator.AttackDamage attackDamage, Unit caster, Skill appliedSkill, Unit target, int targetCount)
	{
		return attackDamage;
	}

	public virtual DamageCalculator.AttackDamage ApplyTacticalBonusFromEachPassive(DamageCalculator.AttackDamage attackDamage, Unit caster, Unit target)
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
