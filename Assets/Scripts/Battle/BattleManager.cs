using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Enums;
using LitJson;
using Battle.Turn;
using Battle.Skills;
using GameData;

public class BattleManager : MonoBehaviour{
	bool TurnManagerStarted = false;
	public TutorialManager tutorialManager;
	private static BattleManager instance;
	public static BattleManager Instance{ get { return instance; } }
	public BattleData.Triggers triggers;

	void Awake (){
		if (instance != null && instance != this) {
			Destroy (this.gameObject);
			return;
		}else {instance = this;}

		BattleData.Initialize ();
		triggers = BattleData.triggers;

        if (!SceneData.isTestMode && !SceneData.isStageMode){
            GameDataManager.Load();
		}

		Load();
		FindObjectOfType<TileManager>().GenerateTiles(Parser.GetParsedTileInfo());
		PartyData.CheckLevelData();
		TileManager.SetInstance ();
        LogManager.SetInstance();
		SkillLocation.tileManager = BattleData.tileManager;
	}

	public IEnumerator Start(){
		StartCoroutine (SoundManager.Instance.PlayBGM ("Script_Tense"));

		LoadBackgroundImage ();

		readyCommandEvent = new UnityEvent ();
		AI.SetBattleManager (this);
		ChainList.InitiateChainList ();

		BattleData.unitManager.SetStandardActivityPoint();

		InitCameraPosition(); // temp init position;
		yield return null;

		// condition panel이 사라진 이후 유닛 배치 UI가 뜨고, 그 이후 유닛 배치를 해야 하므로 일시정지.
		while (true) {
			if (GameObject.Find("ConditionPanel") == null) break;
			else yield return null;
		}

		yield return StartCoroutine (BattleData.unitManager.GenerateUnits ());
	}

	public void BattleModeInitialize(){
		BattleData.unitManager.StartByBattleManager();

		BattleData.readiedUnits = BattleData.unitManager.GetUpdatedReadiedUnits();
		BattleData.SetSelectedUnit(BattleData.readiedUnits[0]);
		BattleData.uiManager.UpdateApBarUI();
	}

	public void StartTurnManager(){
		if(!TurnManagerStarted) {
            BattleData.logDisplayList = new List<LogDisplay>();
            LogManager.Instance.Record(new BattleStartLog());
            StartCoroutine (InstantiateTurnManager ());
			TurnManagerStarted = true;
		}else {Debug.Log ("TurnManager Already Started.");}
	}

	public int GetCurrentPhase(){
		return BattleData.currentPhase;
	}

	public Unit GetSelectedUnit(){
		return BattleData.selectedUnit;
	}

	void InitCameraPosition(){ Camera.main.transform.position = new Vector3(0, 0, -10); }

	public IEnumerator InstantiateTurnManager() {
        while(!(UnitManager.Instance.startFinished && UIManager.Instance.startFinished))
            yield return null;
        foreach (var unit in UnitManager.Instance.GetAllUnits())
            unit.ApplyTriggerOnStart();
        yield return LogManager.Instance.ExecuteLastEventLogAndConsequences();
        while (true) {
            yield return StartCoroutine(StartPhaseOnGameManager());

			if(BattleData.currentPhase == 1){
				tutorialManager.gameObject.SetActive(true);
			}
			BattleData.readiedUnits = BattleData.unitManager.GetUpdatedReadiedUnits ();

			while (BattleData.readiedUnits.Count != 0){
				//전투에 승리해서 결과창이 나오면 진행 정지
				if(FindObjectOfType<ResultPanel>() != null){
					yield break;
				}

				BattleData.SetSelectedUnit(BattleData.readiedUnits[0]);
				BattleData.uiManager.UpdateApBarUI();

				if (BattleData.selectedUnit.IsAI){
					BattleData.currentState = CurrentState.AITurn;
					yield return BattleData.selectedUnit.GetAI().UnitTurn ();
				}else{
					yield return StartCoroutine (ActionAtTurn (BattleData.selectedUnit));
				}

				BattleData.SetSelectedUnit(null);
					
				BattleData.readiedUnits = BattleData.unitManager.GetUpdatedReadiedUnits ();
				yield return null;
			}

			//해당 페이즈에 행동할 유닛들의 턴이 모두 끝나면 오브젝트들이 행동한다
			yield return StartCoroutine (ObjectUnitBehaviour.AllObjectUnitsBehave ());
			yield return StartCoroutine (EndPhaseOnGameManager ());
		}
	}

