using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Enums;
using BattleUI;

public class TutorialScenario{
	public static TutorialManager tutorialManager;
	public static CommandPanel commandPanel;
	public static SkillPanel skillPanel;
	public static SelectDirectionUI selectDirectionUI;

	public int index;
	enum Mission{MoveCommand, SkillCommand, Standby, Rest, SelectTile, SelectDirection, SelectSkill, Apply, Wait, End}
	Mission mission;
	public bool IsEndMission { get { return mission == Mission.End; } }
	Direction missionDirection;
	public Action SetMissionCondition = () => {};
	public Action ResetMissionCondition = () => {};

	public TutorialScenario(string data){
		StringParser parser = new StringParser(data, '\t');
		index = parser.ConsumeInt();
		mission = parser.ConsumeEnum<Mission>();
		UnityEngine.Events.UnityAction ToNextStep = () => {
			tutorialManager.ToNextStep ();
		};

		if (mission == Mission.SelectTile) {
			Vector2 missionTilePos = new Vector2 (parser.ConsumeInt (), parser.ConsumeInt ());
			TileManager TM = TileManager.Instance;
			Tile missionTile = TM.GetTile (missionTilePos);
			List<Tile> clickableTiles = new List<Tile> ();
			clickableTiles.Add (missionTile);
			SetMissionCondition = () => {
				TM.DepreselectAllTiles ();
				TM.PreselectTiles (clickableTiles);
				TM.SetPreselectLock (true);
				TM.SetHighlightTiles (clickableTiles, true);
				missionTile.LeftClick.AddListener (ToNextStep);
			};
			ResetMissionCondition = () => {
				missionTile.LeftClick.RemoveListener (ToNextStep);
				TM.SetHighlightTiles (TM.GetTilesInGlobalRange (), false);
				TM.SetPreselectLock (false);
				TM.DepreselectAllTiles ();
			};
		} else if (mission == Mission.SelectDirection) {
			missionDirection = parser.ConsumeEnum<Direction> ();
			SetMissionCondition = () => {
				selectDirectionUI.EnableOnlyThisDirection (missionDirection);
				selectDirectionUI.AddListenerToDirection (missionDirection, ToNextStep);
			};
			ResetMissionCondition = () => {
				selectDirectionUI.RemoveListenerToDirection (missionDirection, ToNextStep);
				selectDirectionUI.EnableAllDirection ();
			};
		} else if (mission == Mission.SelectSkill) {
			int missionSkillIndex = parser.ConsumeInt ();
			SetMissionCondition = () => {
				skillPanel.TurnOnOnlyOneSkill (missionSkillIndex);
				skillPanel.LockSkillsOnOff ();
				skillPanel.AddListenerToSkillButton (missionSkillIndex, ToNextStep);
			};
			ResetMissionCondition = () => {
				skillPanel.RemoveListenerToSkillButton (missionSkillIndex, ToNextStep);
				skillPanel.UnlockSkillsOnOff ();
			};
		} else if (mission == Mission.Apply) {
			SetMissionCondition = () => {
				UIManager.Instance.EnableSkillCheckWaitButton (true, false);
				UIManager.Instance.LockApplyOrWaitOnOff ();
				UIManager.Instance.AddListenerToApplyButton (ToNextStep);
			};
			ResetMissionCondition = () => {
				UIManager.Instance.RemoveListenerToApplyButton (ToNextStep);
				UIManager.Instance.UnlockApplyOrWaitOnOff ();
			};
		} else if (mission == Mission.Wait) {
			SetMissionCondition = () => {
				UIManager.Instance.EnableSkillCheckWaitButton (false, true);
				UIManager.Instance.LockApplyOrWaitOnOff ();
				UIManager.Instance.AddListenerToWaitButton (ToNextStep);
			};
			ResetMissionCondition = () => {
				UIManager.Instance.RemoveListenerToWaitButton (ToNextStep);
				UIManager.Instance.UnlockApplyOrWaitOnOff ();
			};
		} else if (mission == Mission.MoveCommand || mission == Mission.SkillCommand || mission == Mission.Standby || mission == Mission.Rest) {
			ActionCommand command;
			if (mission == Mission.MoveCommand)
				command = ActionCommand.Move;
			else if (mission == Mission.SkillCommand)
				command = ActionCommand.Skill;
			else if (mission == Mission.Standby)
				command = ActionCommand.Standby;
			else
				command = ActionCommand.Rest;
			SetMissionCondition = () => {
				commandPanel.TurnOnOnlyThisButton (command);
				commandPanel.LockCommandsOnOff ();
				commandPanel.AddListenerToButton (command, ToNextStep);
			};
			ResetMissionCondition = () => {
				commandPanel.RemoveListenerToButton (command, ToNextStep);
				commandPanel.UnlockCommandsOnOff ();
			};
		}
	}
}