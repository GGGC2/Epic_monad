using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Enums;
using GameData;

public class UnitInfo{
	public int index;
	public string nameKor;
	public string nameEng;
	public Side side;
	public Vector2 initPosition;
	public Direction initDirection;
	public UnitClass unitClass;
	public Element element;
	public Celestial celestial;
	public bool isObject;
	public Dictionary<Stat, int> baseStats = new Dictionary<Stat, int>();
	public Dictionary<Stat, int> InitStatChanges = new Dictionary<Stat, int>();

	public UnitInfo (string data){
		StringParser commaParser = new StringParser(data, '\t');
		try{
			index = commaParser.ConsumeInt(); 
			nameKor = commaParser.Consume();
			nameEng = commaParser.Consume();
			side = commaParser.ConsumeEnum<Side>();
			int x = commaParser.ConsumeInt();
			int y = commaParser.ConsumeInt();
			initPosition = new Vector2(x, y);
			initDirection = commaParser.ConsumeEnum<Direction>();
			baseStats.Add(Stat.MaxHealth, commaParser.ConsumeInt());
			baseStats.Add(Stat.Power, commaParser.ConsumeInt());
			baseStats.Add(Stat.Defense, commaParser.ConsumeInt());
			baseStats.Add(Stat.Resistance, commaParser.ConsumeInt());
			baseStats.Add(Stat.Agility, commaParser.ConsumeInt());
			unitClass = commaParser.ConsumeEnum<UnitClass>();
			if(SceneData.stageNumber < Setting.classOpenStage){
				unitClass = UnitClass.None;
			}
			element = commaParser.ConsumeEnum<Element>();
			if(SceneData.stageNumber < Setting.elementOpenStage){
				element = Element.None;
			}
			celestial = commaParser.ConsumeEnum<Celestial>();
			if(SceneData.stageNumber < Setting.elementOpenStage){
				celestial = Celestial.None;
			}
			isObject = commaParser.ConsumeBool();
			int StatChangeCount = commaParser.ConsumeInt();
			for(int i = 0; i < StatChangeCount; i++){
				InitStatChanges.Add(commaParser.ConsumeEnum<Stat>(), commaParser.ConsumeInt());
			}
		}catch{
			Debug.LogError("Cannot make UnitInfo instance : (data)" + data);
		}
	}

	public static int GetStat(string unitName, Stat type){
		TextAsset UnitDataMatrix = Resources.Load<TextAsset>("Data/PCStatData");
		TextAsset CoefTable = Resources.Load<TextAsset>("Data/StatCoefTable");
        int RelativePoint = 0;
        if((int)type <= 5)
		    RelativePoint = Int32.Parse(Parser.FindRowDataOf(UnitDataMatrix.text, unitName)[(int)type]);

		float acc = 0;
		if((int)type < 3)
			acc = float.Parse(Parser.ExtractFromMatrix(CoefTable.text, (int)type, RelativePoint+5));

		float coef = 0;
        if ((int)type <= 5)
            coef = float.Parse(Parser.ExtractFromMatrix(CoefTable.text, (int)type+2, RelativePoint+5));

		float basepoint = 0;
        if ((int)type <= 5)
            basepoint = float.Parse(Parser.ExtractFromMatrix(CoefTable.text, (int)type+7, RelativePoint+5));

		float level = PartyData.level;
		return Convert.ToInt32(acc*level*(level-1)+coef*level+basepoint);
	}

	public static UnitClass GetUnitClass(string Name){
		string className = Parser.FindRowDataOf(Resources.Load<TextAsset>("Data/PCStatData").text, Name)[6];
		if(className == "melee")
			return UnitClass.Melee;
		else if(className == "magic")
			return UnitClass.Magic;
		else
			return UnitClass.None;
	}

	public static Element GetElement(string Name){
		string element = Parser.FindRowDataOf(Resources.Load<TextAsset>("Data/PCStatData").text, Name)[7];
		if (element == "fire")
			return Element.Fire;
		else if (element == "water")
			return Element.Water;
		else if (element == "plant")
			return Element.Plant;
		else if (element == "metal")
			return Element.Metal;
		else
			return Element.None;
	}

	public static Celestial GetCelestial(string Name){
		string celestial = Parser.FindRowDataOf(Resources.Load<TextAsset>("Data/PCStatData").text, Name)[8];
		if (celestial == "sun")
			return Celestial.Sun;
		else if (celestial == "moon")
			return Celestial.Moon;
		else if (celestial == "earth")
			return Celestial.Earth;
		else
			return Celestial.None;
	}

	void SetCelestialImage(string unitName){
		string celestial = Parser.FindRowDataOf(Resources.Load<TextAsset>("Data/PCStatData").text, unitName)[8];
		
	}

	public static string ConvertToKoreanName(string codeName){
		if (codeName == "noel")
			return "노엘";
		else if (codeName == "sepia")
			return "세피아";
		else if (codeName == "arcadia")
			return "아르카디아";
		else if (codeName == "grenev")
			return "그레네브";
		else if (codeName == "darkenir")
			return "달케니르";
		else if (codeName == "yeong")
			return "영";
		else if (codeName == "bianca")
			return "비앙카";
		else if (codeName == "karldrich")
			return "칼드리치";
		else if (codeName == "lucius")
			return "루키어스";
        else if (codeName == "json")
            return "제이선";
        else if (codeName == "triana")
            return "트리아나";
		else if(codeName == "reina")
			return "레이나";
        else if(codeName == "unselected")
			return "Empty";
		else{
			Debug.LogError(codeName + " has NO KoreanName");
			return "";
		}
	}

	public static string ConvertToKoreanFullName(string codeName){
		if (codeName == "noel")
			return "노엘 글로우 포트란";
		else if (codeName == "sepia")
			return "세피아 로젠힐트";
		else if (codeName == "arcadia")
			return "아르카디아";
		else if (codeName == "grenev")
			return "그레네브 다리윌리안";
		else if (codeName == "darkenir")
			return "달케니르 쟌 시리우스";
		else if (codeName == "yeong")
			return "영";
		else if (codeName == "bianca")
			return "비앙카 프레이";
		else if (codeName == "karldrich")
			return "칼드리치 플레임";
		else if (codeName == "lucius")
			return "루키어스 에카르트 하스켈";
        else if (codeName == "json")
            return "제이선 어셈블리";
        else if (codeName == "triana")
            return "트리아나 페르단트";
		else if(codeName == "reina")
			return "레이나 닐 핀토스";
        else if(codeName == "unselected")
			return "Empty";
		else{
			Debug.LogError(codeName + " has NO KoreanName");
			return "";
		}
	}

	public void SetPCData(string PCName){
		nameEng = PCName;
		baseStats.Clear();
		baseStats.Add(Stat.MaxHealth, GetStat(PCName, Stat.MaxHealth));
		baseStats.Add(Stat.Power, GetStat(PCName, Stat.Power));
		baseStats.Add(Stat.Defense, GetStat(PCName, Stat.Defense));
		baseStats.Add(Stat.Resistance, GetStat(PCName, Stat.Resistance));
		baseStats.Add(Stat.Agility, GetStat(PCName, Stat.Agility));
		unitClass = UnitInfo.GetUnitClass (PCName);

		if (SceneData.stageNumber >= Setting.elementOpenStage)
			element = UnitInfo.GetElement (PCName);
		if (SceneData.stageNumber >= Setting.celestialOpenStage)
			celestial = UnitInfo.GetCelestial (PCName);
	}
}