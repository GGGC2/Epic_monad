using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Enums;

public class UnitInfo {
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
	
	public enum StatType{
		Health = 1,
		Power = 2,
		Defense = 3,
		Resist = 4,
		Agility = 5,
	}

	public UnitInfo (string data)
	{
		CommaStringParser commaParser = new CommaStringParser(data);
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

	public static int GetStat(string unitName, StatType type){
		TextAsset UnitDataMatrix = Resources.Load<TextAsset>("Data/UnitDataPC");
		TextAsset CoefTable = Resources.Load<TextAsset>("Data/StatCoefTable");

		int RelativePoint = Int32.Parse(Parser.FindRowDataOf(UnitDataMatrix.text, unitName)[(int)type]);

		float acc = 0;
		if((int)type >= 3)
			acc = float.Parse(Parser.ExtractFromMatrix(CoefTable.text, (int)type, RelativePoint+5));

		float coef = 0;
		coef = float.Parse(Parser.ExtractFromMatrix(CoefTable.text, (int)type+2, RelativePoint+5));

		float basepoint = 0;
		basepoint = float.Parse(Parser.ExtractFromMatrix(CoefTable.text, (int)type+7, RelativePoint+5));

		float level = Save.SaveDataCenter.GetSaveData().party.partyLevel;
		return Convert.ToInt32(acc*level*(level-1)+coef*level+basepoint);
	}

	public static Enums.UnitClass GetUnitClass(string PCName){
		string className = Parser.FindRowDataOf(Resources.Load<TextAsset>("Data/UnitDataPC").text, PCName)[6];
		if(className == "melee")
			return Enums.UnitClass.Melee;
		else if(className == "magic")
			return Enums.UnitClass.Magic;
		else
			return Enums.UnitClass.None;
	}

	public static Enums.Element GetElement(string PCName){
		string element = Parser.FindRowDataOf(Resources.Load<TextAsset>("Data/UnitDataPC").text, PCName)[7];
		if (element == "fire")
			return Enums.Element.Fire;
		else if (element == "water")
			return Enums.Element.Water;
		else if (element == "plant")
			return Enums.Element.Plant;
		else if (element == "metal")
			return Enums.Element.Metal;
		else
			return Enums.Element.None;
	}

	public static Enums.Celestial GetCelestial(string PCName){
		string celestial = Parser.FindRowDataOf(Resources.Load<TextAsset>("Data/UnitDataPC").text, PCName)[8];
		if (celestial == "sun")
			return Enums.Celestial.Sun;
		else if (celestial == "moon")
			return Enums.Celestial.Moon;
		else if (celestial == "earth")
			return Enums.Celestial.Earth;
		else
			return Enums.Celestial.None;
	}

	void SetCelestialImage(string unitName){
		string celestial = Parser.FindRowDataOf(Resources.Load<TextAsset>("Data/UnitDataPC").text, unitName)[8];
		
	}

	public static string ConvertToKoreanName(string codeName){
		if(codeName == "noel")
			return "노엘";
		else if(codeName == "sepia")
			return "세피아";
		else if(codeName == "arcadia")
			return "아르카디아";
		else if(codeName == "grenev")
			return "그레네브";
		else if(codeName == "darkenir")
			return "달케니르";
		else if(codeName == "")
			return "Empty";
		else
			Debug.LogError(codeName + " has NO KoreanName");
			return "";
	}
}