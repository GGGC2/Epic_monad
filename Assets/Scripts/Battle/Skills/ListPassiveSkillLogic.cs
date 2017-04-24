using System.Collections.Generic;

namespace Battle.Skills
{
public class ListPassiveSkillLogic : BasePassiveSkillLogic
{
	List<BasePassiveSkillLogic> passiveSkillLogics;

	public ListPassiveSkillLogic(List<BasePassiveSkillLogic> passiveSkillLogics)
	{
		this.passiveSkillLogics = passiveSkillLogics;
	}

	public override void ApplyStatusEffectByKill(HitInfo hitInfo, Unit deadUnit)
	{
		foreach (var skillLogic in passiveSkillLogics)
		{
			skillLogic.ApplyStatusEffectByKill(hitInfo, deadUnit);
		}
	}

	public override int GetEvasionChance()
	{
		int totalEvasionChance = 0;
		foreach (var skillLogic in passiveSkillLogics)
		{
			totalEvasionChance += skillLogic.GetEvasionChance();
		}

		return totalEvasionChance;
	}

	public override void TriggerEvasionEvent(BattleData battleData, Unit caster, Unit target)
	{
		foreach (var skillLogic in passiveSkillLogics)
		{
			skillLogic.TriggerEvasionEvent(battleData, caster, target);
		}
	}

	public override void TriggerActionEnd(Unit caster)
	{
		foreach (var skillLogic in passiveSkillLogics)
		{
			skillLogic.TriggerActionEnd(caster);
		}
	}

	public override void ApplyBonusDamageFromEachPassive(SkillInstanceData skillInstanceData) { 
		foreach (var skillLogic in passiveSkillLogics)
		{
			skillLogic.ApplyBonusDamageFromEachPassive(skillInstanceData);
		}
	}

	public override DamageCalculator.AttackDamage ApplyTacticalBonusFromEachPassive(DamageCalculator.AttackDamage attackDamage, Unit caster, Unit target)
	{
		foreach (var skill in passiveSkillLogics)
		{
			attackDamage = skill.ApplyTacticalBonusFromEachPassive(attackDamage, caster, target);
		}

		return attackDamage;
	}

	public override float GetAdditionalRelativePowerBonus(Unit caster)
	{
		float totalAdditionalPowerBonus = 1.0f;
		foreach (var skillLogic in passiveSkillLogics)
		{
			totalAdditionalPowerBonus *= skillLogic.GetAdditionalRelativePowerBonus(caster);
		}

		return totalAdditionalPowerBonus;
	}

	public override float GetAdditionalAbsoluteDefenseBonus(Unit caster)
	{
		float totalAdditionalDefenseBonus = 0;
		foreach (var skillLogic in passiveSkillLogics)
		{
			totalAdditionalDefenseBonus += skillLogic.GetAdditionalAbsoluteDefenseBonus(caster);
		}
		return totalAdditionalDefenseBonus;
	}

	public override float ApplyIgnoreDefenceRelativeValueByEachPassive(SkillInstanceData skillInstanceData, float defense)
	{
		foreach (var skillLogic in passiveSkillLogics)
		{
			defense = skillLogic.ApplyIgnoreDefenceRelativeValueByEachPassive(skillInstanceData, defense);
		}
		return defense;
	}

	public override float ApplyIgnoreDefenceAbsoluteValueByEachPassive(SkillInstanceData skillInstanceData, float defense)
	{
		foreach (var skillLogic in passiveSkillLogics)
		{
			defense = skillLogic.ApplyIgnoreDefenceAbsoluteValueByEachPassive(skillInstanceData, defense);
		}
		return defense;
	}

	public override void TriggerActiveSkillDamageApplied(Unit yeong)
	{
		foreach (var skillLogic in passiveSkillLogics)
		{
			skillLogic.TriggerActiveSkillDamageApplied(yeong);
		}
	}
    public override void TriggerDamaged(Unit unit, int damage) {
        foreach (var skillLogic in passiveSkillLogics) {
            skillLogic.TriggerDamaged(unit, damage);
        }
    }
    }
}
