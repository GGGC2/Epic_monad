using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using WorldMap;
using SkillTree;
using GameData;

public class Parser : MonoBehaviour{
	public static string ExtractFromMatrix(string text, int row, int column){
		string[] RowDataStrings = text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
		return RowDataStrings[row].Split(',')[column];
	}
	
	//각 행 중에 첫번째 항목 searchigWord로 시작하는 행을 찾아서 return
	public static string[] FindRowDataOf(string text, string searchingWord){
		string[] RowDataStrings = text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

		foreach(string row in RowDataStrings){
			string[] tempRowData = row.Split('\t');
			if(tempRowData[0] == searchingWord){
				return tempRowData;
			}
		}

		Debug.Log("RowData Not Found : " + searchingWord + "in " + text);
		return null;
	}

	public static List<T> GetParsedData<T>(){
		List<T> DataList = new List<T>();
		TextAsset textAsset = GetDataAddress<T>();
		string[] rowDataList = textAsset.text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

		for(int i = 1; i < rowDataList.Length; i++){
			T data = CreateParsedObject<T>(rowDataList[i]);
			DataList.Add(data);
		}
		return DataList;
	}

    public static T GetParsedData<T>(int index) {
        TextAsset textAsset = GetDataAddress<T>();
        string[] rowDataList = textAsset.text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        for(int i = 1; i < rowDataList.Length; i++) {
            StringParser commaParser = new StringParser(rowDataList[i], ',');
            String parseIndexString = commaParser.Consume();
            int parseIndex;
            try { 
                parseIndex = Int32.Parse(parseIndexString);
            }
            catch {
                parseIndex = new StringParser(rowDataList[i], '\t').ConsumeInt();
            }
            if(parseIndex == index)
                return CreateParsedObject<T>(rowDataList[i]);
        }
        return default(T);
    }

	public static T CreateParsedObject<T>(string rowData){
		if(typeof(T) == typeof(GlossaryData)){
			object data = new GlossaryData(rowData);
			return (T)data;			
		}else if(typeof(T) == typeof(DialogueData)){
			object data = new DialogueData(rowData);
			return (T)data;
		}else if(typeof(T) == typeof(TutorialScenario)){
			object data = new TutorialScenario(rowData);
			return (T)data;
		}else if(typeof(T) == typeof(AIScenario)){
			object data = new AIScenario(rowData);
			return (T)data;
		}else if(typeof(T) == typeof(UnitStatusEffectInfo)){
			object data = new UnitStatusEffectInfo(rowData);
			return (T)data;
		}else if(typeof(T) == typeof(ActiveSkill)){
			object data = new ActiveSkill(rowData);
			return (T)data;
		}else if(typeof(T) == typeof(PassiveSkill)){
			object data = new PassiveSkill(rowData);
			return (T)data;
		}else if(typeof(T) == typeof(BattleTrigger)){
			object data = BattleTriggerFactory.Get(rowData);
			return (T)data;
		}else if(typeof(T) == typeof(AIInfo)){
			object data = new AIInfo(rowData);
			return (T)data;
		}else if(typeof(T) == typeof(UnitInfo)){
			object data = new UnitInfo(rowData);
			return (T)data;
		}
		else{
			Debug.LogError("Invalid Input");
			//컴파일할 때 뭔가 리턴해야 해서 만듦
			object garbage = null;
			return (T)garbage;
		}
	}

