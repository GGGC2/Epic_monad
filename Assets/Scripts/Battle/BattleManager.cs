using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Enums;
using LitJson;
using System;
using Battle.Turn;
using Battle.Skills;

public class BattleManager : MonoBehaviour
{
	public BattleData battleData = new BattleData();
	public StageManager stageManager;

	public class LevelData {
		public int level;
	}

	public int GetPartyLevel()
	{
		return battleData.partyLevel;
	}

	public List<ChainInfo> GetChainList()
	{
		return battleData.chainList;
	}

	void Awake ()
	{
		battleData.tileManager = FindObjectOfType<TileManager>();
		battleData.unitManager = FindObjectOfType<UnitManager>();
		battleData.uiManager = FindObjectOfType<UIManager>();
		battleData.battleManager = this;
		battleData.stageManager = FindObjectOfType<StageManager>();
	}

	public int GetLevelInfoFromJson()
	{
		TextAsset jsonTextAsset = Resources.Load("Data/PartyData") as TextAsset;
		string jsonString = jsonTextAsset.text;
		LevelData levelData = JsonMapper.ToObject<LevelData>(jsonString);

		return levelData.level;
	}

	void Start()
	{
		battleData.partyLevel = GameData.level;
		battleData.unitManager.SetStandardActivityPoint(battleData.partyLevel);
		battleData.selectedUnit = null;
		battleData.currentPhase = 0;

		InitCameraPosition(); // temp init position;
	}

	public int GetCurrentPhase()
	{
		return battleData.currentPhase;
	}

	public Unit GetSelectedUnit()
	{
		return battleData.selectedUnit;
	}

	void InitCameraPosition()
	{
		Camera.main.transform.position = new Vector3(0, 0, -10);
	}

	public IEnumerator InstantiateTurnManager()
	{
        yield return new WaitForSeconds(0.5f);
		while (true)
		{
			yield return StartCoroutine(StartPhaseOnGameManager());

            battleData.readiedUnits = battleData.unitManager.GetUpdatedReadiedUnits();

			while (battleData.readiedUnits.Count != 0) {
                battleData.selectedUnit = battleData.readiedUnits[0];
                battleData.uiManager.UpdateApBarUI(battleData, battleData.unitManager.GetAllUnits());

				if (battleData.selectedUnit.GetSide() == Side.Enemy)
				{
					// yield return AIStates_old.AIStart(battleData);
					yield return AIStates.AIStart(battleData);
				}
				else
				{
					yield return StartCoroutine(ActionAtTurn(battleData.readiedUnits[0]));
				}
				battleData.selectedUnit = null;

				battleData.readiedUnits = battleData.unitManager.GetUpdatedReadiedUnits();
				yield return null;
			}

			yield return StartCoroutine(EndPhaseOnGameManager());
		}
	}

	IEnumerator ActionAtTurn(Unit unit)
	{
		battleData.uiManager.UpdateApBarUI(battleData, battleData.unitManager.GetAllUnits());

		Debug.Log(unit.GetName() + "'s turn");
        foreach(Unit otherUnit in battleData.unitManager.GetAllUnits()) {
            SkillLogicFactory.Get(otherUnit.GetLearnedPassiveSkillList()).TriggerOnTurnStart(otherUnit, unit);
        }
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

	static void CheckStandbyPossible(BattleData battleData)
	{
		bool isPossible = false;

		foreach (var unit in battleData.unitManager.GetAllUnits())
		{
			if ((unit != battleData.selectedUnit) &&
			(unit.GetCurrentActivityPoint() > battleData.selectedUnit.GetCurrentActivityPoint()))
			{
				isPossible = true;
			}
		}

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
            Skill originSkill = tileStatusEffect.GetOriginSkill();
            if (originSkill != null) {
                if (!SkillLogicFactory.Get(originSkill).TriggerTileStatusEffectWhenUnitTryToUseSkill(tileUnderCaster, tileStatusEffect)) {
                    isPossible = false;
                }
            }
        }

        GameObject.Find("SkillButton").GetComponent<Button>().interactable = isPossible;
	}

	static void CheckMovePossible(BattleData battleData)
	{
		bool isPossible = false;

		isPossible = !(battleData.selectedUnit.HasStatusEffect(StatusEffectType.Bind) ||
					 battleData.selectedUnit.HasStatusEffect(StatusEffectType.Faint) ||
					 battleData.alreadyMoved);

		GameObject.Find("MoveButton").GetComponent<Button>().interactable = isPossible;
	}
    

	public static IEnumerator FadeOutEffect(Unit unit, float time)
	{
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

		foreach (Unit deadUnit in battleData.deadUnits)
		{
			BattleTriggerChecker.CountBattleCondition(deadUnit);
			if (deadUnit == battleData.selectedUnit)
				continue;
			// 죽은 유닛에게 추가 이펙트.
			deadUnit.GetComponent<SpriteRenderer>().color = Color.red;
			yield return battleManager.StartCoroutine(FadeOutEffect(deadUnit, 1));
			battleData.unitManager.DeleteDeadUnit(deadUnit);
			Debug.Log(deadUnit.GetName() + " is dead");
			Destroy(deadUnit.gameObject);
		}
	}

