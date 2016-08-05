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

	public BattleEndTrigger()
	{
		result = Enums.BattleResult.Win;
		triggerNumber = 1;
		maxPhase = 100;
		targetUnitNames = new List<string>();
		minNumberOfTargetUnit = 0;
		minNumberOfAlly = 0;
		minNumberOfEnemy = 0;
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
	}	
}