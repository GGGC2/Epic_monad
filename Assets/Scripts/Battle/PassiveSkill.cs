using Enums;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public class PassiveSkill {

	// base info.
	string owner;
	int column;
	string name;
	List<StatusEffect> statusEffectList = new List<StatusEffect>();
	
	public PassiveSkill(string owner, int column, string name)
	{
		this.owner = owner;
		this.column = column;
		this.name = name;
	}

	public void ApplyStatusEffectList(List<StatusEffectInfo> statusEffectInfoList)
	{
		var statusEffectList = statusEffectInfoList
			.Where(statusEffectInfo => statusEffectInfo.GetSkillName().Equals(this.name))
			.Select(statusEffectInfo => statusEffectInfo.GetStatusEffect())
			.ToList();

		this.statusEffectList = statusEffectList;
	}

	public string GetOwner(){return owner;}
	public int GetColumn() { return column; }
	public string GetName() {return name;}
    public List<StatusEffect> GetStatusEffectList() {return statusEffectList;}
}
