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
	{;
		foreach (var skill in passiveSkills)
		{
			attackDamage = skill.ApplyBonusDamageFromEachPassive(attackDamage, caster, appliedSkill, target, targetCount);
		}

		return attackDamage;
	}

	public override float GetAdditionalPowerBonus(Unit caster)
	{
		float additionalPowerBonus = 1.0f;
		foreach (var skill in passiveSkills)
		{
			additionalPowerBonus *= skill.GetAdditionalPowerBonus(caster);
		}

		return additionalPowerBonus;
	}

	public override float ApplyIgnoreDefenceByEachPassive(float defense, Unit caster, Unit target)
	{
		foreach (var skill in passiveSkills)
		{
			defense = skill.ApplyIgnoreDefenceByEachPassive(defense, caster, target);
		}
		return defense;
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
