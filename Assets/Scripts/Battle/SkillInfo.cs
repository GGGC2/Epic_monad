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
        int[] requireAPArray = new int[5];
        string[] requireAPString = commaParser.Consume().Split(' ');
        for(int i = 0; i < 5; i++)
        {
            requireAPArray[i] = Int32.Parse(requireAPString[i]);
        }
		int cooldown = commaParser.ConsumeInt();

        // parsing coefficients for damage calculation
        string statType = commaParser.Consume();
        float[] powerFactorArray = new float[5];
        string[] powerFactorString = commaParser.Consume().Split(' ');
        for(int i = 0; i < 5; i++)
        {
            powerFactorArray[i] = Convert.ToSingle(powerFactorString[i]);
        }
        
	    Dictionary<string, float[]> powerFactor = new Dictionary<string, float[]>();
        powerFactor.Add(statType, powerFactorArray);
		
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

		this.skill = new Skill(name, requireAPArray, cooldown, 
							   powerFactor,
							   skillType,
							   firstRangeForm, firstMinReach, firstMaxReach, firstWidth,
							   secondRangeForm, secondMinReach, secondMaxReach, secondWidth,
							   skillApplyType,
							   effectName, effectVisualType, effectMoveType);
	}
}
