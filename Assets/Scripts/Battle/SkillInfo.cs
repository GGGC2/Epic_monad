using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Enums;

public class SkillInfo {

	public string owner;
	public int requireLevel;
	public Skill skill;
	public int column;
	
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
		this.column = commaParser.ConsumeInt();
		string originRequireAPString = commaParser.Consume();
        string[] requireAPStringGroup = originRequireAPString.Split(' ');
		int[] requireAPArray = new int[5];

		for(int i = 0; i < 5; i++)
		{
			int parsed = 0;
			try
			{
				parsed = Int32.Parse(requireAPStringGroup[i]);				
			}
			catch
			{
				Debug.LogWarning("Parse error in requireAPs : " + originRequireAPString);
				parsed = -1;				
			}
			requireAPArray[i] = parsed;
		}
		string originCooldownString = commaParser.Consume();
		string[] cooldownStringGroup = originCooldownString.Split(' ');
		int[] cooldownArray = new int[5];
		for(int i = 0; i < 5; i++)
		{
			int parsed = 0;
			try
			{
				parsed = Int32.Parse(cooldownStringGroup[i]);				
			}
			catch
			{
				Debug.LogWarning("Parse error in requireAPs : " + originCooldownString);
				parsed = -1;				
			}
			cooldownArray[i] = parsed;
		}

        // parsing coefficients for damage calculation
        string originStatTypeString = commaParser.Consume();
		string[] statTypeArray = originStatTypeString.Split('/');
		string originPowerFactorString = commaParser.Consume();
        string[] powerFactorStringGroup = originPowerFactorString.Split('/');
		float[][] powerFactorArrayGroup = new float[powerFactorStringGroup.Length][];

        for(int i = 0; i < powerFactorStringGroup.Length; i++)
		{
			string[] powerFactorString = powerFactorStringGroup[i].Split(' ');
			for(int j = 0; j < 5; j++)
			{				
				int parsed = 0;
				try
				{
					parsed = Int32.Parse(powerFactorString[j]);
				}
				catch
				{
					Debug.LogWarning("Parse error in powerFactors : " + originRequireAPString);
					parsed = -1;
				}
				powerFactorArrayGroup[i][j] = parsed;
			}
		}
        
	    Dictionary<string, float[]> powerFactor = new Dictionary<string, float[]>();
		for(int i = 0; i < statTypeArray.Length; i++)
		{
			powerFactor.Add(statTypeArray[i], powerFactorArrayGroup[i]);
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
		string originPenetrationString = commaParser.Consume();
		string[] penetrationStringGroup = originPenetrationString.Split(' ');
		float[] penetrationArray = new float[5];
		for(int i = 0; i < 5; i++)
		{
			int parsed = 0;
			try
			{
				parsed = Int32.Parse(penetrationStringGroup[i]);				
			}
			catch
			{
				Debug.LogWarning("Parse error in penetrations : " + originPenetrationString);
				parsed = -1;				
			}
			penetrationArray[i] = parsed;
		}

		string effectName = commaParser.Consume();
		EffectVisualType effectVisualType = commaParser.ConsumeEnum<EffectVisualType>();
		EffectMoveType effectMoveType = commaParser.ConsumeEnum<EffectMoveType>();

		this.skill = new Skill(name, requireAPArray, cooldownArray, 
							   powerFactor,
							   skillType,
							   firstRangeForm, firstMinReach, firstMaxReach, firstWidth,
							   secondRangeForm, secondMinReach, secondMaxReach, secondWidth,
							   skillApplyType, penetrationArray,
							   effectName, effectVisualType, effectMoveType);
	}
}
