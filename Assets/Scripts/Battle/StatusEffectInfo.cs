using System.Collections;
using System;
using UnityEngine;
using Enums;
using System.Collections.Generic;

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

		bool isHidden = commaParser.ConsumeBool();
		bool isBuff = commaParser.ConsumeBool();
        bool isInfinite = commaParser.ConsumeBool();
        bool isStackable = commaParser.ConsumeBool();
		int maxStack = commaParser.ConsumeInt();
        bool isRemovable = commaParser.ConsumeBool();

		string effectName = commaParser.Consume();
		EffectVisualType effectVisualType = commaParser.ConsumeEnum<EffectVisualType>();
		EffectMoveType effectMoveType = commaParser.ConsumeEnum<EffectMoveType>();
		
		List<StatusEffect.ActualElement> actualElements = new List<StatusEffect.ActualElement>();

		int num = commaParser.ConsumeInt();

		for (int i = 0; i < num; i++)
		{
			StatusEffectType statusEffectType = commaParser.ConsumeEnum<StatusEffectType>();;
			Stat applyStat = applyStat = commaParser.ConsumeEnum<Stat>();;
			bool isRelative = isRelative = commaParser.ConsumeBool();
			
			StatusEffect.ActualElement actualElement = new StatusEffect.ActualElement(statusEffectType, applyStat, isRelative);
			actualElements.Add(actualElement);
		}
	
		this.statusEffect = new StatusEffect(skillName, 
                                             isHidden, isBuff, isInfinite, 
											 isStackable, maxStack, isRemovable, 
											 effectName, effectVisualType, effectMoveType,
											 actualElements);
	}
}
