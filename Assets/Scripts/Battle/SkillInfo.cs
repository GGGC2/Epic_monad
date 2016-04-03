﻿using System.Collections;
using System;
using Enums;

public class SkillInfo {

	public string owner;
	public int requireLevel;
	public Skill skill;
	
	public string GetOwner()
	{
		return owner;
	}
	
	public int GetRequireLevel()
	{
		return requireLevel;
	}
	
	public Skill GetSkill()
	{
		return skill;
	}
	
	public SkillInfo (string data)
	{
		string[] stringList = data.Split(',');

		this.owner = stringList[0];
		this.requireLevel = Int32.Parse(stringList[1]);
  
		string name = stringList[2];
		int requireAP = Int32.Parse(stringList[3]);
		int cooldown = Int32.Parse(stringList[4]); 

		float powerFactor = Single.Parse(stringList[5]);
		
		SkillType skillType = (SkillType)Enum.Parse(typeof(SkillType), stringList[6]);

		RangeForm firstRangeForm = (RangeForm)Enum.Parse(typeof(RangeForm), stringList[7]);
		int firstMinReach = Int32.Parse(stringList[8]); 
		int firstMaxReach = Int32.Parse(stringList[9]); 
		int firstWidth = Int32.Parse(stringList[10]); 

		bool includeMyself = Boolean.Parse(stringList[11]); 

		RangeForm secondRangeForm = (RangeForm)Enum.Parse(typeof(RangeForm), stringList[12]);
		int secondMinReach = Int32.Parse(stringList[13]); 
		int secondMaxReach = Int32.Parse(stringList[14]); 
		int secondWidth = Int32.Parse(stringList[15]); 

		SkillApplyType skillApplyType = (SkillApplyType)Enum.Parse(typeof(SkillApplyType), stringList[16]);

		string effectName = stringList[17];
		EffectVisualType effectVisualType = (EffectVisualType)Enum.Parse(typeof(EffectVisualType), stringList[18]);
		EffectMoveType effectMoveType = (EffectMoveType)Enum.Parse(typeof(EffectMoveType), stringList[19]);
	
		this.skill = new Skill(name, requireAP, cooldown, 
							   powerFactor,
							   skillType,
							   firstRangeForm, firstMinReach, firstMaxReach, firstWidth,
							   includeMyself,
							   secondRangeForm, secondMinReach, secondMaxReach, secondWidth,
							   skillApplyType,
							   effectName, effectVisualType, effectMoveType);
	}
}
