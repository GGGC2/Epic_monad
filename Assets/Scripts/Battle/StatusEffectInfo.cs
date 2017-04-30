using System.Collections;
using System;
using UnityEngine;
using Enums;
using System.Collections.Generic;

public class StatusEffectInfo {

	public string owner;
	public string originSkillName; // 효과를 유발하는 기술/특성의 이름
	public string displayName; // 상태창에 표시되는 효과의 이름
	public StatusEffect.FixedElement statusEffect;
	
	public string GetOwner()
	{
		return owner;
	}
	
	public string GetOriginSkillName()
	{
		return originSkillName;
	}

	public string GetDisplayName()
	{
		return displayName;
	}

	public StatusEffect.FixedElement GetStatusEffect()
	{
		return statusEffect;
	}
	
	public StatusEffectInfo(string data)
	{
		Debug.Log(data);
		CommaStringParser commaParser = new CommaStringParser(data);

		this.owner = commaParser.Consume();
		this.originSkillName = commaParser.Consume();
		this.displayName = commaParser.Consume();

		bool isHidden = commaParser.ConsumeBool();
		bool isBuff = commaParser.ConsumeBool();
        bool isInfinite = commaParser.ConsumeBool();
        bool isStackable = commaParser.ConsumeBool();
		bool isOnce = commaParser.ConsumeBool();
		int maxPhase = commaParser.ConsumeInt();
		int maxStack = commaParser.ConsumeInt();
        bool isRemovable = commaParser.ConsumeBool();

		string effectName = commaParser.Consume();
		EffectVisualType effectVisualType = commaParser.ConsumeEnum<EffectVisualType>();
		EffectMoveType effectMoveType = commaParser.ConsumeEnum<EffectMoveType>();
		
		List<StatusEffect.FixedElement.ActualElement> actualElements = new List<StatusEffect.FixedElement.ActualElement>();

		int num = commaParser.ConsumeInt();

		for (int i = 0; i < num; i++)
		{
			StatusEffectType statusEffectType = commaParser.ConsumeEnum<StatusEffectType>();;
			Stat applyStat = applyStat = commaParser.ConsumeEnum<Stat>();;
			bool isRelative = isRelative = commaParser.ConsumeBool();
			
			StatusEffect.FixedElement.ActualElement actualElement = new StatusEffect.FixedElement.ActualElement(statusEffectType, applyStat, isRelative);
			actualElements.Add(actualElement);
		}
	
		this.statusEffect = new StatusEffect.FixedElement(originSkillName, displayName,
                                             isHidden, isBuff, isInfinite, 
											 isStackable, isOnce,
											 maxPhase, maxStack, isRemovable, 
											 effectName, effectVisualType, effectMoveType,
											 actualElements);
	}
}
