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

	public override DamageCalculator.AttackDamage ApplyBonusDamageFromEachPassive(DamageCalculator.AttackDamage attackDamage, Skill appliedSkill, Unit target, int targetCount)
	{;
		foreach (var skill in passiveSkills)
		{
			attackDamage = skill.ApplyBonusDamageFromEachPassive(attackDamage, appliedSkill, target, targetCount);
		}

		return attackDamage;
	}

	public override float GetAdditionalPowerBouns(Unit caster)
	{
		float additionalPowerBouns = 1.0f;
		foreach (var skill in passiveSkills)
		{
			additionalPowerBouns *= skill.GetAdditionalPowerBouns(caster);
		}

		return additionalPowerBouns;
	}
}
}