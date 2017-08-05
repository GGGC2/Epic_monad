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

	public List<ChainInfo> GetChainList(){
		return battleData.chainList;
	}

	void Awake (){
        if (!SceneData.isTestMode && !SceneData.isStageMode)
            GameDataManager.Load();

		PartyData.CheckLevelZero();
		Load();
		battleData.tileManager = FindObjectOfType<TileManager>();
		battleData.unitManager = FindObjectOfType<UnitManager>();
		battleData.uiManager = FindObjectOfType<UIManager>();
		battleData.battleManager = this;
	}

	void Start() {
        SoundManager.Instance.PlayBgm("Script_Tense");

		AIStates_old.SetBattleData (battleData);

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
						yield return AIStates.AIStart(battleData);
						// AI 코루틴이 어디서 끝나는지 몰라서 일단 여기 넣어놓음. 머리 위 화살표와 현재 턴 유닛 정보를 없애는 로직
						battleData.uiManager.DisableSelectedUnitViewerUI();
						battleData.selectedUnit.SetInactive();
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
		battleData.uiManager.UpdateApBarUI(battleData, battleData.unitManager.GetAllUnits());
		FindObjectOfType<CameraMover>().SetFixedPosition(unit.transform.position);

		Debug.Log(unit.GetName() + "'s turn");
        foreach(Unit otherUnit in battleData.unitManager.GetAllUnits())
            SkillLogicFactory.Get(otherUnit.GetLearnedPassiveSkillList()).TriggerOnTurnStart(otherUnit, unit);

        unit.TriggerTileStatusEffectAtTurnStart();
		battleData.selectedUnit = unit;
		battleData.move = new BattleData.Move();
		battleData.alreadyMoved = false; // 연속 이동 불가를 위한 변수.
		ChainList.RemoveChainsFromUnit(battleData.selectedUnit); // 턴이 돌아오면 자신이 건 체인 삭제.
		battleData.currentState = CurrentState.FocusToUnit;

		battleData.uiManager.SetSelectedUnitViewerUI(battleData.selectedUnit);
		battleData.selectedUnit.SetActive();

		yield return StartCoroutine(FocusToUnit(battleData));

        battleData.selectedUnit.TriggerTileStatusEffectAtTurnEnd();
        
		battleData.uiManager.DisableSelectedUnitViewerUI();
		if (battleData.selectedUnit != null)
			battleData.selectedUnit.SetInactive();
	}

	public static bool GetStandbyPossibleWithThisAP(BattleData battleData, Unit unit, int AP){
		bool isPossible = false;
		foreach (var anyUnit in battleData.unitManager.GetAllUnits()){
			if ((anyUnit != unit) &&
				(anyUnit.GetCurrentActivityPoint() > AP))
			{
				isPossible = true;
				return isPossible;
			}
		}
		return isPossible;
	}
	public static bool GetStandbyPossible(BattleData battleData){
		bool isPossible = GetStandbyPossibleWithThisAP (battleData, battleData.selectedUnit, battleData.selectedUnit.GetCurrentActivityPoint ());
		return isPossible;
	}
	private static void CheckStandbyPossible(BattleData battleData){
		bool isPossible = GetStandbyPossible (battleData);

		// Debug.Log("standbyButton : " + GameObject.Find("StandbyButton"));
		GameObject.Find("StandbyButton").GetComponent<Button>().interactable = isPossible;
	}

	static void CheckSkillPossible(BattleData battleData)
	{
        Unit caster = battleData.selectedUnit;
		bool isPossible = false;

		isPossible = !(caster.HasStatusEffect(StatusEffectType.Silence) ||
					 caster.HasStatusEffect(StatusEffectType.Faint));

        Tile tileUnderCaster = caster.GetTileUnderUnit();
        foreach (var tileStatusEffect in tileUnderCaster.GetStatusEffectList()) {
            ActiveSkill originSkill = tileStatusEffect.GetOriginSkill();
            if (originSkill != null) {
                if (!SkillLogicFactory.Get(originSkill).TriggerTileStatusEffectWhenUnitTryToUseSkill(tileUnderCaster, tileStatusEffect)) {
                    isPossible = false;
                }
            }
        }

        GameObject.Find("SkillButton").GetComponent<Button>().interactable = isPossible;
	}

	public static bool GetIsMovePossibleState(BattleData battleData){
		bool isPossible =  !(battleData.selectedUnit.HasStatusEffect(StatusEffectType.Bind) ||
			battleData.selectedUnit.HasStatusEffect(StatusEffectType.Faint));
		return isPossible;
	}
	private static void CheckMovePossible(BattleData battleData){
		bool isPossible = GetIsMovePossibleState(battleData) && !(battleData.alreadyMoved);
		GameObject.Find("MoveButton").GetComponent<Button>().interactable = isPossible;
	}
    

	public static IEnumerator FadeOutEffect(Unit unit)
	{
		float time = 0.5f;
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
			// 죽은 유닛에게 추가 이펙트.
			deadUnit.GetComponent<SpriteRenderer>().color = Color.red;
			yield return battleManager.StartCoroutine(FadeOutEffect(deadUnit));
            RemoveAuraEffectFromDeadOrRetreatUnit(deadUnit);
			battleData.unitManager.DeleteDeadUnit(deadUnit);
			Debug.Log(deadUnit.GetName() + " is dead");
			yield return BattleTriggerManager.CountBattleCondition(deadUnit, BattleTrigger.ActionType.Kill);
			yield return BattleTriggerManager.CountBattleCondition(deadUnit, BattleTrigger.ActionType.Neutralize);
			Destroy(deadUnit.gameObject);
		}
	}

	public static IEnumerator DestroyRetreatUnits(BattleData battleData)
	{
		BattleManager battleManager = battleData.battleManager;

		foreach (Unit retreatUnit in battleData.retreatUnits){
			yield return battleManager.StartCoroutine(FadeOutEffect(retreatUnit));
            RemoveAuraEffectFromDeadOrRetreatUnit(retreatUnit);
            battleData.unitManager.DeleteRetreatUnit(retreatUnit);
			Debug.Log(retreatUnit.GetName() + " retreats");
			yield return BattleTriggerManager.CountBattleCondition(retreatUnit, BattleTrigger.ActionType.Retreat);
			yield return BattleTriggerManager.CountBattleCondition(retreatUnit, BattleTrigger.ActionType.Neutralize);
			Destroy(retreatUnit.gameObject);
		}
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
		Camera.main.transform.position = new Vector3(
				unit.gameObject.transform.position.x,
				unit.gameObject.transform.position.y,
				-10);
	}

	public static void MoveCameraToTile(Tile tile)
	{
		Camera.main.transform.position = new Vector3(
				tile.gameObject.transform.position.x,
				tile.gameObject.transform.position.y,
				-10);	
	}

	public static void MoveCameraToPosition(Vector2 position)
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

	public static IEnumerator FocusToUnit(BattleData battleData){
		while (battleData.currentState == CurrentState.FocusToUnit){
			BattleManager battleManager = battleData.battleManager;
			
			yield return battleManager.StartCoroutine(UpdateRetreatAndDeadUnits(battleData, battleManager));

			if (IsSelectedUnitRetreatOrDie(battleData))
				yield break;
			
			yield return AtActionEnd(battleData);

			MoveCameraToUnit(battleData.selectedUnit);

			battleData.uiManager.SetMovedUICanvasOnCenter((Vector2)battleData.selectedUnit.gameObject.transform.position);

			battleData.uiManager.SetSelectedUnitViewerUI(battleData.selectedUnit);

			battleData.uiManager.SetCommandUIName(battleData.selectedUnit);
			CheckStandbyPossible(battleData);
			CheckMovePossible(battleData);
			CheckSkillPossible(battleData);

			battleData.uiManager.UpdateApBarUI(battleData, battleData.unitManager.GetAllUnits());

			//이미 이동했으면 이동 빼고, 아니면 이동을 포함한 모든 종류의 actionCommand를 기다림
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
				yield return battleManager.StartCoroutine(SkillAndChainStates.SelectSkillState(battleData));
			}
			else if (battleData.triggers.actionCommand.Data == ActionCommand.Rest){
				battleData.currentState = CurrentState.RestAndRecover;
				yield return battleManager.StartCoroutine(RestAndRecover.Run(battleData));
			}
			else if (battleData.triggers.actionCommand.Data == ActionCommand.Standby){
				battleData.currentState = CurrentState.Standby;
				yield return battleManager.StartCoroutine(Standby());
			}
		}
		yield return null;
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