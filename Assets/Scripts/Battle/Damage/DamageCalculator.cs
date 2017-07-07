﻿using System;
using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;
using Battle.Skills;

namespace Battle
{
public class DamageCalculator
{
    public class AttackDamage {
        public float baseDamage = 0;
        public float relativeDamageBonus = 1.0f;
        public float absoluteDamageBonus = 0;
        public DirectionCategory attackDirection = DirectionCategory.Front;
        public float directionBonus = 1.0f;
        public float celestialBonus = 1.0f;
        public float heightBonus = 1.0f;
        public float chainBonus = 1.0f;
        public float smiteAmount = 0;
        public float resultDamage = 0;
    }

    public class DamageInfo
	{
		public List<Unit> casters;
		public float damage;

		public DamageInfo(Unit caster, float damage)
		{
			this.casters = new List<Unit>();
			casters.Add(caster);
			this.damage = damage;
		}
	}

	public static Dictionary<Unit, DamageInfo> CalculateTotalDamage(BattleData battleData, Tile centerTile, List<Tile> tilesInSkillRange, List<Tile> firstRange)
	{
		Dictionary<Unit, DamageInfo> damageList = new Dictionary<Unit, DamageInfo>();

		ChainList.AddChains(battleData.selectedUnit, centerTile, tilesInSkillRange, battleData.SelectedSkill, firstRange);

		List<ChainInfo> allVaildChainInfo = ChainList.GetAllChainInfoToTargetArea(battleData.selectedUnit, tilesInSkillRange);
		int chainCombo = allVaildChainInfo.Count;

		foreach (var chainInfo in allVaildChainInfo)
		{
			var damageListOfEachSkill = CalculateDamageOfEachSkill(chainInfo, chainCombo);
			damageList = MergeDamageList(damageList, damageListOfEachSkill);
		}

		ChainList.RemoveChainsFromUnit(battleData.selectedUnit);

		return damageList;
	}

	private static Dictionary<Unit, DamageInfo> MergeDamageList(Dictionary<Unit, DamageInfo> leftDamgeList, Dictionary<Unit, DamageInfo> rightDamageList)
	{
		var merged = new Dictionary<Unit, DamageInfo>();
		foreach (var damage in leftDamgeList)
		{
			var target = damage.Key;
			merged[target] = damage.Value;
		}
		foreach (var damage in rightDamageList)
		{
			var target = damage.Key;
			if (merged.ContainsKey(target))
			{
				foreach (var caster in rightDamageList[target].casters)
					merged[target].casters.Add(caster);
				merged[target].damage += rightDamageList[target].damage;
			}
			else
			{
				merged[target] = rightDamageList[target];
			}
		}
		return merged;
	}

