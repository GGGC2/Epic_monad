using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Enums;
using LitJson;
using System;
using Battle.Turn;
using Battle.Skills;
using System.Linq;
using GameData;

public class BattleManager : MonoBehaviour{
	bool startFinished = false;
	bool startTurnManager = false;
	public BattleData battleData = new BattleData();
	private static BattleManager instance;
	public static BattleManager Instance{
		get { return instance; }
	}

	void Awake (){
		instance = this;

        if (!SceneData.isTestMode && !SceneData.isStageMode)
            GameDataManager.Load();

		PartyData.CheckLevelZero();
		Load();
		TileManager.SetInstance ();
		battleData.tileManager = FindObjectOfType<TileManager>();
		SkillLocation.tileManager = battleData.tileManager;
		battleData.unitManager = FindObjectOfType<UnitManager>();
		battleData.uiManager = FindObjectOfType<UIManager>();
		battleData.battleManager = this;
	}

	void Start() {
        SoundManager.Instance.PlayBgm("Script_Tense");

		AI.SetBattleManager (this);
		AI.SetBattleData (battleData);
		ActiveSkill.SetBattleData (battleData);
		SkillAndChainStates.SetBattleData (battleData);
		ChainList.InitiateChainList ();

		battleData.unitManager.SetStandardActivityPoint();
		battleData.selectedUnit = null;
		battleData.currentPhase = 0;

		InitCameraPosition(); // temp init position;

		startFinished = true;
	}

	public void StartTurnManager(){
		if (!startTurnManager){
			StartCoroutine(InstantiateTurnManager());
			startTurnManager = true;
		}
	}

	public int GetCurrentPhase(){
		return battleData.currentPhase;
	}

	public Unit GetSelectedUnit(){
		return battleData.selectedUnit;
	}

	void InitCameraPosition()
	{
		Camera.main.transform.position = new Vector3(0, 0, -10);
	}

	public IEnumerator InstantiateTurnManager(){
        if(startFinished && battleData.uiManager.startFinished){
			while (true){
				yield return StartCoroutine(StartPhaseOnGameManager());

				battleData.readiedUnits = battleData.unitManager.GetUpdatedReadiedUnits();

				while (battleData.readiedUnits.Count != 0) {
					battleData.selectedUnit = battleData.readiedUnits[0];
					battleData.uiManager.UpdateApBarUI(battleData, battleData.unitManager.GetAllUnits());

					if (battleData.selectedUnit.GetComponent<AIData>() != null){
						yield return AI.UnitTurn(battleData.selectedUnit);
					}
					else
						yield return StartCoroutine(ActionAtTurn(battleData.readiedUnits[0]));

					battleData.selectedUnit = null;

					battleData.readiedUnits = battleData.unitManager.GetUpdatedReadiedUnits();
					yield return null;
				}

				//해당 페이즈에 행동할 유닛들의 턴이 모두 끝나면 오브젝트들이 행동한다
				yield return StartCoroutine(ObjectUnitBehaviour.AllObjectUnitsBehave (battleData));

				yield return StartCoroutine(EndPhaseOnGameManager());
			}
		}
	}

	IEnumerator ActionAtTurn(Unit unit){
		StartUnitTurn(unit);

		battleData.currentState = CurrentState.FocusToUnit;
		yield return StartCoroutine(PrepareUnitActionAndGetCommand(battleData));

		EndUnitTurn ();
	}

	public void UpdateAPBarAndMoveCameraToSelectedUnit(Unit unit){
		battleData.uiManager.UpdateApBarUI(battleData, battleData.unitManager.GetAllUnits());
		if (unit == null)
			return;
		FindObjectOfType<CameraMover>().SetFixedPosition(unit.transform.position);
	}
	public void StartUnitTurn(Unit unit){
		battleData.battleManager.UpdateAPBarAndMoveCameraToSelectedUnit (unit);

		Debug.Log(unit.GetName() + "'s turn");

		battleData.selectedUnit = unit;
		battleData.move = new BattleData.Move();
		battleData.alreadyMoved = false; // 연속 이동 불가를 위한 변수.
		ChainList.RemoveChainOfThisUnit(battleData.selectedUnit); // 턴이 돌아오면 자신이 건 체인 삭제.

		battleData.battleManager.AllPassiveSkillsTriggerOnTurnStart(unit);
		unit.TriggerTileStatusEffectAtTurnStart();

		battleData.uiManager.SetSelectedUnitViewerUI(battleData.selectedUnit);
		battleData.selectedUnit.SetActive();
	}
	public void EndUnitTurn(){
		battleData.selectedUnit.TriggerTileStatusEffectAtTurnEnd();

		battleData.uiManager.DisableSelectedUnitViewerUI();
		if (battleData.selectedUnit != null)
			battleData.selectedUnit.SetInactive();
	}
	public void AllPassiveSkillsTriggerOnTurnStart(Unit turnStarter){
		foreach(Unit caster in battleData.unitManager.GetAllUnits())
			SkillLogicFactory.Get(caster.GetLearnedPassiveSkillList()).TriggerOnTurnStart(caster, turnStarter);
	}