	IEnumerator ActionAtTurn(Unit unit) {
        LogManager logManager = LogManager.Instance;
        logManager.Record(new TurnStartLog(unit));
        yield return StartUnitTurn(unit);

		BattleData.currentState = CurrentState.FocusToUnit;
		yield return StartCoroutine(PrepareUnitActionAndGetCommand());

		if (BattleData.currentState != CurrentState.Destroyed) {
            LogManager.Instance.Record(new TurnEndLog(unit));
			yield return EndUnitTurn (unit);
		}
	}

	public void UpdateAPBarAndMoveCameraToSelectedUnit(Unit unit){
		BattleData.uiManager.UpdateApBarUI();
		if (unit == null)
			return;
		FindObjectOfType<CameraMover>().SetFixedPosition(unit.realPosition);
	}
	public IEnumerator StartUnitTurn(Unit unit){
        LogManager logManager = LogManager.Instance;
		BattleData.battleManager.UpdateAPBarAndMoveCameraToSelectedUnit (unit);

		BattleData.SetSelectedUnit(unit);
        BattleData.move = new BattleData.Move();
		BattleData.alreadyMoved = false; // 연속 이동 불가를 위한 변수.
		ChainList.RemoveChainOfThisUnit(BattleData.selectedUnit); // 턴이 돌아오면 자신이 건 체인 삭제.

		BattleData.battleManager.AllPassiveSkillsTriggerOnTurnStart(unit);
        yield return logManager.ExecuteLastEventLogAndConsequences();   // 트랩이 발동하여 새로운 Event가 생길 수도 있으니 실행시켜놓는다.
        unit.TriggerTileStatusEffectAtTurnStart();
        unit.UpdateStatusEffectAtTurnStart();

		BattleData.uiManager.SetSelectedUnitViewerUI(BattleData.selectedUnit);
		BattleData.selectedUnit.ShowArrow();
        yield return logManager.ExecuteLastEventLogAndConsequences();
    }
	public IEnumerator EndUnitTurn(Unit unit) {
        BattleData.selectedUnit.TriggerTileStatusEffectAtTurnEnd();
		BattleData.uiManager.DisableSelectedUnitViewerUI();
		BattleData.selectedUnit.HideArrow();
        yield return LogManager.Instance.ExecuteLastEventLogAndConsequences();
	}
	public void AllPassiveSkillsTriggerOnTurnStart(Unit turnStarter){
		foreach(Unit caster in BattleData.unitManager.GetAllUnits())
			SkillLogicFactory.Get(caster.GetLearnedPassiveSkillList()).TriggerOnTurnStart(caster, turnStarter);
	}

	public static IEnumerator FadeOutEffect(Unit unit){
		float time = 0.3f;
		SpriteRenderer sr = unit.gameObject.GetComponent<SpriteRenderer>();
		for (int i = 0; i < 10; i++)
		{
			sr.color -= new Color(0, 0, 0, 0.1f);
			yield return new WaitForSeconds(time / 10f);
		}
	}

