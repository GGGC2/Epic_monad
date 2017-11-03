using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Enums;
using BattleUI;

public class AIScenario{
	public enum ScenarioAct{ Move, UseSkill, StandbyOrRest, SkipTurn }
	public ScenarioAct act;
	public string functionName;
	public int skillIndex;
	public Vector2 targetPos;
	public Direction direction;

	public AIScenario(string data){
		StringParser parser = new StringParser(data, '\t');
		act = parser.ConsumeEnum<ScenarioAct>();

		//BattleManager BM = BattleData.battleManager;
		TileManager TM = BattleData.tileManager;

		if (act == ScenarioAct.Move) {
			targetPos = new Vector2 (parser.ConsumeInt (), parser.ConsumeInt ());
			functionName = "MoveToThePositionAndChangeDirection";
		} else if (act == ScenarioAct.UseSkill) {
			skillIndex = parser.ConsumeInt ();
			targetPos = new Vector2 (parser.ConsumeInt (), parser.ConsumeInt ());
			direction = parser.ConsumeEnum<Direction> ();
			functionName = "UseSkill";
		} else if (act == ScenarioAct.StandbyOrRest) {
			functionName = "StandbyOrRest";
		} else if (act == ScenarioAct.SkipTurn) {
			functionName = "SkipTurn";
		} else {
			Debug.LogError ("Invalid AI scenario action name");
		}
	}
}