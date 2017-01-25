using System;
using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;

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

	static Dictionary<GameObject, float> CalculateDamageOfEachSkill(BattleData battleData, ChainInfo chainInfo, int chainCombo)
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
			float actualDamage = CalculateAttackDamage(battleData, target, casterUnitObject, appliedSkill, chainCombo, targets.Count);
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

	private static float CalculateAttackDamage(BattleData battleData, GameObject target, GameObject casterUnitObject, Skill appliedSkill, int chainCombo, int targetCount)
	{
		Unit casterUnit = casterUnitObject.GetComponent<Unit>();
		Unit targetUnit = target.GetComponent<Unit>();
		// 방향 체크.
		Utility.GetDegreeAtAttack(casterUnitObject, target);
		BattleManager battleManager = battleData.battleManager;

		float damageAmount = 0;

		// 스킬 기본 대미지 계산
		damageAmount += PowerFactorDamage(appliedSkill, casterUnit);
		damageAmount *= DirectionBonus(casterUnitObject, target);
		damageAmount *= CelestialBonus(casterUnitObject, target);
		damageAmount *= ChainComboBonus(battleData, chainCombo);
		damageAmount += SmiteAmount(casterUnit);
		damageAmount = ApplyIndividualSkillDamage(damageAmount, appliedSkill, battleData, targetUnit, targetCount);

		//유닛과 유닛의 데미지를 Dictionary에 추가.
		float actualDamage = targetUnit.GetActualDamage(casterUnit.GetUnitClass(), damageAmount, appliedSkill.GetPenetration(appliedSkill.GetLevel()), false, true);
		return actualDamage;
	}

	private static float PowerFactorDamage(Skill appliedSkill, Unit casterUnit)
	{
		float damage = 0;
		foreach (var powerFactor in appliedSkill.GetPowerFactorDict().Keys)
		{
			Stat stat = (Stat)Enum.Parse(typeof(Stat), powerFactor);
			if (stat.Equals(Stat.UsedAP))
			{
				damage += casterUnit.GetActualRequireSkillAP(appliedSkill) * appliedSkill.GetPowerFactor(stat, appliedSkill.GetLevel());
			}
			else if (stat.Equals(Stat.None))
			{
				damage += appliedSkill.GetPowerFactor(stat, appliedSkill.GetLevel());
			}
			else
			{
				damage += casterUnit.GetActualStat(stat) * appliedSkill.GetPowerFactor(stat, appliedSkill.GetLevel());
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

	private static float ApplyIndividualSkillDamage(float previousDamage, Skill appliedSkill, BattleData battleData, Unit targetUnit, int targetCount) {
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
	}
}
}