	public static IEnumerator DestroyUnit(Unit unit, TrigActionType actionType){
		BattleManager battleManager = BattleData.battleManager;
        
		if(actionType == TrigActionType.Kill){
			unit.GetComponent<SpriteRenderer>().color = Color.red;
		}

		RemoveAuraEffectFromUnit(unit);
		yield return BattleData.battleManager.StartCoroutine(FadeOutEffect(unit));
        UnitManager.Instance.DeleteDestroyedUnit(unit);

        List<Collectible> collectibles = UnitManager.Instance.GetCollectibles();
        Collectible collectibleToRemove = null;
        foreach (var collectible in collectibles)
            if(collectible.unit == unit)
                collectibleToRemove = collectible;
        if(collectibleToRemove != null)
            collectibles.Remove(collectibleToRemove);

        Destroy(unit.gameObject);
	}

    public static void RemoveAuraEffectFromUnit(Unit unit) {
        foreach(var se in unit.StatusEffectList) {
            if(se.IsOfType(StatusEffectType.Aura)) {
                Aura.TriggerOnRemoved(unit, se);
            }
        }
    }

	public static bool IsSelectedUnitRetreatOrDie(){
		if (BattleData.retreatUnits.Contains(BattleData.selectedUnit))
			return true;

		if (BattleData.deadUnits.Contains(BattleData.selectedUnit))
			return true;

		if(BattleData.selectedUnit.CheckReach()){
			return true;
		}

		return false;
	}

	public static void MoveCameraToUnit(Unit unit){
		MoveCameraToObject (unit);
	}
	public static void MoveCameraToTile(Tile tile)
	{
		MoveCameraToObject (tile);
	}
	private static void MoveCameraToObject(MonoBehaviour obj)
	{
		if (obj == null)
			return;
        LogManager.Instance.Record(new CameraMoveLog(obj.gameObject.transform.position));
	}
	public static IEnumerator SlideCameraToPosition(Vector2 position)
	{
		float time = 0;
		const float MOVINGTIME = 0.1f;
		Vector3 destPos = new Vector3 (position.x, position.y, -10);
		Vector3 currentPos = Camera.main.transform.position;
		Vector3 direction = (destPos - currentPos);
		while (true) {
			time += Time.deltaTime;
			if (time > MOVINGTIME) {
				break;
			}
			Camera.main.transform.position += direction * Time.deltaTime / MOVINGTIME;
			yield return null;
		}
		Camera.main.transform.position = destPos;
	}
	public static void MoveCameraToPosition(Vector2 position)
	{
		Vector3 destPos = new Vector3 (position.x, position.y, -10);
		Camera.main.transform.position = destPos;
	}

	public void CheckTriggers() {
		// 매 액션이 끝날때마다 갱신하는 특성 조건들
        //승리 조건이 충족되었는지 확인
        BattleTriggerManager TrigManager = BattleTriggerManager.Instance;
		List<BattleTrigger> winTriggers = TrigManager.triggers.FindAll(trig => trig.resultType == TrigResultType.Win);
		BattleTrigger.TriggerRelation winTrigRelation = TrigManager.triggers.Find(trig => trig.resultType == TrigResultType.Info).winTriggerRelation;
		//All이나 Sequence이면 전부 달성했을 때, One이면 하나라도 달성했을 때 승리
		if(winTrigRelation == BattleTrigger.TriggerRelation.All || winTrigRelation == BattleTrigger.TriggerRelation.Sequence){
			if(winTriggers.All(trig => trig.acquired)){
				TrigManager.WinGame();
			}
		}else if(winTriggers.Any(trig => trig.acquired)){
			TrigManager.WinGame();
		}
        FindObjectOfType<CameraMover>().CalculateBoundary();
    }

	public UnityEvent readyCommandEvent;

