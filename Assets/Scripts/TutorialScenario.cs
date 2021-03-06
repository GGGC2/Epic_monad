﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Enums;
using BattleUI;

public class TutorialScenario{
	public static TutorialManager tutorialManager;
	public static SelectDirectionUI selectDirectionUI;

	public int index;
	enum Mission{ MoveCommand, SkillCommand, Standby, Rest, SelectTile, SelectDirection, SelectAnyDirection, SelectSkill, OpenDetailInfo, CloseDetailInfo, CancelMoveOrSkill, End }
	Mission mission;
	//public bool isMarker;
	public Vector3 mouseMarkPos;
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
				UIManager.Instance.TurnOffAllActions();
				UIManager.Instance.ActionButtonOnOffLock = true;
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
				UIManager.Instance.ActionButtonOnOffLock = false;
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
            int missionSkillIndex = parser.ConsumeInt();
            SetMissionCondition = () => {
                //Debug.Log("Mission : SelectSkill");
                if (parser.ConsumeBool()) {
                    mouseMarkPos = UIManager.Instance.GetActionButtonPosition(missionSkillIndex);
                }
                UIManager.Instance.TurnOnOnlyOneAction (missionSkillIndex);
				UIManager.Instance.ActionButtonOnOffLock = true;
				UIManager.Instance.ControlListenerOfActionButton(missionSkillIndex, true, ToNextStep);
				TM.DepreselectAllTiles();
			};
			ResetMissionCondition = () => {
				UIManager.Instance.ControlListenerOfActionButton(missionSkillIndex, false, ToNextStep);
				UIManager.Instance.ActionButtonOnOffLock = false;
			};
		} else if (mission == Mission.Standby){
            SetMissionCondition = () => {
                int standbyButtonIndex = BattleData.turnUnit.activeSkillList.Count;
                if (parser.ConsumeBool()) {
                    mouseMarkPos = UIManager.Instance.GetActionButtonPosition(standbyButtonIndex);
                }
                UIManager.Instance.TurnOnOnlyOneAction(standbyButtonIndex);
				UIManager.Instance.ActionButtonOnOffLock = true;
				TM.DepreselectAllTiles();
				BattleData.battleManager.readyCommandEvent.AddListener (ToNextStep);
			};
			ResetMissionCondition = () => {
				BattleData.battleManager.readyCommandEvent.RemoveListener (ToNextStep);
				UIManager.Instance.ActionButtonOnOffLock = false;
			};
		} else if (mission == Mission.OpenDetailInfo) {
			Vector2 missionTilePos = new Vector2 (parser.ConsumeInt (), parser.ConsumeInt ());
			Tile missionTile = TM.GetTile (missionTilePos);
			List<Tile> clickableTiles = new List<Tile> ();
			clickableTiles.Add (missionTile);
			SetMissionCondition = () => {
				UIManager.Instance.TurnOffAllActions();
				UIManager.Instance.ActionButtonOnOffLock = true;
				TM.DepreselectAllTiles ();
				TM.PreselectTiles (clickableTiles);
				TM.SetPreselectLock (true);
				TM.SetHighlightTiles (clickableTiles, true);
				BattleData.uiManager.activateDetailInfoEvent.AddListener (ToNextStep);
			};
			ResetMissionCondition = () => {
				BattleData.uiManager.activateDetailInfoEvent.RemoveListener (ToNextStep);
				TM.SetHighlightTiles (TM.GetTilesInGlobalRange (), false);
				TM.SetPreselectLock (false);
				TM.DepreselectAllTiles ();
				UIManager.Instance.ActionButtonOnOffLock = false;
			};
		} else if (mission == Mission.CloseDetailInfo) {
			SetMissionCondition = () => {
				BattleData.uiManager.deactivateDetailInfoEvent.AddListener (ToNextStep);
				BattleData.rightClickLock = false;
			};
			ResetMissionCondition = () => {
				BattleData.rightClickLock = true;
				BattleData.uiManager.deactivateDetailInfoEvent.RemoveListener (ToNextStep);
			};
		} else if(mission==Mission.CancelMoveOrSkill) {
			SetMissionCondition = () => {
				UIManager.Instance.TurnOffAllActions();
				UIManager.Instance.ActionButtonOnOffLock = true;
				TM.DepreselectAllTiles ();
				TM.SetPreselectLock (true);
				BM.readyCommandEvent.AddListener (ToNextStep);
				BattleData.rightClickLock = false;
			};
			ResetMissionCondition = () => {
				BattleData.rightClickLock = true;
				BM.readyCommandEvent.RemoveListener (ToNextStep);
				TM.SetPreselectLock (false);
				UIManager.Instance.ActionButtonOnOffLock = false;
			};
		}

		//마우스 표시가 되는지 아닌지. false의 경우에도 일일이 표기해야 한다
		/*if(parser.ConsumeBool()){
			mouseMarkPos = new Vector3(parser.ConsumeFloat(), parser.ConsumeFloat(), 0);
		}*/
	}
}