using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;
using Util;
using Battle.Skills;
using System.Linq;
using GameData;

public class DeadUnitInfo{
	public readonly string unitName;
	public readonly Side unitSide;

	public DeadUnitInfo(Unit unit){
		unitName = unit.GetNameKor();
		unitSide = unit.GetSide();
	}
}

public class RetreatUnitInfo{
	public readonly string unitName;
	public readonly Side unitSide;

	public RetreatUnitInfo(Unit unit){
		unitName = unit.GetNameKor();
		unitSide = unit.GetSide();
	}
}

public class UnitManager : MonoBehaviour{
	private static UnitManager instance;
	public static UnitManager Instance{ get { return instance; } }

    public bool startFinished = false;

	void Awake(){
		if (instance != null && instance != this){
			Destroy (this.gameObject);
			return;
		}else {instance = this;}
	}

	int standardActivityPoint;

	List<ActiveSkill> activeSkillList = new List<ActiveSkill>();
	List<PassiveSkill> passiveSkillList = new List<PassiveSkill>();
    List<UnitStatusEffectInfo> statusEffectInfoList = new List<UnitStatusEffectInfo>();
    List<TileStatusEffectInfo> tileStatusEffectInfoList = new List<TileStatusEffectInfo>();

	public GameObject unitPrefab;
	public List<Unit> units = new List<Unit>();
	List<Unit> readiedUnits = new List<Unit>();
	List<Unit> deadUnits = new List<Unit>();
	List<Unit> retreatUnits = new List<Unit>();
    List<Unit> enemyUnits = new List<Unit>();
    
	public List<Unit> GetAllUnits(){
		return units;
	}    

	public Unit GetAnUnit(string engName){
		Unit wantedUnit = null;
		foreach(Unit unit in GetAllUnits()){
			if (unit.GetNameEng () == engName) {
				wantedUnit = unit;
			}
		}
		return wantedUnit;
	}

	public void ResetLatelyHitUnits(){
		foreach (var unit in GetAllUnits()){
			unit.GetLatelyHitInfos().Clear();
		}
	}

    public void ApplyTileBuffsAtActionEnd() {
        foreach(var unit in GetAllUnits()) {
            unit.ApplyTileBuffAtActionEnd();
        }
    }

    public void TriggerPassiveSkillsAtActionEnd(){
		foreach(var unit in GetAllUnits()) {
            SkillLogicFactory.Get(unit.GetLearnedPassiveSkillList()).TriggerOnActionEnd(unit);
        }
    }

    public void TriggerStatusEffectsAtActionEnd(){
        foreach(var unit in GetAllUnits()) {
            List<UnitStatusEffect> statusEffectList = unit.StatusEffectList;
            foreach (UnitStatusEffect statusEffect in statusEffectList) {
                Skill skill = statusEffect.GetOriginSkill();
                if (skill is ActiveSkill)
                    ((ActiveSkill)skill).SkillLogic.TriggerStatusEffectAtActionEnd(unit, statusEffect);
            }
        }
    }

    public void UpdateStatusEffectsAtActionEnd() {
        foreach (var unit in GetAllUnits()) {
            foreach (var statusEffect in unit.StatusEffectList) {
                if(statusEffect.IsOfType(StatusEffectType.Aura)) {
                    Aura.Update(unit, statusEffect);
                }
            }
        }
        foreach (var unit in GetAllUnits()) {
            foreach(UnitStatusEffect statusEffect in unit.StatusEffectList) {
                if(statusEffect.GetRemainStack() != 0){
                    for (int i = 0; i < statusEffect.fixedElem.actuals.Count; i++)
                        statusEffect.CalculateAmount(i, true);
                    unit.UpdateStats(statusEffect, false, false);
                    unit.UpdateSpriteByStealth();
                }else{
					unit.RemoveStatusEffect(statusEffect);
				}
            }
        }
    }
    public void UpdateStatsAtActionEnd() {
        foreach(var unit in GetAllUnits()) {
            unit.UpdateStats();
        }
    }
    public void UpdateHealthViewersAtActionEnd() {
        foreach(var unit in GetAllUnits()) {
            if(!unit.IsObject)
                unit.UpdateHealthViewer();
        }
    }
    public void UpdateRealUnitPositions(int direction){
        foreach (var unit in GetAllUnits()) {
            unit.UpdateRealPosition(direction);
        }
    }

