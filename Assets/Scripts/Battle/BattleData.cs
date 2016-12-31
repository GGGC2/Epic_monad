using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Enums;

public enum CurrentState
{
	None, FocusToUnit, SelectMovingPoint, CheckDestination,
	MoveToTile, SelectSkill, SelectSkillApplyPoint, SelectSkillApplyDirection, CheckApplyOrChain,
	ApplySkill, ChainAndStandby, RestAndRecover, Standby
}

public enum ActionCommand
{
	Waiting, Move, Attack, Rest, Standby, Cancel
}

public enum SkillApplyCommand
{
	Waiting, Apply, Chain
}

public class EventTrigger
{
	private bool enabled = false;
	private bool triggered = false;

	public bool Triggered
	{
		get { return triggered; }
	}

	public void Trigger()
	{
		if (enabled)
		{
			triggered = true;
		}
	}

	public IEnumerable Wait()
	{
		Begin();

		while (triggered == false)
		{
			yield return null;
		}

		End();
	}

	private void Begin()
	{
		enabled = true;
		triggered = false;
	}

	private void End()
	{
		enabled = false;
	}

	public static IEnumerator WaitOr(params EventTrigger[] triggers)
	{
		foreach (var trigger in triggers)
		{
			trigger.Begin();
		}

		bool looping = true;
		while (looping)
		{
			foreach (var trigger in triggers)
			{
				if (trigger.triggered)
				{
					looping = false;
					break;
				}
			}
			yield return null;
		}

		foreach (var trigger in triggers)
		{
			trigger.End();
		}
	}
}

public class BattleData
{
	public TileManager tileManager;
	public UnitManager unitManager;
	public UIManager uiManager;
	public BattleManager battleManager;

	public class Triggers
	{
		public EventTrigger rightClicked = new EventTrigger();
		public EventTrigger cancelClicked = new EventTrigger();
		public EventTrigger selectedTileByUser = new EventTrigger();
		public EventTrigger selectedDirectionByUser = new EventTrigger();
		public EventTrigger skillSelected = new EventTrigger();
		public EventTrigger skillApplyCommandChanged = new EventTrigger();
	}

	public Triggers triggers = new Triggers();
	public CurrentState currentState = CurrentState.None;

	public bool isPreSeletedTileByUser = false;
	public int indexOfPreSelectedSkillByUser = 0;
	public int indexOfSeletedSkillByUser = 0;
	public bool isWaitingUserInput = false;

	public bool enemyUnitSelected = false;

	public ActionCommand command = ActionCommand.Waiting;
	public SkillApplyCommand skillApplyCommand = SkillApplyCommand.Waiting;

	public int moveCount;
	public bool alreadyMoved;
	public Vector2 preSelectedTilePosition;
	public Vector2 selectedTilePosition;
	public Direction selectedDirection;
	public GameObject selectedUnitObject; // 현재 턴의 유닛
	public List<GameObject> readiedUnits = new List<GameObject>();
	public List<GameObject> deadUnits = new List<GameObject>();
	public List<GameObject> retreatUnits = new List<GameObject>();
	
	public List<ChainInfo> chainList = new List<ChainInfo>();

	public int currentPhase;

	// temp values.
	public int chainDamageFactor = 1;

	// Load from json.
	public int partyLevel;

	public APAction previewAPAction;

	public float GetChainDamageFactorFromChainCombo(int chainCombo)
	{
		if (chainCombo < 2)	return 1.0f;
		else if (chainCombo == 2) return 1.2f;
		else if (chainCombo == 3) return 1.5f;
		else if (chainCombo == 4) return 2.0f;
		else return 3.0f;  
	}

	public Skill SelectedSkill
	{
		get {
			return selectedUnitObject.GetComponent<Unit>().GetSkillList()[indexOfSeletedSkillByUser - 1];
		}
	}

	public Skill PreSelectedSkill
	{
		get {
			return selectedUnitObject.GetComponent<Unit>().GetSkillList()[indexOfPreSelectedSkillByUser - 1];
		}
	}

	public Tile SelectedTile
	{
		get {
			return tileManager.GetTile(selectedTilePosition).GetComponent<Tile>();
		}
	}

	public Unit SelectedUnit
	{
		get {
			return selectedUnitObject.GetComponent<Unit>();
		}
	}

	public Tile SelectedUnitTile
	{
		get {
			return tileManager.GetTile(SelectedUnit.GetPosition()).GetComponent<Tile>();
		}
	}

	public ChainInfo GetChainInfo(Unit unit)
	{
		foreach (ChainInfo chainInfo in chainList)
		{
			if (chainInfo.GetUnit() == unit.gameObject)
			{
				return chainInfo;
			}
		}
		return null;
	}

	public List<Unit> GetUnitsTargetThisTile(Tile tile)
	{
		List<Unit> resultUnits = new List<Unit>();
		foreach (ChainInfo chainInfo in chainList)
		{
			if (chainInfo.GetTargetArea().Contains(tile.gameObject))
			{
				resultUnits.Add(chainInfo.GetUnit().GetComponent<Unit>());
			}
		}

		return resultUnits;
	}
}
