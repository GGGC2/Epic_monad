﻿using Enums;
using UnityEngine;
using System.Collections.Generic;

public class UnitStatusEffect : StatusEffect {
    public UnitStatusEffect(FixedElement fixedElem, Unit caster, Unit owner, Skill originSkill) : base(fixedElem, caster, originSkill) {
        this.fixedElem = fixedElem;
        this.flexibleElem = new FlexibleElement(this, caster, owner, originSkill);
        ((FlexibleElement.DisplayElement)flexibleElem.display).owner = owner;
        for (int i = 0; i < fixedElem.actuals.Count; i++) {
            CalculateAmount(i, false);
            flexibleElem.actuals[i].remainAmount = GetAmount(i);
        }
    }
    public new class FixedElement : StatusEffect.FixedElement {
        public FixedElement(string ownerOfSkill, bool toBeReplaced, string originSkillName, string displayName,
                  bool isBuff, bool isInfinite, bool isStackable, bool isOnce,
                  int defaultPhase, int maxStack, bool amountToBeUpdated, bool amountNotEffectedByStack, bool isRemovable,
                  string explanation, string effectName, EffectVisualType effectVisualType, EffectMoveType effectMoveType, List<ActualElement> actualEffects)
                  : base(ownerOfSkill, toBeReplaced, originSkillName, displayName, isInfinite, isStackable, isOnce, defaultPhase, maxStack, amountToBeUpdated,
                    amountNotEffectedByStack, isRemovable, explanation, effectName, effectVisualType, effectMoveType, actualEffects) {
            display = new DisplayElement(ownerOfSkill, toBeReplaced, originSkillName, displayName,
                    isBuff, isInfinite, isStackable, isOnce,
                    defaultPhase, maxStack, amountToBeUpdated, amountNotEffectedByStack, isRemovable,
                    explanation, effectName, effectVisualType, effectMoveType);
        }
        public new class DisplayElement : StatusEffect.FixedElement.DisplayElement {
            public readonly bool isBuff;

            public DisplayElement(string ownerOfSkill, bool toBeReplaced, string originSkillName, string displayName,
                  bool isBuff, bool isInfinite, bool isStackable, bool isOnce,
                  int defaultPhase, int maxStack, bool amountToBeUpdated, bool amountNotEffectedByStack, bool isRemovable,
                  string explanation, string effectName, EffectVisualType effectVisualType, EffectMoveType effectMoveType) 
                  : base(ownerOfSkill, toBeReplaced, originSkillName, displayName, isInfinite, isStackable, isOnce, defaultPhase,
                    maxStack, amountToBeUpdated, amountNotEffectedByStack, isRemovable, explanation, effectName, effectVisualType, effectMoveType){
                this.isBuff = isBuff;
            }
        }
    }
    public new class FlexibleElement : StatusEffect.FlexibleElement {
        public bool ownerHadTurnSinceAttached = false;  // '조작을 제한하는 상태이상은, 효과가 생긴 페이즈에 한 번도 턴이 되지 않았다면 지속을 차감하지 않는다.
        public new class DisplayElement : StatusEffect.FlexibleElement.DisplayElement {
            public Unit owner;

            public DisplayElement(Unit caster, Unit owner, Skill originSkill, int maxStack, int defaultPhase)
                    : base(caster, originSkill, maxStack, defaultPhase) {
                this.owner = owner;
            }
        }
        public FlexibleElement(UnitStatusEffect statusEffect, Unit caster, Unit owner, Skill originSkill)
                : base(statusEffect, caster, originSkill) {
            FixedElement fixedElem = (UnitStatusEffect.FixedElement)statusEffect.fixedElem;
            int maxStack = fixedElem.display.maxStack;
            int defaultPhase = fixedElem.display.defaultPhase;
            display = new DisplayElement(caster, owner, originSkill, maxStack, defaultPhase);

            this.actuals = new List<ActualElement>();
            for (int i = 0; i < fixedElem.actuals.Count; i++) {
                actuals.Add(new ActualElement(0));
            }
        }
    }
    public bool GetOwnerHadTurnSinceAttached() { return ((UnitStatusEffect.FlexibleElement)flexibleElem).ownerHadTurnSinceAttached; }
    public bool GetIsBuff() { return ((FixedElement.DisplayElement)fixedElem.display).isBuff; }
    public Unit GetOwner() { return ((UnitStatusEffect.FlexibleElement.DisplayElement)flexibleElem.display).owner; }
    protected override float GetStatusEffectVar(int i) {
        float statusEffectVar = 0;
        Skill originSkill = GetOriginSkill();
        if (originSkill != null) {
            if (originSkill is ActiveSkill)
                statusEffectVar = ((ActiveSkill)GetOriginSkill()).SkillLogic.GetStatusEffectVar(this, i, GetCaster(), GetOwner());
            else if(originSkill is PassiveSkill)
                statusEffectVar = ((PassiveSkill)GetOriginSkill()).SkillLogic.GetStatusEffectVar(this, i, GetCaster(), GetOwner());
        }
        return statusEffectVar;
    }
    public bool IsCrowdControl() {
        return IsOfType(StatusEffectType.Faint) || IsOfType(StatusEffectType.Silence) || IsOfType(StatusEffectType.Bind)
                || IsOfType(StatusEffectType.SpeedChange) || IsOfType(StatusEffectType.Taunt);
    }
}
