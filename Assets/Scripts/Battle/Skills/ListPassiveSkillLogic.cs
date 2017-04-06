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

	public override void ApplyStatusEffectByKill(Unit caster)
	{
		foreach (var skill in passiveSkills)
		{
			skill.ApplyStatusEffectByKill(caster);
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
	{;
		foreach (var skill in passiveSkills)
		{
			attackDamage = skill.ApplyBonusDamageFromEachPassive(attackDamage, caster, appliedSkill, target, targetCount);
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

	public override void triggerActiveSkillDamageApplied(Unit yeong)
	{
		foreach (var skillLogic in passiveSkills)
		{
			skillLogic.triggerActiveSkillDamageApplied(yeong);
		}
	}
    public override void triggerDamaged(Unit unit, int damage) {
        foreach (var skillLogic in passiveSkills) {
            skillLogic.triggerDamaged(unit, damage);
        }
    }
    }
}
