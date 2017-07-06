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

	public virtual float GetAdditionalRelativePowerBonus(Unit caster)
	{
		return 1.0f;
	}

	public virtual float GetAdditionalAbsoluteDefenseBonus(Unit caster)
	{
		return 0;
	}

	public virtual float GetAdditionalAbsoluteResistanceBonus(Unit caster)
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
    public virtual float GetStatusEffectVar(StatusEffect statusEffect, int i, Unit caster, Unit owner) {    //statusEffectï¿½ï¿½ iï¿½ï¿½Â° actualElement ï¿½ï¿½ seVar ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½.
        return 0;
    }
    public virtual float ApplyAdditionalRecoverHealthDuringRest(Unit caster, float baseAmount) {
        return baseAmount;
    }

	public virtual void TriggerOnEvasionEvent(BattleData battleData, Unit caster, Unit target)
	{
	}

	public virtual void TriggerOffensiveActiveSkillApplied(Unit caster)
	{		
	}

	public virtual void TriggerActiveSkillDamageApplied(Unit caster, Unit target)
	{
	}
    
    public virtual void TriggerDamaged(Unit target, int damage, Unit caster) 
    {        
    }

    public virtual bool TriggerStatusEffectApplied(StatusEffect statusEffect, Unit caster, Unit target) //unitï¿½ï¿½ï¿½ï¿½ StatusEffectï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿?ï¿½ï¿½ ï¿½ßµï¿½. falseï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ ï¿½Ø´ï¿½ StatusEffectï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½.
    {
        return true;
    }

    public virtual bool TriggerStatusEffectRemoved(StatusEffect statusEffect, Unit unit) //unitï¿½ï¿½ StatusEffectï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿?ï¿½ï¿½ ï¿½ßµï¿½. falseï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ ï¿½Ø´ï¿½ StatusEffectï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿?ï¿½ï¿½ï¿½ï¿½.
    {
        return true;
    }
    public virtual void TriggerUsingSkill(Unit caster, List<Unit> targets) {
    }
    public virtual IEnumerator TriggerApplyingHeal(SkillInstanceData skillInstanceData) {
        yield return null;
    }

    public virtual void TriggerOnStart(Unit caster) {		
	}

	public virtual IEnumerator TriggerOnPhaseStart(Unit caster) {
        yield return null;
	}
    
    public virtual void TriggerOnPhaseEnd(Unit caster) {
    }    

    public virtual void TriggerOnActionEnd(Unit caster) {    
    }
    
    public virtual bool TriggerStatusEffectsAtActionEnd(Unit target, StatusEffect statusEffect) {   //falseï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ ï¿½Ø´ï¿½ statusEffectï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿?
        return true;
    }
	public virtual void TriggerOnRest(Unit caster) {
    }
    public virtual void TriggerOnTurnStart(Unit caster, Unit turnStarter) {
    }
    public virtual void TriggerStatusEffectsOnRest(Unit target, StatusEffect statusEffect) {
    }
    public virtual void TriggerStatusEffectsOnUsingSkill(Unit target, List<Unit> targetsOfSkill, StatusEffect statusEffect) {
    }
}
}