    /*public List<Unit> GetRetreatUnits(){
		retreatUnits.Clear();
		
		if(SceneData.stageNumber < Setting.retreatOpenStage)
			return retreatUnits;

		foreach (var unit in GetAllUnits()){
			// 오브젝트는 이탈하지 않는다
			if (unit.IsObject) continue;

			float percentHealth = 100f * (float)unit.GetCurrentHealth() / (float)unit.GetMaxHealth();
			if (((percentHealth <= Setting.retreatHpPercent) && (unit.GetCurrentHealth() > 0)) ||
				(retreatUnits.Contains(unit)))
				retreatUnits.Add(unit);
		}

		return retreatUnits;
	}
    
	public List<Unit> GetDeadUnits(){
		// 죽은 유닛들을 체크.
		deadUnits.Clear();
		foreach (var unit in units){
			if ((unit.GetCurrentHealth() <= 0) || (deadUnits.Contains(unit)))
				deadUnits.Add(unit);
		}
		return deadUnits;
	}*/
    public IEnumerable<EventLog> CheckDestroyedUnits() {
        LogManager logManager = LogManager.Instance;
        UnitManager unitManager = UnitManager.Instance;
        List<Unit> allUnits = new List<Unit>();
        foreach(var unit in unitManager.GetAllUnits())
            allUnits.Add(unit);
        foreach (var unit in allUnits) {
            TrigActionType? type = null;
            UnitDestroyedLog unitDestroyedLog = null;
            float percentHealth = 100f * (float)unit.GetCurrentHealth() / (float)unit.GetMaxHealth();
            if ((percentHealth <= Setting.retreatHpPercent) && (unit.GetCurrentHealth() > 0)) {
                if (SceneData.stageNumber >= Setting.retreatOpenStage && !unit.IsObject) {
                    unitDestroyedLog = new UnitDestroyedLog(unit);
                    type = TrigActionType.Retreat;
                }
            }
            else if (unit.GetCurrentHealth() <= 0) {
                unitDestroyedLog = new UnitDestroyedLog(unit);
                type = TrigActionType.Kill;
            }
            else if (unit.CheckReach()) {
                unitDestroyedLog = new UnitDestroyedLog(unit);
                type = TrigActionType.Reach;
            }
            if (unitDestroyedLog != null) {
                logManager.Record(unitDestroyedLog);
                logManager.Record(new DestroyUnitLog(unit, (TrigActionType)type));
                UnitManager.Instance.TriggerOnUnitDestroy(unit, (TrigActionType)type);
                yield return unitDestroyedLog;
            }
        }
    }

    public void TriggerOnUnitDestroy(Unit destroyedUnit, TrigActionType actionType) {
        foreach (var unit in units) {
            foreach (var passive in unit.passiveSkillList) {
                passive.SkillLogic.TriggerOnUnitDestroy(unit, destroyedUnit, actionType);
            }
        }
        if (actionType == TrigActionType.Kill) {
            foreach (var hitInfo in destroyedUnit.GetLatelyHitInfos()) {
                List<PassiveSkill> passiveSkills = hitInfo.caster.GetLearnedPassiveSkillList();
                SkillLogicFactory.Get(passiveSkills).TriggerOnKill(hitInfo, destroyedUnit);

                if (hitInfo.skill != null)
                    SkillLogicFactory.Get(hitInfo.skill).OnKill(hitInfo);
            }
        }
    }
    public void DeleteDestroyedUnit(Unit destroyedUnit) {
        units.Remove(destroyedUnit);
        readiedUnits.Remove(destroyedUnit);
    }


