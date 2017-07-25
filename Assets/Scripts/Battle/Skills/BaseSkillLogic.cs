using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;

namespace Battle.Skills
{
public class BaseSkillLogic
{
    public Skill skill;
	public virtual int CalculateAP(int originAP, Unit caster)
	{
		return originAP;
	}

	public virtual void OnKill(HitInfo hitInfo)
	{		
	}

	public virtual float ApplyIgnoreDefenceRelativeValueBySkill(float defense, Unit caster, Unit target)
	{
		return defense;
	}

	public virtual float ApplyIgnoreDefenceAbsoluteValueBySkill(float defense, Unit caster, Unit target)
	{
		return defense;
	}

	public virtual float ApplyIgnoreResistanceRelativeValueBySkill(float resistance, Unit caster, Unit target)
	{
		return resistance;
	}

	public virtual float ApplyIgnoreResistanceAbsoluteValueBySkill(float resistance, Unit caster, Unit target)
	{
		return resistance;
	}

	public virtual int CalculateAP(BattleData battleData, List<Tile> selectedTiles)
	{
		int requireAP = battleData.selectedUnit.GetActualRequireSkillAP(battleData.SelectedSkill);
		return requireAP;
	}

	public virtual void ApplyAdditionalDamage(SkillInstanceData skillInstanceData)
	{
	}

	public virtual DamageCalculator.AttackDamage GetAdditionalSkillOption(SkillInstanceData skillInstanceData)
	{
		return skillInstanceData.GetDamage();
	}

	public virtual IEnumerator ActionInDamageRoutine(SkillInstanceData skillInstanceData)
	{
        yield return null;
	}
    public virtual bool CheckApplyPossible(Unit caster, List<Tile> targetTiles) {
        return true;
    }
    public virtual bool IgnoreShield(SkillInstanceData skillInstanceData) {
        return false;
    }
    public virtual float GetStatusEffectVar(StatusEffect statusEffect, int i, Unit caster, Unit owner) {    //statusEffect의 i번째 actualElement 의 seVar 값을 구함.
        return 0;
    }
    public virtual bool TriggerStatusEffectApplied(StatusEffect statusEffect, Unit caster, Unit target, List<Tile> targetTiles) //StatusEffect가 적용될 때 발동. false를 반환할 경우 해당 StatusEffect가 적용되지 않음
    {
        return true;
    }
    public virtual bool TriggerStatusEffectRemoved(StatusEffect statusEffect, Unit target) //unit의 StatusEffect가 사라질 때 발동. false를 리턴할 시 해당 StatusEffect가 사라지지 않음.
    {
        return true;
    }
    public virtual void TriggerStatusEffectAtPhaseStart(Unit target, StatusEffect statusEffect) {
    }
    public virtual IEnumerator TriggerStatusEffectAtActionEnd(Unit target, StatusEffect statusEffect) {
        yield return null;
    }
    public virtual IEnumerator TriggerStatusEffectAtReflection(Unit target, StatusEffect statusEffect, Unit reflectTarget) {
        yield return null;
    }
    public virtual bool TriggerStatusEffectWhenStatusEffectApplied(Unit target, StatusEffect statusEffect, StatusEffect appliedStatusEffect) {
        return true;    //false를 리턴할 경우 appliedStatusEffect를 무시한다.
    }
    public virtual bool TriggerTileStatusEffectApplied(TileStatusEffect tileStatusEffect, Unit caster, Tile targetTile) {
        return true;
    }
    public virtual void TriggerTileStatusEffectAtActionEnd(Tile tile, TileStatusEffect tileStatusEffect) {
    }
    public virtual bool TriggerTileStatusEffectRemoved(Tile tile, TileStatusEffect tileStatusEffect){
        return true;
    }
    public virtual bool TriggerTileStatusEffectWhenUnitTryToChain(Tile tile, TileStatusEffect tileStatusEffect) {
        return true;    //false를 리턴할 경우 해당 타일 위의 유닛은 연계 대기를 할 수 없다.
    }
    public virtual bool TriggerTileStatusEffectWhenUnitTryToUseSkill(Tile tile, TileStatusEffect tileStatusEffect) {
        return true;    //false를 리턴할 경우 해당 타일 위의 유닛은 스킬을 사용할 수 없다.
    }
    public virtual bool TriggerTileStatusEffectWhenStatusEffectAppliedToUnit(SkillInstanceData skillInstanceData, Tile tile, TileStatusEffect tileStatusEffect) {
        return true;    //false를 리턴할 경우 해당 타일 위의 유닛에게 적용되는 statusEffect는 무시된다.
    }
    public virtual void TriggerTileStatusEffectAtTurnStart(Unit turnStarter, Tile tile, TileStatusEffect tileStatusEffect) {
    }
    public virtual void TriggerTileStatusEffectAtTurnEnd(Unit turnEnder, Tile tile, TileStatusEffect tileStatusEffect) {
    }
    public virtual IEnumerator TriggerShieldAttacked(Unit target, float amount) {
        yield return null;
    }
    public static List<Tile> GetTilesInFirstRange(BattleData battleData, Direction? direction = null)
	{
		Direction realDirection;
		if (direction.HasValue) {
			realDirection = direction.Value;
		} else {
			realDirection = battleData.selectedUnit.GetDirection();
		}
		var firstRange = battleData.tileManager.GetTilesInRange(battleData.SelectedSkill.GetFirstRangeForm(),
															battleData.selectedUnit.GetPosition(),
															battleData.SelectedSkill.GetFirstMinReach(),
															battleData.SelectedSkill.GetFirstMaxReach(),
															battleData.SelectedSkill.GetFirstWidth(),
															realDirection);

		return firstRange;
	}
}
}