	public static IEnumerator DestroyRetreatUnits(BattleData battleData)
	{
		BattleManager battleManager = battleData.battleManager;

		foreach (Unit retreatUnit in battleData.retreatUnits)
		{
			BattleTriggerChecker.CountBattleCondition(retreatUnit);
			if (retreatUnit == battleData.selectedUnit)
				continue;
			yield return battleManager.StartCoroutine(FadeOutEffect(retreatUnit, 1));
			battleData.unitManager.DeleteRetreatUnit(retreatUnit);
			Debug.Log(retreatUnit.GetName() + " retreats");
			Destroy(retreatUnit.gameObject);
		}
	}

	static bool IsSelectedUnitRetraitOrDie(BattleData battleData)
	{
		if (battleData.retreatUnits.Contains(battleData.selectedUnit))
		{
			return true;
		}

		if (battleData.deadUnits.Contains(battleData.selectedUnit))
		{
			return true;
		}

		return false;
	}

	static IEnumerator UpdateRetraitAndDeadUnits(BattleData battleData, BattleManager battleManager)
	{
		battleData.retreatUnits = battleData.unitManager.GetRetreatUnits();
		battleData.deadUnits = battleData.unitManager.GetDeadUnits();

		yield return battleManager.StartCoroutine(DestroyRetreatUnits(battleData));
		yield return battleManager.StartCoroutine(DestroyDeadUnits(battleData));

		if (battleData.retreatUnits.Contains(battleData.selectedUnit)){
			yield return battleManager.StartCoroutine(FadeOutEffect(battleData.selectedUnit, 1));
			battleData.unitManager.DeleteRetreatUnit(battleData.selectedUnit);
			Debug.Log("SelectedUnit retreats");
			Destroy(battleData.selectedUnit.gameObject);
			yield break;
		}

		if (battleData.deadUnits.Contains(battleData.selectedUnit)){
			battleData.selectedUnit.gameObject.GetComponent<SpriteRenderer>().color = Color.red;
			yield return battleManager.StartCoroutine(FadeOutEffect(battleData.selectedUnit, 1));
			battleData.unitManager.DeleteDeadUnit(battleData.selectedUnit);
			Debug.Log("SelectedUnit is dead");
			Destroy(battleData.selectedUnit.gameObject);
			yield break;
		}
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

	public static IEnumerator FocusToUnit(BattleData battleData)
	{
		while (battleData.currentState == CurrentState.FocusToUnit)
		{
			BattleManager battleManager = battleData.battleManager;
			
			yield return battleManager.StartCoroutine(UpdateRetraitAndDeadUnits(battleData, battleManager));
			
			// 매 액션이 끝날때마다 갱신하는 특성 조건들
			battleData.unitManager.ResetLatelyHitUnits();
			battleData.unitManager.TriggerPassiveSkillsAtActionEnd();
            yield return battleManager.StartCoroutine(battleData.unitManager.TriggerStatusEffectsAtActionEnd());
            battleData.unitManager.UpdateStatusEffectsAtActionEnd();
			
			if (IsSelectedUnitRetraitOrDie(battleData))
				yield break;

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

			if (battleData.alreadyMoved && battleData.triggers.rightClicked.Triggered)
			{
				Battle.Turn.MoveStates.RestoreMoveSnapshot(battleData);
				battleData.alreadyMoved = false;
			}
			else if (battleData.triggers.actionCommand.Data == ActionCommand.Move)
			{
				battleData.currentState = CurrentState.SelectMovingPoint;
				yield return battleManager.StartCoroutine(MoveStates.SelectMovingPointState(battleData));
			}
			else if (battleData.triggers.actionCommand.Data == ActionCommand.Skill)
			{
				battleData.currentState = CurrentState.SelectSkill;
				yield return battleManager.StartCoroutine(SkillAndChainStates.SelectSkillState(battleData));
			}
			else if (battleData.triggers.actionCommand.Data == ActionCommand.Rest)
			{
				battleData.currentState = CurrentState.RestAndRecover;
				yield return battleManager.StartCoroutine(RestAndRecover.Run(battleData));
			}
			else if (battleData.triggers.actionCommand.Data == ActionCommand.Standby)
			{
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
		
		battleData.triggers.selectedDirectionByUser.Trigger();
		battleData.uiManager.DisableSelectDirectionUI();
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetMouseButtonDown(1))
		{
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

	IEnumerator EndPhaseOnGameManager()
	{
		Debug.Log("Phase End.");

		battleData.unitManager.EndPhase(battleData.currentPhase);
        battleData.tileManager.EndPhase(battleData.currentPhase);
		yield return new WaitForSeconds(0.5f);
	}

	IEnumerator StartPhaseOnGameManager()
	{
		battleData.currentPhase++;
		BattleTriggerChecker.CountBattleCondition();

		yield return StartCoroutine(battleData.unitManager.StartPhase(battleData.currentPhase));
        yield return StartCoroutine(battleData.unitManager.ApplyEachHeal());
		yield return StartCoroutine(battleData.unitManager.ApplyEachDOT());

		yield return new WaitForSeconds(0.5f);
	}
}
