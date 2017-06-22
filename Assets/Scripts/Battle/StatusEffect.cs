using Enums;
using UnityEngine;
using System.Collections.Generic;
using Battle.Skills;

public class StatusEffect {
    public FixedElement fixedElem;
    public FlexibleElement flexibleElem;

    // 공유목록
    public class FixedElement {

        public readonly DisplayElement display;
        public readonly List<ActualElement> actuals;
        public class DisplayElement {
            public readonly bool toBeReplaced;  //상위의 강화 스킬이 있는 경우 true. 큐리의 '가연성 부착물'과 '조연성 부착물' 스킬 같은 경우 
                                                //csv 파일에 같은 originSkillName을 가지고 있는데, 이 때 둘 중 하나만 읽어야 하므로 '가연성 부착물'
                                                //statusEffect는 읽지 않게 하기 위함.
            public readonly Skill originSkill;
            public readonly string originSkillName; // 효과를 불러오는 기술의 이름 
            public readonly string displayName; // 유저에게 보일 이름
            public readonly bool isBuff; // 버프일 경우 true
            public readonly bool isInfinite; // 페이즈 지속 제한이 없을 경우 true
            public readonly bool isStackable; // 상태 이상 중첩이 가능한 경우 true
            public readonly bool isOnce; // 다음 1회의 행동에만 적용되는 경우 true (예: 강타)
            public readonly int defaultPhase; // 일반적인 경우 상태이상이 지속되는 페이즈
            public readonly StatusEffectVar stackVar; // 한번에 쌓이는 스택 수
            public readonly int maxStack; // 최대 가능한 스택 수
            public readonly bool isRemovable; // 다른 기술에 의해 해제 가능할 경우 true

            // 이펙트 관련 정보
            public readonly string effectName;
            public readonly EffectVisualType effectVisualType;
            public readonly EffectMoveType effectMoveType;

            public DisplayElement(bool toBeReplaced, string originSkillName, string displayName,
                  bool isBuff, bool isInfinite,
                  bool isStackable, bool isOnce,
                  int defaultPhase, StatusEffectVar stackVar, int maxStack, bool isRemovable,
                  string effectName, EffectVisualType effectVisualType, EffectMoveType effectMoveType) {
                this.toBeReplaced = toBeReplaced;
                this.originSkillName = originSkillName;
                this.displayName = displayName;
                this.isBuff = isBuff;
                this.isInfinite = isInfinite;
                this.isStackable = isStackable;
                this.isOnce = isOnce;
                this.defaultPhase = defaultPhase;
                this.stackVar = stackVar;
                this.maxStack = maxStack;
                this.isRemovable = isRemovable;
                this.effectName = effectName;
                this.effectVisualType = effectVisualType;
                this.effectMoveType = effectMoveType;
            }
        }

        public class ActualElement {
            public readonly StatusEffectType statusEffectType; // 시스템 상으로 구분하는 상태이상의 종류 

            // var * coef + base
            public readonly StatusEffectVar seVar;
            public readonly float seCoef;
            public readonly float seBase;

            public readonly bool isMultiply;

            public ActualElement(StatusEffectType statusEffectType,
                                 StatusEffectVar statusEffectVar, float statusEffectCoef, float statusEffectBase,
                                 bool isMultiply) {
                this.statusEffectType = statusEffectType;
                this.seVar = statusEffectVar;
                this.seCoef = statusEffectCoef;
                this.seBase = statusEffectBase;
                this.isMultiply = isMultiply;
            }
        }

        public FixedElement(bool toBeReplaced, string originSkillName, string displayName,
                  bool isBuff, bool isInfinite,
                  bool isStackable, bool isOnce,
                  int defaultPhase, StatusEffectVar stackVar, int maxStack, bool isRemovable,
                  string effectName, EffectVisualType effectVisualType, EffectMoveType effectMoveType, List<ActualElement> actualEffects) {
            display = new DisplayElement(toBeReplaced, originSkillName, displayName,
                    isBuff, isInfinite,
                    isStackable, isOnce,
                    defaultPhase, stackVar, maxStack, isRemovable,
                    effectName, effectVisualType, effectMoveType);

            actuals = actualEffects;
        }
    }

    public class FlexibleElement {

