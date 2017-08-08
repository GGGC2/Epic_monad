using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Enums;

public class TutorialScenario{
	public int index;
	public enum Mission{None, MoveCommand, SkillCommand, Standby, SelectTile, SelectDirection, SelectSkill, Apply}
	public Mission mission;
	public Vector2 missionTile;
	public Direction missionDirection;
	public int missionSkillIndex;

	public TutorialScenario(string data){
		StringParser parser = new StringParser(data, '\t');
		index = parser.ConsumeInt();
		mission = parser.ConsumeEnum<Mission>();
		
		if(mission == Mission.SelectTile)
			missionTile = new Vector2(parser.ConsumeInt(), parser.ConsumeInt());
		else if(mission == Mission.SelectDirection)
			missionDirection = parser.ConsumeEnum<Direction>();
		else if(mission == Mission.SelectSkill)
			missionSkillIndex = parser.ConsumeInt();
	}
}