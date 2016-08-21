using System.Collections;
using System;
using UnityEngine;
using Enums;

public class StatusEffectInfo {

	public string owner;
	public string skillName;
	public StatusEffect statusEffect;
	
	public string GetOwner()
	{
		return owner;
	}
	
	public string GetSkillName()
	{
		return skillName;
	}
	
	public StatusEffect GetStatusEffect()
	{
		return statusEffect;
	}
	
	public StatusEffectInfo(string data)
	{
		Debug.Log(data);
		CommaStringParser commaParser = new CommaStringParser(data);

		this.owner = commaParser.Consume();
		this.skillName = commaParser.Consume();

		string name = commaParser.Consume();
		StatusEffectType statusEffectType = commaParser.ConsumeEnum<StatusEffectType>();
		bool isBuff = commaParser.ConsumeBool();
        bool isInfinite = commaParser.ConsumeBool();
        bool isStackable = commaParser.ConsumeBool();
        bool isRemovable = commaParser.ConsumeBool();

        string originDegreeString = commaParser.Consume();
		string[] degreeStringArray = originDegreeString.Split(' ');
		float[] degreeArray = new float[5];
		for(int i = 0; i < 5; i++)
		{
			float parsed = 0.0f;
			try
			{
				parsed = Convert.ToSingle(degreeStringArray[i]);			
			}
			catch
			{
				Debug.LogWarning("Parse error in degrees : " + originDegreeString);
				parsed = -1;				
			}
			degreeArray[i] = parsed;
		}
		Stat amountStat = commaParser.ConsumeEnum<Stat>();
		string originAmountFactorString = commaParser.Consume();
		string[] amountFactorStringArray = originAmountFactorString.Split(' ');
		float[] amountFactorArray = new float[5];
		for(int i = 0; i < 5; i++)
		{
			float parsed = 0;
			try
			{
				parsed = Convert.ToSingle(amountFactorStringArray[i]);			
			}
			catch
			{
				Debug.LogWarning("Parse error in amountFactors : " + originAmountFactorString);
				parsed = -1;				
			}
			amountFactorArray[i] = parsed;
		}
        int remainAmount = 0; // 남은 보호막 체크용
		int remainPhase = commaParser.ConsumeInt();
        int remainStack = commaParser.ConsumeInt();
		int cooldown = commaParser.ConsumeInt(); 
		bool toBeRemoved = false;

		string effectName = commaParser.Consume();
		EffectVisualType effectVisualType = commaParser.ConsumeEnum<EffectVisualType>();
		EffectMoveType effectMoveType = commaParser.ConsumeEnum<EffectMoveType>();
	
		this.statusEffect = new StatusEffect(name, statusEffectType, 
                                             isBuff, isInfinite, isStackable, isRemovable, 
                                             degreeArray, amountStat, amountFactorArray, remainAmount, 
											 remainPhase, remainStack, cooldown, toBeRemoved, 
                                             effectName, effectVisualType, effectMoveType);
	}
}