	static TextAsset GetDataAddress<T>(){
		BattleManager BM = FindObjectOfType<BattleManager>();
		string address = "";
		if(typeof(T) == typeof(GlossaryData)) {address = "Data/Glossary";}
		else if(typeof(T) == typeof(DialogueData)) {address = "Data/" + SceneData.dialogueName;}
		else if(typeof(T) == typeof(TutorialScenario)) {address = "Tutorial/" + SceneManager.GetActiveScene().name + SceneData.stageNumber.ToString();}
		else if(typeof(T) == typeof(AIScenario)) {address = "Tutorial/" + SceneManager.GetActiveScene ().name + SceneData.stageNumber.ToString () + "_AI";}
		else if(typeof(T) == typeof(UnitStatusEffectInfo)) {address = "Data/UnitStatusEffectData";}
		else if(typeof(T) == typeof(ActiveSkill)) {address = "Data/ActiveSkillData";}
		else if(typeof(T) == typeof(PassiveSkill)) {address = "Data/PassiveSkillData";}
		else if(typeof(T) == typeof(BattleTrigger)) {return BM.GetBattleTriggerData();}
		else if(typeof(T) == typeof(AIInfo)) {return BM.GetAIData();}
		else if(typeof(T) == typeof(UnitInfo)) {return BM.GetUnitData();}

		if(address == "") {Debug.LogError("Invalid Input : " + typeof(T));}		
		
		return Resources.Load<TextAsset>(address);
	}

	private static List<Skill> skillInfosCache;

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

	private static Dictionary<string, Skill> skillByName;
	public static Skill GetSkillByName(string skillName){
		if (skillByName == null){
			skillByName = new Dictionary<string, Skill>();

			List<ActiveSkill> ActiveSkillList = GetParsedData<ActiveSkill>();
			foreach (ActiveSkill skill in ActiveSkillList)
				skillByName.SmartAdd(skill.korName, skill);
			List<PassiveSkill> PassiveSkillList = GetParsedData<PassiveSkill>();
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

			List<ActiveSkill> ActiveSkillList = GetParsedData<ActiveSkill>();
			foreach (ActiveSkill skill in ActiveSkillList){
				if (!skillByUnit.ContainsKey(skill.owner))
					skillByUnit.SmartAdd(skill.owner, new List<Skill>());
				skillByUnit[skill.owner].Add(skill);
			}
			List<PassiveSkill> PassiveSkillList = GetParsedData<PassiveSkill>();
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

    public static List<TileStatusEffectInfo> GetParsedTileStatusEffectInfo() {
        List<TileStatusEffectInfo> tileStatusEffectInfoList = new List<TileStatusEffectInfo>();

        TextAsset csvFile = Resources.Load("Data/tileStatusEffectData") as TextAsset;
        string csvText = csvFile.text;
        string[] unparsedTileStatusEffectInfoStrings = csvText.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 1; i < unparsedTileStatusEffectInfoStrings.Length; i++) {
            TileStatusEffectInfo tileStatusEffectInfo = new TileStatusEffectInfo(unparsedTileStatusEffectInfoStrings[i]);
            tileStatusEffectInfoList.Add(tileStatusEffectInfo);
        }

        return tileStatusEffectInfoList;
    }

    public static List<TileInfo> GetParsedTileInfo(){
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

	public static List<StoryInfo> GetParsedStoryInfo(){
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

	public static List<SkillColumnInfo> GetSkillColumInfo(){
		List<SkillColumnInfo> columnInfos = new List<SkillColumnInfo>();
		TextAsset csvFile = Resources.Load("Data/unitSkillColumns") as TextAsset;

		string csvText = csvFile.text;
		string[] lines = csvText.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

		List<string> dataLines = new List<string>(lines);
		dataLines = dataLines.GetRange(1, lines.Length - 1);

		foreach (string line in dataLines){
			SkillColumnInfo storyInfo = new SkillColumnInfo(line);
			columnInfos.Add(storyInfo);
		}

		return columnInfos;
	}

	public static List<PlaceInfo> GetPlacesInfo(int stageNumber){
		List<PlaceInfo> placeInfoList = new List<PlaceInfo>();
		TextAsset csvFile = Resources.Load("Data/stage"+stageNumber+"_unitPos") as TextAsset;

		string csvText = csvFile.text;
		string[] lines = csvText.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

		List<string> dataLines = new List<string>(lines);
		dataLines = dataLines.GetRange(1, lines.Length - 1);

		foreach (string line in dataLines){
			PlaceInfo placeInfo = new PlaceInfo(line);
			placeInfoList.Add(placeInfo);
		}

		return placeInfoList;
	}
}
