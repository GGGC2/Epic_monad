using UnityEngine;
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

public class BattleData
{
	public TileManager tileManager;
	public UnitManager unitManager;
	public UIManager uiManager;
	public BattleManager battleManager;

	public CurrentState currentState = CurrentState.None;

	public bool isPreSeletedTileByUser = false;
	public bool isSelectedTileByUser = false;
	public bool isSelectedDirectionByUser = false;
	public int indexOfPreSelectedSkillByUser = 0;
	public int indexOfSeletedSkillByUser = 0;
	public bool isWaitingUserInput = false;

	public bool rightClicked = false; // 우클릭 : 취소
	public bool leftClicked = false; // 좌클릭 : 유닛뷰어 고정

	public bool cancelClicked = false;

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
	
	public List<ChainInfo> chainList = new List<ChainInfo>();

	public int currentPhase;

	// temp values.
	public int chainDamageFactor = 1;

	// Load from json.
	public int partyLevel;

	public APAction previewAPAction;

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
