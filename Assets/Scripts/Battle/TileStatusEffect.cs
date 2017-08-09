using Enums;
using System.Collections.Generic;

public class TileStatusEffect : StatusEffect {
    public TileStatusEffect(FixedElement fixedElem, Unit caster, Tile ownerTile, Skill originSkill) : base(fixedElem, caster, originSkill){
        this.fixedElem = fixedElem;
        this.flexibleElem = new FlexibleElement(this, caster, ownerTile, originSkill);
        ((FlexibleElement.DisplayElement)flexibleElem.display).ownerTile = ownerTile;
        for (int i = 0; i < fixedElem.actuals.Count; i++) {
            CalculateAmount(i, false);
            SetRemainAmount(i, GetAmount(i));
        }
    }
    public new class FixedElement : StatusEffect.FixedElement {
        public FixedElement(bool toBeReplaced, string originSkillName, string displayName,
                  bool isInfinite, bool isStackable, bool isOnce,
                  int defaultPhase, int maxStack, bool amountNotEffectedByStack, bool isRemovable,
                  string explanation, string effectName, EffectVisualType effectVisualType, EffectMoveType effectMoveType, List<ActualElement> actualEffects) 
                  : base(toBeReplaced, originSkillName, displayName, isInfinite, isStackable, isOnce, defaultPhase, maxStack, 
                    amountNotEffectedByStack, isRemovable, explanation, effectName, effectVisualType, effectMoveType, actualEffects){
        }
    }
    public new class FlexibleElement : StatusEffect.FlexibleElement {
        
        public new class DisplayElement : StatusEffect.FlexibleElement.DisplayElement {
            public Tile ownerTile;

            public DisplayElement(Unit caster, Tile ownerTile, Skill originSkill, int maxStack, int defaultPhase) 
                    : base(caster, originSkill, maxStack, defaultPhase) {
                this.ownerTile = ownerTile;
            }
        }
        public FlexibleElement(StatusEffect statusEffect, Unit caster, Tile ownerTile, Skill originSkill) 
                : base(statusEffect, caster, originSkill) {
            FixedElement fixedElem = (TileStatusEffect.FixedElement)statusEffect.fixedElem;
            int maxStack = fixedElem.display.maxStack;
            int defaultPhase = fixedElem.display.defaultPhase;
            display = new DisplayElement(caster, ownerTile, originSkill, maxStack, defaultPhase);

            this.actuals = new List<ActualElement>();
            for (int i = 0; i < fixedElem.actuals.Count; i++) {
                actuals.Add(new ActualElement(0));
            }
        }
    }
    public Tile GetOwnerTile() { return ((FlexibleElement.DisplayElement)flexibleElem.display).ownerTile; }
}
