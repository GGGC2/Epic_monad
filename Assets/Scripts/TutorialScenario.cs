using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Enums;

public class TutorialScenario{
	public int index;
	public enum Mission{None, MoveCommand, SkillCommand, Standby, Rest, SelectTile, SelectDirection, SelectSkill, Apply, End}
	public Mission mission;
	public Vector2 missionTilePos;
	public Direction missionDirection;
	public int missionSkillIndex;
	public Action SetMissionCondition = () => {};
	public Action ResetMissionCondition = () => {};

	public TutorialScenario(string data){
		StringParser parser = new StringParser(data, '\t');
		index = parser.ConsumeInt();
		mission = parser.ConsumeEnum<Mission>();

		if (mission == Mission.SelectTile) {
			missionTilePos = new Vector2 (parser.ConsumeInt (), parser.ConsumeInt ());
			Tile missionTile = TileManager.Instance.GetTile (missionTilePos);
			List<Tile> clickableTiles = new List<Tile> ();
			clickableTiles.Add (missionTile);
			SetMissionCondition = () => {
				//뭔가 해보려다 일단 중단한 거니 지우지 말 것
				//TileManager.Instance.DepreselectAllTiles ();
				//TileManager.Instance.PreselectTiles (clickableTiles);
				TileManager.Instance.PaintTiles (clickableTiles, TileColor.Black);
			};
			ResetMissionCondition = () => {
				//TileManager.Instance.DepreselectAllTiles ();
				TileManager.Instance.DepaintAllTiles (TileColor.Black);
			};
		}
		else if(mission == Mission.SelectDirection)
			missionDirection = parser.ConsumeEnum<Direction>();
		else if(mission == Mission.SelectSkill)
			missionSkillIndex = parser.ConsumeInt();
	}
}