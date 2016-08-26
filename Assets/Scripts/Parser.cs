using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using WorldMap;
using SkillTree;

public class Parser : MonoBehaviour {

	public static List<DialogueData> GetParsedDialogueData(TextAsset dialogueDataFile)
	{
		List<DialogueData> dialogueDataList = new List<DialogueData>();

		string csvText = dialogueDataFile.text;
		string[] unparsedDialogueDataStrings = csvText.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

		for (int i = 0; i < unparsedDialogueDataStrings.Length; i++)
		{
			DialogueData dialogueData = new DialogueData(unparsedDialogueDataStrings[i]);
			dialogueDataList.Add(dialogueData);
		}

		return dialogueDataList;
	}

	public static List<BattleEndTrigger> GetParsedBattleEndConditionData()
	{
		List<BattleEndTrigger> battleEndTriggers = new List<BattleEndTrigger>();

		TextAsset csvFile;
		if (FindObjectOfType<StageManager>() == null)
		{
			BattleEndTrigger battleEndTrigger = new BattleEndTrigger();
			battleEndTriggers.Add(battleEndTrigger);
			return battleEndTriggers;
		}

		csvFile = FindObjectOfType<StageManager>().GetBattleEndConditionData() as TextAsset;
		string csvText = csvFile.text;
		string[] unparsedBattleEndConditionDataStrings = csvText.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

		for (int i = 1; i < unparsedBattleEndConditionDataStrings.Length; i++)
		{
			BattleEndTrigger battleEndTrigger = new BattleEndTrigger(unparsedBattleEndConditionDataStrings[i]);
			battleEndTriggers.Add(battleEndTrigger);
		}

		return battleEndTriggers;
	}

	public static List<UnitInfo> GetParsedUnitInfo()
	{
		List<UnitInfo> unitInfoList = new List<UnitInfo>();

		TextAsset csvFile;
		if (FindObjectOfType<StageManager>() != null)
			csvFile = FindObjectOfType<StageManager>().GetUnitData() as TextAsset;
		else
			csvFile = Resources.Load("Data/testStageUnitData") as TextAsset;
		string csvText = csvFile.text;
		string[] unparsedUnitInfoStrings = csvText.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

		for (int i = 1; i < unparsedUnitInfoStrings.Length; i++)
		{
			try
			{
				UnitInfo unitInfo = new UnitInfo(unparsedUnitInfoStrings[i]);
				unitInfoList.Add(unitInfo);
			}
			catch (Exception e)
			{
				Debug.LogError("Parsing failed in \n" +
						" line is : " + i + "\n" +
						" data is : " + unparsedUnitInfoStrings[i]);
				throw e;
			}
		}

		return unitInfoList;
	}

	private static List<SkillInfo> skillInfosCache;
	public static List<SkillInfo> GetParsedSkillInfo()
	{
		if (skillInfosCache != null)
		{
			return skillInfosCache;
		}

		List<SkillInfo> skillInfoList = new List<SkillInfo>();

		TextAsset csvFile = Resources.Load("Data/testSkillData") as TextAsset;
		string csvText = csvFile.text;
		string[] unparsedSkillInfoStrings = csvText.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

		for (int i = 1; i < unparsedSkillInfoStrings.Length; i++)
		{
			SkillInfo skillInfo = new SkillInfo(unparsedSkillInfoStrings[i]);
			skillInfoList.Add(skillInfo);
		}

		skillInfosCache = skillInfoList;
		return skillInfoList;
	}

	private static Dictionary<string, SkillInfo> skillInfoByName;
	public static SkillInfo GetSkillInfoByName(string skillName)
	{
		if (skillInfoByName == null)
		{
			skillInfoByName = new Dictionary<string, SkillInfo>();

			List<SkillInfo> skillInfos = GetParsedSkillInfo();
			foreach (SkillInfo skillInfo in skillInfos)
			{
				skillInfoByName.Add(skillInfo.skill.GetName(), skillInfo);
			}
		}

		return skillInfoByName[skillName];
	}

