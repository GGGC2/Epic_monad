using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using WorldMap;
using SkillTree;

public class Parser : MonoBehaviour{
	public static string ExtractFromMatrix(string text, int row, int column){
		string[] RowDataStrings = text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
		return RowDataStrings[row].Split(',')[column];
	}
	
	public static string[] FindRowDataOf(string text, string searchingWord){
		string[] RowDataStrings = text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

		foreach(string row in RowDataStrings){
			string[] tempRowData = row.Split(',');
			if(tempRowData[0] == searchingWord)
				return tempRowData;
		}

		Debug.Log("RowData Not Found : " + searchingWord + "in " + text);
		return null;
	}

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

	public static List<BattleTrigger> GetParsedBattleTriggerData()
	{
		List<BattleTrigger> battleEndTriggers = new List<BattleTrigger>();

		TextAsset csvFile = FindObjectOfType<BattleManager>().GetBattleConditionData() as TextAsset;
		string csvText = csvFile.text;
		string[] unparsedBattleEndConditionDataStrings = csvText.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

		for (int i = 1; i < unparsedBattleEndConditionDataStrings.Length; i++){
			BattleTrigger battleTrigger = new BattleTrigger(unparsedBattleEndConditionDataStrings[i]);
			battleEndTriggers.Add(battleTrigger);
		}

		return battleEndTriggers;
	}

	public static List<AIInfo> GetParsedAIInfo()
	{
		List<AIInfo> aiInfoList = new List<AIInfo>();
		TextAsset csvFile = null;
		if (FindObjectOfType<BattleManager>() != null)
			csvFile = FindObjectOfType<BattleManager>().GetAIData() as TextAsset;
		else
			Debug.LogError("No BattleManager");

		string csvText = csvFile.text;
		string[] unparsedAIInfoStrings = csvText.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

		for (int i = 1; i < unparsedAIInfoStrings.Length; i++)
		{
			try
			{
				AIInfo aiInfo = new AIInfo(unparsedAIInfoStrings[i]);
				aiInfoList.Add(aiInfo);
			}
			catch (Exception e)
			{
				Debug.LogError("Parsing failed in \n" +
						" line is : " + i + "\n" +
						" data is : " + unparsedAIInfoStrings[i]);
				throw e;
			}
		}

		return aiInfoList;
	}