	private void OnOffStandbyButton(BattleData battleData){
		bool isPossible = battleData.selectedUnit.IsStandbyPossible (battleData);
		GameObject.Find("StandbyButton").GetComponent<Button>().interactable = isPossible;
	}
	private void OnOffSkillButton(BattleData battleData)
	{
		bool isPossible = battleData.selectedUnit.IsSkillUsePossibleState (battleData);
        GameObject.Find("SkillButton").GetComponent<Button>().interactable = isPossible;
	}
	private void OnOffMoveButton(BattleData battleData){
		bool isPossible = battleData.selectedUnit.IsMovePossibleState(battleData);
		GameObject.Find("MoveButton").GetComponent<Button>().interactable = isPossible;
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

	public static IEnumerator DestroyDeadUnits(BattleData battleData)
	{
		BattleManager battleManager = battleData.battleManager;

		foreach (Unit deadUnit in battleData.deadUnits){
			Debug.Log(deadUnit.GetName() + " is dead");
			// 죽은 유닛에게 추가 이펙트.
			deadUnit.GetComponent<SpriteRenderer>().color = Color.red;
			yield return DestroyDeadOrRetreatUnit (battleData, deadUnit, BattleTrigger.ActionType.Kill);
		}
	}

	public static IEnumerator DestroyRetreatUnits(BattleData battleData)
	{
		BattleManager battleManager = battleData.battleManager;

		foreach (Unit retreatUnit in battleData.retreatUnits){
			Debug.Log(retreatUnit.GetName() + " retreats");
			yield return DestroyDeadOrRetreatUnit (battleData, retreatUnit, BattleTrigger.ActionType.Retreat);
		}
	}

	public static IEnumerator DestroyDeadOrRetreatUnit(BattleData battleData, Unit unit, BattleTrigger.ActionType deadOrRetreat)
	{
		ChainList.RemoveChainOfThisUnit (unit);
		yield return battleData.battleManager.StartCoroutine(FadeOutEffect(unit));
		RemoveAuraEffectFromDeadOrRetreatUnit(unit);
        yield return battleData.battleManager.StartCoroutine(battleData.unitManager.DeleteDeadUnit(unit));
		battleData.unitManager.DeleteRetreatUnit(unit);
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

	static bool IsSelectedUnitRetreatOrDie(BattleData battleData)
	{
		if (battleData.retreatUnits.Contains(battleData.selectedUnit))
			return true;

		if (battleData.deadUnits.Contains(battleData.selectedUnit))
			return true;

		return false;
	}

	public static IEnumerator UpdateRetreatAndDeadUnits(BattleData battleData, BattleManager battleManager)
	{
		battleData.retreatUnits = battleData.unitManager.GetRetreatUnits();
		battleData.deadUnits = battleData.unitManager.GetDeadUnits();
		yield return battleManager.StartCoroutine(DestroyRetreatUnits(battleData));
		yield return battleManager.StartCoroutine(DestroyDeadUnits(battleData));
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

	public static IEnumerator AtActionEnd(BattleData battleData){
		BattleManager battleManager = battleData.battleManager;
		// 매 액션이 끝날때마다 갱신하는 특성 조건들
		battleData.unitManager.ResetLatelyHitUnits();
		battleData.unitManager.TriggerPassiveSkillsAtActionEnd();
		yield return battleManager.StartCoroutine(battleData.unitManager.TriggerStatusEffectsAtActionEnd());
		battleData.unitManager.UpdateStatusEffectsAtActionEnd();
		battleData.tileManager.UpdateTileStatusEffectsAtActionEnd();

		//승리 조건이 충족되었으면 결과창 출력하기
		BattleTriggerManager Checker = FindObjectOfType<BattleTriggerManager>();
		if(Checker.battleTriggers.Any(trig => trig.resultType == BattleTrigger.ResultType.Win && trig.acquired))
			Checker.InitializeResultPanel();
		// 액션마다 갱신사항 종료
	}

	public IEnumerator PrepareUnitActionAndGetCommand(BattleData battleData){
		while (battleData.currentState == CurrentState.FocusToUnit){
			BattleManager battleManager = battleData.battleManager;
			Unit unit = battleData.selectedUnit;

			yield return BeforeActCommonAct ();

			//AI 턴에선 쓸모없는 부분
			battleData.uiManager.ActivateCommandUIAndSetName(unit);
			OnOffCommandButtons ();

			//직전에 이동한 상태면 actionCommand 클릭 말고도 우클릭으로 이동 취소도 가능, 아니면 그냥 actionCommand를 기다림
			if (battleData.alreadyMoved)
				yield return battleManager.StartCoroutine(EventTrigger.WaitOr(battleData.triggers.actionCommand, battleData.triggers.rightClicked));
			else
				yield return battleManager.StartCoroutine(battleData.triggers.actionCommand.Wait());

			if (battleData.alreadyMoved && battleData.triggers.rightClicked.Triggered){
				Battle.Turn.MoveStates.RestoreMoveSnapshot(battleData);
				battleData.alreadyMoved = false;
			}
			else if (battleData.triggers.actionCommand.Data == ActionCommand.Move){
				battleData.currentState = CurrentState.SelectMovingPoint;
				yield return battleManager.StartCoroutine(MoveStates.SelectMovingPointState(battleData));
			}
			else if (battleData.triggers.actionCommand.Data == ActionCommand.Skill){
				battleData.currentState = CurrentState.SelectSkill;
				yield return battleManager.StartCoroutine(SkillAndChainStates.SelectSkillState());
			}
			else if (battleData.triggers.actionCommand.Data == ActionCommand.Rest){
				battleData.currentState = CurrentState.RestAndRecover;
				yield return battleManager.StartCoroutine(RestAndRecover.Run(battleData));
				yield return null;
			}
			else if (battleData.triggers.actionCommand.Data == ActionCommand.Standby){
				battleData.currentState = CurrentState.Standby;
				yield return battleManager.StartCoroutine(Standby());
				yield return null;
			}
		}
		yield return null;
	}

	public void MoveCameraToUnitAndDisplayUnitInfoViewer(BattleData battleData, Unit unit){
		MoveCameraToUnit(unit);
		battleData.uiManager.SetMovedUICanvasOnUnitAsCenter(unit);
		battleData.uiManager.SetSelectedUnitViewerUI(unit);
	}
	private void OnOffCommandButtons(){
		OnOffStandbyButton(battleData);
		OnOffMoveButton(battleData);
		OnOffSkillButton(battleData);
	}
	public IEnumerator BeforeActCommonAct(){
		yield return StartCoroutine(UpdateRetreatAndDeadUnits(battleData, this));
		if (IsSelectedUnitRetreatOrDie(battleData))
			yield break;
		yield return AtActionEnd(battleData);
		MoveCameraToUnitAndDisplayUnitInfoViewer(battleData, battleData.selectedUnit);
		battleData.battleManager.UpdateAPBarAndMoveCameraToSelectedUnit (battleData.selectedUnit);
	}

	public void CallbackMoveCommand()
	{
		battleData.uiManager.DisableCommandUI();
		battleData.triggers.actionCommand.Trigger(ActionCommand.Move);
	}

	public void CallbackSkillCommand()
	{
		battleData.uiManager.DisableCommandUI();
		battleData.triggers.actionCommand.Trigger(ActionCommand.Skill);
	}

	public void CallbackRestCommand()
	{
		battleData.uiManager.DisableCommandUI();
		battleData.triggers.actionCommand.Trigger(ActionCommand.Rest);
	}

	public void CallbackStandbyCommand()
	{
		battleData.uiManager.DisableCommandUI();
		battleData.triggers.actionCommand.Trigger(ActionCommand.Standby);
	}

	public void CallbackOnPointerEnterRestCommand()
	{
		battleData.previewAPAction = new APAction(APAction.Action.Rest, RestAndRecover.GetRestCostAP(battleData));
		battleData.uiManager.UpdateApBarUI(battleData, battleData.unitManager.GetAllUnits());
	}

	public void CallbackOnPointerExitRestCommand()
	{
		battleData.previewAPAction = null;
		battleData.uiManager.UpdateApBarUI(battleData, battleData.unitManager.GetAllUnits());
	}

	public void CallbackCancel()
	{
		battleData.triggers.cancelClicked.Trigger();
	}

	public static IEnumerator Standby()
	{
		yield return new WaitForSeconds(0.1f);
	}

	public void CallbackSkillIndex(int index)
	{
		battleData.indexOfSelectedSkillByUser = index;
		battleData.triggers.skillSelected.Trigger();
		Debug.Log(index + "th skill is selected");
	}

	public void CallbackPointerEnterSkillIndex(int index)
	{
		battleData.indexOfPreSelectedSkillByUser = index;
	}

	public void CallbackPointerExitSkillIndex(int index)
	{
		if (index == battleData.indexOfPreSelectedSkillByUser)
		{
			battleData.indexOfPreSelectedSkillByUser = 0;
		}
	}

	public void CallbackSkillUICancel()
	{
		battleData.triggers.cancelClicked.Trigger();
	}

	public void CallbackApplyCommand()
	{
		battleData.uiManager.DisableSkillCheckUI();
		battleData.triggers.skillApplyCommandChanged.Trigger();
		battleData.skillApplyCommand = SkillApplyCommand.Apply;
	}

	public void CallbackChainCommand()
	{
		battleData.uiManager.DisableSkillCheckUI();
		battleData.triggers.skillApplyCommandChanged.Trigger();
		battleData.skillApplyCommand = SkillApplyCommand.Chain;
	}

	public void CallbackRightClick()
	{
		battleData.triggers.rightClicked.Trigger();
	}

	public void CallbackDirection(String directionString)
	{
		if (!battleData.isWaitingUserInput)
		{
			Debug.LogError("is NOT waiting user input!");
			return;
		}

		if (directionString == "LeftUp")
			battleData.move.selectedDirection = Direction.LeftUp;
		else if (directionString == "LeftDown")
			battleData.move.selectedDirection = Direction.LeftDown;
		else if (directionString == "RightUp")
			battleData.move.selectedDirection = Direction.RightUp;
		else if (directionString == "RightDown")
			battleData.move.selectedDirection = Direction.RightDown;
		
		battleData.triggers.directionSelectedByUser.Trigger();
		battleData.uiManager.DisableSelectDirectionUI();
	}

	void Update(){
		if (Input.GetMouseButtonDown(1)){
			if (battleData.enemyUnitSelected)
				battleData.enemyUnitSelected = false; // 유닛 고정이 되어있을 경우, 고정 해제가 우선으로 된다.
			else
				CallbackRightClick(); // 우클릭 취소를 받기 위한 핸들러.
		}

		if (battleData.currentState != CurrentState.FocusToUnit)
		{
			battleData.enemyUnitSelected = false; // 행동을 선택하면 홀드가 자동으로 풀림.
		}

		if (Input.GetMouseButtonDown(0))
		{
			// 유닛 뷰어가 뜬 상태에서 좌클릭하면, 유닛 뷰어가 고정된다. 단, 행동 선택 상태(FocusToUnit)에서만 가능.
			if ((battleData.currentState == CurrentState.FocusToUnit) && (battleData.uiManager.IsUnitViewerShowing()))
				battleData.enemyUnitSelected = true;
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
		return battleData.enemyUnitSelected;
	}

	public void OnMouseEnterHandlerFromTile(Vector2 position)
	{
		if (battleData.isWaitingUserInput)
		{
			battleData.preSelectedTilePosition = position;
		}
	}

	public void OnMouseExitHandlerFromTile(Vector2 position)
	{
		if (battleData.isWaitingUserInput)
		{
			battleData.preSelectedTilePosition = null;
		}
	}

	public void OnMouseDownHandlerFromTile(Vector2 position)
	{
		if (battleData.isWaitingUserInput)
		{
			battleData.triggers.tileSelectedByUser.Trigger();
			battleData.move.selectedTilePosition = position;
		}
	}

	IEnumerator EndPhaseOnGameManager(){
		Debug.Log("Phase End.");

		battleData.unitManager.EndPhase(battleData.currentPhase);
        battleData.tileManager.EndPhase(battleData.currentPhase);
		yield return new WaitForSeconds(0.5f);
	}

	IEnumerator StartPhaseOnGameManager(){
		battleData.currentPhase++;
		BattleTriggerManager.CountBattleCondition();

		yield return StartCoroutine(battleData.uiManager.MovePhaseUI(battleData.currentPhase));

		yield return StartCoroutine(battleData.unitManager.StartPhase(battleData.currentPhase));
        yield return StartCoroutine(battleData.unitManager.ApplyEachHeal());
		yield return StartCoroutine(battleData.unitManager.ApplyEachDOT());

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