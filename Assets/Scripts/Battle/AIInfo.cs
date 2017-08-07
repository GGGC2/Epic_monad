using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Enums;
using System.Linq;

public class AIInfo {
	public int index; // 고유 인덱스. Unit의 인덱스와 동일.
	public List<int> activeTriggers = new List<int>();
	public int activePhase;
	public List<List<Tile>> trigger3Area = new List<List<Tile>>();
	public List<List<Tile>> trigger4Area = new List<List<Tile>>();

	public AIInfo (string data)
	{
		TileManager tileManager = MonoBehaviour.FindObjectOfType<TileManager>();
		UnitManager unitManager = MonoBehaviour.FindObjectOfType<UnitManager>();
		StringParser commaParser = new StringParser(data, ',');

		this.index = commaParser.ConsumeInt();

		commaParser.Consume();
		commaParser.Consume();

		string unParsedActiveTriggers = commaParser.Consume();
		string[] origin = unParsedActiveTriggers.Split('.');
		origin.ToList().ForEach(text => activeTriggers.Add(Int32.Parse(text)));

		this.activePhase = commaParser.ConsumeInt();
		
		int numOfTrigger3Area = commaParser.ConsumeInt();
		int numOfTrigger4Area = commaParser.ConsumeInt();
		
		for (int i = 0; i < numOfTrigger3Area; i++)
		{
			List<Tile> triggerArea = new List<Tile>();

			RangeForm rangeForm = commaParser.ConsumeEnum<RangeForm>();
			int midPosX = commaParser.ConsumeInt();
			int midPosY = commaParser.ConsumeInt();
			Vector2 midPosition = new Vector2(midPosX, midPosY);
			Vector2 unitPosition = unitManager.GetAllUnits().Find(unit => unit.GetIndex() == index).GetInitPosition();
			midPosition = midPosition + unitPosition;
			int minReach = commaParser.ConsumeInt();
			int maxReach = commaParser.ConsumeInt();
			int width = commaParser.ConsumeInt();
			Direction direction = commaParser.ConsumeEnum<Direction>();

			triggerArea = tileManager.GetTilesInRange(rangeForm, midPosition, minReach, maxReach, width, direction);

			trigger3Area.Add(triggerArea);
		}

		for (int i = 0; i < numOfTrigger4Area; i++)
		{
			List<Tile> triggerArea = new List<Tile>();

			RangeForm rangeForm = commaParser.ConsumeEnum<RangeForm>();
			int midPosX = commaParser.ConsumeInt();
			int midPosY = commaParser.ConsumeInt();
			Vector2 midPosition = new Vector2(midPosX, midPosY);
			int minReach = commaParser.ConsumeInt();
			int maxReach = commaParser.ConsumeInt();
			int width = commaParser.ConsumeInt();
			Direction direction = commaParser.ConsumeEnum<Direction>();

			triggerArea = tileManager.GetTilesInRange(rangeForm, midPosition, minReach, maxReach, width, direction);

			trigger4Area.Add(triggerArea);
		}
	}
}
