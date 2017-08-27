using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Enums;

public class BattleTrigger{
	public enum ResultType{Win, Lose, Bonus, End}
	public enum UnitType{Target, Ally, Enemy, None, PC}
	public enum ActionType{Neutralize, Reach, Phase, Kill, Retreat}
	public bool acquired;
	public bool repeatable;
	public ResultType resultType;
	public UnitType unitType;
	public string unitName;
	public ActionType actionType;
	public int reward;
	public int count;
	public int targetCount;

	public List<string> targetUnitNames;
	public List<Vector2> targetTiles = new List<Vector2>();
	public string nextSceneIndex;
	public string korName;
	//승리, 패배 조건이 전부 만족시켜야 하는가? 하나만 만족시켜도 되는가?
	public bool winTriggerAll;
	public bool loseTriggerAll;

	public BattleTrigger(string data){
		StringParser commaParser = new StringParser(data, '\t');

		resultType = commaParser.ConsumeEnum<ResultType>();
		if(resultType == ResultType.End) {
			nextSceneIndex = commaParser.Consume();
			winTriggerAll = commaParser.ConsumeBool();
			loseTriggerAll = commaParser.ConsumeBool();
		}
		else{
			korName = commaParser.Consume();
			unitType = commaParser.ConsumeEnum<UnitType>();
		
			actionType = commaParser.ConsumeEnum<ActionType>();
			targetCount = commaParser.ConsumeInt();
			repeatable = commaParser.ConsumeBool();
			reward = commaParser.ConsumeInt();

			if(unitType == UnitType.Target){
				targetCount = commaParser.ConsumeInt();
				targetUnitNames = new List<string>();
				for (int i = 0; i < targetCount; i++){
					string targetUnitName = commaParser.Consume();
					targetUnitNames.Add(targetUnitName);
				}
			}

			if(actionType == ActionType.Reach){
				targetTiles = new List<Vector2>();
				int numberOfTiles = commaParser.ConsumeInt();
				for (int i = 0; i < numberOfTiles; i++){
					int x = commaParser.ConsumeInt();
					int y = commaParser.ConsumeInt();
					Vector2 position = new Vector2(x, y);
					targetTiles.Add(position);
				}
			}
		}
	}	
}