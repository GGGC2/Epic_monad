using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Enums;
using GameData;

public class UnitInfo {
	public int index;
	public string name;
	public string nameInCode;
	public Side side;
	public Vector2 initPosition;
	public Direction initDirection;
	public int baseHealth;
	public int basePower;
	public int baseDefense;
	public int baseResistance;
	public int baseAgility;
	public UnitClass unitClass;
	public Element element;
	public Celestial celestial;
	public bool isObject;

	public UnitInfo (string data){
		StringParser commaParser = new StringParser(data, ',');
		this.index = commaParser.ConsumeInt(); 
		this.name = commaParser.Consume();
		this.nameInCode = commaParser.Consume();
		this.side = commaParser.ConsumeEnum<Side>();
		int x = commaParser.ConsumeInt();
		int y = commaParser.ConsumeInt();
		this.initPosition = new Vector2(x, y);
		this.initDirection = commaParser.ConsumeEnum<Direction>();
		this.baseHealth = commaParser.ConsumeInt();
		this.basePower = commaParser.ConsumeInt();
		this.baseDefense = commaParser.ConsumeInt();
		this.baseResistance = commaParser.ConsumeInt();
		this.baseAgility = commaParser.ConsumeInt();
		this.unitClass = commaParser.ConsumeEnum<UnitClass>();
		this.element = commaParser.ConsumeEnum<Element>();
		this.celestial = commaParser.ConsumeEnum<Celestial>();
		this.isObject = commaParser.ConsumeBool();
	}

	public static int GetStat(string unitName, Stat type){
		TextAsset UnitDataMatrix = Resources.Load<TextAsset>("Data/UnitDataPC");
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
		string className = Parser.FindRowDataOf(Resources.Load<TextAsset>("Data/UnitDataPC").text, Name)[6];
		if(className == "melee")
			return UnitClass.Melee;
		else if(className == "magic")
			return UnitClass.Magic;
		else
			return UnitClass.None;
	}

	public static Element GetElement(string Name){
		string element = Parser.FindRowDataOf(Resources.Load<TextAsset>("Data/UnitDataPC").text, Name)[7];
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
		string celestial = Parser.FindRowDataOf(Resources.Load<TextAsset>("Data/UnitDataPC").text, Name)[8];
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
		string celestial = Parser.FindRowDataOf(Resources.Load<TextAsset>("Data/UnitDataPC").text, unitName)[8];
		
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
		else if(codeName == "unselected")
			return "Empty";
		else{
			Debug.LogError(codeName + " has NO KoreanName");
			return "";
		}
	}

	public void SetPCData(string PCName){
		nameInCode = PCName;
		baseHealth = UnitInfo.GetStat (PCName, Stat.MaxHealth);
		basePower = UnitInfo.GetStat (PCName, Stat.Power);
		baseDefense = UnitInfo.GetStat (PCName, Stat.Defense);
		baseResistance = UnitInfo.GetStat (PCName, Stat.Resistance);
		baseAgility = UnitInfo.GetStat (PCName, Stat.Agility);
		unitClass = UnitInfo.GetUnitClass (PCName);

		if (SceneData.stageNumber >= Setting.elementOpenStage)
			element = UnitInfo.GetElement (PCName);
		if (SceneData.stageNumber >= Setting.celestialOpenStage)
			celestial = UnitInfo.GetCelestial (PCName);
	}
}