        public DisplayElement display;
        public List<ActualElement> actuals;
        public class DisplayElement {
            public Unit caster; // 시전자
            public Unit owner;  // statusEffect를 가지고 있는 유닛
            public Skill originSkill;
            public PassiveSkill originPassiveSkill;
            public int remainStack; // 지속 단위가 적용 횟수 단위인 경우 사용
            public int remainPhase; // 지속 단위가 페이즈 단위인 경우 사용
            public Element element; // StatusEffect의 속성. 큐리 패시브 등에 사용
            public List<Unit> memorizedUnits;  // StatusEffect가 기억할 유닛. 유진의 '순백의 방패'와 같이 중첩 가능한 오오라 효과에 사용.

            public DisplayElement(Unit caster, Unit owner, Skill originSkill, PassiveSkill originPassiveSkill, int maxStack, int defaultPhase) {
                this.originSkill = originSkill;
                this.originPassiveSkill = originPassiveSkill;
                this.caster = caster;
                this.owner  = owner;
                this.remainStack = 1;
                this.remainPhase = defaultPhase;
                this.memorizedUnits = new List<Unit>();
            }
        }

        public class ActualElement {
            public float amount; // 영향을 주는 실제 값
            public float remainAmount; // 남은 수치 (실드 등)

            public ActualElement(float amount) {
                this.amount = amount;
                this.remainAmount = amount; // 초기화
            }
        }

        public FlexibleElement(StatusEffect statusEffect, Unit caster, Unit owner, Skill originSkill, PassiveSkill originPassiveSkill) {
            StatusEffect.FixedElement fixedElem = statusEffect.fixedElem;
            int maxStack = fixedElem.display.maxStack;
            int defaultPhase = fixedElem.display.defaultPhase;
            display = new DisplayElement(caster, owner, originSkill, originPassiveSkill, maxStack, defaultPhase);

            this.actuals = new List<ActualElement>();
            for (int i = 0; i < fixedElem.actuals.Count; i++) {
                actuals.Add(new ActualElement(0));
            }
        }
    }

    public StatusEffect(FixedElement fixedElem, Unit caster, Unit owner, Skill originSkill, PassiveSkill originPassiveSkill) {
        this.fixedElem = fixedElem;
        this.flexibleElem = new FlexibleElement(this, caster, owner, originSkill, originPassiveSkill);
        for(int i = 0; i<fixedElem.actuals.Count; i++) {
            float statusEffectVar = GetStatusEffectVar(i);
            CalculateAmount(i, statusEffectVar);
        }
    }

    public bool GetToBeReplaced() { return fixedElem.display.toBeReplaced; }
    public string GetOriginSkillName() { return fixedElem.display.originSkillName; }
    public string GetDisplayName() { return fixedElem.display.displayName; }
    public bool GetIsBuff() { return fixedElem.display.isBuff; }
    public bool GetIsInfinite() { return fixedElem.display.isInfinite; }
    public bool GetIsStackable() { return fixedElem.display.isStackable; }
    public bool GetIsOnce() { return fixedElem.display.isOnce; }
    public bool GetIsRemovable() { return fixedElem.display.isRemovable; }
    public string GetEffectName() { return fixedElem.display.effectName; }
    public EffectVisualType GetEffectVisualType() { return fixedElem.display.effectVisualType; }
    public EffectMoveType GetEffectMoveType() { return fixedElem.display.effectMoveType; }
    public Skill GetOriginSkill() { return flexibleElem.display.originSkill; }
    public PassiveSkill GetOriginPassiveSkill() { return flexibleElem.display.originPassiveSkill; }
    public Unit GetCaster() { return flexibleElem.display.caster; }
    public Unit GetOwner()  { return flexibleElem.display.owner;  }
    public int GetRemainPhase() { return flexibleElem.display.remainPhase; }
    public int GetRemainStack() { return flexibleElem.display.remainStack; }
    public Element GetElement() { return flexibleElem.display.element; }
    public List<Unit> GetMemorizedUnits() { return flexibleElem.display.memorizedUnits; }

    public StatusEffectType GetStatusEffectType() { return fixedElem.actuals[0].statusEffectType; }
    public StatusEffectType GetStatusEffectType(int index) { return fixedElem.actuals[index].statusEffectType; }
    public bool GetIsMultiply() { return fixedElem.actuals[0].isMultiply; }
    public bool GetIsMultiply(int index) { return fixedElem.actuals[index].isMultiply; }
    public float GetAmount() { return flexibleElem.actuals[0].amount; }
    public float GetAmount(int index) { return flexibleElem.actuals[index].amount; }
    public float GetRemainAmount() { return flexibleElem.actuals[0].remainAmount; }
    public float GetRemainAmount(int index) { return flexibleElem.actuals[index].remainAmount; }

