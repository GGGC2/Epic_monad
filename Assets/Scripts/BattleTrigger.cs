﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Enums;

public class BattleTrigger
{
	public enum ResultType{Win, Lose, Bonus, End}
	public enum UnitType{Target, Ally, Enemy, None}
	public enum ActionType{Neutralize, Reach, Phase}
	public bool acquired;
	public ResultType resultType;
	public UnitType unitType;
	public ActionType actionType;
	public bool phaseCheck;
	public int triggerNumber;
	public int reward;
	public int countDown;
	public List<string> targetUnitNames;
	public List<Vector2> targetTiles = new List<Vector2>();
	public List<string> reachedTargetUnitNames = new List<string>();
	public string nextSceneIndex;

	public BattleTrigger(string data)
	{
		CommaStringParser commaParser = new CommaStringParser(data);

		resultType = commaParser.ConsumeEnum<ResultType>();
		if(resultType == ResultType.End)
			nextSceneIndex = commaParser.Consume();
		else{
			unitType = commaParser.ConsumeEnum<UnitType>();
		
			actionType = commaParser.ConsumeEnum<ActionType>();
			countDown = commaParser.ConsumeInt();
			reward = commaParser.ConsumeInt();

			if(unitType == UnitType.Target){
				int targetCount = commaParser.ConsumeInt();
				targetUnitNames = new List<string>();
				for (int i = 0; i < targetCount; i++)
				{
					string targetUnitName = commaParser.Consume();
					targetUnitNames.Add(targetUnitName);
				}
			}

			if(actionType == ActionType.Reach){
				targetTiles = new List<Vector2>();
				int numberOfTiles = commaParser.ConsumeInt();
				for (int i = 0; i < numberOfTiles; i++)
				{
					int x = commaParser.ConsumeInt();
					int y = commaParser.ConsumeInt();
					Vector2 position = new Vector2(x, y);
					targetTiles.Add(position);
				}
			}
		}
	}	
}