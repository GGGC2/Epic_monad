using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

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

	public static List<UnitInfo> GetParsedUnitInfo()
	{
		Debug.LogError("Parse Unit INfo");
		List<UnitInfo> unitInfoList = new List<UnitInfo>();
		
		TextAsset csvFile;
		if (FindObjectOfType<StageManager>() != null)
			csvFile = FindObjectOfType<StageManager>().unitData as TextAsset;
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
	
	public static List<SkillInfo> GetParsedSkillInfo()
	{
		List<SkillInfo> skillInfoList = new List<SkillInfo>();
		
		TextAsset csvFile = Resources.Load("Data/testSkillData") as TextAsset;
		string csvText = csvFile.text;
		string[] unparsedSkillInfoStrings = csvText.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
		
		for (int i = 1; i < unparsedSkillInfoStrings.Length; i++)
		{
			SkillInfo skillInfo = new SkillInfo(unparsedSkillInfoStrings[i]);
			skillInfoList.Add(skillInfo);
		}
		
		return skillInfoList;
	}
	
	public static List<TileInfo> GetParsedTileInfo()
	{
		List<TileInfo> tileInfoList = new List<TileInfo>();
		
		TextAsset csvFile;
		if (FindObjectOfType<StageManager>() != null)
			csvFile = FindObjectOfType<StageManager>().mapData as TextAsset;
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
}
