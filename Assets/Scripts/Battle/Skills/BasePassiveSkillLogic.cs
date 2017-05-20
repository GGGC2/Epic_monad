using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;

namespace Battle.Skills
{
public class BasePassiveSkillLogic
{
	public PassiveSkill passiveSkill;

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

	public virtual float ApplyIgnoreDefenceRelativeValueByEachPassive(SkillInstanceData skillInstanceData, float defense)
	{
		return defense;
	}

	public virtual float ApplyIgnoreDefenceAbsoluteValueByEachPassive(SkillInstanceData skillInstanceData, float defense)
	{
		return defense;
	}

	public virtual float ApplyIgnoreResistanceRelativeValueByEachPassive(SkillInstanceData skillInstanceData, float resistance)
	{
		return resistance;
	}

	public virtual float ApplyIgnoreResistanceAbsoluteValueByEachPassive(SkillInstanceData skillInstanceData, float resistance)
	{
		return resistance;
	}

	public virtual void ApplyBonusDamageFromEachPassive(SkillInstanceData skillInstanceData)
	{
	}

	public virtual void ApplyTacticalBonusFromEachPassive(SkillInstanceData skillInstanceData)
	{
	}
	
	public virtual int GetEvasionChance()
	{
		return 0;
	}

	public virtual void TriggerEvasionEvent(BattleData battleData, Unit caster, Unit target)
	{
	}

	public virtual void TriggerOffensiveActiveSkillApplied(Unit caster)
	{		
	}

	public virtual void TriggerActiveSkillDamageApplied(Unit caster, Unit target)
	{
	}

	public virtual void TriggerUsingSkill(Unit caster)
	{		
	}

    public virtual void TriggerDamaged(Unit target, int damage, Unit caster) 
    {        
    }

    public virtual bool TriggerStatusEffectApplied(StatusEffect statusEffect, Unit caster, Unit target) //unit���� StatusEffect�� ����� �� �ߵ�. false�� ������ �� �ش� StatusEffect�� ����.
    {
        return true;
    }

    public virtual bool TriggerStatusEffectRemoved(StatusEffect statusEffect, Unit unit) //unit�� StatusEffect�� ����� �� �ߵ�. false�� ������ �� �ش� StatusEffect�� ������� ����.
    {
        return true;
    }
    public virtual void TriggerStart(Unit caster) 
	{		
	}

	public virtual void TriggerOnPhaseStart(Unit caster) 
	{		
	}
    
    public virtual void TriggerOnPhaseEnd(Unit caster) 
    {
    }    

    public virtual void TriggerActionEnd(Unit caster) 
	{    
    }
    
    public virtual IEnumerator TriggerStatusEffectsAtActionEnd(Unit target, StatusEffect statusEffect) {
        return null;
    }
	public virtual void TriggerRest(Unit caster)
	{

	}
}
}
