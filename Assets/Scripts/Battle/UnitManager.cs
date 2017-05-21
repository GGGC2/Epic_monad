﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;
using Util;
using Battle.Skills;

public class DeadUnitInfo
{
	public readonly string unitName;
	public readonly Side unitSide;

	public DeadUnitInfo(Unit unit)
	{
		unitName = unit.GetName();
		unitSide = unit.GetSide();
	}
}

public class RetreatUnitInfo
{
	public readonly string unitName;
	public readonly Side unitSide;

	public RetreatUnitInfo(Unit unit)
	{
		unitName = unit.GetName();
		unitSide = unit.GetSide();
	}
}

public class UnitManager : MonoBehaviour {

	int standardActivityPoint;

	List<SkillInfo> skillInfoList = new List<SkillInfo>();
	List<PassiveSkillInfo> passiveSkillInfoList = new List<PassiveSkillInfo>();
    List<StatusEffectInfo> statusEffectInfoList = new List<StatusEffectInfo>();

	public GameObject unitPrefab;
	List<Unit> units = new List<Unit>();
	List<Unit> readiedUnits = new List<Unit>();
	List<Unit> deadUnits = new List<Unit>();
	List<Unit> retreatUnits = new List<Unit>();
    List<Unit> enemyUnits = new List<Unit>();
	List<DeadUnitInfo> deadUnitsInfo = new List<DeadUnitInfo>();
	List<RetreatUnitInfo> retreatUnitsInfo = new List<RetreatUnitInfo>();

	public List<Unit> GetAllUnits()
	{
		return units;
	}

	public void ResetLatelyHitUnits()
	{
		foreach (var unit in GetAllUnits())
		{
			unit.latelyHitInfos.Clear();
		}
	}

    public void TriggerPassiveSkillsAtActionEnd() {
        foreach(var unit in GetAllUnits()) {
            SkillLogicFactory.Get(unit.GetLearnedPassiveSkillList()).TriggerActionEnd(unit);
        }
    }

    public IEnumerator TriggerStatusEffectsAtActionEnd() {
        foreach(var unit in GetAllUnits()) {
            List<StatusEffect> statusEffectList = unit.GetStatusEffectList();
            foreach(StatusEffect statusEffect in statusEffectList) {
                if(statusEffect.GetDisplayName()=="가연성 부착물") {
                    yield return new Curi_2_m_SkillLogic().Trigger(unit, statusEffect);
                }
            }
        }
    }

	public List<Unit> GetRetreatUnits()
	{
		retreatUnits.Clear();
		foreach (var unit in units)
		{
			float percentHealth = 100f * (float)unit.GetCurrentHealth() / (float)unit.GetMaxHealth();
			if (((percentHealth <= 10) && (unit.GetCurrentHealth() > 0)) ||
				(retreatUnits.Contains(unit)))
				retreatUnits.Add(unit);
		}

		return retreatUnits;
	}

	public void	MakeRetreatUnitInfo()
	{
		foreach (var retreatUnit in retreatUnits)
		{
			RetreatUnitInfo retreatUnitInfo = new RetreatUnitInfo(retreatUnit);
			retreatUnitsInfo.Add(retreatUnitInfo);
		}
	}

	public List<RetreatUnitInfo> GetRetreatUnitsInfo()
	{
		return retreatUnitsInfo;
	}

	public List<Unit> GetDeadUnits()
	{
		// 죽은 유닛들을 체크.
		deadUnits.Clear();
		foreach (var unit in units)
		{
			if ((unit.GetCurrentHealth() <= 0) || (deadUnits.Contains(unit)))
				deadUnits.Add(unit);
		}

		return deadUnits;
	}

	void MakeDeadUnitInfo()
	{
		foreach (var deadUnit in deadUnits)
		{
			DeadUnitInfo deadUnitInfo = new DeadUnitInfo(deadUnit);
			deadUnitsInfo.Add(deadUnitInfo);
		}
	}

	public List<DeadUnitInfo> GetDeadUnitsInfo()
	{
		MakeDeadUnitInfo();
		return deadUnitsInfo;
	}

	public int GetStandardActivityPoint()
	{
		return standardActivityPoint;
	}

	public void SetStandardActivityPoint(int partyLevel)
	{
		standardActivityPoint = partyLevel + 60;
	}

	public void GenerateUnits ()
	{
		// TileManager tileManager = GetComponent<TileManager>();
		float tileWidth = 0.5f*200/100;
		float tileHeight = 0.5f*100/100;

		List<UnitInfo> unitInfoList = Parser.GetParsedUnitInfo();

		foreach (var unitInfo in unitInfoList)
		{
			Unit unit = Instantiate(unitPrefab).GetComponent<Unit>();

			unit.ApplyUnitInfo(unitInfo);
			unit.ApplySkillList(skillInfoList, statusEffectInfoList, passiveSkillInfoList);
            SkillLogicFactory.Get(unit.GetLearnedPassiveSkillList()).TriggerStart(unit);

			Vector2 initPosition = unit.GetInitPosition();
			// Vector3 tilePosition = tileManager.GetTilePos(initPosition);
			// Vector3 respawnPos = tilePosition + new Vector3(0,0,5f);
			Vector3 respawnPos = FindObjectOfType<TileManager>().GetTilePos(new Vector2(initPosition.x, initPosition.y));
			respawnPos -= new Vector3(0,0,0.05f);
			// Vector3 respawnPos = new Vector3(tileWidth * (initPosition.y + initPosition.x) * 0.5f,
			// 								 tileHeight * (initPosition.y - initPosition.x) * 0.5f,
			// 								 (initPosition.y - initPosition.x) * 0.1f - 5f);
			unit.gameObject.transform.position = respawnPos;

			Tile tileUnderUnit = FindObjectOfType<TileManager>().GetTile((int)initPosition.x, (int)initPosition.y);
			tileUnderUnit.SetUnitOnTile(unit);

			units.Add(unit);
		}

		Debug.Log("Generate units complete");
	}

