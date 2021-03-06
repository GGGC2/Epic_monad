using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;

namespace Battle.Skills
{
public class BasePassiveSkillLogic
{
	public PassiveSkill passiveSkill;

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

	public virtual float ApplyIgnoreDefenceRelativeValueByEachPassive(ActiveSkill appliedSkill, Unit target, Unit caster, float defense)
	{
		return defense;
	}

	public virtual float ApplyIgnoreDefenceAbsoluteValueByEachPassive(ActiveSkill appliedSkill, Unit target, Unit caster, float defense)
	{
		return defense;
	}

	public virtual float ApplyIgnoreResistanceRelativeValueByEachPassive(ActiveSkill appliedSkill, Unit target, Unit caster, float resistance)
	{
		return resistance;
	}

	public virtual float ApplyIgnoreResistanceAbsoluteValueByEachPassive(ActiveSkill appliedSkill, Unit target, Unit caster, float resistance)
	{
		return resistance;
	}

	public virtual void ApplyBonusDamageFromEachPassive(CastingApply castingApply){
	}

    public virtual void ApplyTacticalBonusFromEachPassive(CastingApply castingApply){
    }

    public virtual void ApplyAdditionalDamageFromCasterStatusEffect(CastingApply castingApply, StatusEffect statusEffect) {
    }

    public virtual void ActionInDamageRoutine(CastingApply castingApply) {
    }
    public virtual float GetStatusEffectVar(UnitStatusEffect statusEffect, int i, Unit caster, Unit owner) {    //statusEffect�� i��° actualElement �� seVar ���� ����.
        return 0;
    }
    public virtual string GetStatusEffectExplanation(StatusEffect statusEffect) {
        return "";
    }
    public virtual float ApplyAdditionalRecoverHealthDuringRest(Unit caster, float baseAmount) {
        return baseAmount;
    }
    
    public virtual void TriggerOnKill(HitInfo hitInfo, Unit deadUnit) {
    }

    public virtual void TriggerOnUnitDestroy(Unit unit, Unit destroyedUnit, TrigActionType actionType) {
        
    }
    public virtual void TriggerOnEvasionEvent(Unit caster, Unit target)
	{
	}

	public virtual void TriggerOffensiveActiveSkillApplied(Unit caster)
	{		
	}

	public virtual void TriggerActiveSkillDamageApplied(Unit caster, Unit target)
	{
	}

    public virtual bool TriggerDamagedByCasting(Unit caster, Unit target, float damage) {
        return true;
    }
    
    public virtual bool TriggerDamagedByNonCasting(Unit caster, float damage, Unit target, bool isSourceTrap) {
        return true;
    }
    public virtual void TriggerAfterDamaged(Unit target, int damage, Unit caster) 
    {        
    }
    public virtual void TriggerAfterMove(Unit caster, Tile beforeTile, Tile afterTile) {
    }

    public virtual bool TriggerStatusEffectApplied(UnitStatusEffect statusEffect, Unit caster, Unit target) {
        return true;
    }

    public virtual bool TriggerStatusEffectAppliedToOwner(UnitStatusEffect statusEffect, Unit caster, Unit target) //unit���� StatusEffect�� �����?�� �ߵ�. false�� ������ �� �ش� StatusEffect�� ����.
    {
        return true;
    }

    public virtual bool TriggerStatusEffectRemoved(UnitStatusEffect statusEffect, Unit unit) //unit�� StatusEffect�� �����?�� �ߵ�. false�� ������ �� �ش� StatusEffect�� �������?����.
    {
        return true;
    }
    public virtual void TriggerUsingSkill(Casting casting, List<Unit> targets) {
    }
    public virtual void TriggerWhenShieldWhoseCasterIsOwnerIsAttacked(Unit attacker, Unit shieldCaster, Unit target, float amount) {
    }
    public virtual void TriggerOnMove(Unit caster) {
    }
    public virtual bool TriggerOnForceMove(Unit caster, Tile tileAfter) {
        return true;
    }
    public virtual void TriggerApplyingHeal(CastingApply castingApply) {
    }

    public virtual void TriggerOnStart(Unit caster) {		
	}

	public virtual void TriggerOnPhaseStart(Unit caster, int phase) {
	}
    
    public virtual void TriggerOnPhaseEnd(Unit caster) {
    }    

    public virtual void TriggerOnActionEnd(Unit caster) {    
    }
	public virtual void TriggerOnRest(Unit caster) {
    }
    public virtual void TriggerOnTurnStart(Unit caster, Unit turnStarter) {
    }
    public virtual void TriggerStatusEffectsOnRest(Unit target, UnitStatusEffect statusEffect) {
    }
    public virtual void TriggerStatusEffectsOnUsingSkill(Unit target, List<Unit> targetsOfSkill, UnitStatusEffect statusEffect) {
    }
    public virtual void TriggerStatusEffectsOnMove(Unit target, UnitStatusEffect statusEffect) {
    }
    public virtual bool TriggerOnSteppingTrap(Unit caster, TileStatusEffect trap) {
        return true;
    }
}
}
