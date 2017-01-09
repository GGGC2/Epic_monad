using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Enums;
using LitJson;
using System;
using Battle.Turn;

public class BattleManager : MonoBehaviour
{
	public BattleData battleData = new BattleData();

	public class LevelData {
		public int level;
	}

	public int GetLevelInfoFromJson()
	{
		TextAsset jsonTextAsset = Resources.Load("Data/PartyData") as TextAsset;
		string jsonString = jsonTextAsset.text;
		LevelData levelData = JsonMapper.ToObject<LevelData>(jsonString);

		return levelData.level;
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
	}

	// Use this for initialization
	void Start()
	{
		battleData.partyLevel = Save.PartyDB.GetPartyLevel();
		battleData.unitManager.SetStandardActivityPoint(battleData.partyLevel);

		battleData.selectedUnitObject = null;

		battleData.currentPhase = 0;

		InitCameraPosition(); // temp init position;

		StartCoroutine(InstantiateTurnManager());
	}

	public int GetCurrentPhase()
	{
		return battleData.currentPhase;
	}

	public GameObject GetSelectedUnit()
	{
		return battleData.selectedUnitObject;
	}

	void InitCameraPosition()
	{
		Camera.main.transform.position = new Vector3(0, 0, -10);
	}

	IEnumerator InstantiateTurnManager()
	{
		while (true)
		{
			battleData.readiedUnits = battleData.unitManager.GetUpdatedReadiedUnits();

			while (battleData.readiedUnits.Count != 0)
			{
				battleData.uiManager.UpdateApBarUI(battleData, battleData.unitManager.GetAllUnits());

				battleData.selectedUnitObject = battleData.readiedUnits[0];
				if (battleData.SelectedUnit.GetSide() == Side.Enemy)
				{
					yield return AIStates.StartAI(battleData);
				}
				else
				{
					yield return StartCoroutine(ActionAtTurn(battleData.readiedUnits[0]));
				}
				battleData.selectedUnitObject = null;

				battleData.readiedUnits = battleData.unitManager.GetUpdatedReadiedUnits();
				yield return null;
			}

			yield return StartCoroutine(EndPhaseOnGameManager());
		}
	}

	IEnumerator ActionAtTurn(GameObject unit)
	{
		battleData.uiManager.UpdateApBarUI(battleData, battleData.unitManager.GetAllUnits());

		Debug.Log(unit.GetComponent<Unit>().GetName() + "'s turn");
		battleData.selectedUnitObject = unit;
		battleData.moveCount = 0; // 누적 이동 수
		battleData.alreadyMoved = false; // 연속 이동 불가를 위한 변수.
		ChainList.RemoveChainsFromUnit(battleData.selectedUnitObject); // 턴이 돌아오면 자신이 건 체인 삭제.
		battleData.currentState = CurrentState.FocusToUnit;

		battleData.uiManager.SetSelectedUnitViewerUI(battleData.selectedUnitObject);
		battleData.selectedUnitObject.GetComponent<Unit>().SetActive();

		yield return StartCoroutine(FocusToUnit(battleData));

		battleData.uiManager.DisableSelectedUnitViewerUI();
		if (battleData.selectedUnitObject != null)
			battleData.selectedUnitObject.GetComponent<Unit>().SetInactive();
	}

	static void CheckStandbyPossible(BattleData battleData)
	{
		bool isPossible = false;

		foreach (var unit in battleData.unitManager.GetAllUnits())
		{
			if ((unit != battleData.selectedUnitObject) &&
			(unit.GetComponent<Unit>().GetCurrentActivityPoint() > battleData.selectedUnitObject.GetComponent<Unit>().GetCurrentActivityPoint()))
			{
				isPossible = true;
			}
		}

		GameObject.Find("StandbyButton").GetComponent<Button>().interactable = isPossible;
	}

	static void CheckSkillPossible(BattleData battleData)
	{
		bool isPossible = false;

		isPossible = !(battleData.selectedUnitObject.GetComponent<Unit>().HasStatusEffect(StatusEffectType.Silence) ||
					 battleData.selectedUnitObject.GetComponent<Unit>().HasStatusEffect(StatusEffectType.Faint));

		GameObject.Find("SkillButton").GetComponent<Button>().interactable = isPossible;
	}

	static void CheckMovePossible(BattleData battleData)
	{
		bool isPossible = false;

		isPossible = !(battleData.selectedUnitObject.GetComponent<Unit>().HasStatusEffect(StatusEffectType.Bind) ||
					 battleData.selectedUnitObject.GetComponent<Unit>().HasStatusEffect(StatusEffectType.Faint) ||
					 battleData.alreadyMoved);

		GameObject.Find("MoveButton").GetComponent<Button>().interactable = isPossible;
	}

	List<GameObject> deadUnits = new List<GameObject>();

	public static IEnumerator FadeOutEffect(GameObject unitObject, float time)
	{
		SpriteRenderer sr = unitObject.GetComponent<SpriteRenderer>();
		for (int i = 0; i < 10; i++)
		{
			sr.color -= new Color(0, 0, 0, 0.1f);
			yield return new WaitForSeconds(time / 10f);
		}
	}