	private static Dictionary<Unit, DamageInfo> CalculateDamageOfEachSkill(ChainInfo chainInfo, int chainCombo)
	{
		var damageList = new Dictionary<Unit, DamageInfo>();
		Skill appliedSkill = chainInfo.GetSkill();
		if (appliedSkill.GetSkillApplyType() != SkillApplyType.DamageHealth) {
			return damageList;
		}
        
		Unit caster = chainInfo.GetUnit();			
		List<Tile> selectedTiles = chainInfo.GetTargetArea();

		Direction oldDirection = caster.GetDirection();

		// 시전 방향으로 유닛의 바라보는 방향을 돌림.
		if (appliedSkill.GetSkillType() != SkillType.Auto)
			caster.SetDirection(Utility.GetDirectionToTarget(caster, selectedTiles));

		List<Unit> targets = GetTargetUnits(selectedTiles);

		foreach (var target in targets)
		{
            SkillInstanceData skillInstanceData = new SkillInstanceData(new AttackDamage(), appliedSkill, caster, selectedTiles, target, targets.Count);
			CalculateAttackDamage(skillInstanceData, chainCombo);

            float actualDamage = skillInstanceData.GetDamage().resultDamage;
            float reflectDamage = CalculateReflectDamage(actualDamage, target, caster, caster.GetUnitClass());
            actualDamage -= reflectDamage;

            float targetDefense = CalculateDefense(appliedSkill, target, caster);
            float targetResistance = CalculateResistance(appliedSkill, target, caster);
            actualDamage = ApplyDefenseAndResistance(actualDamage, caster.GetUnitClass(), targetDefense, targetResistance);
            
            float reflectTargetDefense = CalculateDefense(appliedSkill, caster, target);
            float reflectTargetResistance = CalculateDefense(appliedSkill, caster, target);
            reflectDamage = ApplyDefenseAndResistance(reflectDamage, target.GetUnitClass(), reflectTargetDefense, reflectTargetResistance);

			DamageInfo damageInfo = new DamageInfo(caster, actualDamage);
            DamageInfo reflectDamageInfo = new DamageInfo(target, reflectDamage);
			damageList.Add(target, damageInfo);
            damageList.Add(caster, reflectDamageInfo);

			Debug.Log("Apply " + actualDamage + " damage to " + target.GetName() + "\n" +
						"ChainCombo : " + chainCombo);
		}

		caster.SetDirection(oldDirection);
		return damageList;
	}

	private static List<Unit> GetTargetUnits(List<Tile> targetTiles) {
		var targets = new List<Unit>();
		foreach (var tile in targetTiles)
		{
			if (tile.IsUnitOnTile())
			{
				targets.Add(tile.GetUnitOnTile());
			}
		}
		return targets;
	}

    public static void CalculateAmountOtherThanAttackDamage(SkillInstanceData skillInstanceData) {
        Unit caster = skillInstanceData.GetCaster();
        Unit target = skillInstanceData.GetMainTarget();
        AttackDamage attackDamage = skillInstanceData.GetDamage();
        Skill appliedSkill = skillInstanceData.GetSkill();

        attackDamage.baseDamage = PowerFactorDamage(appliedSkill, caster);
        // 해당 기술의 추가데미지 계산
        Debug.LogWarning("Apply Additional Amount from" + appliedSkill.GetName());
        SkillLogicFactory.Get(appliedSkill).ApplyAdditionalDamage(skillInstanceData);
        // 특성에 의한 추가데미지
        List<PassiveSkill> passiveSkills = caster.GetLearnedPassiveSkillList();
        SkillLogicFactory.Get(passiveSkills).ApplyBonusDamageFromEachPassive(skillInstanceData);

        attackDamage.resultDamage = attackDamage.baseDamage * attackDamage.relativeDamageBonus;
        Debug.Log("resultAmount : " + attackDamage.resultDamage);
    }
	public static void CalculateAttackDamage(SkillInstanceData skillInstanceData, int chainCombo)
	{
        Unit caster = skillInstanceData.GetCaster();
        Unit target = skillInstanceData.GetMainTarget();
        AttackDamage attackDamage = skillInstanceData.GetDamage();
        Skill appliedSkill = skillInstanceData.GetSkill();

		attackDamage.baseDamage = PowerFactorDamage(appliedSkill, caster);
		attackDamage.directionBonus = DirectionBonus(caster, target);
		attackDamage.attackDirection = AttackDirection(caster, target);
		attackDamage.celestialBonus = CelestialBonus(caster, target);
		attackDamage.heightBonus = HeightBonus(caster, target);
		attackDamage.chainBonus = ChainComboBonus(chainCombo);
		attackDamage.smiteAmount = SmiteAmount(caster);

        if(caster.GetElement() != Element.None){
            StatusEffectType casterElementWeakness = EnumConverter.GetCorrespondingStatusEffectType(caster.GetElement());
            if (target.HasStatusEffect(casterElementWeakness)){
                attackDamage.relativeDamageBonus *= target.CalculateActualAmount(1, casterElementWeakness);
            }
        }

        // 해당 기술의 추가데미지 계산
        Debug.LogWarning("Apply Additional Damage from" + appliedSkill.GetName());
        SkillLogicFactory.Get(appliedSkill).ApplyAdditionalDamage(skillInstanceData);
		// 특성에 의한 추가데미지
		List<PassiveSkill> passiveSkills = caster.GetLearnedPassiveSkillList();
		SkillLogicFactory.Get(passiveSkills).ApplyBonusDamageFromEachPassive(skillInstanceData);
		// 시전자 효과에 의한 추가데미지
		attackDamage.baseDamage = caster.CalculateActualAmount(attackDamage.baseDamage, StatusEffectType.DamageChange);
		// 특성에 의한 전략보너스 추가
		SkillLogicFactory.Get(passiveSkills).ApplyTacticalBonusFromEachPassive(skillInstanceData);

		// '지형지물'은 방향 보너스를 받지 않음
		if (target.IsObject()) attackDamage.directionBonus = 1.0f;

		attackDamage.resultDamage = ((attackDamage.baseDamage + attackDamage.smiteAmount)
									* attackDamage.relativeDamageBonus + attackDamage.absoluteDamageBonus) 
									* attackDamage.directionBonus
									* attackDamage.celestialBonus
									* attackDamage.heightBonus
									* attackDamage.chainBonus;

		Debug.Log("resultDamage : " + attackDamage.resultDamage);
	}