	public void DeleteDeadUnit(Unit deadUnit)
	{
		// 시전자에게 대상 사망 시 발동되는 효과가 있을 경우 발동.
		foreach (var hitInfo in deadUnit.latelyHitInfos)
		{
			List<PassiveSkill> passiveSkills = hitInfo.caster.GetLearnedPassiveSkillList();
			SkillLogicFactory.Get(passiveSkills).ApplyStatusEffectByKill(hitInfo, deadUnit);

			if (hitInfo.skill != null)
				SkillLogicFactory.Get(hitInfo.skill).OnKill(hitInfo);
		}

		units.Remove(deadUnit);
		readiedUnits.Remove(deadUnit);
	}

	public void DeleteRetreatUnit(Unit retreateUnit)
	{
		units.Remove(retreateUnit);
		readiedUnits.Remove(retreateUnit);
	}

	public List<Unit> GetUpdatedReadiedUnits()
	{
		readiedUnits.Clear();
		// check each unit and add all readied units.
		foreach (var unit in units)
		{
			if (unit.GetCurrentActivityPoint() >= standardActivityPoint)
			{
				readiedUnits.Add(unit);
				Debug.Log(unit.GetName() + " is readied");
			}
		}

		// AP가 큰 순서대로 소팅.
		readiedUnits.Sort(SortHelper.Chain(new List<Comparison<Unit>>
		{
			SortHelper.CompareBy<Unit>(go => go.GetCurrentActivityPoint()),
			SortHelper.CompareBy<Unit>(go => go.GetActualStat(Stat.Dexturity)),
			SortHelper.CompareBy<Unit>(go => go.gameObject.GetInstanceID())
		}, reverse:true));

		return readiedUnits;
	}

    public List<Unit> GetEnemyUnits()
    {
        foreach (var unit in units)
        {
            if (unit.GetSide() == Side.Enemy)
            {
                enemyUnits.Add(unit);
                Debug.Log(unit.GetName() + " is enemy");
            }
        }
        return enemyUnits;
    }

	public IEnumerator ApplyEachDOT()
	{
		foreach (var unit in units)
		{
			yield return StartCoroutine(unit.ApplyDamageOverPhase());
		}
	}

	public void StartPhase()
	{
		foreach (var unit in units)
		{
			unit.UpdateStartPosition();
			unit.ApplyTriggerOnPhaseStart();
		}
	}

	public void EndPhase()
	{
		// Decrease each buff & debuff phase
		foreach (var unit in units)
		{
			unit.UpdateRemainPhaseAtPhaseEnd();
			unit.UpdateStatusEffect();
			unit.UpdateSkillCooldown();
			
		}

		foreach (var unit in units)
			unit.RegenerateActionPoint();
        foreach (var unit in units)
        {
            unit.ApplyTriggerOnPhaseEnd();
        }
	}

	void LoadSkills()
	{
		skillInfoList = Parser.GetParsedSkillInfo();
	}

	void LoadPassiveSkills()
	{
		passiveSkillInfoList = Parser.GetParsedPassiveSkillInfo();
	}

    void LoadStatusEffects()
    {
        statusEffectInfoList = Parser.GetParsedStatusEffectInfo();
    }

	// Use this for initialization
	void Start () {
		LoadSkills();
		LoadPassiveSkills();
        LoadStatusEffects();
		GenerateUnits();
        GetEnemyUnits();
	}

	// Update is called once per frame
	void Update () {

		int standardActivityPoint = GetStandardActivityPoint();
		List<Unit> currentTurnUnits =
			units.FindAll(go => go.GetCurrentActivityPoint() >= standardActivityPoint);
		List<Unit> nextTurnUnits =
			units.FindAll(go => go.GetCurrentActivityPoint() < standardActivityPoint);

		currentTurnUnits.Sort(SortHelper.Chain(new List<Comparison<Unit>>
		{
			SortHelper.CompareBy<Unit>(go => go.GetCurrentActivityPoint()),
			SortHelper.CompareBy<Unit>(go => go.GetActualStat(Stat.Dexturity)),
			SortHelper.CompareBy<Unit>(go => go.gameObject.GetInstanceID())
		}, reverse:true));

		nextTurnUnits.Sort(SortHelper.Chain(new List<Comparison<Unit>>
		{
			SortHelper.CompareBy<Unit>(go => {
					int currentAP = go.GetCurrentActivityPoint();
					int recover = go.GetActualStat(Stat.Dexturity);
					return currentAP + recover;
			}),
			SortHelper.CompareBy<Unit>(go => go.GetActualStat(Stat.Dexturity)),
			SortHelper.CompareBy<Unit>(go => go.gameObject.GetInstanceID())
		}, reverse:true));

	   // 유닛 전체에 대해서도 소팅. 변경점이 있을때마다 반영된다.
		units.Clear();
		units.AddRange(currentTurnUnits);
		units.AddRange(nextTurnUnits);
	}
}
