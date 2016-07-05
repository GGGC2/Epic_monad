using System.Collections;
using System;
using UnityEngine;
using Enums;

public class StatusEffectInfo {

	public string owner;
	public Skill skill;
	public StatusEffect statusEffect;
	
	public string GetOwner()
	{
		return owner;
	}
	
	public Skill GetSkill()
	{
		return skill;
	}
	
	public StatusEffect GetStatusEffect()
	{
		return statusEffect;
	}
	
	public StatusEffectInfo(string data)
	{
		string[] stringList = data.Split(',');

		this.owner = stringList[0];
		this.skill = (Skill)Enum.Parse(typeof(Skill), stringList[1]);
  
		string name = stringList[2];
		StatusEffectType statusEffectType = (StatusEffectType)Enum.Parse(typeof(StatusEffectType), stringList[3]);
        bool isInfinite = bool.Parse(stringList[4]);
        bool isStackable = bool.Parse(stringList[5]);
        bool isRemovable = bool.Parse(stringList[6]);
        float degree = Single.Parse(stringList[7]);
        int amount = Int32.Parse(stringList[8]);
        int remainPhase = Int32.Parse(stringList[9]);
        int remainStack = Int32.Parse(stringList[10]);
		int cooldown = Int32.Parse(stringList[11]); 

		string effectName = stringList[12];
		EffectVisualType effectVisualType = (EffectVisualType)Enum.Parse(typeof(EffectVisualType), stringList[13]);
		EffectMoveType effectMoveType = (EffectMoveType)Enum.Parse(typeof(EffectMoveType), stringList[14]);
	
		this.statusEffect = new StatusEffect(name, statusEffectType, 
                                             isInfinite, isStackable, isRemovable, 
                                             degree, amount, remainPhase, remainStack, cooldown, 
                                             effectName, effectVisualType, effectMoveType);
	}
}