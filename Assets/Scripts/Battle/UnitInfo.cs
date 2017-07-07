using UnityEngine;
using System.Collections;
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
	public int baseDexturity;
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
		this.baseDexturity = commaParser.ConsumeInt();
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
}