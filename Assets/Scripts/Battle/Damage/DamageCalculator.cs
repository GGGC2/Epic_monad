using System;
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

	public static Dictionary<Unit, DamageInfo> CalculateTotalDamage(BattleData battleData, Tile centerTile, List<GameObject> tilesInSkillRange, List<GameObject> firstRange)
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
        
		Unit casterUnit = chainInfo.GetUnit().GetComponent<Unit>();			
		List<GameObject> selectedTiles = chainInfo.GetTargetArea();

		Direction oldDirection = casterUnit.GetDirection();

		// 시전 방향으로 유닛의 바라보는 방향을 돌림.
		if (appliedSkill.GetSkillType() != SkillType.Auto)
			casterUnit.SetDirection(Utility.GetDirectionToTarget(casterUnit, selectedTiles));

		List<Unit> targets = GetTargetUnits(selectedTiles);

		foreach (var target in targets)
		{
            SkillInstanceData skillInstanceData = new SkillInstanceData(new AttackDamage(), appliedSkill, casterUnit, target, targets.Count);
			CalculateAttackDamage(skillInstanceData, chainCombo);

            float attackDamage = skillInstanceData.getDamage().resultDamage;

			float actualDamage = GetActualDamage(skillInstanceData, false, true);

			DamageInfo damageInfo = new DamageInfo(casterUnit, actualDamage);
			damageList.Add(target, damageInfo);

			Debug.Log("Apply " + actualDamage + " damage to " + target.GetName() + "\n" +
						"ChainCombo : " + chainCombo);
		}

		casterUnit.SetDirection(oldDirection);
		return damageList;
	}

	private static List<Unit> GetTargetUnits(List<GameObject> targetTiles) {
		var targets = new List<Unit>();
		foreach (var tileObject in targetTiles)
		{
			Tile tile = tileObject.GetComponent<Tile>();
			if (tile.IsUnitOnTile())
			{
				targets.Add(tile.GetUnitOnTile());
			}
		}
		return targets;
	}

	public static void CalculateAttackDamage(SkillInstanceData skillInstanceData, int chainCombo)
	{
        Unit caster = skillInstanceData.getCaster();
        Unit target = skillInstanceData.getTarget();
        AttackDamage attackDamage = skillInstanceData.getDamage();
        Skill appliedSkill = skillInstanceData.getSkill();

		attackDamage.baseDamage = PowerFactorDamage(appliedSkill, caster);
		attackDamage.directionBonus = DirectionBonus(caster, target);
		attackDamage.attackDirection = AttackDirection(caster, target);
		attackDamage.celestialBonus = CelestialBonus(caster, target);
		attackDamage.heightBonus = HeightBonus(caster, target);
		attackDamage.chainBonus = ChainComboBonus(chainCombo);
		attackDamage.smiteAmount = SmiteAmount(caster);

		// 해당 기술의 추가데미지 계산
		ApplyBonusDamageFromEachSkill(skillInstanceData);
		// 특성에 의한 추가데미지
		List<PassiveSkill> passiveSkills = caster.GetLearnedPassiveSkillList();
		SkillLogicFactory.Get(passiveSkills).ApplyBonusDamageFromEachPassive(skillInstanceData);
		// 시전자 효과에 의한 추가데미지
		attackDamage.baseDamage = caster.GetActualEffect(attackDamage.baseDamage, StatusEffectType.DamageChange);
		

		attackDamage.resultDamage = (attackDamage.baseDamage
									* attackDamage.relativeDamageBonus
									+ attackDamage.absoluteDamageBonus
									+ attackDamage.smiteAmount) 
									* attackDamage.directionBonus
									* attackDamage.celestialBonus
									* attackDamage.heightBonus
									* attackDamage.chainBonus;
	}

	private static float PowerFactorDamage(Skill appliedSkill, Unit casterUnit)
	{
		float damage = 0;
		
		float powerFactor = appliedSkill.GetPowerFactor(Enums.Stat.Power);
		float powerStat = casterUnit.GetActualStat(Enums.Stat.Power);

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
		smiteAmount = casterUnit.GetActualEffect(smiteAmount, StatusEffectType.Smite);
		Debug.Log("smiteAmount : " + smiteAmount);
		return smiteAmount;
	}

	private static void ApplyBonusDamageFromEachSkill(SkillInstanceData skillInstanceData) {
		Skill appliedSkill = skillInstanceData.getSkill();
        SkillLogicFactory.Get(appliedSkill).ApplyAdditionalDamage(skillInstanceData);
        
	}

	public static float CalculateReflectDamage(float attackDamage, Unit target)
	{
		float reflectAmount = attackDamage;
		foreach (var statusEffect in target.GetStatusEffectList())
		{
			if (statusEffect.IsOfType(StatusEffectType.Reflect))
			{
				reflectAmount = reflectAmount * statusEffect.GetAmount();
				break;
			}
		}
		return reflectAmount;
	}
    
	public static float GetActualDamage(SkillInstanceData skillInstanceData, bool isDot, bool isHealth)
	{
		float actualDamage = skillInstanceData.getDamage().resultDamage;
		int finalDamage = 0; // 최종 대미지 (정수로 표시되는)
        Skill appliedSkill = skillInstanceData.getSkill();
        Unit target = skillInstanceData.getTarget();
        Unit caster = skillInstanceData.getCaster();
		// 대미지 증가/감소 효과 적용
		// 공격이 물리인지 마법인지 체크
		// 기술 / 특성의 추가피해 / 감소 효과
		// 방어력 / 저항력 중 맞는 값을 적용 (적용 단계에서 능력치 변동 효과 반영)
		// 보호막 있을 경우 대미지 삭감
		// 체력 깎임
		// 체인 해제
		if (isHealth == true)
		{
			// 피격자의 효과/특성으로 인한 대미지 증감 효과 적용 - 아직 미완성
			actualDamage = target.GetActualEffect(actualDamage, StatusEffectType.TakenDamageChange);
			
			float targetDefense = target.GetActualStat(Stat.Defense);
			float targetResistance = target.GetActualStat(Stat.Resistance);

			// 기술에 의한 방어/저항 무시 (상대값)
			targetDefense = SkillLogicFactory.Get(appliedSkill).ApplyIgnoreDefenceRelativeValueBySkill(targetDefense, caster, target);
			targetResistance = SkillLogicFactory.Get(appliedSkill).ApplyIgnoreResistanceRelativeValueBySkill(targetResistance, caster, target);

			// 특성에 의한 방어/저항 무시 (상대값)
			List<PassiveSkill> casterPassiveSkills = caster.GetLearnedPassiveSkillList();
			
            targetDefense = SkillLogicFactory.Get(casterPassiveSkills).ApplyIgnoreDefenceRelativeValueByEachPassive(skillInstanceData, targetDefense); 
            targetResistance = SkillLogicFactory.Get(casterPassiveSkills).ApplyIgnoreResistanceRelativeValueByEachPassive(targetResistance, caster, target);
           

			// 기술에 의한 방어/저항 무시 (절대값)
			targetDefense = SkillLogicFactory.Get(appliedSkill).ApplyIgnoreDefenceAbsoluteValueBySkill(targetDefense, caster, target);
			targetResistance = SkillLogicFactory.Get(appliedSkill).ApplyIgnoreResistanceAbsoluteValueBySkill(targetResistance, caster, target);

			// 특성에 의한 방어/저항 무시 (절대값)
			
			targetDefense = SkillLogicFactory.Get(casterPassiveSkills).ApplyIgnoreDefenceAbsoluteValueByEachPassive(skillInstanceData, targetDefense);
            targetResistance = SkillLogicFactory.Get(casterPassiveSkills).ApplyIgnoreResistanceAbsoluteValueByEachPassive(targetResistance, caster, target);


            if (caster.GetUnitClass() == UnitClass.Melee)
			{
				// 실제 피해 = 원래 피해 x 200/(200+방어력)
				actualDamage = actualDamage * 200.0f / (200.0f + target.GetActualStat(Stat.Defense));
				Debug.Log("Actual melee damage without status effect : " + actualDamage);
			}
			else if (caster.GetUnitClass() == UnitClass.Magic)
			{
				actualDamage = actualDamage * 200.0f / (200.0f + target.GetActualStat(Stat.Resistance));
				Debug.Log("Actual magic damage without status effect: " + actualDamage);
			}
			else if (caster.GetUnitClass() == UnitClass.None)
			{
				// actualDamage = actualDamage;
			}

			// 피격자의 기술 특성으로 인한 데미지 증감 효과 - 아직 미구현

			List<PassiveSkill> targetPassiveSkills = target.GetLearnedPassiveSkillList();
			
			finalDamage = (int) actualDamage;

			// 보호막에 따른 대미지 삭감 - 실제로 여기서 실드를 깎으면 안됨. 다른곳으로 옮길 것.
			if (target.HasStatusEffect(StatusEffectType.Shield))
			{
				List<StatusEffect> statusEffectList = target.GetStatusEffectList();
				int shieldAmount = 0;
				for (int i = 0; i < statusEffectList.Count; i++)
				{
					if (statusEffectList[i].IsOfType(StatusEffectType.Shield))
					{
						shieldAmount = (int)statusEffectList[i].GetRemainAmount();
						if (shieldAmount > finalDamage)
						{
							statusEffectList[i].SetRemainAmount(shieldAmount - finalDamage);
							finalDamage = 0;
							Debug.Log("Remain Shield Amount : " + statusEffectList[i].GetRemainAmount());
							break;
						}
						else
						{
							finalDamage -= shieldAmount;
							statusEffectList[i].SetToBeRemoved(true);
						}
					}
				}
				target.UpdateStatusEffect(); // 버그있을듯 (미리보기할때 업데이트 해도 되나?)
			}
		}
		
		return (float) finalDamage;
	}	
}
}