	public IEnumerator PrepareUnitActionAndGetCommand(){
        LogManager logManager = LogManager.Instance;
		while (BattleData.currentState == CurrentState.FocusToUnit){
			Unit unit = BattleData.selectedUnit;
            yield return SlideCameraToPosition(unit.transform.position);

            if (IsSelectedUnitRetreatOrDie()) {
				BattleData.currentState = CurrentState.Destroyed;
				Debug.Log ("Current PC Destroyed.");
				yield break;
			}

			yield return ToDoBeforeAction ();

			BattleData.tileManager.PreselectTiles (BattleData.tileManager.GetTilesInGlobalRange ());
			BattleData.isWaitingUserInput = true;

			Dictionary<Vector2, TileWithPath> movableTilesWithPath = new Dictionary<Vector2, TileWithPath>();
			List<Tile> movableTiles = new List<Tile>();
			IEnumerator update = null;
			//이동 가능한 범위 표시
			if(BattleData.selectedUnit.IsMovePossibleState()){
				movableTilesWithPath = PathFinder.CalculateMovablePaths(BattleData.selectedUnit);
				movableTilesWithPath.Remove (unit.GetPosition ());
				foreach (KeyValuePair<Vector2, TileWithPath> movableTileWithPath in movableTilesWithPath){
					movableTiles.Add(movableTileWithPath.Value.tile);
				}
				BattleData.tileManager.PaintTiles(movableTiles, TileColor.Blue);
				update = UpdatePreviewPathAndAP(movableTiles, movableTilesWithPath);
				StartCoroutine(update);
			}//이동 가능한 범위 표시 끝

			//기술 Viewer 끄고(디폴트) 아이콘 불러오기
			UIManager.Instance.skillViewer.gameObject.SetActive(false);
			UIManager.Instance.SetActionButtons();
            UnitManager.Instance.CheckCollectableObjects();

			// (지금은) 튜토리얼용인데 나중에 더 용도를 찾을 수도 있다
			readyCommandEvent.Invoke ();

			//직전에 이동한 상태면 actionCommand 클릭 말고도 우클릭으로 이동 취소도 가능, 아니면 그냥 actionCommand를 기다림
			if (BattleData.alreadyMoved){
				yield return StartCoroutine(EventTrigger.WaitOr(triggers.actionCommand, triggers.rightClicked, triggers.skillSelected,
																triggers.tileSelectedByUser, triggers.tileLongSelectedByUser));
			}else{
				yield return StartCoroutine(EventTrigger.WaitOr(triggers.actionCommand, triggers.skillSelected,
																triggers.tileSelectedByUser, triggers.tileLongSelectedByUser));
			}
            
            if (update != null){
				StopCoroutine(update);
			}
			unit.HideAfterImage ();
			BattleData.tileManager.DepaintAllTiles ();
			Tile tileUnderMouse = BattleData.tileManager.preSelectedMouseOverTile;
			if (tileUnderMouse != null) {
				tileUnderMouse.CostAP.text = "";
			}

            if (BattleData.alreadyMoved && triggers.rightClicked.Triggered){
				Debug.Log("Apply MoveSnapShot");
                logManager.Record(new MoveCancelLog(unit, BattleData.moveSnapshot));
                BattleData.selectedUnit.ApplySnapshot();
			}
			// 길게 눌러서 유닛 상세정보창을 열 수 있다
			else if (triggers.tileLongSelectedByUser.Triggered) {
				Debug.Log("LongClicked trigger");
				Tile triggeredTile = BattleData.SelectedTile;
				if (triggeredTile.IsUnitOnTile()) {
					BattleData.uiManager.ActivateDetailInfoUI(triggeredTile.GetUnitOnTile());
				}
			}else if(triggers.tileSelectedByUser.Triggered && movableTiles.Contains(BattleData.SelectedTile)){
				BattleData.moveSnapshot = new BattleData.MoveSnapshot(unit);
				Vector2 currentTilePos = BattleData.selectedUnit.GetPosition();
				Vector2 distanceVector = BattleData.move.selectedTilePosition - currentTilePos;
				int distance = (int)Mathf.Abs(distanceVector.x) + (int)Mathf.Abs(distanceVector.y);

				BattleData.move.moveCount += distance;

				BattleData.tileManager.DepreselectAllTiles ();
				BattleData.currentState = CurrentState.CheckDestination;
                Vector2 destPos = BattleData.move.selectedTilePosition;
                logManager.Record(new MoveLog(unit, unit.GetTileUnderUnit().GetTilePos(), destPos));
                MoveStates.MoveToTile(destPos, movableTilesWithPath);
                unit.BreakCollecting();
			}else if(triggers.skillSelected.Triggered){
				yield return StartCoroutine(SkillAndChainStates.SkillSelected());
                BattleData.previewAPAction = null;
                BattleData.uiManager.UpdateApBarUI();
				UIManager.Instance.selectedUnitViewerUI.GetComponent<BattleUI.UnitViewer>().OffPreviewAp();
            }
			else if (triggers.actionCommand.Data == ActionCommand.Standby){
				if(BattleData.selectedUnit.IsStandbyPossible()){
					BattleData.currentState = CurrentState.Standby;
                    logManager.Record(new StandbyLog(unit));
                    Standby(unit);
				}else{
					BattleData.currentState = CurrentState.RestAndRecover;
                    logManager.Record(new RestLog(unit));
                    RestAndRecover.Run();
				}
			}
            else if (triggers.actionCommand.Data == ActionCommand.Collect) {
                BattleData.currentState = CurrentState.Standby;
                logManager.Record(new CollectStartLog(unit, BattleData.nearestCollectible.unit));
                BattleData.selectedUnit.CollectNearestCollectible();
            }
            yield return logManager.ExecuteLastEventLogAndConsequences();
		}
		UIManager.Instance.HideActionButtons();
	}

