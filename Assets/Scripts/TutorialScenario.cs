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
	enum Mission{MoveCommand, SkillCommand, Standby, Rest, SelectTile, SelectDirection, SelectSkill, End}
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
				BattleManager.Instance.readyCommandEvent.AddListener(ToNextStep);
			};
			ResetMissionCondition = () => {
				BattleManager.Instance.readyCommandEvent.RemoveListener(ToNextStep);
				TM.SetHighlightTiles (TM.GetTilesInGlobalRange (), false);
				TM.SetPreselectLock (false);
				TM.DepreselectAllTiles ();
			};
		} else if (mission == Mission.SelectDirection) {
			missionDirection = parser.ConsumeEnum<Direction> ();
			SetMissionCondition = () => {
				selectDirectionUI.EnableOnlyThisDirection (missionDirection);
				BattleManager.Instance.readyCommandEvent.AddListener(ToNextStep);
			};
			ResetMissionCondition = () => {
				BattleManager.Instance.readyCommandEvent.RemoveListener(ToNextStep);
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

			// 이동 버튼이나 기술 버튼 누르는 미션은 버튼 누른 시점에 다음 단계로 넘어감
			if (command == ActionCommand.Move || command == ActionCommand.Skill) {
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
			// 대기 버튼이나 휴식 버튼 누르는 미션은 다음으로 PC 행동턴이 돌아와 커맨드를 누를 수 있는 시점에 다음 단계로 넘어감
			else {
				SetMissionCondition = () => {
					commandPanel.TurnOnOnlyThisButton (command);
					commandPanel.LockCommandsOnOff ();
					BattleManager.Instance.readyCommandEvent.AddListener(ToNextStep);
				};
				ResetMissionCondition = () => {
					BattleManager.Instance.readyCommandEvent.RemoveListener(ToNextStep);
					commandPanel.UnlockCommandsOnOff ();
				};
			}
		}
	}
}