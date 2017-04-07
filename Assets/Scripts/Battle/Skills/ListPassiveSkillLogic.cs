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

	public override void ApplyStatusEffectByKill(Unit caster)
	{
		foreach (var skillLogic in passiveSkillLogics)
		{
			skillLogic.ApplyStatusEffectByKill(caster);
		}
	}

	public override bool checkEvade()
	{
		foreach (var skillLogic in passiveSkillLogics)
		{
			if (skillLogic.checkEvade())
			{
				return true;
			}
		}

		return false;
	}

	public override void triggerEvasionEvent(BattleData battleData, Unit unit)
	{
		foreach (var skillLogic in passiveSkillLogics)
		{
			skillLogic.triggerEvasionEvent(battleData, unit);
		}
	}

	public override void ApplyBonusDamageFromEachPassive(SkillInstanceData skillInstanceData) { 
		foreach (var skillLogic in passiveSkillLogics)
		{
			skillLogic.ApplyBonusDamageFromEachPassive(skillInstanceData);
		}
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

	public override float ApplyIgnoreDefenceRelativeValueByEachPassive(float defense, Unit caster, Unit target)
	{
		foreach (var skillLogic in passiveSkillLogics)
		{
			defense = skillLogic.ApplyIgnoreDefenceRelativeValueByEachPassive(defense, caster, target);
		}
		return defense;
	}

	public override float ApplyIgnoreDefenceAbsoluteValueByEachPassive(float defense, Unit caster, Unit target)
	{
		foreach (var skillLogic in passiveSkillLogics)
		{
			defense = skillLogic.ApplyIgnoreDefenceAbsoluteValueByEachPassive(defense, caster, target);
		}
		return defense;
	}

	public override void triggerActiveSkillDamageApplied(Unit yeong)
	{
		foreach (var skillLogic in passiveSkillLogics)
		{
			skillLogic.triggerActiveSkillDamageApplied(yeong);
		}
	}
    public override void triggerDamaged(Unit unit, int damage) {
        foreach (var skillLogic in passiveSkillLogics) {
            skillLogic.triggerDamaged(unit, damage);
        }
    }
    }
}
