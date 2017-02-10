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
	public static Dictionary<GameObject, float> CalculateTotalDamage(BattleData battleData, Tile centerTile, List<GameObject> tilesInSkillRange, List<GameObject> firstRange)
	{
		// List<ChainInfo> tempChainList = new List<ChainInfo>();
		Dictionary<GameObject, float> damageList = new Dictionary<GameObject, float>();

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

	private static Dictionary<GameObject, float> MergeDamageList(Dictionary<GameObject, float> lhs, Dictionary<GameObject, float> rhs)
	{
		var merged = new Dictionary<GameObject, float>();
		foreach (var kv in lhs)
		{
			merged[kv.Key] = kv.Value;
		}
		foreach (var kv in rhs)
		{
			if (merged.ContainsKey(kv.Key))
			{
				merged[kv.Key] += rhs[kv.Key];
			}
			else
			{
				merged[kv.Key] = rhs[kv.Key];
			}
		}
		return merged;
	}

	private static Dictionary<GameObject, float> CalculateDamageOfEachSkill(BattleData battleData, ChainInfo chainInfo, int chainCombo)
	{
		var damageList = new Dictionary<GameObject, float>();
		Skill appliedSkill = chainInfo.GetSkill();
		if (appliedSkill.GetSkillApplyType() != SkillApplyType.DamageHealth) {
			return damageList;
		}

		GameObject casterUnitObject = chainInfo.GetUnit();
		Unit caterUnit = casterUnitObject.GetComponent<Unit>();			
		List<GameObject> selectedTiles = chainInfo.GetTargetArea();

		Direction oldDirection = caterUnit.GetDirection();

		// 시전 방향으로 유닛의 바라보는 방향을 돌림.
		if (appliedSkill.GetSkillType() != SkillType.Auto)
			caterUnit.SetDirection(Utility.GetDirectionToTarget(caterUnit.gameObject, selectedTiles));

		List<GameObject> targets = GetTargetUnits(selectedTiles);

		foreach (var target in targets)
		{
			Unit targetUnit = target.GetComponent<Unit>();
			float attackDamage = CalculateAttackDamage(battleData, target, casterUnitObject, appliedSkill, chainCombo, targets.Count).resultDamage;
			float actualDamage = targetUnit.GetActualDamage(casterUnitObject.GetComponent<Unit>().GetUnitClass(), attackDamage,
				appliedSkill.GetPenetration(), false, true);
			damageList.Add(targetUnit.gameObject, actualDamage);

			Debug.Log("Apply " + actualDamage + " damage to " + targetUnit.GetName() + "\n" +
						"ChainCombo : " + chainCombo);
		}

		caterUnit.SetDirection(oldDirection);
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
		public float directionBonus = 0;
		public float celestialBonus = 0;
		public float chainBonus = 0;
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
		attackDamage.celestialBonus = CelestialBonus(casterUnitObject, target);
		attackDamage.chainBonus = ChainComboBonus(battleData, chainCombo);
		attackDamage.smiteAmount = SmiteAmount(casterUnit);
		attackDamage.resultDamage = (attackDamage.baseDamage
									+ attackDamage.smiteAmount) 
									* attackDamage.directionBonus
									* attackDamage.celestialBonus
									* attackDamage.chainBonus;

		attackDamage = ApplyIndividualSkillDamage(attackDamage, appliedSkill, battleData, casterUnit, targetUnit, targetCount);

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

	private static float CelestialBonus(GameObject casterUnitObject, GameObject target) {
		float celestialBonus = Utility.GetCelestialBonus(casterUnitObject, target);
		Debug.Log("celestialBonus : " + celestialBonus);
		return celestialBonus;
	}

	private static float ChainComboBonus(BattleData battleData, int chainCombo) {
		float chainBonus = battleData.GetChainDamageFactorFromChainCombo(chainCombo);
		Debug.Log("chainBonus : " + chainBonus);
		return chainBonus;
	}

	private static float SmiteAmount(Unit casterUnit) {
		int smiteAmount = 0;
		if (casterUnit.HasStatusEffect(StatusEffectType.Smite))
		{
			// FIXME: smiteAmount(0) is passed wihch is multiplied in GetActualEffect
			// Always smiteAmount is 0
			smiteAmount = (int) casterUnit.GetActualEffect((float)smiteAmount, StatusEffectType.Smite);
		}
		Debug.Log("smiteAmount : " + smiteAmount);
		return smiteAmount;
	}

	private static DamageCalculator.AttackDamage ApplyIndividualSkillDamage(DamageCalculator.AttackDamage attackDamage, Skill appliedSkill, BattleData battleData, Unit casterUnit, Unit targetUnit, int targetCount) {
		attackDamage = SkillLogicFactory.Get(appliedSkill).ApplyIndividualAdditionalDamage(attackDamage, appliedSkill, battleData, casterUnit, targetUnit, targetCount);

		/*		
		switch (appliedSkill.GetName())
		{
			// reina_m_12 속성 보너스
			case "불의 파동":
				if (targetUnit.GetElement().Equals(Element.Plant))
				{
					float[] elementDamageBonus = new float[]{1.1f, 1.2f, 1.3f, 1.4f, 1.5f};
					return previousDamage * elementDamageBonus[0];
				}
				else
				{
					return previousDamage;
				}
			// yeong_l_18 대상 숫자 보너스
			case "섬광 찌르기":
				if (targetCount > 1)
				{
					float targetNumberBonus = (float)targetCount*0.1f + 1.0f;
					return previousDamage * targetNumberBonus;
				}
				else
				{
					return previousDamage;
				}
			case "전자기학":
				Element tileElement = battleData.tileManager.GetTile(targetUnit.GetPosition()).GetComponent<Tile>().GetTileElement();
				if (!tileElement.Equals(Element.Metal) && !tileElement.Equals(Element.Water)) 
				{
					return previousDamage * 0;
				}
				else
				{
					return previousDamage;
				}
			default:
			return previousDamage;
		}
		*/
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
}
}
