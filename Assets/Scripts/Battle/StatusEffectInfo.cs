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
		// Debug.Log(data);
		CommaStringParser commaParser = new CommaStringParser(data);

		this.owner = commaParser.Consume();
		this.originSkillName = commaParser.Consume();
		this.displayName = commaParser.Consume();

		bool isBuff = commaParser.ConsumeBool();
        bool isInfinite = commaParser.ConsumeBool();
        bool isStackable = commaParser.ConsumeBool();
		bool isOnce = commaParser.ConsumeBool();
		int defaultPhase = commaParser.ConsumeInt();
		StatusEffectVar stackVar = commaParser.ConsumeEnum<StatusEffectVar>();
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

			StatusEffectVar statusEffectVar = commaParser.ConsumeEnum<StatusEffectVar>();
			float statusEffectCoef = commaParser.ConsumeFloat("X", 0);
			float statusEffectBase = commaParser.ConsumeFloat("X", 0);

			bool isMultifly = commaParser.ConsumeBool("NONE", false);

			StatusEffect.FixedElement.ActualElement actualElement = 
				new StatusEffect.FixedElement.ActualElement(statusEffectType, 
															statusEffectVar,
															statusEffectCoef,
															statusEffectBase, 
															isMultifly);
			actualElements.Add(actualElement);
		}

		this.statusEffect = new StatusEffect.FixedElement(originSkillName, displayName,
                                             isBuff, isInfinite, 
											 isStackable, isOnce,
											 defaultPhase, stackVar, maxStack, isRemovable, 
											 effectName, effectVisualType, effectMoveType,
											 actualElements);
	}
}