	private static float PowerFactorDamage(Skill appliedSkill, Unit casterUnit)
	{
		float damage = 0;
		
		float powerFactor = appliedSkill.GetPowerFactor(Stat.Power);
		float powerStat = casterUnit.GetStat(Stat.Power);

		damage = powerFactor * powerStat;

		Debug.Log("baseDamage : " + damage);

		return damage;
	}

	private static float DirectionBonus(Unit caster, Unit target) {
		float directionBonus = Utility.GetDirectionBonus(caster, target);
		Debug.Log("directionBonus : " + directionBonus);
		return directionBonus;
	}

	private static DirectionCategory AttackDirection(Unit caster, Unit target)
	{
		float directionBonus = Utility.GetDirectionBonus(caster, target);
		Debug.Log("directionBonus : " + directionBonus);
		if (directionBonus == 1.1f)
			return DirectionCategory.Side;
		else if (directionBonus == 1.25f)
			return DirectionCategory.Back;
		else
			return DirectionCategory.Front; 
	}

	private static float CelestialBonus(Unit caster, Unit target) {
		float celestialBonus = Utility.GetCelestialBonus(caster, target);
		Debug.Log("celestialBonus : " + celestialBonus);
		return celestialBonus;
	}

	private static float HeightBonus(Unit caster, Unit target) {
		float heightBonus = Utility.GetHeightBonus(caster, target);
		Debug.Log("heightBonus : " + heightBonus);
		return heightBonus;
	}

	private static float ChainComboBonus(int chainCombo) {
		float chainBonus = GetChainDamageFactorFromChainCombo(chainCombo);
		Debug.Log("chainBonus : " + chainBonus);
		return chainBonus;
	}

	public static float GetChainDamageFactorFromChainCombo(int chainCombo)
	{
		if (chainCombo < 2)	return 1.0f;
		else if (chainCombo == 2) return 1.2f;
		else if (chainCombo == 3) return 1.5f;
		else if (chainCombo == 4) return 2.0f;
		else return 3.0f;  
	}

	private static float SmiteAmount(Unit casterUnit) {
		float smiteAmount = 0;
		smiteAmount = casterUnit.CalculateActualAmount(smiteAmount, StatusEffectType.Smite);
		Debug.Log("smiteAmount : " + smiteAmount);
		return smiteAmount;
	}
    
