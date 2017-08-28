using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Enums;
using BattleUI;

public class AIScenario{
	public static TutorialManager tutorialManager;

	public int index;
	enum Act{ Move, UseSkill, StandbyOrRest, SkipTurn, End }
	Act act;
	public string functionName;
	public object parameter;
	//public bool IsEndMission { get { return act == Act.End; } }

	public AIScenario(string data){
		StringParser parser = new StringParser(data, '\t');
		index = parser.ConsumeInt();
		act = parser.ConsumeEnum<Act>();

		BattleManager BM = BattleData.battleManager;
		TileManager TM = BattleData.tileManager;

		if (act == Act.Move) {
			Vector2 destPos = new Vector2 (parser.ConsumeInt (), parser.ConsumeInt ());
			Tile destTile = TM.GetTile (destPos);
			functionName = "MoveToTheTileAndChangeDirection";
			parameter = destTile;
		} else if (act == Act.UseSkill) {
			int missionSkillIndex = parser.ConsumeInt ();
			Direction missionDirection = parser.ConsumeEnum<Direction> ();
			Vector2 targetPos = new Vector2 (parser.ConsumeInt (), parser.ConsumeInt ());
			functionName = "UseSkill";
		} else if (act == Act.StandbyOrRest){
			functionName = "StandbyOrRest";
			parameter = null;
		} else if (act == Act.SkipTurn){
			functionName = "SkipTurn";
			parameter = null;
		}
	}
}