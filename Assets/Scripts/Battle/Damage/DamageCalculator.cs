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
	public static Dictionary<GameObject, DamageInfo> CalculateTotalDamage(BattleData battleData, Tile centerTile, List<GameObject> tilesInSkillRange, List<GameObject> firstRange)
	{
		Dictionary<GameObject, DamageInfo> damageList = new Dictionary<GameObject, DamageInfo>();

		ChainList.AddChains(battleData.selectedUnitObject, centerTile, tilesInSkillRange, battleData.SelectedSkill, firstRange);

		List<ChainInfo> allVaildChainInfo = ChainList.GetAllChainInfoToTargetArea(battleData.selectedUnitObject, tilesInSkillRange);
		int chainCombo = allVaildChainInfo.Count;

		foreach (var chainInfo in allVaildChainInfo)
		{
			var damageListOfEachSkill = CalculateDamageOfEachSkill(battleData, chainInfo, chainCombo);
			damageList = MergeDamageList(damageList, damageListOfEachSkill);
		}

		ChainList.RemoveChainsFromUnit(battleData.selectedUnitObject);

		return damageList;
	}

	private static Dictionary<GameObject, DamageInfo> MergeDamageList(Dictionary<GameObject, DamageInfo> lhs, Dictionary<GameObject, DamageInfo> rhs)
	{
		var merged = new Dictionary<GameObject, DamageInfo>();
		foreach (var kv in lhs)
		{
			merged[kv.Key] = kv.Value;
		}
		foreach (var kv in rhs)
		{
			if (merged.ContainsKey(kv.Key))
			{
				foreach (var caster in rhs[kv.Key].casters)
					merged[kv.Key].casters.Add(caster);
				merged[kv.Key].damage += rhs[kv.Key].damage;
			}
			else
			{
				merged[kv.Key] = rhs[kv.Key];
			}
		}
		return merged;
	}

	private static Dictionary<GameObject, DamageInfo> CalculateDamageOfEachSkill(BattleData battleData, ChainInfo chainInfo, int chainCombo)
	{
		var damageList = new Dictionary<GameObject, DamageInfo>();
		Skill appliedSkill = chainInfo.GetSkill();
		if (appliedSkill.GetSkillApplyType() != SkillApplyType.DamageHealth) {
			return damageList;
		}

		GameObject casterUnitObject = chainInfo.GetUnit();
		Unit casterUnit = casterUnitObject.GetComponent<Unit>();			
		List<GameObject> selectedTiles = chainInfo.GetTargetArea();

		Direction oldDirection = casterUnit.GetDirection();

		// 시전 방향으로 유닛의 바라보는 방향을 돌림.
		if (appliedSkill.GetSkillType() != SkillType.Auto)
			casterUnit.SetDirection(Utility.GetDirectionToTarget(casterUnit.gameObject, selectedTiles));

		List<GameObject> targets = GetTargetUnits(selectedTiles);

		foreach (var target in targets)
		{
			Unit targetUnit = target.GetComponent<Unit>();
			float attackDamage = CalculateAttackDamage(battleData, target, casterUnitObject, appliedSkill, chainCombo, targets.Count).resultDamage;
			float actualDamage = GetActualDamage(appliedSkill, targetUnit, casterUnitObject.GetComponent<Unit>(), attackDamage,
				appliedSkill.GetPenetration(), false, true);

			DamageInfo damageInfo = new DamageInfo(casterUnit, actualDamage);
			damageList.Add(targetUnit.gameObject, damageInfo);

			Debug.Log("Apply " + actualDamage + " damage to " + targetUnit.GetName() + "\n" +
						"ChainCombo : " + chainCombo);
		}

		casterUnit.SetDirection(oldDirection);
		return damageList;
	}

	private static List<GameObject> GetTargetUnits(List<GameObject> targetTiles) {
		var targets = new List<GameObject>();
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

	public class AttackDamage
	{
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

	public static AttackDamage CalculateAttackDamage(BattleData battleData, GameObject target, GameObject casterUnitObject, Skill appliedSkill, int chainCombo, int targetCount)
	{
		Unit casterUnit = casterUnitObject.GetComponent<Unit>();
		Unit targetUnit = target.GetComponent<Unit>();
		// 방향 체크.
		Utility.GetDegreeAtAttack(casterUnitObject, target);
		BattleManager battleManager = battleData.battleManager;

		AttackDamage attackDamage = new AttackDamage();
		attackDamage.baseDamage = PowerFactorDamage(appliedSkill, casterUnit);
		attackDamage.directionBonus = DirectionBonus(casterUnitObject, target);
		attackDamage.attackDirection = AttackDirection(casterUnitObject, target);
		attackDamage.celestialBonus = CelestialBonus(casterUnitObject, target);
		attackDamage.heightBonus = HeightBonus(casterUnitObject, target);
		attackDamage.chainBonus = ChainComboBonus(battleData, chainCombo);
		attackDamage.smiteAmount = SmiteAmount(casterUnit);

		// 해당 기술의 추가데미지 계산
		attackDamage = ApplyBonusDamageFromEachSkill(attackDamage, appliedSkill, battleData, casterUnit, targetUnit, targetCount);
		// 특성에 의한 추가데미지
		List<PassiveSkill> passiveSkills = casterUnit.GetLearnedPassiveSkillList();
		attackDamage = SkillLogicFactory.Get(passiveSkills).ApplyBonusDamageFromEachPassive(attackDamage, casterUnit, appliedSkill, targetUnit, targetCount);

		attackDamage.resultDamage = (attackDamage.baseDamage
									* attackDamage.relativeDamageBonus
									+ attackDamage.absoluteDamageBonus
									+ attackDamage.smiteAmount) 
									* attackDamage.directionBonus
									* attackDamage.celestialBonus
									* attackDamage.heightBonus
									* attackDamage.chainBonus;

		return attackDamage;
	}

	private static float PowerFactorDamage(Skill appliedSkill, Unit casterUnit)
	{
		float damage = 0;
		foreach (var powerFactor in appliedSkill.GetPowerFactorDict().Keys)
		{
			Stat stat = (Stat)Enum.Parse(typeof(Stat), powerFactor);
			if (stat.Equals(Stat.UsedAP))
			{
				damage += casterUnit.GetActualRequireSkillAP(appliedSkill) * appliedSkill.GetPowerFactor(stat);
			}
			else if (stat.Equals(Stat.None))
			{
				damage += appliedSkill.GetPowerFactor(stat);
			}
			else
			{
				damage += casterUnit.GetActualStat(stat) * appliedSkill.GetPowerFactor(stat);
			}
		}

		Debug.Log("baseDamage : " + damage);

		return damage;
	}

	private static float DirectionBonus(GameObject casterUnitObject, GameObject target) {
		float directionBonus = Utility.GetDirectionBonus(casterUnitObject, target);
		Debug.Log("directionBonus : " + directionBonus);
		return directionBonus;
	}

	private static DirectionCategory AttackDirection(GameObject casterUnitObject, GameObject target)
	{
		float directionBonus = Utility.GetDirectionBonus(casterUnitObject, target);
		Debug.Log("directionBonus : " + directionBonus);
		if (directionBonus == 1.1f)
			return DirectionCategory.Side;
		else if (directionBonus == 1.25f)
			return DirectionCategory.Back;
		else
			return DirectionCategory.Front; 
	}

	private static float CelestialBonus(GameObject casterUnitObject, GameObject target) {
		float celestialBonus = Utility.GetCelestialBonus(casterUnitObject, target);
		Debug.Log("celestialBonus : " + celestialBonus);
		return celestialBonus;
	}

	private static float HeightBonus(GameObject casterUnitObject, GameObject target) {
		float heightBonus = Utility.GetHeightBonus(casterUnitObject, target);
		Debug.Log("heightBonus : " + heightBonus);
		return heightBonus;
	}

	private static float ChainComboBonus(BattleData battleData, int chainCombo) {
		float chainBonus = battleData.GetChainDamageFactorFromChainCombo(chainCombo);
		Debug.Log("chainBonus : " + chainBonus);
		return chainBonus;
	}

	private static float SmiteAmount(Unit casterUnit) {
		float smiteAmount = 0;
		smiteAmount = casterUnit.GetActualEffect(smiteAmount, StatusEffectType.Smite);
		Debug.Log("smiteAmount : " + smiteAmount);
		return smiteAmount;
	}

	private static DamageCalculator.AttackDamage ApplyBonusDamageFromEachSkill(DamageCalculator.AttackDamage attackDamage, Skill appliedSkill, BattleData battleData, Unit casterUnit, Unit targetUnit, int targetCount) {
		attackDamage = SkillLogicFactory.Get(appliedSkill).ApplyAdditionalDamage(attackDamage, appliedSkill, battleData, casterUnit, targetUnit, targetCount);

		return attackDamage;
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

	public static float GetActualDamage(Skill appliedSkill, Unit target, Unit caster, float damageAmount, float penetration, bool isDot, bool isHealth)
	{
		float actualDamage = damageAmount;
		int finalDamage = 0; // 최종 대미지 (정수로 표시되는)

		// 대미지 증가/감소 효과 적용
		// 공격이 물리인지 마법인지 체크
		// 기술 / 특성의 추가피해 / 감소 효과
		// 방어력 / 저항력 중 맞는 값을 적용 (적용 단계에서 능력치 변동 효과 반영)
		// 보호막 있을 경우 대미지 삭감
		// 체력 깎임
		// 체인 해제
		if (isHealth == true)
		{
			// 효과/특성으로 인한 대미지 증감 효과 적용 - 아직 미완성
			if (target.HasStatusEffect(StatusEffectType.DamageChange))
			{
				actualDamage = target.GetActualEffect(actualDamage, StatusEffectType.DamageChange);
			}

			float targetDefense = target.GetActualStat(Stat.Defense);
			float targetResistance = target.GetActualStat(Stat.Resistance);

			// 기술에 의한 방어/저항 무시 (상대값)
			// targetDefence = ApplyIgnoreDefenceBySkill(appliedSkill, );

			// 특성에 의한 방어/저항 무시 (상대값)
			List<PassiveSkill> casterPassiveSkills = caster.GetLearnedPassiveSkillList();
			targetDefense = SkillLogicFactory.Get(casterPassiveSkills).ApplyIgnoreDefenceRelativeValueByEachPassive(targetDefense, caster, target);

			// 기술에 의한 방어/저항 무시 (절대값)
			// targetDefence = ApplyIgnoreDefenceBySkill(appliedSkill, );

			// 특성에 의한 방어/저항 무시 (절대값)
			targetDefense = SkillLogicFactory.Get(casterPassiveSkills).ApplyIgnoreDefenceAbsoluteValueByEachPassive(targetDefense, caster, target);

			if (caster.GetUnitClass() == UnitClass.Melee)
			{
				// 실제 피해 = 원래 피해 x 200/(200+방어력)
				actualDamage = actualDamage * 200.0f / (200.0f + target.GetActualStat(Stat.Defense) * (1.0f - penetration));
				Debug.Log("Actual melee damage without status effect : " + actualDamage);
			}
			else if (caster.GetUnitClass() == UnitClass.Magic)
			{
				actualDamage = actualDamage * 200.0f / (200.0f + target.GetActualStat(Stat.Resistance) * (1.0f - penetration));
				Debug.Log("Actual magic damage without status effect: " + actualDamage);
			}
			else if (caster.GetUnitClass() == UnitClass.None)
			{
				// actualDamage = actualDamage;
			}

			// 기술 특성으로 인한 데미지 증감 효과 - 아직 미구현

			finalDamage = (int) actualDamage;

			// 보호막에 따른 대미지 삭감
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
