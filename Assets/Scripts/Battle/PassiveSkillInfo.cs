using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Enums;

public class PassiveSkillInfo {

	public string owner;
	public int requireLevel;
	public int column;
	public string name;
	
	public string GetOwner()
	{
		return owner;
	}
	
	public int GetRequireLevel()
	{
		return requireLevel;
	}
	
	public PassiveSkillInfo (string data)
	{
		CommaStringParser commaParser = new CommaStringParser(data);

		this.owner = commaParser.Consume();
        this.requireLevel = commaParser.ConsumeInt();
  
		this.name = commaParser.Consume();
		this.column = commaParser.ConsumeInt();
	}

	/*public PassiveSkill GetSkill() {
		return new PassiveSkill(owner, column, name, requireLevel);
	}*/
}
