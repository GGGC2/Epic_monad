using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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
		CommaStringParser commaParser = new CommaStringParser(data);

		this.owner = commaParser.Consume();
        this.requireLevel = commaParser.ConsumeInt();
  
		string name = commaParser.Consume();
        int requireAP = commaParser.ConsumeInt();
		int cooldown = commaParser.ConsumeInt();

        // parsing coefficients for damage calculation
        string powerFactorString = commaParser.Consume();
	    Dictionary<string, float> powerFactor = new Dictionary<string, float>();
        string[] powerFactorList = powerFactorString.Split(' ');
        foreach (string powerFactorPair in powerFactorList)
        {
            string[] parsedPowerFactorPair = powerFactorPair.Split('=');
            powerFactor.Add(parsedPowerFactorPair[0], Convert.ToSingle(parsedPowerFactorPair[1]));                
        }
		
		SkillType skillType = commaParser.ConsumeEnum<SkillType>();

		RangeForm firstRangeForm = commaParser.ConsumeEnum<RangeForm>();
		int firstMinReach = commaParser.ConsumeInt();
		int firstMaxReach = commaParser.ConsumeInt();
		int firstWidth = commaParser.ConsumeInt();
		
		RangeForm secondRangeForm = commaParser.ConsumeEnum<RangeForm>();
		int secondMinReach = commaParser.ConsumeInt();
		int secondMaxReach = commaParser.ConsumeInt();
		int secondWidth = commaParser.ConsumeInt();

		SkillApplyType skillApplyType = commaParser.ConsumeEnum<SkillApplyType>();

		string effectName = commaParser.Consume();
		EffectVisualType effectVisualType = commaParser.ConsumeEnum<EffectVisualType>();
		EffectMoveType effectMoveType = commaParser.ConsumeEnum<EffectMoveType>();

		this.skill = new Skill(name, requireAP, cooldown, 
							   powerFactor,
							   skillType,
							   firstRangeForm, firstMinReach, firstMaxReach, firstWidth,
							   secondRangeForm, secondMinReach, secondMaxReach, secondWidth,
							   skillApplyType,
							   effectName, effectVisualType, effectMoveType);
	}
}
