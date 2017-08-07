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
	Waiting, Move, Skill, Rest, Standby, Cancel
}

public enum SkillApplyCommand
{
	Waiting, Apply, Chain
}

public interface IEventTrigger
{
	void Begin();
	void End();
	bool Triggered { get; }
}

public class EventTrigger: IEventTrigger
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

	public IEnumerator Wait()
	{
		Begin();

		while (triggered == false)
		{
			yield return null;
		}

		End();
	}

	// Wait이나 WaitOr을 쓰면 자동으로 호출됩니다.
	public void Begin()
	{
		enabled = true;
		triggered = false;
	}

	// Wait이나 WaitOr을 쓰면 자동으로 호출됩니다.
	public void End()
	{
		enabled = false;
	}

	public static IEnumerator WaitOr(params IEventTrigger[] triggers)
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
				if (trigger.Triggered)
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

public class EventTrigger<T>: IEventTrigger
{
	private bool enabled = false;
	private bool triggered = false;
	private T data = default(T);

	public bool Triggered
	{
		get { return triggered; }
	}

	public T Data
	{
		get { return data; }
	}

	public void Trigger(T data)
	{
		if (enabled)
		{
			triggered = true;
			this.data = data;
		}
	}

	public IEnumerator Wait()
	{
		Begin();

		while (triggered == false)
		{
			yield return null;
		}

		End();
	}

	// Wait이나 WaitOr을 쓰면 자동으로 호출됩니다.
	public void Begin()
	{
		enabled = true;
		triggered = false;
		this.data = default(T);
	}

	// Wait이나 WaitOr을 쓰면 자동으로 호출됩니다.
	public void End()
	{
		enabled = false;
	}
}

public class BattleData{
	public TileManager tileManager;
	public UnitManager unitManager;
	public UIManager uiManager;
	public BattleManager battleManager;

	public class Triggers
	{
		public EventTrigger rightClicked = new EventTrigger();
		public EventTrigger cancelClicked = new EventTrigger();
		public EventTrigger tileSelectedByUser = new EventTrigger();
		public EventTrigger directionSelectedByUser = new EventTrigger();
		public EventTrigger skillSelected = new EventTrigger();
		public EventTrigger skillApplyCommandChanged = new EventTrigger();
		public EventTrigger<ActionCommand> actionCommand = new EventTrigger<ActionCommand>();
	}

	public Triggers triggers = new Triggers();
	public CurrentState currentState = CurrentState.None;

	public Vector2? preSelectedTilePosition;
	public int indexOfPreSelectedSkillByUser = 0;
	public int indexOfSelectedSkillByUser = 0;
	public int rewardPoint;
	public bool isWaitingUserInput = false;
	public bool enemyUnitSelected = false;

	public SkillApplyCommand skillApplyCommand = SkillApplyCommand.Waiting;

	public class Move {
		public int moveCount = 0;
		public Vector2 selectedTilePosition = Vector2.zero;
		public Direction selectedDirection = Direction.LeftUp;
	}

	public class MoveSnapshopt {
		public Tile tile;
		public int ap;
		public Direction direction;
	}

	public Move move = new Move();
	public bool alreadyMoved;
	// 이동을 취소하기 위해서 필요
	public MoveSnapshopt moveSnapshot;

	public Unit selectedUnit; // 현재 턴의 유닛
	public List<Unit> readiedUnits = new List<Unit>();
	public List<Unit> deadUnits = new List<Unit>();
	public List<Unit> retreatUnits = new List<Unit>();
	
	public List<ChainInfo> chainList = new List<ChainInfo>();

	public int currentPhase;

	// temp values.
	public int chainDamageFactor = 1;

	public APAction previewAPAction;

	public ActiveSkill SelectedSkill
	{
		get {
			if(selectedUnit.GetSkillList().Count<indexOfSelectedSkillByUser)
				return null;
			return selectedUnit.GetSkillList()[indexOfSelectedSkillByUser - 1];
		}
	}

	public ActiveSkill PreSelectedSkill
	{
		get {
			return selectedUnit.GetSkillList()[indexOfPreSelectedSkillByUser - 1];
		}
	}

	public Tile SelectedTile
	{
		get {
			return tileManager.GetTile(move.selectedTilePosition);
		}
	}

	public Tile SelectedUnitTile
	{
		get {
			return tileManager.GetTile(selectedUnit.GetPosition());
		}
	}

	public ChainInfo GetChainInfo(Unit unit)
	{
		foreach (ChainInfo chainInfo in chainList)
		{
			if (chainInfo.GetUnit() == unit)
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
			if (chainInfo.GetSecondRange().Contains(tile))
			{
				resultUnits.Add(chainInfo.GetUnit());
			}
		}

		return resultUnits;
	}
	public List<Unit> GetObjectUnitsList(){
		return unitManager.GetAllUnits ().FindAll (unit => unit.IsObject () == true);
	}
}