    public int GetStandardActivityPoint(){
		return standardActivityPoint;
	}

	public void SetStandardActivityPoint(){
		standardActivityPoint = GameData.PartyData.level + 60;
	}

	public void ApplyAIInfo(){
		List<AIInfo> aiInfoList = Parser.GetParsedData<AIInfo>();
		aiInfoList.ForEach(aiInfo => {
			int index = aiInfo.index;
			Unit targetUnit = GetAllUnits().Find(unit => unit.GetIndex() == index);
			if(targetUnit == null){
				Debug.Log("Unit Number " + index + " is null");
			}
			AIData _AIData = targetUnit.gameObject.GetComponent<AIData>();
			_AIData.SetAIInfo(aiInfo);
			_AIData.SetGoalArea(targetUnit);
			Battle.Turn.AI _AI = targetUnit.gameObject.AddComponent<Battle.Turn.AI>();
			_AI.Initialize(targetUnit, _AIData);
			targetUnit.SetAI(_AI);
			targetUnit.SetAsAI();
		});
	}

	public IEnumerator GenerateUnits(){
		List<UnitInfo> unitInfoList = Parser.GetParsedData<UnitInfo>();
		int generatedPC = 0;
		int enemyCount = 0;

		ReadyManager RM = FindObjectOfType<ReadyManager>();
		List<string> controllableUnitNameList = new List<string>();
		//UnitInfo들을 받아와서 그것이 unselected이면 선택된 PC 정보로 대체한다
		foreach (var unitInfo in unitInfoList){
			string PCName = "";
			if (unitInfo.nameKor == "unselected") {
				if (generatedPC >= RM.selectedUnits.Count) continue;
				PCName = RM.selectedUnits [generatedPC].name;
			}
			else if (unitInfo.nameKor.Length >= 2 && unitInfo.nameKor.Substring(0,2) == "PC") {
				PCName = unitInfo.nameKor.Substring(2, unitInfo.nameKor.Length-2);
			}
			
			if(PCName != ""){
				unitInfo.nameKor = UnitInfo.ConvertToKoreanName (PCName);
				controllableUnitNameList.Add(PCName);
					
				if (unitInfo.nameKor != "Empty") {
					unitInfo.SetPCData(PCName);
					generatedPC += 1;
				}
			}

			if(unitInfo.side == Side.Enemy){
				enemyCount += 1;
			}
		}

		//조건이 PC(또는 적)이고 목표카운트가 0인 트리거를 생성된 '모든' PC(또는 적)의 숫자로 맞춰준다
		List<BattleTrigger> allTriggers = FindObjectOfType<BattleTriggerManager>().triggers;
		Debug.Log("Triggers Count : " + allTriggers.Count);
		List<BattleTrigger> triggersOfAllPC = allTriggers.FindAll (trig => trig.unitType == TrigUnitType.PC && trig.reqCount == 0);
		triggersOfAllPC.ForEach(trig => trig.reqCount = generatedPC);
		List<BattleTrigger> triggersOfAllEnemy = allTriggers.FindAll (trig => trig.unitType == TrigUnitType.Enemy && trig.reqCount == 0);
		triggersOfAllEnemy.ForEach(trig => trig.reqCount = enemyCount);

		unitInfoList = unitInfoList.FindAll(info => info.nameKor != "Empty");

		// 유닛 배치 (자동)
		foreach (var unitInfo in unitInfoList){
			if (unitInfo.nameEng == "unselected") continue;

			if (RM != null && RM.selectedUnits.Any(selectedUnit => selectedUnit.name == unitInfo.nameEng)) continue;

			Unit unit = Instantiate(unitPrefab).GetComponent<Unit>();
			unit.myInfo = unitInfo;
			unit.ApplySkillList(activeSkillList, statusEffectInfoList, tileStatusEffectInfoList, passiveSkillList);

			Vector2 initPosition = unit.GetInitPosition();
			Vector3 respawnPos = FindObjectOfType<TileManager>().GetTilePos(new Vector2(initPosition.x, initPosition.y));
			respawnPos -= new Vector3(0, 0, 0.05f);
			unit.transform.position = respawnPos;

			Tile tileUnderUnit = FindObjectOfType<TileManager>().GetTile((int)initPosition.x, (int)initPosition.y);
			tileUnderUnit.SetUnitOnTile(unit);
			units.Add(unit);
        }

		// 배치 가능 위치 표시 & 카메라 이동
		var selectablePlaceList = new List<PlaceInfo>();
		var selectableTileList = new List<Tile>();
		if (RM != null) {
			selectablePlaceList = Parser.GetPlacesInfo(SceneData.stageNumber);
			selectableTileList = new List<Tile>();
			selectablePlaceList.ForEach(sp => selectableTileList.Add(BattleData.tileManager.GetTile(sp.position)));

			BattleData.tileManager.PaintTiles(selectableTileList, TileColor.Blue);
			BattleData.tileManager.PreselectTiles(selectableTileList);
            
            Vector2 position = selectableTileList.Last().realPosition;
            //event log가 나타나기 전이므로 로그를 남기지 않고 수동으로 조작
            Camera.main.transform.position = new Vector3(position.x, position.y, -10); 
            //BattleManager.MoveCameraToTile(selectableTileList.Last());
		}

		yield return StartCoroutine(GenerateUnitsByManual(unitInfoList, selectablePlaceList));
        
        startFinished = true;

		// 배치 가능 위치 지우고 턴 시작
		if(FindObjectOfType<PlacedUnitCheckPanel>() != null){
			FindObjectOfType<PlacedUnitCheckPanel>().SetText("배치를 이대로 확정할까요?");
		}

		BattleData.tileManager.DepaintAllTiles(TileColor.Blue);
			
		units.ForEach(unit => {
			if (controllableUnitNameList.Contains(unit.GetNameEng())){
				Destroy(unit.GetComponent<AIData>());
			}
		});

		if(RM != null) {Destroy(RM.gameObject);}
		yield return null;
		BattleData.battleManager.BattleModeInitialize();
	}

