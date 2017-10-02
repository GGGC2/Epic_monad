using Enums;
using UnityEngine;
using System.Collections.Generic;

public class UnitStatusEffect : StatusEffect {
    public UnitStatusEffect(FixedElement fixedElem, Unit caster, Unit owner, Skill originSkill) : base(fixedElem, caster, originSkill) {
        this.fixedElem = fixedElem;
        this.flexibleElem = new FlexibleElement(this, caster, owner, originSkill);
        ((FlexibleElement.DisplayElement)flexibleElem.display).owner = owner;
        for (int i = 0; i < fixedElem.actuals.Count; i++) {
            CalculateAmount(i, false);
            SetRemainAmount(i, GetAmount(i));
        }
    }
    public new class FixedElement : StatusEffect.FixedElement {
        public FixedElement(bool toBeReplaced, string originSkillName, string displayName,
                  bool isBuff, bool isInfinite, bool isStackable, bool isOnce,
                  int defaultPhase, int maxStack, bool amountToBeUpdated, bool amountNotEffectedByStack, bool isRemovable,
                  string explanation, string effectName, EffectVisualType effectVisualType, EffectMoveType effectMoveType, List<ActualElement> actualEffects)
                  : base(toBeReplaced, originSkillName, displayName, isInfinite, isStackable, isOnce, defaultPhase, maxStack, amountToBeUpdated,
                    amountNotEffectedByStack, isRemovable, explanation, effectName, effectVisualType, effectMoveType, actualEffects) {
            display = new DisplayElement(toBeReplaced, originSkillName, displayName,
                    isBuff, isInfinite, isStackable, isOnce,
                    defaultPhase, maxStack, amountToBeUpdated, amountNotEffectedByStack, isRemovable,
                    explanation, effectName, effectVisualType, effectMoveType);
        }
        public new class DisplayElement : StatusEffect.FixedElement.DisplayElement {
            public readonly bool isBuff;

            public DisplayElement(bool toBeReplaced, string originSkillName, string displayName,
                  bool isBuff, bool isInfinite, bool isStackable, bool isOnce,
                  int defaultPhase, int maxStack, bool amountToBeUpdated, bool amountNotEffectedByStack, bool isRemovable,
                  string explanation, string effectName, EffectVisualType effectVisualType, EffectMoveType effectMoveType) 
                  : base(toBeReplaced, originSkillName, displayName, isInfinite, isStackable, isOnce, defaultPhase,
                    maxStack, amountToBeUpdated, amountNotEffectedByStack, isRemovable, explanation, effectName, effectVisualType, effectMoveType){
                this.isBuff = isBuff;
            }
        }
    }
    public new class FlexibleElement : StatusEffect.FlexibleElement {

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
    public bool GetIsBuff() { return ((FixedElement.DisplayElement)fixedElem.display).isBuff; }
    public Unit GetOwner() { return ((UnitStatusEffect.FlexibleElement.DisplayElement)flexibleElem.display).owner; }
    protected override float GetStatusEffectVar(int i) {
        float statusEffectVar = 0;
        Skill originSkill = GetOriginSkill();
        if (originSkill != null) {
            if (originSkill.GetType() == typeof(ActiveSkill))
                statusEffectVar = ((ActiveSkill)GetOriginSkill()).SkillLogic.GetStatusEffectVar(this, i, GetCaster(), GetOwner());
            else
                statusEffectVar = ((PassiveSkill)GetOriginSkill()).SkillLogic.GetStatusEffectVar(this, i, GetCaster(), GetOwner());
        }
        return statusEffectVar;
    }
}