	public void MoveCameraToUnitAndDisplayUnitInfoViewer(Unit unit) {
		BattleData.uiManager.SetMovedUICanvasOnUnitAsCenter(unit);
		BattleData.uiManager.SetSelectedUnitViewerUI(unit);
	}
	public IEnumerator ToDoBeforeAction(){
		MoveCameraToUnitAndDisplayUnitInfoViewer(BattleData.selectedUnit);
		BattleData.battleManager.UpdateAPBarAndMoveCameraToSelectedUnit (BattleData.selectedUnit);
		yield return null;
	}

	public void CallbackMoveCommand(){
		triggers.actionCommand.Trigger(ActionCommand.Move);
	}

	public void CallbackSkillCommand(){
		triggers.actionCommand.Trigger(ActionCommand.Skill);
	}

	public void CallbackStandbyCommand(){
		triggers.actionCommand.Trigger(ActionCommand.Standby);
	}

    public void CallbackCollectCommand() {
        triggers.actionCommand.Trigger(ActionCommand.Collect);
    }

	public void CallbackOnPointerEnterRestCommand(){
		BattleData.previewAPAction = new APAction(APAction.Action.Rest, RestAndRecover.GetRestCostAP());
		BattleData.uiManager.UpdateApBarUI();
	}

	public void CallbackOnPointerExitRestCommand(){
		BattleData.previewAPAction = null;
		BattleData.uiManager.UpdateApBarUI();
	}

	public void CallbackCancel() {triggers.cancelClicked.Trigger();}

	public static void Standby(Unit unit){
        LogManager.Instance.Record(new WaitForSecondsLog(0.1f));
	}

	public void CallbackSkillSelect(ActiveSkill skill){
		Debug.Log(skill.GetName() + "Selected.");
		BattleData.selectedSkill = skill;
		triggers.skillSelected.Trigger();
	}

	public void CallbackPointerEnterSkillIndex(ActiveSkill skill){
		BattleData.preSelectedSkill = skill;
	}

	public void CallbackPointerExitSkillIndex(){
		BattleData.preSelectedSkill = null;
	}

	public void CallbackSkillUICancel(){
		triggers.cancelClicked.Trigger();
	}

	public void CallbackRightClick(){
		triggers.rightClicked.Trigger();
		BattleData.uiManager.DeactivateDetailInfoUI ();
	}

