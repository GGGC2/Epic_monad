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
	public List<Unit> allUnits = new List<Unit>();
	List<Unit> readiedUnits = new List<Unit>();
	List<Unit> deadUnits = new List<Unit>();
	List<Unit> retreatUnits = new List<Unit>();
    List<Unit> enemyUnits = new List<Unit>();
    List<Collectible> collectibles = new List<Collectible>();
    
	public List<Unit> GetAllUnits(){
		return allUnits;
	}    

    public List<Collectible> GetCollectibles() {
        return collectibles;
    }

	public Unit GetAnUnit(string engName){
		Unit wantedUnit = null;
		foreach(Unit unit in GetAllUnits()){
			if (unit.EngName == engName) {
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

    public void CheckCollectableObjects() {
        foreach(var collectible in collectibles) {
            if (collectible.unit == null) continue; // 유닛 정보가 없는 collectible에 접근하는 경우가 있음
            int distance = Utility.GetDistance(BattleData.turnUnit.GetPosition(), collectible.unit.GetPosition());
            if(distance <= collectible.range) {
                Debug.Log(BattleData.turnUnit.EngName + " " + collectible.unit.EngName + "수집 중");
                UIManager.Instance.AddCollectableActionButton();
                BattleData.nearestCollectible = collectible;
                return;
            }
        }
        BattleData.nearestCollectible = null;
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

		// 특수 상태이상(기절, 속박, 침묵)이 갱신되었는지 체크
        GetAllUnits().ForEach(unit => unit.UpdateCCIcon());
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
    
    public IEnumerable<EventLog> CheckDestroyedUnits() {
        LogManager logManager = LogManager.Instance;
        UnitManager unitManager = UnitManager.Instance;
        List<Unit> allUnits_Clone = new List<Unit>();
        foreach(var unit in unitManager.GetAllUnits())
            allUnits_Clone.Add(unit);

        foreach(var unit in allUnits_Clone) {
            TrigActionType? type = null;
            UnitDestroyedLog unitDestroyedLog = null;
            int retreatHP = (int)(unit.GetMaxHealth () * Setting.retreatHPFloat);
            if(unit.CheckEscape()){
                unitDestroyedLog = new UnitDestroyedLog (new List<Unit>{ unit });
                type = TrigActionType.Escape;
            }else if(unit.GetCurrentHealth() <= 0){
                unitDestroyedLog = new UnitDestroyedLog (new List<Unit>{ unit });
                if(unit.IsKillable) {type = TrigActionType.Kill;}
                else {type = TrigActionType.Retreat;}
            }else if(unit.GetCurrentHealth() <= retreatHP && unit.CanRetreatBefore0HP){
                unitDestroyedLog = new UnitDestroyedLog (new List<Unit>{ unit });
                type = TrigActionType.Retreat;
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
        ChainList.RemoveChainOfThisUnit(destroyedUnit);
        foreach (var unit in allUnits) {
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
        allUnits.Remove(destroyedUnit);
        readiedUnits.Remove(destroyedUnit);
    }


    public int GetStandardActivityPoint(){
		return standardActivityPoint;
	}

	public void SetStandardActivityPoint(){
		standardActivityPoint = GameData.PartyData.level + 60;
	}

    public void ApplyAIInfo(Unit unit, int index) {
        AIInfo aiInfo = Parser.GetParsedData<AIInfo>(index);
        if (aiInfo == null)
            return;
        AIData _AIData = unit.gameObject.GetComponent<AIData>();
        _AIData.SetAIInfo(aiInfo);
        _AIData.SetGoalArea(unit);
        Battle.Turn.AI _AI = unit.gameObject.AddComponent<Battle.Turn.AI>();
        _AI.Initialize(unit, _AIData);
        unit.SetAI(_AI);
        unit.SetAsAI();
    }

	public IEnumerator GenerateUnits(){
		int generatedPC = 0;
        List<UnitInfo> unitInfoList = Parser.GetParsedData<UnitInfo>();

        ReadyManager RM = FindObjectOfType<ReadyManager>();
		List<string> controllableUnitNameList = new List<string>();
		//UnitInfo들을 받아와서 그것이 unselected이면 선택된 PC 정보로 대체한다
		foreach (var unitInfo in unitInfoList){
			string PCName = "";
			if (unitInfo.nameKor == "unselected") {
				if (generatedPC >= RM.selectedUnits.Count) continue;
				PCName = RM.selectedUnits [generatedPC].name;
			}else if (unitInfo.nameKor.Length >= 2 && unitInfo.nameKor.Substring(0,2) == "PC") {
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
		}

		//target 조건이 PC(또는 적)이고 목표카운트가 0인 트리거를 생성된 '모든' PC(또는 적)의 숫자로 맞춰준다
		List<BattleTrigger> allTriggers = FindObjectOfType<BattleTriggerManager>().triggers;
		Debug.Log("Triggers Count : " + allTriggers.Count);
		List<BattleTrigger> triggersOfAllPC = allTriggers.FindAll (trig => trig.target == TrigUnitType.PC && trig.reqCount == 0);
		triggersOfAllPC.ForEach(trig => trig.reqCount = generatedPC);
		List<BattleTrigger> triggersOfAllEnemy = allTriggers.FindAll (trig => trig.target == TrigUnitType.Enemy && trig.reqCount == 0);
		triggersOfAllEnemy.ForEach(trig => trig.reqCount = unitInfoList.FindAll(info => info.side == Side.Enemy).Count);

		unitInfoList = unitInfoList.FindAll(info => info.nameKor != "Empty");
        
        GenerateUnitsAutomatically(unitInfoList);

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

		yield return StartCoroutine(GenerateUnitsManually(unitInfoList, selectablePlaceList));
        
        startFinished = true;

		// 배치 가능 위치 지우고 턴 시작
		if(FindObjectOfType<PlacedUnitCheckPanel>() != null){
			FindObjectOfType<PlacedUnitCheckPanel>().SetText("배치를 이대로 확정할까요?");
		}

		BattleData.tileManager.DepaintAllTiles(TileColor.Blue);
			
		allUnits.ForEach(unit => {
			if (controllableUnitNameList.Contains(unit.EngName)){
				Destroy(unit.GetComponent<AIData>());
			}
		});
        
        var Allies = allUnits.FindAll(unit => unit.GetSide() == Side.Ally);
        Vector2 averagePositionPC = new Vector2(0, 0);
        Allies.ForEach(ally => {
            averagePositionPC.x += ally.transform.position.x;
            averagePositionPC.y += ally.transform.position.y;
        });
        averagePositionPC /= generatedPC;
        Camera.main.transform.position = new Vector3(averagePositionPC.x, averagePositionPC.y, -10);

		if(RM != null) {Destroy(RM.gameObject);}
		yield return null;
		BattleData.battleManager.BattleModeInitialize();
	}

    void GenerateUnitsAutomatically(List<UnitInfo> unitInfoList) {
        ReadyManager RM = FindObjectOfType<ReadyManager>();
        // 유닛 배치 (자동)
        foreach (var unitInfo in unitInfoList) {
            if(unitInfo.index < 0)  continue;

            if (unitInfo.nameEng == "unselected") continue;

            if (RM != null && RM.selectedUnits.Any(selectedUnit => selectedUnit.name == unitInfo.nameEng)) continue;

            GenerateUnitWith(unitInfo, false);
        }
    }

	IEnumerator GenerateUnitsManually(List<UnitInfo> unitInfoList, List<PlaceInfo> selectablePlaceList) {
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
            unitInfo.initPosition = triggeredPos;

			GenerateUnitWith(unitInfo, true);

			List<Tile> triggeredTiles = new List<Tile>();
			triggeredTiles.Add(triggeredTile);
			BattleData.tileManager.DepaintTiles(triggeredTiles, TileColor.Blue);
			BattleData.tileManager.DepreselectTiles(triggeredTiles);
		}
    }

    public void GenerateUnitsAtPosition(int index, List<Vector2> positions, List<Direction> directions) {
        //info의 index가 index인 유닛을 positions에, directions의 방향으로 생성
        for (int i = 0; i < positions.Count; i++) {
            UnitInfo unitInfo = Parser.GetParsedData<UnitInfo>(index);

            int range = 0;
            Vector2? position = null;
            do {
                foreach (var tile in TileManager.Instance.GetTilesInRange(RangeForm.Diamond, positions[i], 0, range, 0, Direction.Down)) {
                    if (!tile.IsUnitOnTile()) {
                        position = tile.GetTilePos();
                        break;
                    }
                }
                range++;
            } while (position == null);

            unitInfo.initPosition = (Vector2)position;
            unitInfo.initDirection = directions[i];

            Unit unit = GenerateUnitWith(unitInfo, false);
            
            LogManager.Instance.Record(new CameraMoveLog(unit.transform.position));
            LogManager.Instance.Record(new WaitForSecondsLog(0.5f));
        }
    }

    public Unit GenerateUnitWith(UnitInfo unitInfo, bool isSelectedFromReadyScene) {
        Unit unit = Instantiate(unitPrefab).GetComponent<Unit>();
        unit.myInfo = unitInfo;
        List<Skill> skills = new List<Skill>();
        List<PassiveSkill> passiveSkills = new List<PassiveSkill>();
        if (!isSelectedFromReadyScene) {
            skills = activeSkillList.FindAll(skill => skill.owner == unitInfo.nameEng && skill.requireLevel <= PartyData.level).
                            Cast<Skill>().ToList();
            if (SceneData.stageNumber >= Setting.passiveOpenStage) {
                skills.AddRange(passiveSkillList.FindAll(skill => skill.owner == unitInfo.nameEng && skill.requireLevel <= PartyData.level).
                            Cast<Skill>());
            }
        }
        else {
            ReadyManager RM = FindObjectOfType<ReadyManager>();
            skills = RM.selectedUnits.Find(info => info.name == unitInfo.nameEng).selectedSkills;
        }
        unit.ApplySkillList(skills, statusEffectInfoList, tileStatusEffectInfoList);

        Vector2 initPosition = unit.GetInitPosition();
        unit.transform.position = FindObjectOfType<TileManager>().GetTilePos(new Vector2(initPosition.x, initPosition.y)) - new Vector3(0, 0, 0.05f);

        Tile tileUnderUnit = FindObjectOfType<TileManager>().GetTile((int)initPosition.x, (int)initPosition.y);
        tileUnderUnit.SetUnitOnTile(unit);
        allUnits.Add(unit);
        ApplyAIInfo(unit, unitInfo.index);
        unit.healthViewer.SetInitHealth(unit.myInfo.baseStats[Stat.MaxHealth], unit.myInfo.side, unit.IsAI, unit.myInfo.isNamed);
        return unit;
    }

    public List<Unit> GetUpdatedReadiedUnits(){
		readiedUnits.Clear();
		// check each unit and add all readied units.
		foreach (var unit in allUnits){
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
        foreach (var unit in allUnits){
            if (unit.GetSide() == Side.Enemy){
                enemyUnits.Add(unit);
            }
        }
        return enemyUnits;
    }

	public List<Unit> GetEnemyUnitsToThisAIUnit(Unit AIUnit){
		List<Unit> enemyUnits = new List<Unit> ();
		foreach (var unit in allUnits){
			if (unit.IsSeenAsEnemyToThisAIUnit(AIUnit))
				enemyUnits.Add(unit);
		}
		return enemyUnits;
	}

	public void ApplyEachDOT() {
        List<Unit> unitList = new List<Unit>();
        allUnits.ForEach(x => unitList.Add(x));
        foreach (var unit in unitList){
			if (unit != null && unit.HasStatusEffect(StatusEffectType.DamageOverPhase))
				unit.ApplyDamageOverPhase();
		}
	}

    public void ApplyEachHeal() {
        List<Unit> unitList = new List<Unit>();
        allUnits.ForEach(x => unitList.Add(x));
        foreach (var unit in unitList) {
            if(unit != null && unit.HasStatusEffect(StatusEffectType.HealOverPhase))
                unit.ApplyHealOverPhase();
        }
    }

    public IEnumerator StartPhase(int phase) {
		ApplyEachHeal();
		ApplyEachDOT();
        foreach (var unit in allUnits) {
            unit.ResetMovedTileCount();
			unit.UpdateStartPosition();
			unit.ApplyTriggerOnPhaseStart(phase);
        }
        TileManager.Instance.TriggerTileStatusEffectsAtPhaseStart();
        yield return LogManager.Instance.ExecuteLastEventLogAndConsequences();
    }

	public void EndPhase(int phase){
		// Decrease each buff & debuff phase
		foreach (var unit in allUnits){
			unit.UpdateRemainPhaseAtPhaseEnd();
			unit.UpdateSkillCooldown();
		}

		foreach (var unit in allUnits) {unit.RegenerateActionPoint();}
		//행동력 회복시킨 후 순서 정렬하는 역할
		UpdateUnitOrder();

		foreach (var unit in allUnits) {unit.ApplyTriggerOnPhaseEnd();}
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

    void ReadOtherStatusEffectInfos() {
        foreach (var statusEffectInfo in statusEffectInfoList) {
            if (statusEffectInfo.GetOwnerOfSkill() == "collecting") {
                BattleData.collectingStatusEffectInfo = statusEffectInfo.GetStatusEffect();
            }
        }
    }

    void ReadCollectableObjects() {
        TextAsset csvFile = Resources.Load<TextAsset>("Data/CollectableObjects");
        string[] stageData = Parser.FindRowDataOf(csvFile.text, SceneData.stageNumber.ToString());

		if(stageData != null){
			int num = Int32.Parse(stageData[1]);

        	for (int i = 0; i < num; i++) {
            	collectibles.Add(new Collectible(new List<string> { stageData[3 * i + 2], stageData[3 * i + 3], stageData[3 * i + 4] }));
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
        ReadTileBuffInfos();
        ReadOtherStatusEffectInfos();
        ReadCollectableObjects();
        GetEnemyUnits();
	}

	public void UpdateUnitOrder (){
		int standardActivityPoint = GetStandardActivityPoint();
		List<Unit> currentPhaseUnits =
			allUnits.FindAll(go => go.GetCurrentActivityPoint() >= standardActivityPoint);
		List<Unit> nextPhaseUnits =
			allUnits.FindAll(go => go.GetCurrentActivityPoint() < standardActivityPoint);

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
		allUnits.Clear();
		allUnits.AddRange(currentPhaseUnits);
		allUnits.AddRange(nextPhaseUnits);
	}
}