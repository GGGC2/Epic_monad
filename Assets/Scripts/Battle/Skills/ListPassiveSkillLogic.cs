using System.Collections.Generic;

namespace Battle.Skills
{
public class ListPassiveSkillLogic : BasePassiveSkillLogic
{
	List<BasePassiveSkillLogic> passiveSkills;

	public ListPassiveSkillLogic(List<BasePassiveSkillLogic> passiveSkills)
	{
		this.passiveSkills = passiveSkills;
	}

	public override void ApplyStatusEffectByKill(HitInfo hitInfo, Unit deadUnit)
	{
		foreach (var skill in passiveSkills)
		{
			skill.ApplyStatusEffectByKill(hitInfo, deadUnit);
		}
	}

	public override bool checkEvade()
	{
		foreach (var skill in passiveSkills)
		{
			if (skill.checkEvade())
			{
				return true;
			}
		}

		return false;
	}

	public override void triggerEvasionEvent(BattleData battleData, Unit unit)
	{
		foreach (var skill in passiveSkills)
		{
			skill.triggerEvasionEvent(battleData, unit);
		}
	}

	public override DamageCalculator.AttackDamage ApplyBonusDamageFromEachPassive(DamageCalculator.AttackDamage attackDamage, Unit caster, Skill appliedSkill, Unit target, int targetCount)
	{
		foreach (var skill in passiveSkills)
		{
			attackDamage = skill.ApplyBonusDamageFromEachPassive(attackDamage, caster, appliedSkill, target, targetCount);
		}

		return attackDamage;
	}

	public override DamageCalculator.AttackDamage ApplyTacticalBonusFromEachPassive(DamageCalculator.AttackDamage attackDamage, Unit caster, Unit target)
	{
		foreach (var skill in passiveSkills)
		{
			attackDamage = skill.ApplyTacticalBonusFromEachPassive(attackDamage, caster, target);
		}

		return attackDamage;
	}

	public override float GetAdditionalRelativePowerBonus(Unit caster)
	{
		float totalAdditionalPowerBonus = 1.0f;
		foreach (var skill in passiveSkills)
		{
			totalAdditionalPowerBonus *= skill.GetAdditionalRelativePowerBonus(caster);
		}

		return totalAdditionalPowerBonus;
	}

	public override float GetAdditionalAbsoluteDefenseBonus(Unit caster)
	{
		float totalAdditionalDefenseBonus = 0;
		foreach (var skill in passiveSkills)
		{
			totalAdditionalDefenseBonus += skill.GetAdditionalAbsoluteDefenseBonus(caster);
		}
		return totalAdditionalDefenseBonus;
	}

	public override float ApplyIgnoreDefenceRelativeValueByEachPassive(float defense, Unit caster, Unit target)
	{
		foreach (var skill in passiveSkills)
		{
			defense = skill.ApplyIgnoreDefenceRelativeValueByEachPassive(defense, caster, target);
		}
		return defense;
	}

	public override float ApplyIgnoreDefenceAbsoluteValueByEachPassive(float defense, Unit caster, Unit target)
	{
		foreach (var skill in passiveSkills)
		{
			defense = skill.ApplyIgnoreDefenceAbsoluteValueByEachPassive(defense, caster, target);
		}
		return defense;
	}

	public override float ApplyIgnoreResistanceRelativeValueByEachPassive(float resistance, Unit caster, Unit target)
	{
		foreach (var skill in passiveSkills)
		{
			resistance = skill.ApplyIgnoreDefenceRelativeValueByEachPassive(resistance, caster, target);
		}
		return resistance;
	}

	public override float ApplyIgnoreResistanceAbsoluteValueByEachPassive(float resistance, Unit caster, Unit target)
	{
		foreach (var skill in passiveSkills)
		{
			resistance = skill.ApplyIgnoreDefenceAbsoluteValueByEachPassive(resistance, caster, target);
		}
		return resistance;
	}

	public override void triggerActiveSkillDamageApplied(Unit yeong)
	{
		foreach (var skillLogic in passiveSkills)
		{
			skillLogic.triggerActiveSkillDamageApplied(yeong);
		}
	}
}
}