	public void CallbackDirection(Direction direction){
		BattleData.move.selectedDirection = direction;
		triggers.directionSelectedByUser.Trigger();
		BattleData.uiManager.DisableSelectDirectionUI();
	}
	public void CallbackDirectionLong(Direction direction)
	{
		BattleData.move.selectedDirection = direction;
		triggers.directionLongSelectedByUser.Trigger();
		BattleData.uiManager.DisableSelectDirectionUI();
	}

	void Update(){
		if (Input.GetMouseButtonDown(1)){
			BattleData.unitInUnitViewer = null;
			BattleData.tileInTileViewer = null;
			if(!BattleData.rightClickLock){
				CallbackRightClick();
			}
		}

		if (BattleData.currentState != CurrentState.FocusToUnit){
            BattleData.tileInTileViewer = null;
			BattleData.unitInUnitViewer = null; // 행동을 선택하면 홀드가 자동으로 풀림.
		}

        if (Input.GetMouseButtonDown(0)) {
            // 유닛 뷰어가 뜬 상태에서 좌클릭하면, 유닛 뷰어가 고정된다. 단, 행동 선택 상태(FocusToUnit)에서만 가능.
            if ((BattleData.currentState == CurrentState.FocusToUnit) && (BattleData.uiManager.IsUnitViewerShowing())) {
                BattleData.unitInUnitViewer = BattleData.uiManager.GetUnitInUnitViewer();
            }
            if ((BattleData.currentState == CurrentState.FocusToUnit) && (BattleData.uiManager.IsTileViewerShowing()))
                BattleData.tileInTileViewer = BattleData.uiManager.GetTileInTileViewer();
        }

		if (Input.GetKeyDown(KeyCode.CapsLock)){
			BattleTriggerManager.Instance.WinGame ();
		}
		if (Input.GetKeyDown(KeyCode.Delete)){
			BattleTriggerManager.Instance.LoadLoseScene ();
		}
        if(Input.GetKeyDown(KeyCode.X)) {
            Unit unit = BattleData.unitInUnitViewer;
            if (unit != null) {
                LogManager.Instance.Record(new UnitDestroyedLog(new List<Unit>{ unit }));
                LogManager.Instance.Record(new DestroyUnitLog(unit, TrigActionType.Kill));
            }
        }

		if(Input.GetKeyDown(KeyCode.B))
			SceneManager.LoadScene("BattleReady");

        if(Input.GetKeyDown(KeyCode.A))
            ChangeAspect(1);
        if(Input.GetKeyDown(KeyCode.D))
            ChangeAspect(-1);
        if (Input.GetKeyDown(KeyCode.Escape)) {
            UIManager.Instance.ToggleMenuPanelActive();
        }
	}

    public void ChangeAspect(int direction) { // direction = 1 : 반시계 방향, direction = -1 : 시계방향
        TileManager tileManager = TileManager.Instance;
        UnitManager unitManager = UnitManager.Instance;
        int aspectBefore = (int)BattleData.aspect;
        int aspectAfter = (aspectBefore + direction + 4) % 4;
        BattleData.aspect = (Aspect)aspectAfter;
        tileManager.UpdateRealTilePositions();
        unitManager.UpdateRealUnitPositions(direction);

        CameraMover cm = FindObjectOfType<CameraMover>();
        cm.SetFixedPosition(BattleData.selectedUnit.realPosition);
        cm.CalculateBoundary();
		MoveCameraToPosition(cm.fixedPosition);
    }

	public bool EnemyUnitSelected()
	{
		return BattleData.unitInUnitViewer != null;
	}
    public bool TileSelected() {
        return BattleData.tileInTileViewer != null;
    }

	public void OnMouseEnterHandlerFromTile(Vector2 position){
		if (BattleData.isWaitingUserInput){
			BattleData.mouseOverTilePosition = position;
		}
	}