	public IEnumerator GenerateUnitsByManual(List<UnitInfo> unitInfoList, List<PlaceInfo> selectablePlaceList) {
		ReadyManager RM = FindObjectOfType<ReadyManager>();
		
		// 유닛 배치 (수동)
		foreach (var unitInfo in unitInfoList){
			if (RM == null) continue;
			if (unitInfo.nameEng == "unselected") continue;
			if (!RM.selectedUnits.Any(selectedUnit => selectedUnit.name == unitInfo.nameEng)) continue;

			Debug.Log("unit add ready : " + unitInfo.nameEng);
			FindObjectOfType<PlacedUnitCheckPanel>().HighlightPortrait(unitInfo.nameEng);
			BattleData.isWaitingUserInput = true;
			yield return StartCoroutine(EventTrigger.WaitOr(BattleData.battleManager.triggers.tileSelectedByUser));
			BattleData.isWaitingUserInput = false;
		
			Vector2 triggeredPos = BattleData.move.selectedTilePosition; 
			Tile triggeredTile = BattleData.tileManager.GetTile(triggeredPos);

			Unit unit = Instantiate(unitPrefab).GetComponent<Unit>();
			unit.myInfo = unitInfo;
			unit.ApplySkillList(RM.selectedUnits, statusEffectInfoList, tileStatusEffectInfoList);
			//unit.ApplySkillList(activeSkillList, statusEffectInfoList, tileStatusEffectInfoList, passiveSkillList);

			Vector2 initPosition = triggeredTile.GetTilePos();
			Vector3 respawnPos = FindObjectOfType<TileManager>().GetTilePos(new Vector2(initPosition.x, initPosition.y));
			respawnPos -= new Vector3(0, 0, 0.05f);
			unit.transform.position = respawnPos;

			Direction triggerDirection = selectablePlaceList.Find(sp => sp.position == triggeredPos).direction;
			unit.SetDirection(triggerDirection);

			Tile tileUnderUnit = FindObjectOfType<TileManager>().GetTile((int)initPosition.x, (int)initPosition.y);
			tileUnderUnit.SetUnitOnTile(unit);
			units.Add(unit);
            Debug.Log(unit.GetNameKor() + "Added");

			List<Tile> triggeredTiles = new List<Tile>();
			triggeredTiles.Add(triggeredTile);
			BattleData.tileManager.DepaintTiles(triggeredTiles, TileColor.Blue);
			BattleData.tileManager.DepreselectTiles(triggeredTiles);
		}
	}

