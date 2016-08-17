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
		CommaStringParser commaParser = new CommaStringParser(data);

		this.owner = commaParser.Consume();
		this.skillName = commaParser.Consume();

		string name = commaParser.Consume();
		StatusEffectType statusEffectType = commaParser.ConsumeEnum<StatusEffectType>();
		bool isBuff = commaParser.ConsumeBool();
        bool isInfinite = commaParser.ConsumeBool();
        bool isStackable = commaParser.ConsumeBool();
        bool isRemovable = commaParser.ConsumeBool();

        float degree = commaParser.ConsumeFloat();
        int amount = commaParser.ConsumeInt();
		int remainPhase = commaParser.ConsumeInt();
        int remainStack = commaParser.ConsumeInt();
		int cooldown = commaParser.ConsumeInt(); 

		string effectName = commaParser.Consume();
		EffectVisualType effectVisualType = commaParser.ConsumeEnum<EffectVisualType>();
		EffectMoveType effectMoveType = commaParser.ConsumeEnum<EffectMoveType>();
	
		this.statusEffect = new StatusEffect(name, statusEffectType, 
                                             isBuff, isInfinite, isStackable, isRemovable, 
                                             degree, amount, remainPhase, remainStack, cooldown, 
                                             effectName, effectVisualType, effectMoveType);
	}
}
