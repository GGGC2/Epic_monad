using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Enums;

public class BattleTrigger{
	public bool acquired;
	public bool repeatable;
	public TrigResultType resultType;
	public TrigUnitType unitType;
	public string unitName;
	public TrigActionType actionType;
	public int reward;
	public int count;
	public int targetCount;

	public List<string> targetUnitNames;
	public List<Vector2> targetTiles = new List<Vector2>();
	public string nextSceneIndex;
	public string korName;
	
	//승리&패배 조건이 전부 만족시켜야 하는지/하나만 만족시켜도 되는지에 대한 정보
	public enum TriggerRelation{One, All, Sequence}
	public TriggerRelation winTriggerRelation;
	public TriggerRelation loseTriggerRelation;

	public BattleTrigger(string data){
		StringParser commaParser = new StringParser(data, '\t');

		resultType = commaParser.ConsumeEnum<TrigResultType>();
		if(resultType == TrigResultType.End) {
			nextSceneIndex = commaParser.Consume();
			winTriggerRelation = commaParser.ConsumeEnum<TriggerRelation>();
			loseTriggerRelation = commaParser.ConsumeEnum<TriggerRelation>();
		}else{
			korName = commaParser.Consume();
			unitType = commaParser.ConsumeEnum<TrigUnitType>();
		
			actionType = commaParser.ConsumeEnum<TrigActionType>();
			targetCount = commaParser.ConsumeInt();
			repeatable = commaParser.ConsumeBool();
			reward = commaParser.ConsumeInt();

			if(unitType == TrigUnitType.Target){
				targetCount = commaParser.ConsumeInt();
				targetUnitNames = new List<string>();
				for (int i = 0; i < targetCount; i++){
					string targetUnitName = commaParser.Consume();
					targetUnitNames.Add(targetUnitName);
				}
			}

			if(actionType == TrigActionType.Reach){
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