    public void SetAmount(float amount) { flexibleElem.actuals[0].amount = amount; }
    public void SetAmount(int index, float amount) { flexibleElem.actuals[index].amount = amount; }
    public void SetRemainAmount(float amount) { flexibleElem.actuals[0].remainAmount = amount; }
    public void SetRemainAmount(int index, float amount) { flexibleElem.actuals[index].remainAmount = amount; }
    public void SubAmount(int index, float amount) { flexibleElem.actuals[index].remainAmount -= amount; }
    public void AddRemainPhase(int phase) { flexibleElem.display.remainPhase += phase; }
    public void DecreaseRemainPhase() { flexibleElem.display.remainPhase -= 1; }
    public void DecreaseRemainPhase(int phase) { flexibleElem.display.remainPhase -= phase; }
    public void SetRemainPhase(int phase) { flexibleElem.display.remainPhase = phase; }
    public void AddRemainStack(int stack) { 
		flexibleElem.display.remainStack += stack;
        if(flexibleElem.display.remainStack > fixedElem.display.maxStack) {
            flexibleElem.display.remainStack = fixedElem.display.maxStack;
        }
    }
    
    public void DecreaseRemainStack() { 
		flexibleElem.display.remainStack -= 1;
        if (flexibleElem.display.remainStack < 0) {
            flexibleElem.display.remainStack = 0;
        }
    }
    
    public void DecreaseRemainStack(int stack) {
		flexibleElem.display.remainStack -= stack;
        if (flexibleElem.display.remainStack < 0) {
            flexibleElem.display.remainStack = 0;
        }
    }

    public void SetRemainStack(int stack) {
		flexibleElem.display.remainStack = stack;
        if (flexibleElem.display.remainStack > fixedElem.display.maxStack) {
            flexibleElem.display.remainStack = fixedElem.display.maxStack;
        }
        if (flexibleElem.display.remainStack < 0) {
            flexibleElem.display.remainStack = 0;
        }
    }

    public bool IsOfType(StatusEffectType statusEffectType) {
        bool isOfType = false;
        if (statusEffectType.Equals(this.GetStatusEffectType()))
        {
            isOfType = true;
        }
        
        return isOfType;
    }

    public bool IsOfType(int index, StatusEffectType statusEffectType) {
        return statusEffectType.Equals(this.GetStatusEffectType(index));
    }
    
    public bool IsSameStatusEffect(StatusEffect anotherStatusEffect) {
        return (GetOriginSkillName().Equals(anotherStatusEffect.GetOriginSkillName()) &&
                    GetDisplayName().Equals(anotherStatusEffect.GetDisplayName()) &&
                    GetCaster().Equals(anotherStatusEffect.GetCaster()));
    }

    public void CalculateAmount(float statusEffectVar) {
        flexibleElem.actuals[0].amount = (statusEffectVar * fixedElem.actuals[0].seCoef + fixedElem.actuals[0].seBase) * GetRemainStack();
    }
    public void CalculateAmount(int i, float statusEffectVar) {
        flexibleElem.actuals[i].amount = (statusEffectVar * fixedElem.actuals[i].seCoef + fixedElem.actuals[i].seBase) * GetRemainStack();
    }
    
    public float GetStatusEffectVar(int i) {
        Unit caster = GetCaster();
        Unit owner  = GetOwner();
        float result = 0;
        StatusEffectVar seVarEnum = fixedElem.actuals[i].seVar;
        if (seVarEnum == StatusEffectVar.Level)
            result = MonoBehaviour.FindObjectOfType<BattleManager>().GetPartyLevel();
        else if (seVarEnum == StatusEffectVar.LostHpPercent)
            result = 100 - (100 * ((float)caster.GetCurrentHealth() / (float)caster.GetMaxHealth()));
        else if (seVarEnum == StatusEffectVar.Power)
            result = caster.GetActualStat(Stat.Power);
        else {
            if(GetOriginSkill() != null)
                result = SkillLogicFactory.Get(GetOriginSkill()).GetStatusEffectVar(this, i, caster, owner);
            if(GetOriginPassiveSkill() != null)
                result = SkillLogicFactory.Get(GetOriginPassiveSkill()).GetStatusEffectVar(this, i, caster, owner);
        }
        return result;
    }
}
