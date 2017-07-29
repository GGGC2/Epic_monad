using LitJson;
using System.Text;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using GameData;

public class GameDataManager {
    static string filePath = Application.persistentDataPath + "/save.txt";
    
    public static void Reset() {
        PartyData.level = 1;
        PartyData.exp = 0;
        PartyData.SetReqExp();
        SceneData.dialogueName = "Scene#1-1";
        SceneData.stageNumber = 1;
        Save();
    }
    private static string ConvertGameDataToString() {
        string result = "";
        result += PartyData.level + ",";
        result += PartyData.exp + ",";
        result += PartyData.reqExp + ",";
        result += SceneData.dialogueName + ",";
        result += SceneData.stageNumber;
        return result;
    }
    private static void ConvertStringToGameData(string str) {
        CommaStringParser commastringParser = new CommaStringParser(str);
        PartyData.level = commastringParser.ConsumeInt();
        PartyData.exp = commastringParser.ConsumeInt();
        PartyData.reqExp = commastringParser.ConsumeInt();
        SceneData.dialogueName = commastringParser.Consume();
        SceneData.stageNumber = commastringParser.ConsumeInt();
    }
    public static void Save() {
        string data = ConvertGameDataToString();
        Debug.Log("Saved " + data + " to " + filePath);
        File.WriteAllText(filePath, data, Encoding.UTF8);
    }

    public static void Load() {
        if (!File.Exists(filePath)) {
            Debug.Log("Save is not exist, Make new save file at " + filePath);
            Reset();
            return;
        }

        Debug.Log("Save is loaded from " + filePath);
        string data = File.ReadAllText(filePath, Encoding.UTF8);
        ConvertStringToGameData(data);
    }

}
