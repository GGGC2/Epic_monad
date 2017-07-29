using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Enums;

public class BattleTrigger{
	public enum ResultType{Win, Lose, Bonus, End}
	public enum UnitType{Target, Ally, Enemy, None}
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

	public BattleTrigger(string data)
	{
		CommaStringParser commaParser = new CommaStringParser(data);

		resultType = commaParser.ConsumeEnum<ResultType>();
		if(resultType == ResultType.End)
			nextSceneIndex = commaParser.Consume();
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