	public void OnMouseExitHandlerFromTile(Vector2 position)
	{
		if (BattleData.isWaitingUserInput)
		{
			BattleData.mouseOverTilePosition = null;
		}
	}

	public void OnMouseDownHandlerFromTile(Vector2 position){
		if (BattleData.isWaitingUserInput){
			triggers.tileSelectedByUser.Trigger();
			BattleData.move.selectedTilePosition = position;
		}
	}
	public void OnLongMouseDownHandlerFromTile(Vector2 position){
		if (BattleData.isWaitingUserInput){
			triggers.tileLongSelectedByUser.Trigger();
			BattleData.move.selectedTilePosition = position;
		}
	}

	IEnumerator EndPhaseOnGameManager(){
		Debug.Log("Phase End.");
        int phase = BattleData.currentPhase;
        LogManager.Instance.Record(new PhaseEndLog(phase));
        BattleData.unitManager.EndPhase(phase);
        BattleData.tileManager.EndPhase(phase);
        BattleTriggerManager.Instance.CountTriggers(TrigActionType.Phase);
        yield return LogManager.Instance.ExecuteLastEventLogAndConsequences();
        yield return new WaitForSeconds(0.5f);
	}

	IEnumerator StartPhaseOnGameManager(){
		BattleData.currentPhase++;
        int phase = BattleData.currentPhase;
		HighlightBattleTriggerTiles();

		yield return StartCoroutine(BattleData.uiManager.MovePhaseUI(BattleData.currentPhase));
        LogManager.Instance.Record(new PhaseStartLog(phase));
        yield return BattleData.unitManager.StartPhase(BattleData.currentPhase);

		yield return new WaitForSeconds(0.5f);
	}

	// 승/패 조건과 관련된 타일을 하이라이트 처리
	void HighlightBattleTriggerTiles(){
		List<BattleTrigger> tileTriggers = BattleTriggerManager.Instance.triggers.FindAll(
			bt => (bt.actionType == TrigActionType.ReachPosition || bt.actionType == TrigActionType.ReachTile)
			&& (bt.resultType == TrigResultType.Win || bt.resultType == TrigResultType.Lose)
		);

		tileTriggers.ForEach(trigger => {
			trigger.targetTiles.ForEach(tilePos => BattleData.tileManager.GetTile(tilePos).SetHighlight(true));
		});
	}