	public static List<UnitInfo> GetParsedUnitInfo()
	{
		List<UnitInfo> unitInfoList = new List<UnitInfo>();

		TextAsset csvFile = null;
		if (FindObjectOfType<BattleManager>() != null)
			csvFile = FindObjectOfType<BattleManager>().GetUnitData() as TextAsset;
		else
			Debug.LogError("No BattleManager");
			//csvFile = Resources.Load("Data/testStageUnitData") as TextAsset;
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

	private static List<Skill> skillInfosCache;

	public static List<ActiveSkill> GetActiveSkills(){
		List<ActiveSkill> ActiveSkills = new List<ActiveSkill>();

		string csvText = Resources.Load<TextAsset>("Data/ActiveSkillData").text;
		string[] rowDataList = csvText.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

		for(int i = 1; i < rowDataList.Length; i++){
			ActiveSkill skill = new ActiveSkill(rowDataList[i]);
			ActiveSkills.Add(skill);
		}

		return ActiveSkills;
	}

	public static List<Skill> GetSkills(){
		List<Skill> Skills = new List<Skill>();

		string activeSkillData = Resources.Load<TextAsset>("Data/ActiveSkillData").text;
		string[] activeRowDataList = activeSkillData.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
		for(int i = 1; i < activeRowDataList.Length; i++){
			Skill skill = new ActiveSkill(activeRowDataList[i]);
			Skills.Add(skill);
		}

		string passiveSkillData = Resources.Load<TextAsset>("Data/PassiveSkillData").text;
		string[] passiveRowDataList = passiveSkillData.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
		for(int i = 1; i < passiveRowDataList.Length; i++){
			Skill skill = new PassiveSkill(passiveRowDataList[i]);
			Skills.Add(skill);
		}

		return Skills;
	}
	
	public static List<PassiveSkill> GetPassiveSkills(){
		List<PassiveSkill> PassiveSkills = new List<PassiveSkill>();

		string csvText = Resources.Load<TextAsset>("Data/PassiveSkillData").text;
		string[] rowDataList = csvText.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

		for(int i = 1; i < rowDataList.Length; i++){
			PassiveSkill skill = new PassiveSkill(rowDataList[i]);
			PassiveSkills.Add(skill);
		}

		return PassiveSkills;
	}

	private static Dictionary<string, Skill> skillByName;
	public static Skill GetSkillByName(string skillName){
		if (skillByName == null){
			skillByName = new Dictionary<string, Skill>();

			List<ActiveSkill> ActiveSkillList = GetActiveSkills();
			foreach (ActiveSkill skill in ActiveSkillList)
				skillByName.SmartAdd(skill.korName, skill);
			List<PassiveSkill> PassiveSkillList = GetPassiveSkills();
			foreach(PassiveSkill skill in PassiveSkillList)
				skillByName.SmartAdd(skill.korName, skill);
		}

		if (skillByName.ContainsKey(skillName))
			return skillByName[skillName];
		else
			return null;
	}

	private static Dictionary<string, List<Skill>> skillByUnit;
	public static List<Skill> GetSkillByUnit(string unitName){
		if (skillByUnit == null){
			skillByUnit = new Dictionary<string, List<Skill>>();

			List<ActiveSkill> ActiveSkillList = GetActiveSkills();
			foreach (ActiveSkill skill in ActiveSkillList){
				if (!skillByUnit.ContainsKey(skill.owner))
					skillByUnit.SmartAdd(skill.owner, new List<Skill>());
				skillByUnit[skill.owner].Add(skill);
			}
			List<PassiveSkill> PassiveSkillList = GetPassiveSkills();
			foreach(PassiveSkill skill in PassiveSkillList){
				if (!skillByUnit.ContainsKey(skill.owner))
					skillByUnit.SmartAdd(skill.owner, new List<Skill>());
				skillByUnit[skill.owner].Add(skill);
			}
		}

		try
		{
			return skillByUnit[unitName];
		}
		catch (Exception e)
		{
			Debug.LogError("Cannot find skills for unit " + unitName);
			throw e;
		}
	}

    public static List<StatusEffectInfo> GetParsedStatusEffectInfo() {
        List<StatusEffectInfo> statusEffectInfoList = new List<StatusEffectInfo>();

        TextAsset csvFile = Resources.Load("Data/StatusEffectData") as TextAsset;
		string csvText = csvFile.text;
		string[] unparsedStatusEffectInfoStrings = csvText.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

		for (int i = 1; i < unparsedStatusEffectInfoStrings.Length; i++) {
            StatusEffectInfo statusEffectInfo = new StatusEffectInfo(unparsedStatusEffectInfoStrings[i]);
			statusEffectInfoList.Add(statusEffectInfo);
		}

		return statusEffectInfoList;
    }

    public static List<TileStatusEffectInfo> GetParsedTileStatusEffectInfo() {
        List<TileStatusEffectInfo> tileStatusEffectInfoList = new List<TileStatusEffectInfo>();

        TextAsset csvFile = Resources.Load("Data/testTileStatusEffectData") as TextAsset;
        string csvText = csvFile.text;
        string[] unparsedTileStatusEffectInfoStrings = csvText.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 1; i < unparsedTileStatusEffectInfoStrings.Length; i++) {
            TileStatusEffectInfo tileStatusEffectInfo = new TileStatusEffectInfo(unparsedTileStatusEffectInfoStrings[i]);
            tileStatusEffectInfoList.Add(tileStatusEffectInfo);
        }

        return tileStatusEffectInfoList;
    }

    public static List<TileInfo> GetParsedTileInfo()
	{
		List<TileInfo> tileInfoList = new List<TileInfo>();

		TextAsset csvFile;
		if (FindObjectOfType<BattleManager>() != null)
			csvFile = FindObjectOfType<BattleManager>().GetMapData() as TextAsset;
		else
			csvFile = Resources.Load("Data/testMapData") as TextAsset;
		string csvText = csvFile.text;
		string[] unparsedTileInfoStrings = csvText.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

		for (int reverseY = unparsedTileInfoStrings.Length -1; reverseY >= 0 ; reverseY--){
			string[] parsedTileInfoStrings = unparsedTileInfoStrings[reverseY].Split(',');
			for (int x = 1; x <= parsedTileInfoStrings.Length; x++){
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
