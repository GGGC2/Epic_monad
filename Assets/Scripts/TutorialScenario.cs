﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Enums;
using BattleUI;

public class TutorialScenario{
	public static TutorialManager tutorialManager;
	public static CommandPanel commandPanel;
	public static SelectDirectionUI selectDirectionUI;

	public int index;
	enum Mission{ MoveCommand, SkillCommand, Standby, Rest, SelectTile, SelectDirection, SelectAnyDirection, SelectSkill, OpenDetailInfo, CloseDetailInfo, End }
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

		BattleManager BM = BattleData.battleManager;
		TileManager TM = BattleData.tileManager;
		if (mission == Mission.SelectTile) {
			Vector2 missionTilePos = new Vector2 (parser.ConsumeInt (), parser.ConsumeInt ());
			Tile missionTile = TM.GetTile (missionTilePos);
			List<Tile> clickableTiles = new List<Tile> ();
			clickableTiles.Add (missionTile);
			SetMissionCondition = () => {
				TM.DepreselectAllTiles ();
				TM.PreselectTiles (clickableTiles);
				TM.SetPreselectLock (true);
				TM.SetHighlightTiles (clickableTiles, true);
				BM.readyCommandEvent.AddListener (ToNextStep);
			};
			ResetMissionCondition = () => {
				BM.readyCommandEvent.RemoveListener (ToNextStep);
				TM.SetHighlightTiles (TM.GetTilesInGlobalRange (), false);
				TM.SetPreselectLock (false);
				TM.DepreselectAllTiles ();
			};
		} else if (mission == Mission.SelectDirection) {
			missionDirection = parser.ConsumeEnum<Direction> ();
			SetMissionCondition = () => {
				selectDirectionUI.EnableOnlyThisDirection (missionDirection);
				BM.readyCommandEvent.AddListener (ToNextStep);
			};
			ResetMissionCondition = () => {
				BM.readyCommandEvent.RemoveListener (ToNextStep);
				selectDirectionUI.EnableAllDirection ();
			};
		} else if (mission == Mission.SelectAnyDirection) {
			SetMissionCondition = () => {
				BM.readyCommandEvent.AddListener (ToNextStep);
			};
			ResetMissionCondition = () => {
				BM.readyCommandEvent.RemoveListener (ToNextStep);
			};
		} else if (mission == Mission.SelectSkill) {
			int missionSkillIndex = parser.ConsumeInt ();
			SetMissionCondition = () => {
				UIManager.Instance.TurnOnOnlyOneSkill (missionSkillIndex);
				UIManager.Instance.ControlListenerOfActionButton(missionSkillIndex, true, ToNextStep);
				TM.DepaintAllTiles(TileColor.Blue);
				TM.DepreselectAllTiles();
				//skillPanel.LockSkillsOnOff ();
			};
			ResetMissionCondition = () => {
				UIManager.Instance.ControlListenerOfActionButton(missionSkillIndex, false, ToNextStep);
				//skillPanel.RemoveListenerToSkillButton (missionSkillIndex, ToNextStep);
			};
		} else if (mission == Mission.Standby){
			SetMissionCondition = () => {
				UIManager.Instance.TurnOnOnlyOneSkill(BattleData.selectedUnit.activeSkillList.Count);
				//commandPanel.LockCommandsOnOff ();
				BM.readyCommandEvent.AddListener (ToNextStep);
			};
			ResetMissionCondition = () => {
				BM.readyCommandEvent.RemoveListener (ToNextStep);
				//commandPanel.UnlockCommandsOnOff ();
			};
		} else if (mission == Mission.OpenDetailInfo) {
			SetMissionCondition = () => {
				BattleData.uiManager.activateDetailInfoEvent.AddListener (ToNextStep);
			};
			ResetMissionCondition = () => {
				BattleData.uiManager.activateDetailInfoEvent.RemoveListener (ToNextStep);
			};
		} else if (mission == Mission.CloseDetailInfo) {
			SetMissionCondition = () => {
				BattleData.uiManager.deactivateDetailInfoEvent.AddListener (ToNextStep);
			};
			ResetMissionCondition = () => {
				BattleData.uiManager.deactivateDetailInfoEvent.RemoveListener (ToNextStep);
			};
		}

	}
}