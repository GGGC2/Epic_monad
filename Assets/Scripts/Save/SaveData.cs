using LitJson;
using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Save{
	public class SaveData{
		public Progress progress;
		public PartySaveData party;
		public List<SkillSaveData> skills;

		public static SaveData MakeInitialData(){
			SaveData saveData = new SaveData();
			saveData.progress = Progress.MakeInitialData();
			saveData.party = PartySaveData.MakeInitialData();
			saveData.skills = new List<SkillSaveData>();
			return saveData;
		}
	}

	public class Progress{
		public string worldMap = "";
		public string dialogue = "";
		public string battle = "";

		public static Progress MakeInitialData(){
			Progress progress = new Progress();
			progress.worldMap = "Pintos1";
			progress.dialogue = "Scene#1";
			return progress;
		}
	}

	public class PartySaveData
	{
		public int partyLevel;
		public List<string> partyUnitNames;

		public static PartySaveData MakeInitialData()
		{
			PartySaveData newData = new PartySaveData();
			newData.partyLevel = 1;
			newData.partyUnitNames = new List<string> { "reina", "lucius" };
			return newData;
		}
	}

	public class SkillSaveData
	{
		public string skillName;
		public int level;

		public SkillSaveData()
		{
		}

		public SkillSaveData(string skillName, int level)
		{
			this.skillName = skillName;
			this.level = level;
		}
	}

	public class SaveDataCenter
	{
		private static SaveDataCenter instance = null;

		private static SaveDataCenter GetInstance()
		{
			if (instance == null)
			{
				instance = new SaveDataCenter();
				Load();
			}

			return instance;
		}

		private SaveData data;

		public static void Reset()
		{
			GetInstance().data = SaveData.MakeInitialData();
			Save();
		}

		public static SaveData GetSaveData()
		{
			return GetInstance().data;
		}

		public SaveDataCenter()
		{
		}

		public static void Save()
		{
			string filePath = Application.persistentDataPath + "/save.json";
			string jsonData = JsonMapper.ToJson(GetInstance().data);
			Debug.Log(jsonData);
			File.WriteAllText(filePath, jsonData, Encoding.UTF8);
		}

		public static void Load()
		{
			string filePath = Application.persistentDataPath + "/save.json";
			if (!File.Exists(filePath))
			{
				Debug.Log("Save is not exist, Make new save file at " + filePath);
				GetInstance().data = SaveData.MakeInitialData();
				return;
			}

			Debug.Log("Save is loaded from " + filePath);
			string jsonData = File.ReadAllText(filePath, Encoding.UTF8);
			GetInstance().data = JsonMapper.ToObject<SaveData>(jsonData);
		}
	}
}