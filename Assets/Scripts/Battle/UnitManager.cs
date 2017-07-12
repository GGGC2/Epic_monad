using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;
using Util;
using Battle.Skills;
using System.Linq;

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
    List<TileStatusEffectInfo> tileStatusEffectInfoList = new List<TileStatusEffectInfo>();

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
			unit.GetLatelyHitInfos().Clear();
		}
	}

    public void TriggerPassiveSkillsAtActionEnd() {
        foreach(var unit in GetAllUnits()) {
            SkillLogicFactory.Get(unit.GetLearnedPassiveSkillList()).TriggerOnActionEnd(unit);
        }
    }

    public IEnumerator TriggerStatusEffectsAtActionEnd() {
        foreach(var unit in GetAllUnits()) {
            List<StatusEffect> statusEffectList = unit.GetStatusEffectList();
            foreach (StatusEffect statusEffect in statusEffectList) {
                Skill skill = statusEffect.GetOriginSkill();
                PassiveSkill passiveSkill = statusEffect.GetOriginPassiveSkill();
                if (skill != null)
                    yield return StartCoroutine(SkillLogicFactory.Get(skill).TriggerStatusEffectsAtActionEnd(unit, statusEffect));
            }
        }
    }

    public void UpdateStatusEffectsAtActionEnd() {
        foreach (var unit in GetAllUnits()) {
            foreach (StatusEffect statusEffect in unit.GetStatusEffectList()) {
                if(statusEffect.IsOfType(StatusEffectType.Aura)) {
                    Aura.Update(unit, statusEffect);
                }
            }
        }
        foreach (var unit in GetAllUnits()) {
            foreach(StatusEffect statusEffect in unit.GetStatusEffectList()) {
                if (statusEffect.GetRemainStack() != 0) {
                    for (int i = 0; i < statusEffect.fixedElem.actuals.Count; i++) {
                        statusEffect.CalculateAmount(i, true);
                    }
                    unit.updateStats(statusEffect, false, false);
                }
                else
                    unit.RemoveStatusEffect(statusEffect);
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

	public void SetStandardActivityPoint()
	{
		standardActivityPoint = GameData.level + 60;
	}

	public void GenerateUnits ()
	{
		List<UnitInfo> unitInfoList = Parser.GetParsedUnitInfo();
		int GeneratedPC = 0;

		ReadyManager readyManager = FindObjectOfType<ReadyManager>();

		if (readyManager != null)
		{
			foreach (var unitInfo in unitInfoList)
			{
				if (unitInfo.name == "unselected") 
				{
					string PCName = readyManager.selected[GeneratedPC].unitName;
					unitInfo.name = UnitInfo.ConvertToKoreanName(PCName);
					
					if (unitInfo.name != "Empty") 
					{
						unitInfo.nameInCode = PCName;
						unitInfo.baseHealth = UnitInfo.GetStat(PCName, UnitInfo.StatType.Health);
						unitInfo.basePower = UnitInfo.GetStat(PCName, UnitInfo.StatType.Power);
						unitInfo.baseDefense = UnitInfo.GetStat(PCName, UnitInfo.StatType.Defense);
						unitInfo.baseResistance = UnitInfo.GetStat(PCName, UnitInfo.StatType.Resist);
						unitInfo.baseAgility = UnitInfo.GetStat(PCName, UnitInfo.StatType.Agility);
					}
					
					GeneratedPC += 1;
				}
			}
			unitInfoList = unitInfoList.FindAll(info => info.name != "Empty");

			foreach (var unitInfo in unitInfoList)
			{
				Unit unit = Instantiate(unitPrefab).GetComponent<Unit>();

				unit.ApplyUnitInfo(unitInfo);
				unit.ApplySkillList(skillInfoList, statusEffectInfoList, tileStatusEffectInfoList, passiveSkillInfoList);

				Vector2 initPosition = unit.GetInitPosition();
				Vector3 respawnPos = FindObjectOfType<TileManager>().GetTilePos(new Vector2(initPosition.x, initPosition.y));
				respawnPos -= new Vector3(0, 0, 0.05f);
				unit.gameObject.transform.position = respawnPos;

				Tile tileUnderUnit = FindObjectOfType<TileManager>().GetTile((int)initPosition.x, (int)initPosition.y);
				tileUnderUnit.SetUnitOnTile(unit);

				units.Add(unit);
			}

			List<string> controllableUnitNameList = new List<string>();
			readyManager.selected.ToList().ForEach(panel => {
				if (panel.unitName != "Empty")
				{
					controllableUnitNameList.Add(panel.unitName);
				}
			});
			
			units.ForEach(unit => {
				if (controllableUnitNameList.Contains(unit.GetNameInCode()))
				{
					Destroy(unit.GetComponent<AIData>());
				}
			});

			Destroy(GameObject.Find("ReadyManager").gameObject);
		}
		else 
		{
			foreach (var unitInfo in unitInfoList)
			{
				Unit unit = Instantiate(unitPrefab).GetComponent<Unit>();

				unit.ApplyUnitInfo(unitInfo);
				unit.ApplySkillList(skillInfoList, statusEffectInfoList, tileStatusEffectInfoList, passiveSkillInfoList);

				Vector2 initPosition = unit.GetInitPosition();
				Vector3 respawnPos = FindObjectOfType<TileManager>().GetTilePos(new Vector2(initPosition.x, initPosition.y));
				respawnPos -= new Vector3(0, 0, 0.05f);
				unit.gameObject.transform.position = respawnPos;

				Tile tileUnderUnit = FindObjectOfType<TileManager>().GetTile((int)initPosition.x, (int)initPosition.y);
				tileUnderUnit.SetUnitOnTile(unit);

				units.Add(unit);
			}

			units.ForEach(unit => {
				if (unit.GetSide() == Side.Ally)
				{
					Destroy(unit.GetComponent<AIData>());
				}
			});
		}
		// Debug.Log("Generate units complete");
	}

	public void DeleteDeadUnit(Unit deadUnit)
	{
		// 시전자에게 대상 사망 시 발동되는 효과가 있을 경우 발동.
		foreach (var hitInfo in deadUnit.GetLatelyHitInfos())
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
				// Debug.Log(unit.GetName() + " is readied");
			}
		}

		// AP가 큰 순서대로 소팅.
		readiedUnits.Sort(SortHelper.Chain(new List<Comparison<Unit>>
		{
			SortHelper.CompareBy<Unit>(go => go.GetCurrentActivityPoint()),
			SortHelper.CompareBy<Unit>(go => go.GetStat(Stat.Dexturity)),
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
                // Debug.Log(unit.GetName() + " is enemy");
            }
        }
        return enemyUnits;
    }

	public IEnumerator ApplyEachDOT() {
        List<Unit> unitList = new List<Unit>();
        units.ForEach(x => unitList.Add(x));
        foreach (var unit in unitList)
		{
			if (unit != null)
				yield return StartCoroutine(unit.ApplyDamageOverPhase());
			else
				yield return null;
		}
	}

    public IEnumerator ApplyEachHeal() {
        List<Unit> unitList = new List<Unit>();
        units.ForEach(x => unitList.Add(x));
        foreach (var unit in unitList) {
            if(unit != null)
                yield return unit.ApplyHealOverPhase();
            else yield return null;
        }
    }

	public IEnumerator StartPhase(int phase)
	{
		foreach (var unit in units)
		{
			unit.UpdateStartPosition();
			yield return StartCoroutine(unit.ApplyTriggerOnPhaseStart());
            if(phase == 1) {
                unit.ApplyTriggerOnStart();
            }
		}
	}

	public void EndPhase(int phase)
	{
		// Decrease each buff & debuff phase
		foreach (var unit in units)
		{
			unit.UpdateRemainPhaseAtPhaseEnd();
			unit.UpdateSkillCooldown();
		}

		foreach (var unit in units)
		{
			unit.RegenerateActionPoint();
		}
		//행동력 회복시킨 후 순서 정렬하는 역할
		UpdateUnitOrder();

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

    void LoadTileStatusEffects()
    {
        tileStatusEffectInfoList = Parser.GetParsedTileStatusEffectInfo();
    }

	void Start () 
	{
		LoadSkills();
		LoadPassiveSkills();
        LoadStatusEffects();
        LoadTileStatusEffects();
		GenerateUnits();
        GetEnemyUnits();
		StartCoroutine(FindObjectOfType<BattleManager>().InstantiateTurnManager());
	}

	public void UpdateUnitOrder ()
	{
		int standardActivityPoint = GetStandardActivityPoint();
		List<Unit> currentPhaseUnits =
			units.FindAll(go => go.GetCurrentActivityPoint() >= standardActivityPoint);
		List<Unit> nextPhaseUnits =
			units.FindAll(go => go.GetCurrentActivityPoint() < standardActivityPoint);

		currentPhaseUnits.Sort(SortHelper.Chain(new List<Comparison<Unit>>
		{
			SortHelper.CompareBy<Unit>(go => go.GetCurrentActivityPoint()),
			SortHelper.CompareBy<Unit>(go => go.GetStat(Stat.Dexturity)),
			SortHelper.CompareBy<Unit>(go => go.gameObject.GetInstanceID())
		}, reverse:true));

		nextPhaseUnits.Sort(SortHelper.Chain(new List<Comparison<Unit>>
		{
			SortHelper.CompareBy<Unit>(go => {
					int currentAP = go.GetCurrentActivityPoint();
					int recover = go.GetStat(Stat.Dexturity);
					return currentAP + recover;
			}),
			SortHelper.CompareBy<Unit>(go => go.GetStat(Stat.Dexturity)),
			SortHelper.CompareBy<Unit>(go => go.gameObject.GetInstanceID())
		}, reverse:true));

	   // 유닛 전체에 대해서도 소팅. 변경점이 있을때마다 반영된다.
		units.Clear();
		units.AddRange(currentPhaseUnits);
		units.AddRange(nextPhaseUnits);
	}
}