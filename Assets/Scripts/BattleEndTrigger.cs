using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Enums;

public class BattleEndTrigger
{
	public BattleResult result;
	public int triggerNumber;
	public int maxPhase;
	public List<string> targetUnitNames;
	public int minNumberOfTargetUnit;
	public int minNumberOfAlly;
	public int minNumberOfEnemy;
	public List<Vector2> targetTiles = new List<Vector2>();
	public List<string> reachedTargetUnitNames = new List<string>();
	public int minNumberOfReachedTargetUnit;
	public int minNumberOfReachedAlly;
	public int minNumberOfReachedEnemy;
	
	public BattleEndTrigger()
	{
		result = Enums.BattleResult.Win;
		triggerNumber = 1;
		maxPhase = 100;
		targetUnitNames = new List<string>();
		minNumberOfTargetUnit = 0;
		minNumberOfAlly = 0;
		minNumberOfEnemy = 0;
		targetTiles = new List<Vector2>();
		reachedTargetUnitNames = new List<string>();
		minNumberOfReachedTargetUnit = 0;
		minNumberOfReachedAlly = 0;
		minNumberOfReachedEnemy = 0;
	}

	public BattleEndTrigger(string data)
	{
		CommaStringParser commaParser = new CommaStringParser(data);

		result = commaParser.ConsumeEnum<BattleResult>();
		triggerNumber = commaParser.ConsumeInt();

		if (triggerNumber == 5 || triggerNumber == 6 || triggerNumber == 9 || triggerNumber == 10)
			return;

		if (triggerNumber == 1)
		{
			maxPhase = commaParser.ConsumeInt();
		}
		else if (triggerNumber == 2 || triggerNumber == 3 || triggerNumber == 4)
		{
			targetUnitNames = new List<string>();
			int numberOfName = commaParser.ConsumeInt();
			for (int i = 0; i < numberOfName; i++)
			{
				string targetUnitName = commaParser.Consume();
				targetUnitNames.Add(targetUnitName);
			}

			if (triggerNumber == 3)
			{
				minNumberOfTargetUnit = commaParser.ConsumeInt();
			}
		}
		else if (triggerNumber == 7)
		{
			minNumberOfAlly = commaParser.ConsumeInt();
		}
		else if (triggerNumber == 8)
		{
			minNumberOfEnemy = commaParser.ConsumeInt();
		}
		else if (triggerNumber > 10 && triggerNumber < 20)
		{
			targetTiles = new List<Vector2>();
			int numberOfTiles = commaParser.ConsumeInt();
			for (int i = 0; i < numberOfTiles; i++)
			{
				int x = commaParser.ConsumeInt();
				int y = commaParser.ConsumeInt();
				Vector2 position = new Vector2(x, y);
				targetTiles.Add(position);
			}

			if (triggerNumber == 11 || triggerNumber == 12 || triggerNumber == 13)
			{
				reachedTargetUnitNames = new List<string>();
				int numberOfName = commaParser.ConsumeInt();
				for (int i = 0; i < numberOfName; i++)
				{
					string targetUnitName = commaParser.Consume();
					reachedTargetUnitNames.Add(targetUnitName);
				}

				if (triggerNumber == 12)
				{
					minNumberOfReachedTargetUnit = commaParser.ConsumeInt();
				}
			}
			else if (triggerNumber == 16)
			{
				minNumberOfReachedAlly = commaParser.ConsumeInt();
			}
			else if (triggerNumber == 17)
			{
				minNumberOfReachedEnemy = commaParser.ConsumeInt();
			}
		}
	}	
}