	void LoadBackgroundImage(){
		string bgImageName = "Dark";
		TextAsset csvFile = Resources.Load("Data/StageBackgrounds") as TextAsset;
		string csvText = csvFile.text;
		string[] unparsedTileInfoStrings = csvText.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 1; i < unparsedTileInfoStrings.Length; i++) {
			StringParser commaParser = new StringParser (unparsedTileInfoStrings [i], ',');
			int stageNum = commaParser.ConsumeInt ();
			if (stageNum == SceneData.stageNumber) {
				bgImageName = commaParser.Consume ();
			}
		}
		Sprite bgSprite = Resources.Load<Sprite>("Background/" + bgImageName);
		GameObject.Find ("BattleBackground").GetComponent<SpriteRenderer>().sprite = bgSprite;
	}

	//이하는 StageManager의 Load기능 통합
	public TextAsset mapData;
	public TextAsset GetMapData(){
		if (loaded == false){
			Load();
		}
		return mapData;
	}

	public TextAsset unitData;
	public TextAsset GetUnitData()
	{
		if (loaded == false)
		{
			Load();
		}
		return unitData;
	}

	public TextAsset aiData;
	public TextAsset GetAIData()
	{
		if (loaded == false)
		{
			Load();
		}
		return aiData;
	}

	public TextAsset battleConditionData;
	public TextAsset GetBattleTriggerData()
	{
		if (loaded == false)
		{
			Load();
		}
		return battleConditionData;
	}
	public TextAsset bgmData;
	public TextAsset GetBgmData()
	{
		if (loaded == false)
		{
			Load();
		}
		return bgmData;
	}

	private bool loaded = false;

	public void Load(){
		loaded = true;
		GetStageDataFiles();
	}

	void GetStageDataFiles(){
        if (SceneData.isTestMode) {
            mapData = Resources.Load<TextAsset>("Data/EQ_test_map");
            unitData = Resources.Load<TextAsset>("Data/EQ_test_unit");
            battleConditionData = Resources.Load<TextAsset>("Data/EQ_test_battleEndCondition");
        } else {
            if (SceneData.stageNumber == 0){
				SceneData.stageNumber = 1;
			}

            TextAsset nextMapFile = Resources.Load<TextAsset>("Data/Stage" + SceneData.stageNumber + "_map");
            mapData = nextMapFile;
            TextAsset nextUnitFile = Resources.Load<TextAsset>("Data/Stage" + SceneData.stageNumber + "_unit");
            unitData = nextUnitFile;
            TextAsset nextAIDataFile = Resources.Load<TextAsset>("Data/Stage" + SceneData.stageNumber + "_AI");
            aiData = nextAIDataFile;
            TextAsset nextBattleTriggerFile = Resources.Load<TextAsset>("Data/Stage" + SceneData.stageNumber + "_battleTrigger");
            battleConditionData = nextBattleTriggerFile;
            TextAsset nextBgmFile = Resources.Load<TextAsset>("Data/Stage" + SceneData.stageNumber + "_bgm");
            bgmData = nextBgmFile;
        }
	}

	private static IEnumerator UpdatePreviewPathAndAP(List<Tile> movableTiles, Dictionary<Vector2, TileWithPath> movableTilesWithPath){
		BattleData.mouseOverTilePosition = null;
		BattleUI.UnitViewer viewer = GameObject.Find ("SelectedUnitViewerPanel").GetComponent<BattleUI.UnitViewer> ();
		Vector2? previousFrameDest = null;
		while (true) {
			if(previousFrameDest == BattleData.mouseOverTilePosition){
				yield return null;
				continue;
			}else{
				previousFrameDest = BattleData.mouseOverTilePosition;
			}

			BattleData.tileManager.DepaintAllTiles (TileColor.Red);
			BattleData.tileManager.PaintTiles(movableTiles, TileColor.Blue);
			Vector2? mouseOverTilePos = BattleData.mouseOverTilePosition;
			if (mouseOverTilePos.HasValue == false || !movableTilesWithPath.ContainsKey (mouseOverTilePos.Value)) {
				viewer.OffPreviewAp ();
				BattleData.previewAPAction = null;
				BattleData.selectedUnit.HideAfterImage();
			}else{
				var preSelectedTile = mouseOverTilePos.Value;
				if (movableTilesWithPath.ContainsKey (preSelectedTile)){
					//movableTilesWithPath[preSelectedTile].tile.transform.position += new Vector3(0, 0, -0.5f);
					int requiredAP = movableTilesWithPath [preSelectedTile].requireActivityPoint;
					BattleData.previewAPAction = new APAction (APAction.Action.Move, requiredAP);
					Tile tileUnderMouse = BattleData.tileManager.preSelectedMouseOverTile;
					tileUnderMouse.CostAP.text = requiredAP.ToString ();
					viewer.PreviewAp (BattleData.selectedUnit, requiredAP);
					List<Tile> path = movableTilesWithPath[tileUnderMouse.GetTilePos()].path;
					BattleData.selectedUnit.SetAfterImageAt(tileUnderMouse.GetTilePos(), 
						Utility.GetFinalDirectionOfPath(tileUnderMouse, path, BattleData.selectedUnit.GetDirection()));
					// path.Add (tileUnderMouse);
					foreach (Tile tile in path) {
						tile.DepaintTile ();
						tile.PaintTile (TileColor.Red);
					}
				}
			}
			BattleData.uiManager.UpdateApBarUI ();
			yield return null;
		}
	}
}