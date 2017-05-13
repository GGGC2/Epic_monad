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
}
