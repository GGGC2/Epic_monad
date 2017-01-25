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

		GameObject unitObjectInChain = chainInfo.GetUnit();
		Unit unitInChain = unitObjectInChain.GetComponent<Unit>();			
		List<GameObject> selectedTiles = chainInfo.GetTargetArea();

		Direction oldDirection = unitInChain.GetDirection();

		// 시전 방향으로 유닛의 바라보는 방향을 돌림.
		if (appliedSkill.GetSkillType() != SkillType.Auto)
			unitInChain.SetDirection(Utility.GetDirectionToTarget(unitInChain.gameObject, selectedTiles));

		List<GameObject> targets = new List<GameObject>();

		foreach (var tileObject in selectedTiles)
		{
			Tile tile = tileObject.GetComponent<Tile>();
			if (tile.IsUnitOnTile())
			{
				targets.Add(tile.GetUnitOnTile());
			}
		}

		foreach (var target in targets)
		{
			Unit targetUnit = target.GetComponent<Unit>();
			// 방향 체크.
			Utility.GetDegreeAtAttack(unitObjectInChain, target);
			BattleManager battleManager = battleData.battleManager;
			// sisterna_r_6의 타일 속성 판정
			if (appliedSkill.GetName().Equals("전자기학"))
			{
				Element tileElement = battleData.tileManager.GetTile(targetUnit.GetPosition()).GetComponent<Tile>().GetTileElement();
				if(!tileElement.Equals(Element.Metal) && !tileElement.Equals(Element.Water)) continue;
			}

			// 스킬 기본 대미지 계산
			float baseDamage = 0.0f;
			foreach (var powerFactor in appliedSkill.GetPowerFactorDict().Keys)
			{
				Stat stat = (Stat)Enum.Parse(typeof(Stat), powerFactor);
				if (stat.Equals(Stat.UsedAP))
				{
					baseDamage += unitInChain.GetActualRequireSkillAP(appliedSkill) * appliedSkill.GetPowerFactor(stat, appliedSkill.GetLevel());
				}
				else if (stat.Equals(Stat.None))
				{
					baseDamage += appliedSkill.GetPowerFactor(stat, appliedSkill.GetLevel());
				}
				else
				{
					baseDamage += unitInChain.GetActualStat(stat) * appliedSkill.GetPowerFactor(stat, appliedSkill.GetLevel());
				}
			}

			// 방향 보너스.
			float directionBonus = Utility.GetDirectionBonus(unitObjectInChain, target);
			// if (directionBonus > 1f) unitInChain.PrintDirectionBonus(directionBonus);

			// 천체속성 보너스.
			float celestialBonus = Utility.GetCelestialBonus(unitObjectInChain, target);
			// if (celestialBonus > 1f) unitInChain.PrintCelestialBonus();
			// else if (celestialBonus == 0.8f) targetUnit.PrintCelestialBonus();

			// 체인 보너스.
			float chainBonus = battleData.GetChainDamageFactorFromChainCombo(chainCombo);
			// if (directionBonus > 1f) unitInChain.PrintDirectionBonus(chainCombo);

			// 강타 효과에 의한 대미지 추가
			int smiteAmount = 0;
			if (unitInChain.HasStatusEffect(StatusEffectType.Smite))
			{
				smiteAmount = (int) unitInChain.GetActualEffect((float)smiteAmount, StatusEffectType.Smite);
			}

			var damageAmount = (baseDamage * directionBonus * celestialBonus * chainBonus) + (float) smiteAmount;
			Debug.Log("baseDamage : " + baseDamage);
			Debug.Log("directionBonus : " + directionBonus);
			Debug.Log("celestialBonus : " + celestialBonus);
			Debug.Log("chainBonus : " + chainBonus);
			Debug.Log("smiteAmount : " + smiteAmount);

			// reina_m_12 속성 보너스
			if (appliedSkill.GetName().Equals("불의 파동") && targetUnit.GetElement().Equals(Element.Plant))
			{
				float[] elementDamageBonus = new float[]{1.1f, 1.2f, 1.3f, 1.4f, 1.5f};
				damageAmount = damageAmount * elementDamageBonus[0];
			}

			// yeong_l_18 대상 숫자 보너스
			if (appliedSkill.GetName().Equals("섬광 찌르기") && targets.Count > 1)
			{
				float targetNumberBonus = (float)targets.Count*0.1f + 1.0f;
				damageAmount = damageAmount * targetNumberBonus;
			}

			//유닛과 유닛의 데미지를 Dictionary에 추가.
			float actualDamage = targetUnit.GetActualDamage(unitInChain.GetUnitClass(), damageAmount, appliedSkill.GetPenetration(appliedSkill.GetLevel()), false, true);
			if (!damageList.ContainsKey(targetUnit.gameObject))
			{
				damageList.Add(targetUnit.gameObject, actualDamage);
			}
			else
			{
				float totalDamage = damageList[targetUnit.gameObject];
				totalDamage += actualDamage;
				damageList.Remove(targetUnit.gameObject);
				damageList.Add(targetUnit.gameObject, totalDamage);
			}

			// targetUnit이 반사 효과를 지니고 있을 경우 반사 대미지 코루틴 준비
			// 반사 미적용.

			Debug.Log("Apply " + damageAmount + " damage to " + targetUnit.GetName() + "\n" +
						"ChainCombo : " + chainCombo);
		
		}

		unitInChain.SetDirection(oldDirection);
		return damageList;
	}
}
}
