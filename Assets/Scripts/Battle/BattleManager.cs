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
	BattleData.Triggers triggers;

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
		SkillLocation.tileManager = BattleData.tileManager;
	}

	public IEnumerator Start(){
        SoundManager.Instance.PlayBGM("Script_Tense");

		readyCommandEvent = new UnityEvent ();
		AI.SetBattleManager (this);
		ChainList.InitiateChainList ();

		BattleData.unitManager.SetStandardActivityPoint();

		InitCameraPosition(); // temp init position;
		yield return null;
		BattleData.readiedUnits = BattleData.unitManager.GetUpdatedReadiedUnits();
		BattleData.selectedUnit = BattleData.readiedUnits[0];
		BattleData.uiManager.UpdateApBarUI();
	}

	public void StartTurnManager(){
		if(!TurnManagerStarted){
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

	public IEnumerator InstantiateTurnManager(){
		if (BattleData.uiManager.startFinished) {
			while (true) {
				yield return StartCoroutine (StartPhaseOnGameManager ());

				if(BattleData.currentPhase == 1){
					tutorialManager.gameObject.SetActive(true);
				}
				BattleData.readiedUnits = BattleData.unitManager.GetUpdatedReadiedUnits ();

				while (BattleData.readiedUnits.Count != 0) {
					BattleData.selectedUnit = BattleData.readiedUnits [0];
					BattleData.uiManager.UpdateApBarUI();

					if (BattleData.selectedUnit.IsAI){
						yield return BattleData.selectedUnit.GetAI().UnitTurn ();
					}else{
						yield return StartCoroutine (ActionAtTurn (BattleData.selectedUnit));
					}

					BattleData.selectedUnit = null;
					BattleData.readiedUnits = BattleData.unitManager.GetUpdatedReadiedUnits ();
					yield return null;
				}

				//해당 페이즈에 행동할 유닛들의 턴이 모두 끝나면 오브젝트들이 행동한다
				yield return StartCoroutine (ObjectUnitBehaviour.AllObjectUnitsBehave ());
				
				yield return StartCoroutine (EndPhaseOnGameManager ());
			}
		} else {
			Debug.Log ("uiManager is not started.");
		}
	}

	IEnumerator ActionAtTurn(Unit unit){
		StartUnitTurn(unit);

		BattleData.currentState = CurrentState.FocusToUnit;
		yield return StartCoroutine(PrepareUnitActionAndGetCommand());

		if (BattleData.currentState != CurrentState.Dead) {
			EndUnitTurn ();
		}
	}

	public void UpdateAPBarAndMoveCameraToSelectedUnit(Unit unit){
		BattleData.uiManager.UpdateApBarUI();
		if (unit == null)
			return;
		FindObjectOfType<CameraMover>().SetFixedPosition(unit.realPosition);
	}
	public void StartUnitTurn(Unit unit){
		BattleData.battleManager.UpdateAPBarAndMoveCameraToSelectedUnit (unit);

		Debug.Log(unit.GetNameKor() + "'s turn");

		BattleData.selectedUnit = unit;
		BattleData.move = new BattleData.Move();
		BattleData.alreadyMoved = false; // 연속 이동 불가를 위한 변수.
		ChainList.RemoveChainOfThisUnit(BattleData.selectedUnit); // 턴이 돌아오면 자신이 건 체인 삭제.

		BattleData.battleManager.AllPassiveSkillsTriggerOnTurnStart(unit);
		unit.TriggerTileStatusEffectAtTurnStart();

		BattleData.uiManager.SetSelectedUnitViewerUI(BattleData.selectedUnit);
		BattleData.selectedUnit.SetActive();
	}
	public void EndUnitTurn(){
		BattleData.selectedUnit.TriggerTileStatusEffectAtTurnEnd();
		BattleData.uiManager.DisableSelectedUnitViewerUI();
		BattleData.selectedUnit.SetInactive();
	}
	public void AllPassiveSkillsTriggerOnTurnStart(Unit turnStarter){
		foreach(Unit caster in BattleData.unitManager.GetAllUnits())
			SkillLogicFactory.Get(caster.GetLearnedPassiveSkillList()).TriggerOnTurnStart(caster, turnStarter);
	}

	/*private void OnOffSkillButton(){
		bool isPossible = BattleData.selectedUnit.IsSkillUsePossibleState ();
		BattleData.uiManager.commandPanel.OnOffButton (ActionCommand.Skill, isPossible);
	}
    private void SetStandbyButton(){
		BattleData.uiManager.commandPanel.OnOffButton (ActionCommand.Standby, true);
	}*/

	public static IEnumerator FadeOutEffect(Unit unit)
	{
		float time = 0.3f;
		SpriteRenderer sr = unit.gameObject.GetComponent<SpriteRenderer>();
		for (int i = 0; i < 10; i++)
		{
			sr.color -= new Color(0, 0, 0, 0.1f);
			yield return new WaitForSeconds(time / 10f);
		}
	}

	public static IEnumerator DestroyDeadUnits(){
		BattleManager battleManager = BattleData.battleManager;

		foreach (Unit deadUnit in BattleData.deadUnits){
			Debug.Log(deadUnit.GetNameKor() + " is dead");
			// 죽은 유닛에게 추가 이펙트.
			deadUnit.GetComponent<SpriteRenderer>().color = Color.red;
			yield return DestroyDeadOrRetreatUnit (deadUnit, BattleTrigger.ActionType.Kill);
		}
	}

	public static IEnumerator DestroyRetreatUnits()
	{
		BattleManager battleManager = BattleData.battleManager;

		foreach (Unit retreatUnit in BattleData.retreatUnits){
			Debug.Log(retreatUnit.GetNameKor() + " retreats");
			yield return DestroyDeadOrRetreatUnit (retreatUnit, BattleTrigger.ActionType.Retreat);
		}
	}

	public static IEnumerator DestroyDeadOrRetreatUnit(Unit unit, BattleTrigger.ActionType deadOrRetreat)
	{
		ChainList.RemoveChainOfThisUnit (unit);
		yield return BattleData.battleManager.StartCoroutine(FadeOutEffect(unit));
		RemoveAuraEffectFromDeadOrRetreatUnit(unit);
        yield return BattleData.battleManager.StartCoroutine(BattleData.unitManager.DeleteDeadUnit(unit));
		BattleData.unitManager.DeleteRetreatUnit(unit);
		yield return BattleTriggerManager.CountBattleTrigger(unit, deadOrRetreat);
		yield return BattleTriggerManager.CountBattleTrigger(unit, BattleTrigger.ActionType.Neutralize);
		Destroy(unit.gameObject);
	}

    public static void RemoveAuraEffectFromDeadOrRetreatUnit(Unit unit) {
        foreach(var se in unit.GetStatusEffectList()) {
            if(se.IsOfType(StatusEffectType.Aura)) {
                Aura.TriggerOnRemoved(unit, se);
            }
        }
    }

	public static bool IsSelectedUnitRetreatOrDie()
	{
		if (BattleData.retreatUnits.Contains(BattleData.selectedUnit))
			return true;

		if (BattleData.deadUnits.Contains(BattleData.selectedUnit))
			return true;

		return false;
	}

	public IEnumerator UpdateRetreatAndDeadUnits()
	{
		BattleData.retreatUnits = BattleData.unitManager.GetRetreatUnits();
		BattleData.deadUnits = BattleData.unitManager.GetDeadUnits();
		yield return StartCoroutine(DestroyRetreatUnits());
		yield return StartCoroutine(DestroyDeadUnits());
	}

	public static void MoveCameraToUnit(Unit unit)
	{
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
		Vector2 objPos = (Vector2)obj.gameObject.transform.position;
		MoveCameraToPosition (objPos);
	}
	private static void MoveCameraToPosition(Vector2 position)
	{
		Camera.main.transform.position = new Vector3(
				position.x,
				position.y,
				-10);	
	}

	public IEnumerator AtActionEnd(){
		yield return StartCoroutine(UpdateRetreatAndDeadUnits());

		// 매 액션이 끝날때마다 갱신하는 특성 조건들
		BattleData.unitManager.ResetLatelyHitUnits();
		BattleData.unitManager.TriggerPassiveSkillsAtActionEnd();
		if (!IsSelectedUnitRetreatOrDie ()) {
			yield return StartCoroutine (BattleData.unitManager.TriggerStatusEffectsAtActionEnd ());
		}
		BattleData.unitManager.UpdateStatusEffectsAtActionEnd();
		BattleData.tileManager.UpdateTileStatusEffectsAtActionEnd();

		//승리 조건이 충족되었으면 결과창 출력하기
		BattleTriggerManager Checker = FindObjectOfType<BattleTriggerManager>();
		if(Checker.triggers.Any(trig => trig.resultType == BattleTrigger.ResultType.Win && trig.acquired))
			Checker.InitializeResultPanel();
        FindObjectOfType<CameraMover>().CalculateBoundary();
    }

	public UnityEvent readyCommandEvent;

	public IEnumerator PrepareUnitActionAndGetCommand(){
		while (BattleData.currentState == CurrentState.FocusToUnit){
			Unit unit = BattleData.selectedUnit;

			if (IsSelectedUnitRetreatOrDie()) {
				BattleData.currentState = CurrentState.Dead;
				Debug.Log ("Current PC Died.");
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
				update = UpdatePreviewPathAndAP(movableTilesWithPath);
				StartCoroutine(update);
			}//이동 가능한 범위 표시 끝

			//기술 Viewer 끄고(디폴트) 아이콘 불러오기
			UIManager.Instance.skillViewer.gameObject.SetActive(false);
			UIManager.Instance.SetActionButtons();

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

			if(update != null){
				StopCoroutine(update);
			}

			if (BattleData.alreadyMoved && triggers.rightClicked.Triggered){
				Debug.Log("Apply MoveSnapShot");
				BattleData.selectedUnit.ApplySnapshot();
				yield return StartCoroutine(AtActionEnd());
				BattleData.alreadyMoved = false;
			}
			// 길게 눌러서 유닛 상세정보창을 열 수 있다
			else if (triggers.tileLongSelectedByUser.Triggered) {
				Debug.Log("LongClicked trigger");
				Tile triggeredTile = BattleData.SelectedTile;
				if (triggeredTile.IsUnitOnTile()) {
					BattleData.uiManager.ActivateDetailInfoUI(triggeredTile.GetUnitOnTile());
				}
			}else if(triggers.tileSelectedByUser.Triggered && movableTiles.Contains(BattleData.SelectedTile)){
				Tile destTile = BattleData.tileManager.GetTile(BattleData.move.selectedTilePosition);
				List<Tile> destPath = movableTilesWithPath[BattleData.move.selectedTilePosition].path;
				Vector2 currentTilePos = BattleData.selectedUnit.GetPosition();
				Vector2 distanceVector = BattleData.move.selectedTilePosition - currentTilePos;
				int distance = (int)Mathf.Abs(distanceVector.x) + (int)Mathf.Abs(distanceVector.y);
				int totalUseActivityPoint = movableTilesWithPath[BattleData.move.selectedTilePosition].requireActivityPoint;

				BattleData.move.moveCount += distance;

				BattleData.tileManager.DepaintTiles (movableTiles, TileColor.Blue);
				BattleData.tileManager.DepaintAllTiles (TileColor.Red);
				BattleData.tileManager.DepreselectAllTiles ();
				BattleData.currentState = CurrentState.CheckDestination;
				yield return StartCoroutine(MoveStates.CheckDestination(destTile, destPath, totalUseActivityPoint));
			}else if(triggers.skillSelected.Triggered){
				ActiveSkill selectedSkill = BattleData.SelectedSkill;
				UIManager.Instance.selectedUnitViewerUI.GetComponent<BattleUI.UnitViewer>().PreviewAp(BattleData.selectedUnit, selectedSkill.GetRequireAP());
                SkillType skillTypeOfSelectedSkill = selectedSkill.GetSkillType();
                if (skillTypeOfSelectedSkill == SkillType.Auto ||
                    skillTypeOfSelectedSkill == SkillType.Self ||
                    skillTypeOfSelectedSkill == SkillType.Route) {
                    BattleData.currentState = CurrentState.SelectSkillApplyDirection;
                    yield return StartCoroutine(SkillAndChainStates.SelectSkillApplyDirection(BattleData.selectedUnit.GetDirection()));
                }
                else{
                    BattleData.currentState = CurrentState.SelectSkillApplyPoint;
                    yield return StartCoroutine(SkillAndChainStates.SelectSkillApplyPoint(BattleData.selectedUnit.GetDirection()));
                }

                BattleData.previewAPAction = null;
                BattleData.uiManager.UpdateApBarUI();
				UIManager.Instance.selectedUnitViewerUI.GetComponent<BattleUI.UnitViewer>().OffPreviewAp();
			}
			else if (triggers.actionCommand.Data == ActionCommand.Standby){
				if(BattleData.selectedUnit.IsStandbyPossible()){
					BattleData.currentState = CurrentState.Standby;
					yield return StartCoroutine(Standby());
				}else{
					BattleData.currentState = CurrentState.RestAndRecover;
					yield return StartCoroutine(RestAndRecover.Run());
				}
				BattleData.tileManager.DepaintTiles(movableTiles, TileColor.Blue);
			}
		}
		UIManager.Instance.HideActionButtons();
	}

	public void MoveCameraToUnitAndDisplayUnitInfoViewer(Unit unit){
		MoveCameraToUnit(unit);
		BattleData.uiManager.SetMovedUICanvasOnUnitAsCenter(unit);
		BattleData.uiManager.SetSelectedUnitViewerUI(unit);
	}
	public IEnumerator ToDoBeforeAction(){
		MoveCameraToUnitAndDisplayUnitInfoViewer(BattleData.selectedUnit);
		BattleData.battleManager.UpdateAPBarAndMoveCameraToSelectedUnit (BattleData.selectedUnit);
		yield return null;
	}

	public void CallbackMoveCommand(){
		//BattleData.uiManager.DisableCommandUI();
		triggers.actionCommand.Trigger(ActionCommand.Move);
	}

	public void CallbackSkillCommand(){
		//BattleData.uiManager.DisableCommandUI();
		triggers.actionCommand.Trigger(ActionCommand.Skill);
	}

	public void CallbackRestCommand(){
		//BattleData.uiManager.DisableCommandUI();
		triggers.actionCommand.Trigger(ActionCommand.Rest);
	}

	public void CallbackStandbyCommand(){
		//BattleData.uiManager.DisableCommandUI();
		triggers.actionCommand.Trigger(ActionCommand.Standby);
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

	public static IEnumerator Standby(){
		BattleData.alreadyMoved = false;
		yield return new WaitForSeconds(0.1f);
	}

	public void CallbackSkillSelect(ActiveSkill skill){
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
			BattleData.enemyUnitSelected = false;
			BattleData.tileSelected = false;
			if(!BattleData.rightClickLock){
				CallbackRightClick();
			}
		}

		if (BattleData.currentState != CurrentState.FocusToUnit){
            BattleData.tileSelected = false;
			BattleData.enemyUnitSelected = false; // 행동을 선택하면 홀드가 자동으로 풀림.
		}

        if (Input.GetMouseButtonDown(0)) {
            // 유닛 뷰어가 뜬 상태에서 좌클릭하면, 유닛 뷰어가 고정된다. 단, 행동 선택 상태(FocusToUnit)에서만 가능.
            if ((BattleData.currentState == CurrentState.FocusToUnit) && (BattleData.uiManager.IsUnitViewerShowing()))
                BattleData.enemyUnitSelected = true;
            if ((BattleData.currentState == CurrentState.FocusToUnit) && (BattleData.uiManager.IsTileViewerShowing()))
                BattleData.tileSelected = true;
        }

		if (Input.GetKeyDown(KeyCode.CapsLock)){
			BattleTriggerManager Checker = FindObjectOfType<BattleTriggerManager>();
			Checker.InitializeResultPanel ();
		}

		if(Input.GetKeyDown(KeyCode.B))
			SceneManager.LoadScene("BattleReady");
	}

	public bool EnemyUnitSelected()
	{
		return BattleData.enemyUnitSelected;
	}
    public bool TileSelected() {
        return BattleData.tileSelected;
    }

	public void OnMouseEnterHandlerFromTile(Vector2 position){
		if (BattleData.isWaitingUserInput){
			BattleData.preSelectedTilePosition = position;
		}
	}

	public void OnMouseExitHandlerFromTile(Vector2 position)
	{
		if (BattleData.isWaitingUserInput)
		{
			BattleData.preSelectedTilePosition = null;
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

		BattleData.unitManager.EndPhase(BattleData.currentPhase);
        BattleData.tileManager.EndPhase(BattleData.currentPhase);
		yield return new WaitForSeconds(0.5f);
	}

	IEnumerator StartPhaseOnGameManager(){
		BattleData.currentPhase++;
		BattleTriggerManager.CountBattleTrigger();
		HighlightBattleTriggerTiles();

		yield return StartCoroutine(BattleData.uiManager.MovePhaseUI(BattleData.currentPhase));
		BattleData.unitManager.StartPhase(BattleData.currentPhase);
        yield return StartCoroutine(BattleData.unitManager.ApplyEachHeal());
		yield return StartCoroutine(BattleData.unitManager.ApplyEachDOT());

		yield return new WaitForSeconds(0.5f);
	}

	// 승/패 조건과 관련된 타일을 하이라이트 처리
	void HighlightBattleTriggerTiles(){
		List<BattleTrigger> tileTriggers = FindObjectOfType<BattleTriggerManager>().triggers.FindAll(bt => bt.actionType == BattleTrigger.ActionType.Reach);
		tileTriggers.ForEach(trigger => {
			trigger.targetTiles.ForEach(tilePos => BattleData.tileManager.GetTile(tilePos).SetHighlight(true));
		});
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

	private static IEnumerator UpdatePreviewPathAndAP(Dictionary<Vector2, TileWithPath> movableTilesWithPath){
		BattleData.preSelectedTilePosition = null;
		while (true) {
			BattleUI.UnitViewer viewer = GameObject.Find ("SelectedUnitViewerPanel").GetComponent<BattleUI.UnitViewer> ();
			BattleData.tileManager.DepaintAllTiles (TileColor.Red);
			viewer.OffPreviewAp ();
			if (BattleData.preSelectedTilePosition.HasValue == false) {
				BattleData.previewAPAction = null;
			} else {
				var preSelectedTile = BattleData.preSelectedTilePosition.Value;
				if (movableTilesWithPath.ContainsKey (preSelectedTile)){
					movableTilesWithPath[preSelectedTile].tile.transform.position += new Vector3(0, 0, -0.5f);
					int requiredAP = movableTilesWithPath [preSelectedTile].requireActivityPoint;
					BattleData.previewAPAction = new APAction (APAction.Action.Move, requiredAP);
					Tile tileUnderMouse = BattleData.tileManager.preSelectedMouseOverTile;
					tileUnderMouse.CostAP.text = requiredAP.ToString ();
					viewer.PreviewAp (BattleData.selectedUnit, requiredAP);
					foreach (Tile tile in movableTilesWithPath[tileUnderMouse.GetTilePos()].path) {
						tile.PaintTile (TileColor.Red);
					}
				}
			}
			BattleData.uiManager.UpdateApBarUI ();
			yield return null;
		}
	}
}