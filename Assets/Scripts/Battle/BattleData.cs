using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Enums;

public enum CurrentState{
	None, Destroy, FocusToUnit, CheckDestination,
	SelectSkillApplyPoint, SelectSkillApplyDirection,
	ApplySkill, WaitChain, RestAndRecover, Standby
}

public enum ActionCommand {Waiting, Move, Skill, Rest, Standby, Cancel}
public enum SkillApplyCommand {Waiting, Apply, Chain}

public interface IEventTrigger{
	void Begin();
	void End();
	void Revert();
	bool Triggered { get; }
}

public class EventTrigger: IEventTrigger{
	private bool enabled = false;
	private bool triggered = false;

	public bool Triggered
	{
		get { return triggered; }
	}

	public void Trigger(){
		if (enabled)
		{
			triggered = true;
		}
	}

	public void Revert(){
		if(enabled)
			triggered = false;
	}

	public IEnumerator Wait(){
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

	public static IEnumerator WaitOr(params IEventTrigger[] triggers){
		foreach (var trigger in triggers)
			trigger.Begin();
		
		bool looping = true;
		while (looping){
			BattleManager battleManager = BattleManager.Instance;
			foreach (var trigger in triggers){
				if (trigger.Triggered) {
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

public class EventTrigger<T>: IEventTrigger{
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

	public IEnumerator Wait(){
		Begin();

		while (triggered == false)
			yield return null;

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

	public void Revert(){
		if(enabled)
			triggered = false;
	}
}

public static class BattleData{
	public static TileManager tileManager;
	public static UnitManager unitManager;
	public static UIManager uiManager;
	public static BattleManager battleManager;

	public class Triggers{
		public EventTrigger rightClicked = new EventTrigger();
		public EventTrigger cancelClicked = new EventTrigger();
		public EventTrigger tileSelectedByUser = new EventTrigger();
		public EventTrigger tileLongSelectedByUser = new EventTrigger();
		public EventTrigger directionSelectedByUser = new EventTrigger();
		public EventTrigger directionLongSelectedByUser = new EventTrigger();
		public EventTrigger skillSelected = new EventTrigger();
		public EventTrigger skillApplyCommandChanged = new EventTrigger();
		public EventTrigger<ActionCommand> actionCommand = new EventTrigger<ActionCommand>();
	}

	public static bool onTutorial = false;
	public static bool rightClickLock = false;

	public static Triggers triggers = new Triggers();
	public static CurrentState currentState = CurrentState.None;

	public static Vector2? mouseOverTilePosition;
	public static ActiveSkill preSelectedSkill;
	public static ActiveSkill selectedSkill;
	public static int rewardPoint;
	public static bool isWaitingUserInput = false;
	public static bool enemyUnitSelected = false;
    public static bool tileSelected = false;
    public static Aspect aspect = Aspect.North;

	public static SkillApplyCommand skillApplyCommand = SkillApplyCommand.Waiting;

	public class Move {
		public int moveCount = 0;
		public Vector2 selectedTilePosition = Vector2.zero;
		public Direction selectedDirection = Direction.LeftUp;
	}

	public class MoveSnapshot {
		public Tile tile;
		public int ap;
		public int movedTileCount;
		public Direction direction;
		public List<UnitStatusEffect> statEffectList;

		public MoveSnapshot(Unit unit){
			tile = unit.GetTileUnderUnit();
			ap = unit.GetCurrentActivityPoint();
			movedTileCount = unit.GetMovedTileCount();
			direction = unit.GetDirection();
			statEffectList = new List<UnitStatusEffect>();
			unit.StatusEffectList.ForEach(effect => statEffectList.Add(effect));
			Debug.Log("show snapshot's effectlist : " + statEffectList.Count);
			statEffectList.ForEach(effect => Debug.Log(effect.GetDisplayName()));
		}
	}

	public static Move move = new Move();
	public static bool alreadyMoved;
	public static MoveSnapshot moveSnapshot; // 이동을 취소하기 위해서 필요

	public static Unit selectedUnit; // 현재 턴의 유닛
	public static List<Unit> readiedUnits = new List<Unit>();
	public static List<Unit> deadUnits = new List<Unit> ();
	public static List<Unit> retreatUnits = new List<Unit> ();

	public static int currentPhase;

	public static APAction previewAPAction;

	// 중요 - BattleData 클래스 내 모든 변수는 static이라서 Initialize 함수 내에서 초기화를 해야 하므로
	// 변수 하나 추가할 때마다 반드시 여기에 초기화하는 코드도 추가할 것
	// 또 전투 시작할 때 반드시 Initialize() 함수를 불러야 한다(현재는 BattleManager 인스턴스의 Awake()시 호출함)
	public static void Initialize(){
		tileManager = GameObject.FindObjectOfType<TileManager>();
		unitManager = GameObject.FindObjectOfType<UnitManager>();
		uiManager = GameObject.FindObjectOfType<UIManager>();
		battleManager = GameObject.FindObjectOfType<BattleManager>();

		onTutorial = false;
		rightClickLock = false;

		triggers = new Triggers();
		currentState = CurrentState.None;

		rewardPoint = 0;
		isWaitingUserInput = false;
		enemyUnitSelected = false;
		tileSelected = false;
		aspect = Aspect.North;

		skillApplyCommand = SkillApplyCommand.Waiting;

		move = new Move();
		alreadyMoved = false;

		selectedUnit = null;
		readiedUnits = new List<Unit>();
		deadUnits = new List<Unit>();
		retreatUnits = new List<Unit>();

		currentPhase = 0;

		previewAPAction = null;
	}

	public static void SetSelectedUnit(Unit unit){
		selectedUnit = unit;
	}

	public static Tile SelectedTile{
		get { return tileManager.GetTile(move.selectedTilePosition); }
	}
	public static Tile SelectedUnitTile{
		get{ return selectedUnit.GetTileUnderUnit (); }
	}
	public static List<Unit> GetObjectUnitsList(){
		return unitManager.GetAllUnits ().FindAll (unit => unit.IsObject == true);
	}
}