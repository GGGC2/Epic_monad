using Enums;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class PassiveSkill {

	// base info.
	string owner;
	int column;
	string name;
	
	public PassiveSkill(string owner, int column, string name)
	{
		this.owner = owner;
		this.column = column;
		this.name = name;
	}

	public string GetOwner(){return owner;}
	public int GetColumn() { return column; }
	public string GetName() {return name;}
}