	public List<Unit> GetUpdatedReadiedUnits(){
		readiedUnits.Clear();
		// check each unit and add all readied units.
		foreach (var unit in units){
			// 오브젝트의 턴은 돌아오지 않는다
			if (unit.IsObject) continue;

			if (unit.GetCurrentActivityPoint() >= standardActivityPoint){
				readiedUnits.Add(unit);
			}
		}

		// AP가 큰 순서대로 소팅.
		readiedUnits.Sort(SortHelper.Chain(new List<Comparison<Unit>>{
			SortHelper.CompareBy<Unit>(go => go.GetCurrentActivityPoint()),
			SortHelper.CompareBy<Unit>(go => go.GetStat(Stat.Agility)),
			SortHelper.CompareBy<Unit>(go => go.gameObject.GetInstanceID())
		}, reverse:true));

		return readiedUnits;
	}

	public List<Unit> GetEnemyUnits(){
		List<Unit> enemyUnits = new List<Unit> ();
        foreach (var unit in units){
            if (unit.GetSide() == Side.Enemy){
                enemyUnits.Add(unit);
            }
        }
        return enemyUnits;
    }

	public List<Unit> GetEnemyUnitsToThisAIUnit(Unit AIUnit){
		List<Unit> enemyUnits = new List<Unit> ();
		foreach (var unit in units){
			if (unit.IsSeenAsEnemyToThisAIUnit(AIUnit))
				enemyUnits.Add(unit);
		}
		return enemyUnits;
	}

	public void ApplyEachDOT() {
        List<Unit> unitList = new List<Unit>();
        units.ForEach(x => unitList.Add(x));
        foreach (var unit in unitList){
			if (unit != null && unit.HasStatusEffect(StatusEffectType.DamageOverPhase))
				unit.ApplyDamageOverPhase();
		}
	}

    public void ApplyEachHeal() {
        List<Unit> unitList = new List<Unit>();
        units.ForEach(x => unitList.Add(x));
        foreach (var unit in unitList) {
            if(unit != null && unit.HasStatusEffect(StatusEffectType.HealOverPhase))
                unit.ApplyHealOverPhase();
        }
    }

    public IEnumerator StartPhase(int phase) {
		ApplyEachHeal();
		ApplyEachDOT();
        foreach (var unit in units) {
            unit.ResetMovedTileCount();
			unit.UpdateStartPosition();
			unit.ApplyTriggerOnPhaseStart(phase);
        }
        TileManager.Instance.TriggerTileStatusEffectsAtPhaseStart();
        yield return LogManager.Instance.ExecuteLastEventLogAndConsequences();
    }

	public void EndPhase(int phase){
		// Decrease each buff & debuff phase
		foreach (var unit in units){
			unit.UpdateRemainPhaseAtPhaseEnd();
			unit.UpdateSkillCooldown();
		}

		foreach (var unit in units) {unit.RegenerateActionPoint();}
		//행동력 회복시킨 후 순서 정렬하는 역할
		UpdateUnitOrder();

		foreach (var unit in units) {unit.ApplyTriggerOnPhaseEnd();}
	}

