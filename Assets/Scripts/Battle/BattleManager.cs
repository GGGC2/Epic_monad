using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using Enums;
using LitJson;
using System;
using Battle.Turn;
using Battle.Skills;
using System.Linq;
using GameData;

public class BattleManager : MonoBehaviour{
	bool TurnManagerStarted = false;
	public TutorialManager tutorialManager;
	private static BattleManager instance;
	public static BattleManager Instance{ get { return instance; } }

	void Awake (){
		if (instance != null && instance != this) {
			Destroy (this.gameObject);
			return;
		}else {instance = this;}

		BattleData.Initialize (FindObjectOfType<TileManager> (), FindObjectOfType<UnitManager> (), FindObjectOfType<UIManager> (), this);

        if (!SceneData.isTestMode && !SceneData.isStageMode)
            GameDataManager.Load();

		PartyData.CheckLevelData();
		Load();
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
		BattleData.uiManager.UpdateApBarUI(BattleData.unitManager.GetAllUnits());
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

				BattleData.readiedUnits = BattleData.unitManager.GetUpdatedReadiedUnits ();

				while (BattleData.readiedUnits.Count != 0) {
					BattleData.selectedUnit = BattleData.readiedUnits [0];
					BattleData.uiManager.UpdateApBarUI (BattleData.unitManager.GetAllUnits ());

					if (BattleData.selectedUnit.IsAI) {yield return BattleData.selectedUnit.GetAI().UnitTurn ();}
					else{
						Debug.Log(BattleData.selectedUnit.name + "is NOT AI.");
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

		EndUnitTurn ();
	}

	public void UpdateAPBarAndMoveCameraToSelectedUnit(Unit unit){
		BattleData.uiManager.UpdateApBarUI(BattleData.unitManager.GetAllUnits());
		if (unit == null)
			return;
		FindObjectOfType<CameraMover>().SetFixedPosition(unit.realPosition);
	}
	public void StartUnitTurn(Unit unit){
		BattleData.battleManager.UpdateAPBarAndMoveCameraToSelectedUnit (unit);

		Debug.Log(unit.GetName() + "'s turn");

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

	private void OnOffStandbyButton(){
		bool isPossible = BattleData.selectedUnit.IsStandbyPossible ();
		BattleData.uiManager.commandPanel.OnOffButton (ActionCommand.Standby, isPossible);
	}
	private void OnOffSkillButton()
	{
		bool isPossible = BattleData.selectedUnit.IsSkillUsePossibleState ();
		BattleData.uiManager.commandPanel.OnOffButton (ActionCommand.Skill, isPossible);
	}
	private void OnOffMoveButton(){
		bool isPossible = BattleData.selectedUnit.IsMovePossibleState();
		BattleData.uiManager.commandPanel.OnOffButton (ActionCommand.Move, isPossible);
	}
    

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

	public static IEnumerator DestroyDeadUnits()
	{
		BattleManager battleManager = BattleData.battleManager;

		foreach (Unit deadUnit in BattleData.deadUnits){
			Debug.Log(deadUnit.GetName() + " is dead");
			// 죽은 유닛에게 추가 이펙트.
			deadUnit.GetComponent<SpriteRenderer>().color = Color.red;
			yield return DestroyDeadOrRetreatUnit (deadUnit, BattleTrigger.ActionType.Kill);
		}
	}

	public static IEnumerator DestroyRetreatUnits()
	{
		BattleManager battleManager = BattleData.battleManager;

		foreach (Unit retreatUnit in BattleData.retreatUnits){
			Debug.Log(retreatUnit.GetName() + " retreats");
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
		yield return BattleTriggerManager.CountBattleCondition(unit, deadOrRetreat);
		yield return BattleTriggerManager.CountBattleCondition(unit, BattleTrigger.ActionType.Neutralize);
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

	public static IEnumerator UpdateRetreatAndDeadUnits(BattleManager battleManager)
	{
		BattleData.retreatUnits = BattleData.unitManager.GetRetreatUnits();
		BattleData.deadUnits = BattleData.unitManager.GetDeadUnits();
		yield return battleManager.StartCoroutine(DestroyRetreatUnits());
		yield return battleManager.StartCoroutine(DestroyDeadUnits());
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

	public static IEnumerator AtActionEnd(){
		BattleManager battleManager = BattleData.battleManager;
		// 매 액션이 끝날때마다 갱신하는 특성 조건들
		BattleData.unitManager.ResetLatelyHitUnits();
		BattleData.unitManager.TriggerPassiveSkillsAtActionEnd();
		yield return battleManager.StartCoroutine(BattleData.unitManager.TriggerStatusEffectsAtActionEnd());
		BattleData.unitManager.UpdateStatusEffectsAtActionEnd();
		BattleData.tileManager.UpdateTileStatusEffectsAtActionEnd();

		//승리 조건이 충족되었으면 결과창 출력하기
		BattleTriggerManager Checker = FindObjectOfType<BattleTriggerManager>();
		if(Checker.battleTriggers.Any(trig => trig.resultType == BattleTrigger.ResultType.Win && trig.acquired))
			Checker.InitializeResultPanel();
        FindObjectOfType<CameraMover>().CalculateBoundary();
        // 액션마다 갱신사항 종료
    }

	public UnityEvent readyCommandEvent;

	public IEnumerator PrepareUnitActionAndGetCommand(){
		while (BattleData.currentState == CurrentState.FocusToUnit){
			BattleManager battleManager = BattleData.battleManager;
			Unit unit = BattleData.selectedUnit;

			yield return BeforeActCommonAct ();

			//AI 턴에선 쓸모없는 부분
			BattleData.uiManager.ActivateCommandUIAndSetName(unit);
			OnOffCommandButtons ();

			// (지금은) 튜토리얼용인데 나중에 더 용도를 찾을 수도 있다
			readyCommandEvent.Invoke ();

			//직전에 이동한 상태면 actionCommand 클릭 말고도 우클릭으로 이동 취소도 가능, 아니면 그냥 actionCommand를 기다림
			if (BattleData.alreadyMoved)
				yield return battleManager.StartCoroutine(EventTrigger.WaitOr(BattleData.triggers.actionCommand, BattleData.triggers.rightClicked));
			else
				yield return battleManager.StartCoroutine(BattleData.triggers.actionCommand.Wait());

			if (BattleData.alreadyMoved && BattleData.triggers.rightClicked.Triggered){
				Debug.Log("Apply MoveSnapShot");
				BattleData.selectedUnit.ApplySnapshot();
				BattleData.alreadyMoved = false;
			}
			else if (BattleData.triggers.actionCommand.Data == ActionCommand.Move){
				BattleData.currentState = CurrentState.SelectMovingPoint;
				yield return battleManager.StartCoroutine(MoveStates.SelectMovingPointState());
			}
			else if (BattleData.triggers.actionCommand.Data == ActionCommand.Skill){
				BattleData.currentState = CurrentState.SelectSkill;
				yield return battleManager.StartCoroutine(SkillAndChainStates.SelectSkillState());
			}
			else if (BattleData.triggers.actionCommand.Data == ActionCommand.Rest){
				BattleData.currentState = CurrentState.RestAndRecover;
				yield return battleManager.StartCoroutine(RestAndRecover.Run());
				yield return null;
			}
			else if (BattleData.triggers.actionCommand.Data == ActionCommand.Standby){
				BattleData.currentState = CurrentState.Standby;
				yield return battleManager.StartCoroutine(Standby());
				yield return null;
			}
		}
		yield return null;
	}

	public void MoveCameraToUnitAndDisplayUnitInfoViewer(Unit unit){
		MoveCameraToUnit(unit);
		BattleData.uiManager.SetMovedUICanvasOnUnitAsCenter(unit);
		BattleData.uiManager.SetSelectedUnitViewerUI(unit);
	}
	private void OnOffCommandButtons(){
		OnOffStandbyButton();
		OnOffMoveButton();
		OnOffSkillButton();
	}
	public IEnumerator BeforeActCommonAct(){
		yield return StartCoroutine(UpdateRetreatAndDeadUnits(this));
		yield return AtActionEnd();
		if (IsSelectedUnitRetreatOrDie())
			yield break;
		MoveCameraToUnitAndDisplayUnitInfoViewer(BattleData.selectedUnit);
		BattleData.battleManager.UpdateAPBarAndMoveCameraToSelectedUnit (BattleData.selectedUnit);
	}

	public void CallbackMoveCommand()
	{
		BattleData.uiManager.DisableCommandUI();
		BattleData.triggers.actionCommand.Trigger(ActionCommand.Move);
	}

	public void CallbackSkillCommand()
	{
		BattleData.uiManager.DisableCommandUI();
		BattleData.triggers.actionCommand.Trigger(ActionCommand.Skill);
	}

	public void CallbackRestCommand()
	{
		BattleData.uiManager.DisableCommandUI();
		BattleData.triggers.actionCommand.Trigger(ActionCommand.Rest);
	}

	public void CallbackStandbyCommand()
	{
		BattleData.uiManager.DisableCommandUI();
		BattleData.triggers.actionCommand.Trigger(ActionCommand.Standby);
	}

	public void CallbackOnPointerEnterRestCommand()
	{
		BattleData.previewAPAction = new APAction(APAction.Action.Rest, RestAndRecover.GetRestCostAP());
		BattleData.uiManager.UpdateApBarUI(BattleData.unitManager.GetAllUnits());
	}

	public void CallbackOnPointerExitRestCommand()
	{
		BattleData.previewAPAction = null;
		BattleData.uiManager.UpdateApBarUI(BattleData.unitManager.GetAllUnits());
	}

	public void CallbackCancel()
	{
		BattleData.triggers.cancelClicked.Trigger();
	}

	public static IEnumerator Standby(){
		BattleData.alreadyMoved = false;
		yield return new WaitForSeconds(0.1f);
	}

	public void CallbackSkillIndex(int index){
		BattleData.indexOfSelectedSkillByUser = index;
		BattleData.triggers.skillSelected.Trigger();
		Debug.Log(index + "th skill is selected");
	}

	public void CallbackPointerEnterSkillIndex(int index)
	{
		BattleData.indexOfPreSelectedSkillByUser = index;
	}

	public void CallbackPointerExitSkillIndex(int index)
	{
		if (index == BattleData.indexOfPreSelectedSkillByUser)
		{
			BattleData.indexOfPreSelectedSkillByUser = 0;
		}
	}

	public void CallbackSkillUICancel()
	{
		BattleData.triggers.cancelClicked.Trigger();
	}

	public void CallbackRightClick()
	{
		BattleData.triggers.rightClicked.Trigger();
	}

	public void CallbackDirection(Direction direction){
		BattleData.move.selectedDirection = direction;
		BattleData.triggers.directionSelectedByUser.Trigger();
		BattleData.uiManager.DisableSelectDirectionUI();
	}
	public void CallbackDirectionLong(Direction direction)
	{
		BattleData.move.selectedDirection = direction;
		BattleData.triggers.directionLongSelectedByUser.Trigger();
		BattleData.uiManager.DisableSelectDirectionUI();
	}

	void Update(){
		if (Input.GetMouseButtonDown(1)){
            if (BattleData.enemyUnitSelected || BattleData.tileSelected) {
                BattleData.enemyUnitSelected = false; // 유닛 고정이 되어있을 경우, 고정 해제가 우선으로 된다.
                BattleData.tileSelected = false;
			} else{
				if(!BattleData.rightClickLock)
	                CallbackRightClick(); // 우클릭 취소를 받기 위한 핸들러.
			}
		}

		if (BattleData.currentState != CurrentState.FocusToUnit)
		{
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

		if (Input.GetKeyDown(KeyCode.CapsLock))
		{
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
			BattleData.triggers.tileSelectedByUser.Trigger();
			BattleData.move.selectedTilePosition = position;
		}
	}
	public void OnLongMouseDownHandlerFromTile(Vector2 position){
		if (BattleData.isWaitingUserInput){
			BattleData.triggers.tileLongSelectedByUser.Trigger();
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
		BattleTriggerManager.CountBattleCondition();

		yield return StartCoroutine(BattleData.uiManager.MovePhaseUI(BattleData.currentPhase));
		BattleData.unitManager.StartPhase(BattleData.currentPhase);
        yield return StartCoroutine(BattleData.unitManager.ApplyEachHeal());
		yield return StartCoroutine(BattleData.unitManager.ApplyEachDOT());

		yield return new WaitForSeconds(0.5f);
	}

	//이하는 StageManager의 Load기능 통합
	public TextAsset mapData;
	public TextAsset GetMapData()
	{
		if (loaded == false)
		{
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
	public TextAsset GetBattleConditionData()
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

	public void Load()
	{
		loaded = true;
		GetStageDataFiles();
	}

	void GetStageDataFiles(){
        if (SceneData.isTestMode) {
            mapData = Resources.Load<TextAsset>("Data/EQ_test_map");
            unitData = Resources.Load<TextAsset>("Data/EQ_test_unit");
        } else {
            if (SceneData.stageNumber == 0)
                SceneData.stageNumber = 1;

            TextAsset nextMapFile = Resources.Load<TextAsset>("Data/Stage" + SceneData.stageNumber + "_map");
            mapData = nextMapFile;
            TextAsset nextUnitFile = Resources.Load<TextAsset>("Data/Stage" + SceneData.stageNumber + "_unit");
            unitData = nextUnitFile;
            TextAsset nextAIDataFile = Resources.Load<TextAsset>("Data/Stage" + SceneData.stageNumber + "_AI");
            aiData = nextAIDataFile;
            TextAsset nextBattleConditionFile = Resources.Load<TextAsset>("Data/Stage" + SceneData.stageNumber + "_battleCondition");
            battleConditionData = nextBattleConditionFile;
            TextAsset nextBgmFile = Resources.Load<TextAsset>("Data/Stage" + SceneData.stageNumber + "_bgm");
            bgmData = nextBgmFile;
        }
	}
}