	public static IEnumerator DestroyDeadUnits(BattleData battleData)
	{
		BattleManager battleManager = battleData.battleManager;

		foreach (GameObject deadUnit in battleData.deadUnits)
		{
			if (deadUnit == battleData.selectedUnitObject)
				continue;
			// 죽은 유닛에게 추가 이펙트.
			deadUnit.GetComponent<SpriteRenderer>().color = Color.red;
			yield return battleManager.StartCoroutine(FadeOutEffect(deadUnit, 1));
			battleData.unitManager.DeleteDeadUnit(deadUnit);
			Debug.Log(deadUnit.GetComponent<Unit>().GetName() + " is dead");
			Destroy(deadUnit);
		}
	}

	public static IEnumerator DestroyRetreatUnits(BattleData battleData)
	{
		BattleManager battleManager = battleData.battleManager;

		foreach (GameObject retreatUnit in battleData.retreatUnits)
		{
			if (retreatUnit == battleData.selectedUnitObject)
				continue;
			yield return battleManager.StartCoroutine(FadeOutEffect(retreatUnit, 1));
			battleData.unitManager.DeleteRetreatUnit(retreatUnit);
			Debug.Log(retreatUnit.GetComponent<Unit>().GetName() + " retreats");
			Destroy(retreatUnit);
		}
	}

	public static IEnumerator FocusToUnit(BattleData battleData)
	{
		while (battleData.currentState == CurrentState.FocusToUnit)
		{
			BattleManager battleManager = battleData.battleManager;
			battleData.retreatUnits = battleData.unitManager.GetRetreatUnits();
			battleData.deadUnits = battleData.unitManager.GetDeadUnits();

			yield return battleManager.StartCoroutine(DestroyRetreatUnits(battleData));
			yield return battleManager.StartCoroutine(DestroyDeadUnits(battleData));

			if (battleData.retreatUnits.Contains(battleData.selectedUnitObject))
			{
				yield return battleManager.StartCoroutine(FadeOutEffect(battleData.selectedUnitObject, 1));
				battleData.unitManager.DeleteRetreatUnit(battleData.selectedUnitObject);
				Debug.Log("SelectedUnit retreats");
				Destroy(battleData.selectedUnitObject);
				yield break;
			}

			if (battleData.deadUnits.Contains(battleData.selectedUnitObject))
			{
				battleData.selectedUnitObject.GetComponent<SpriteRenderer>().color = Color.red;
				yield return battleManager.StartCoroutine(FadeOutEffect(battleData.selectedUnitObject, 1));
				battleData.unitManager.DeleteDeadUnit(battleData.selectedUnitObject);
				Debug.Log("SelectedUnit is dead");
				Destroy(battleData.selectedUnitObject);
				yield break;
			}

			// battleData.unitManager.MakeDeadUnitInfo(); // 문제없이 잘 돌아가면 삭제해도 무방.

			Camera.main.transform.position = new Vector3(
				battleData.selectedUnitObject.transform.position.x,
				battleData.selectedUnitObject.transform.position.y,
				-10);

			battleData.uiManager.SetMovedUICanvasOnCenter((Vector2)battleData.selectedUnitObject.transform.position);

			battleData.uiManager.SetSelectedUnitViewerUI(battleData.selectedUnitObject);

			battleData.uiManager.SetCommandUIName(battleData.selectedUnitObject);
			CheckStandbyPossible(battleData);
			CheckMovePossible(battleData);
			CheckSkillPossible(battleData);

			battleData.uiManager.UpdateApBarUI(battleData, battleData.unitManager.GetAllUnits());

			yield return battleManager.StartCoroutine(battleData.triggers.actionCommand.Wait());

			if (battleData.triggers.actionCommand.Data == ActionCommand.Move)
			{
				battleData.currentState = CurrentState.SelectMovingPoint;
				yield return battleManager.StartCoroutine(MoveStates.SelectMovingPointState(battleData));
			}
			else if (battleData.triggers.actionCommand.Data == ActionCommand.Attack)
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

	public void CallbackAttackCommand()
	{
		battleData.uiManager.DisableCommandUI();
		battleData.triggers.actionCommand.Trigger(ActionCommand.Attack);
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
		Debug.Log("Pointer Enter to Rest in battleManager.");
		battleData.previewAPAction = new APAction(APAction.Action.Rest, RestAndRecover.GetRestCostAP(battleData));
		battleData.uiManager.UpdateApBarUI(battleData, battleData.unitManager.GetAllUnits());
	}

	public void CallbackOnPointerExitRestCommand()
	{
		Debug.Log("Pointer exit from Rest in battleManager.");
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
		battleData.indexOfSeletedSkillByUser = index;
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
			return;

		if (directionString == "LeftUp")
			battleData.selectedDirection = Direction.LeftUp;
		else if (directionString == "LeftDown")
			battleData.selectedDirection = Direction.LeftDown;
		else if (directionString == "RightUp")
			battleData.selectedDirection = Direction.RightUp;
		else if (directionString == "RightDown")
			battleData.selectedDirection = Direction.RightDown;

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
			battleData.triggers.selectedTileByUser.Trigger();
			battleData.selectedTilePosition = position;
		}
	}

	IEnumerator EndPhaseOnGameManager()
	{
		Debug.Log("Phase End.");

		battleData.currentPhase++;

		battleData.unitManager.EndPhase();
		yield return new WaitForSeconds(0.5f);
	}
}
