using System.Collections;
using System;
using UnityEngine;
using Enums;
using System.Collections.Generic;

public class TileStatusEffectInfo {

    string owner;
    int requireLevel;
    bool toBeReplaced;
    string originSkillName; // 효과를 유발하는 기술/특성의 이름
    string displayName; // 상태창에 표시되는 효과의 이름
    TileStatusEffect.FixedElement statusEffect;

    public string GetOwner() { return owner; }
    public int GetRequireLevel() { return requireLevel; }
    public bool GetToBeReplaced() { return toBeReplaced; }
    public string GetOriginSkillName() { return originSkillName; }
    public string GetDisplayName() { return displayName; }
    public TileStatusEffect.FixedElement GetStatusEffect() { return statusEffect; }

    public TileStatusEffectInfo(string data) {
        // Debug.Log(data);
        StringParser commaParser = new StringParser(data, ',');

        owner = commaParser.Consume();
        requireLevel = commaParser.ConsumeInt();
        toBeReplaced = commaParser.ConsumeBool();
        originSkillName = commaParser.Consume();
        displayName = commaParser.Consume();
        
        bool isInfinite = commaParser.ConsumeBool();
        bool isStackable = commaParser.ConsumeBool();
        bool isOnce = commaParser.ConsumeBool();
        int defaultPhase = commaParser.ConsumeInt();
        int maxStack = commaParser.ConsumeInt();
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

            bool isMultiply = commaParser.ConsumeBool("NONE", false);

            TileStatusEffect.FixedElement.ActualElement actualElement =
                new TileStatusEffect.FixedElement.ActualElement(statusEffectType,
                                                            statusEffectVar,
                                                            statusEffectCoef,
                                                            statusEffectBase,
                                                            isMultiply);
            actualElements.Add(actualElement);
        }

        this.statusEffect = new TileStatusEffect.FixedElement(toBeReplaced, originSkillName, displayName,
                                             isInfinite, isStackable, isOnce,
                                             defaultPhase, maxStack, isRemovable,
                                             effectName, effectVisualType, effectMoveType,
                                             actualElements);
    }
}