    void LoadUnitStatusEffects(){
        statusEffectInfoList = Parser.GetParsedData<UnitStatusEffectInfo>();
    }

    void LoadTileStatusEffects(){
        tileStatusEffectInfoList = Parser.GetParsedTileStatusEffectInfo();
    }

    public void ReadTileBuffInfos() {
        if(BattleData.tileBuffInfos.Count != 0)
            return;
        foreach (var statusEffectInfo in statusEffectInfoList) {
            UnitStatusEffect.FixedElement statusEffectToAdd = statusEffectInfo.GetStatusEffect();
            if (statusEffectInfo.GetOwnerOfSkill() == "tile") {
                switch (statusEffectToAdd.actuals[0].statusEffectType) {
                case StatusEffectType.PowerChange:
                    BattleData.tileBuffInfos.Add(Element.Fire, statusEffectToAdd);
                    break;
                case StatusEffectType.DefenseChange:
                    BattleData.tileBuffInfos.Add(Element.Metal, statusEffectToAdd);
                    break;
                case StatusEffectType.SpeedChange:
                    BattleData.tileBuffInfos.Add(Element.Water, statusEffectToAdd);
                    break;
                case StatusEffectType.HealOverPhase:
                    BattleData.tileBuffInfos.Add(Element.Plant, statusEffectToAdd);
                    break;
                default:
                    Debug.Log("fail reading tile buff infos");
                    break;
                }
            }
        }
    }

    void Start() {
		GameData.PartyData.CheckLevelData();
		activeSkillList = Parser.GetParsedData<ActiveSkill>();
		passiveSkillList = Parser.GetParsedData<PassiveSkill>();
        LoadUnitStatusEffects();
        LoadTileStatusEffects();
	}

	public void StartByBattleManager() {
		if (!GameData.SceneData.isTestMode) {
            ApplyAIInfo();
        }
        ReadTileBuffInfos();
        GetEnemyUnits();
		foreach (var unit in GetAllUnits()) {
			unit.healthViewer.SetInitHealth(unit.GetMaxHealth(), unit.myInfo.side, unit.IsAI, unit.myInfo.isNamed);
		}
	}

	public void UpdateUnitOrder (){
		int standardActivityPoint = GetStandardActivityPoint();
		List<Unit> currentPhaseUnits =
			units.FindAll(go => go.GetCurrentActivityPoint() >= standardActivityPoint);
		List<Unit> nextPhaseUnits =
			units.FindAll(go => go.GetCurrentActivityPoint() < standardActivityPoint);

		currentPhaseUnits.Sort(SortHelper.Chain(new List<Comparison<Unit>>{
			SortHelper.CompareBy<Unit>(go => go.GetCurrentActivityPoint()),
			SortHelper.CompareBy<Unit>(go => go.GetStat(Stat.Agility)),
			SortHelper.CompareBy<Unit>(go => go.gameObject.GetInstanceID())
		}, reverse:true));

		nextPhaseUnits.Sort(SortHelper.Chain(new List<Comparison<Unit>>
		{
			SortHelper.CompareBy<Unit>(go => {
					int currentAP = go.GetCurrentActivityPoint();
					int recover = go.GetStat(Stat.Agility);
					return currentAP + recover;
			}),
			SortHelper.CompareBy<Unit>(go => go.GetStat(Stat.Agility)),
			SortHelper.CompareBy<Unit>(go => go.gameObject.GetInstanceID())
		}, reverse:true));

	   // 유닛 전체에 대해서도 소팅. 변경점이 있을때마다 반영된다.
		units.Clear();
		units.AddRange(currentPhaseUnits);
		units.AddRange(nextPhaseUnits);
	}
}