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
	
	public UnitInfo (string data)
	{
		string[] stringList = data.Split(',');

		this.name = stringList[0];
		this.nameInCode = stringList[1];
		this.side = (Side)Enum.Parse(typeof(Side), stringList[2]);
		this.initPosition = new Vector2(Int32.Parse(stringList[3]), Int32.Parse(stringList[4]));
		this.initDirection = (Direction)Enum.Parse(typeof(Direction), stringList[5]);
		this.baseHealth = Int32.Parse(stringList[6]);
		this.basePower = Int32.Parse(stringList[7]);
		this.baseDefense = Int32.Parse(stringList[8]);
		this.baseResistance = Int32.Parse(stringList[9]);
		this.baseDexturity = Int32.Parse(stringList[10]);
		this.unitClass = (UnitClass)Enum.Parse(typeof(UnitClass), stringList[11]);
		this.element = (Element)Enum.Parse(typeof(Element), stringList[12]);
		this.celestial = (Celestial)Enum.Parse(typeof(Celestial), stringList[13]);
	}
}