	public static float CalculateReflectDamage(float attackDamage, Unit target, Unit reflectTarget, UnitClass damageType)
	{
		float reflectAmount = 0;
		foreach (var statusEffect in target.GetStatusEffectList())
		{
            bool canReflect = statusEffect.IsOfType(StatusEffectType.Reflect) ||
                                (statusEffect.IsOfType(StatusEffectType.MagicReflect) && damageType == UnitClass.Magic) ||
                                (statusEffect.IsOfType(StatusEffectType.MeleeReflect) && damageType == UnitClass.Melee);
			if (canReflect)
			{
                float reflectPercent = statusEffect.GetAmountOfType(StatusEffectType.Reflect);
                if(damageType == UnitClass.Magic) reflectPercent += statusEffect.GetAmountOfType(StatusEffectType.MagicReflect);
                if(damageType == UnitClass.Melee) reflectPercent += statusEffect.GetAmountOfType(StatusEffectType.MeleeReflect);
                reflectAmount += attackDamage * reflectPercent/100;
			}
		}
		return reflectAmount;
	}
    
    public static float ApplyDefenseAndResistance(float damage, UnitClass damageType, float defense, float resistance) {
        if (damageType == UnitClass.Melee) {
            // 실제 피해 = 원래 피해 x 200/(200+방어력)
            // 방어력이 -180 이하일 시 -180으로 적용
            if (defense <= -180) damage = damage * 10;
            else damage = damage * 200.0f / (200.0f + defense);
            Debug.Log("Actual melee damage without status effect : " + damage);
        } else if (damageType == UnitClass.Magic) {
            if (resistance <= -180) damage = damage * 10;
            else damage = damage * 200.0f / (200.0f + resistance);
            Debug.Log("Actual magic damage without status effect: " + damage);
        }
        return damage;
    }
	public static float CalculateDefense(Skill appliedSkill, Unit target, Unit caster) {
		float defense = target.GetStat(Stat.Defense);
			
		// 기술에 의한 방어 무시 (상대값)
		defense = SkillLogicFactory.Get(appliedSkill).ApplyIgnoreDefenceRelativeValueBySkill(defense, caster, target);
			
		// 특성에 의한 방어 무시 (상대값)
		List<PassiveSkill> casterPassiveSkills = caster.GetLearnedPassiveSkillList();
        defense = SkillLogicFactory.Get(casterPassiveSkills).ApplyIgnoreDefenceRelativeValueByEachPassive(appliedSkill, target, caster, defense); 
            
		// 기술에 의한 방어 무시 (절대값)
		defense = SkillLogicFactory.Get(appliedSkill).ApplyIgnoreDefenceAbsoluteValueBySkill(defense, caster, target);
			
		// 특성에 의한 방어 무시 (절대값)
		defense = SkillLogicFactory.Get(casterPassiveSkills).ApplyIgnoreDefenceAbsoluteValueByEachPassive(appliedSkill, target, caster, defense);
		return defense;
	}
    public static float CalculateResistance(Skill appliedSkill, Unit target, Unit caster) {
        float resistance = target.GetStat(Stat.Resistance);

        // 기술에 의한 저항 무시 (상대값)
        resistance = SkillLogicFactory.Get(appliedSkill).ApplyIgnoreResistanceRelativeValueBySkill(resistance, caster, target);

        // 특성에 의한 저항 무시 (상대값)
        List<PassiveSkill> casterPassiveSkills = caster.GetLearnedPassiveSkillList();
        resistance = SkillLogicFactory.Get(casterPassiveSkills).ApplyIgnoreResistanceRelativeValueByEachPassive(appliedSkill, target, caster, resistance);

        // 기술에 의한 저항 무시 (절대값)
        resistance = SkillLogicFactory.Get(appliedSkill).ApplyIgnoreResistanceAbsoluteValueBySkill(resistance, caster, target);

        // 특성에 의한 저항 무시 (절대값)
        resistance = SkillLogicFactory.Get(casterPassiveSkills).ApplyIgnoreResistanceAbsoluteValueByEachPassive(appliedSkill, target, caster, resistance);
        return resistance;
    }
    }
}
