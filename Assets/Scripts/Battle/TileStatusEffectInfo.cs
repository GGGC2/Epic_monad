﻿using System.Collections;
using System;
using UnityEngine;
using Enums;
using System.Collections.Generic;

public class TileStatusEffectInfo {

    string ownerOfSkill;
    int requireLevel;
    bool toBeReplaced;
    string originSkillName; // 효과를 유발하는 기술/특성의 이름
    string displayName; // 상태창에 표시되는 효과의 이름
    TileStatusEffect.FixedElement statusEffect;

    public string GetOwnerOfSkill() { return ownerOfSkill; }
    public int GetRequireLevel() { return requireLevel; }
    public bool GetToBeReplaced() { return toBeReplaced; }
    public string GetOriginSkillName() { return originSkillName; }
    public string GetDisplayName() { return displayName; }
    public TileStatusEffect.FixedElement GetStatusEffect() { return statusEffect; }

    public TileStatusEffectInfo(string data) {
        // Debug.Log(data);
        StringParser commaParser = new StringParser(data, ',');

        ownerOfSkill = commaParser.Consume();
        requireLevel = commaParser.ConsumeInt();
        toBeReplaced = commaParser.ConsumeBool();
        originSkillName = commaParser.Consume();
        displayName = commaParser.Consume();
        
        bool isInfinite = commaParser.ConsumeBool();
        bool isStackable = commaParser.ConsumeBool();
        bool isOnce = commaParser.ConsumeBool();
        int defaultPhase = commaParser.ConsumeInt();
        int maxStack = commaParser.ConsumeInt();
        bool amountToBeUpdated = commaParser.ConsumeBool();
        bool amountNotEffectedByStack = commaParser.ConsumeBool();
        bool isRemovable = commaParser.ConsumeBool();

        string effectName = commaParser.Consume();
        EffectVisualType effectVisualType = commaParser.ConsumeEnum<EffectVisualType>();
        EffectMoveType effectMoveType = commaParser.ConsumeEnum<EffectMoveType>();

        List<TileStatusEffect.FixedElement.ActualElement> actualElements = new List<TileStatusEffect.FixedElement.ActualElement>();

        int num = commaParser.ConsumeInt();

        for (int i = 0; i < num; i++) {
            StatusEffectType statusEffectType = commaParser.ConsumeEnum<StatusEffectType>(); ;

            StatusEffectVar statusEffectVar = commaParser.ConsumeEnum<StatusEffectVar>();
            float statusEffectCoef = commaParser.ConsumeFloat("X", 0);
            float statusEffectBase = commaParser.ConsumeFloat("X", 0);

            bool isPercent = commaParser.ConsumeBool("NONE", false);
            bool isMultiply = commaParser.ConsumeBool("NONE", false);

            TileStatusEffect.FixedElement.ActualElement actualElement =
                new TileStatusEffect.FixedElement.ActualElement(statusEffectType,
                                                            statusEffectVar,
                                                            statusEffectCoef,
                                                            statusEffectBase,
                                                            isPercent,
                                                            isMultiply);
            actualElements.Add(actualElement);
        }

        for (int i = num; i < 3; i++) {
            for (int j = 0; j < 6; j++)
                commaParser.Consume();
        }
        string explanation = commaParser.Consume();
        this.statusEffect = new TileStatusEffect.FixedElement(ownerOfSkill, toBeReplaced, originSkillName, displayName,
                                             isInfinite, isStackable, isOnce, defaultPhase, 
                                             maxStack, amountToBeUpdated, amountNotEffectedByStack, isRemovable, explanation,
                                             effectName, effectVisualType, effectMoveType,
                                             actualElements);
    }
}