	private static Dictionary<string, List<SkillInfo>> skillInfoByUnit;
	public static List<SkillInfo> GetSkillInfoByUnit(string unitName)
	{
		if (skillInfoByUnit == null)
		{
			skillInfoByUnit = new Dictionary<string, List<SkillInfo>>();

			List<SkillInfo> skillInfos = GetParsedSkillInfo();
			foreach (SkillInfo skillInfo in skillInfos)
			{
				if (!skillInfoByUnit.ContainsKey(skillInfo.owner))
				{
					skillInfoByUnit.Add(skillInfo.owner, new List<SkillInfo>());
				}

				skillInfoByUnit[skillInfo.owner].Add(skillInfo);
			}
		}

		try
		{
			return skillInfoByUnit[unitName];
		}
		catch (Exception e)
		{
			Debug.LogError("Cannot find skills for unit " + unitName);
			throw e;
		}
	}

    public static List<StatusEffectInfo> GetParsedStatusEffectInfo()
    {
        List<StatusEffectInfo> statusEffectInfoList = new List<StatusEffectInfo>();

        TextAsset csvFile = Resources.Load("Data/testStatusEffectData") as TextAsset;
		string csvText = csvFile.text;
		string[] unparsedStatusEffectInfoStrings = csvText.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

		for (int i = 1; i < unparsedStatusEffectInfoStrings.Length; i++)
		{
			StatusEffectInfo statusEffectInfo = new StatusEffectInfo(unparsedStatusEffectInfoStrings[i]);
			statusEffectInfoList.Add(statusEffectInfo);
		}

		return statusEffectInfoList;
    }

	public static List<TileInfo> GetParsedTileInfo()
	{
		List<TileInfo> tileInfoList = new List<TileInfo>();

		TextAsset csvFile;
		if (FindObjectOfType<StageManager>() != null)
			csvFile = FindObjectOfType<StageManager>().GetMapData() as TextAsset;
		else
			csvFile = Resources.Load("Data/testMapData") as TextAsset;
		string csvText = csvFile.text;
		string[] unparsedTileInfoStrings = csvText.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

		for (int reverseY = unparsedTileInfoStrings.Length -1; reverseY >= 0 ; reverseY--)
		{
			string[] parsedTileInfoStrings = unparsedTileInfoStrings[reverseY].Split(',');
			for (int x = 1; x <= parsedTileInfoStrings.Length; x++)
			{
				Vector2 tilePosition = new Vector2(x, unparsedTileInfoStrings.Length - reverseY);
				// Debug.Log(x + ", " + (unparsedTileInfoStrings.Length - reverseY));
				TileInfo tileInfo = new TileInfo(tilePosition, parsedTileInfoStrings[x-1]);
				tileInfoList.Add(tileInfo);
			}
		}

		return tileInfoList;
	}

	public static List<StoryInfo> GetParsedStoryInfo()
	{
		List<StoryInfo> stories = new List<StoryInfo>();
		TextAsset csvFile;

		csvFile = Resources.Load("Data/worldMap") as TextAsset;

		string csvText = csvFile.text;
		string[] lines = csvText.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

		List<string> dataLines = new List<string>(lines);
		dataLines = dataLines.GetRange(1, lines.Length - 1);

		foreach (string line in dataLines)
		{
			StoryInfo storyInfo = new StoryInfo(line);
			stories.Add(storyInfo);
		}

		return stories;
	}

	public static List<SkillColumnInfo> GetSkillColumInfo()
	{
		List<SkillColumnInfo> columnInfos = new List<SkillColumnInfo>();
		TextAsset csvFile = Resources.Load("Data/unitSkillColumns") as TextAsset;;

		string csvText = csvFile.text;
		string[] lines = csvText.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

		List<string> dataLines = new List<string>(lines);
		dataLines = dataLines.GetRange(1, lines.Length - 1);

		foreach (string line in dataLines)
		{
			SkillColumnInfo storyInfo = new SkillColumnInfo(line);
			columnInfos.Add(storyInfo);
		}

		return columnInfos;
	}
}
