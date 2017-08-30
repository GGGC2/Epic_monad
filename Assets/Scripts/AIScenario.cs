using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Enums;
using BattleUI;

public class AIScenario{
	public int index;
	enum Act{ Move, UseSkill, StandbyOrRest, SkipTurn }
	Act act;
	public string functionName;
	public object parameter;
	public int skillIndex;
	Vector2 casterPos;
	Vector2 targetPos;
	Direction direction;
	public SkillLocation skillLocation;

	public AIScenario(string data){
		StringParser parser = new StringParser(data, '\t');
		index = parser.ConsumeInt();
		act = parser.ConsumeEnum<Act>();

		//BattleManager BM = BattleData.battleManager;
		TileManager TM = BattleData.tileManager;

		if (act == Act.Move) {
			Vector2 destPos = new Vector2 (parser.ConsumeInt (), parser.ConsumeInt ());
			Tile destTile = TM.GetTile (destPos);
			functionName = "MoveToTheTileAndChangeDirection";
			parameter = destTile;
		} else if (act == Act.UseSkill) {
			skillIndex = parser.ConsumeInt ();
			casterPos = new Vector2 (parser.ConsumeInt (), parser.ConsumeInt ());
			targetPos = new Vector2 (parser.ConsumeInt (), parser.ConsumeInt ());
			direction = parser.ConsumeEnum<Direction> ();
			skillLocation = new SkillLocation (casterPos, targetPos, direction);
			functionName = "UseSkill";
		} else if (act == Act.StandbyOrRest) {
			functionName = "StandbyOrRest";
			parameter = null;
		} else if (act == Act.SkipTurn) {
			functionName = "SkipTurn";
			parameter = null;
		} else {
			Debug.LogError ("Invalid AI scenario action name");
		}
	}
}