using Enums;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class StatusEffect {
    public FixedElement fixedElem;
    public FlexibleElement flexibleElem;

    // 공유목록
    public class FixedElement {

        public readonly DisplayElement display;
        public readonly List<ActualElement> actuals;
        public class DisplayElement
		{
            public readonly string originSkillName; // 효과를 불러오는 기술의 이름 
            public readonly string displayName; // 유저에게 보일 이름
            public readonly bool isHidden; // 효과 아이콘이 표시될지 (특성으로 얻은 효과의 경우 표시되지 않음(true))
            public readonly bool isBuff; // 버프일 경우 true
            public readonly bool isInfinite; // 페이즈 지속 제한이 없을 경우 true
            public readonly bool isStackable; // 상태 이상 중첩이 가능한 경우 true
            public readonly bool isDisposable; // 다음 1회의 행동에만 적용되는 경우 true (예: 강타)
			public readonly int maxPhase; // 상태 이상이 지속가능한 phase
            public readonly int maxStack; // 최대 가능한 스택 수
            public readonly bool isRemovable; // 다른 기술에 의해 해제 가능할 경우 true

            // 이펙트 관련 정보
            public readonly string effectName;
            public readonly EffectVisualType effectVisualType;
            public readonly EffectMoveType effectMoveType;

            public DisplayElement(string originSkillName, string displayName,  
                  bool isHidden, bool isBuff, bool isInfinite, 
                  bool isStackable, bool isDisposable,
                  int maxPhase, int maxStack, bool isRemovable, 
                  string effectName, EffectVisualType effectVisualType, EffectMoveType effectMoveType)
            {
                this.originSkillName = originSkillName;
                this.displayName = displayName;
                this.isHidden = isHidden;
                this.isBuff = isBuff;
                this.isInfinite = isInfinite;
                this.isStackable = isStackable;
                this.isDisposable = isDisposable;
				this.maxPhase = maxPhase;
                this.maxStack = maxStack;
                this.isRemovable = isRemovable;
                this.effectName = effectName;
                this.effectVisualType = effectVisualType;
                this.effectMoveType = effectMoveType;
            }
        }

		public class ActualElement
		{
            public readonly StatusEffectType statusEffectType; // 시스템 상으로 구분하는 상태이상의 종류        
            public readonly Stat amountStat; // 영향을 주는 스탯
			public readonly bool isRelative;

            public ActualElement(StatusEffectType statusEffectType, Stat amountStat, bool isRelative)
            {
                this.statusEffectType = statusEffectType;
                this.amountStat = amountStat;   
				this.isRelative = isRelative;
            }
		}

		public FixedElement(string originSkillName, string displayName, 
                  bool isHidden, bool isBuff, bool isInfinite, 
                  bool isStackable, bool isDisposable,
                  int maxPhase, int maxStack, bool isRemovable,
                  string effectName, EffectVisualType effectVisualType, EffectMoveType effectMoveType, List<ActualElement> actualEffects)
		{
			display = new DisplayElement(originSkillName, displayName,
					isHidden, isBuff, isInfinite,
					isStackable, isDisposable,
                    maxPhase, maxStack, isRemovable,
					effectName, effectVisualType, effectMoveType);

			actuals = actualEffects;
		}
	}
        
	public class FlexibleElement {

        public DisplayElement display;
        public List<ActualElement> actuals;
        public class DisplayElement
		{
			public Unit caster; // 시전자
			public int remainStack; // 지속 단위가 적용 횟수 단위인 경우 사용
			public int remainPhase; // 지속 단위가 페이즈 단위인 경우 사용
			// 없애면 터져서 일단 넣어놓음
			public bool toBeRemoved; // 지속 단위가 0일 때, 또는 특정 조건에 의해 효과가 사라져야 할 경우 true로 바뀜

            public DisplayElement(Unit caster, int remainStack, int remainPhase)
            {
                this.caster = caster;
                this.remainStack = remainStack;
                this.remainPhase = remainPhase;
                this.toBeRemoved = false; 
            }
		}

		public class ActualElement
		{
            public float amount; // 영향을 주는 실제 값
            public float remainAmount; // 남은 수치 (실드 등)

            public ActualElement(float amount)
            {
                this.amount = amount;
                this.remainAmount = amount; // 초기화
            }
		}

		public FlexibleElement(FixedElement fixedElem, Unit caster)
		{
			int maxStack = fixedElem.display.maxStack;
			int maxPhase = fixedElem.display.maxPhase;
			display = new DisplayElement(caster, maxStack, maxPhase);

			List<ActualElement> actuals = new List<ActualElement>();
			for (int i = 0; i < fixedElem.actuals.Count; i++)
            {
                if (fixedElem.actuals[i].isRelative)
                    actuals.Add(new ActualElement(1.0f));
                else
                    actuals.Add(new ActualElement(0));

            }
            this.actuals = actuals;
		}
	}
	
	public StatusEffect(FixedElement fixedElem, Unit caster)
	{
		this.fixedElem = fixedElem;
		this.flexibleElem = new FlexibleElement(fixedElem, caster);
    }
	
    public string GetOriginSkillName() {return fixedElem.display.originSkillName;}
    public string GetDisplayName() {return fixedElem.display.displayName;}
    public bool GetIsBuff() {return fixedElem.display.isBuff;}
    public bool GetIsInfinite() {return fixedElem.display.isInfinite;}
    public bool GetIsStackable() {return fixedElem.display.isStackable;}
    public bool GetIsDisposable() {return fixedElem.display.isDisposable;}
    public bool GetIsRemovable() {return fixedElem.display.isRemovable;}
    public string GetEffectName() {return fixedElem.display.effectName;}
    public EffectVisualType GetEffectVisualType() {return fixedElem.display.effectVisualType;}
    public EffectMoveType GetEffectMoveType() {return fixedElem.display.effectMoveType;}
    public Unit GetCaster() {return flexibleElem.display.caster;}
    public int GetRemainPhase() {return flexibleElem.display.remainPhase;}
    public int GetRemainStack() {return flexibleElem.display.remainStack;}
    public bool GetToBeRemoved() {return flexibleElem.display.toBeRemoved;}

    public StatusEffectType GetStatusEffectType() {return fixedElem.actuals[0].statusEffectType;}
    public StatusEffectType GetStatusEffectType(int index) {return fixedElem.actuals[index].statusEffectType;}
    public Stat GetAmountStat() {return fixedElem.actuals[0].amountStat;}
    public Stat GetAmountStat(int index) {return fixedElem.actuals[index].amountStat;} 
    public bool GetIsRelative() {return  fixedElem.actuals[0].isRelative;}
    public bool GetIsRelative(int index) {return  fixedElem.actuals[index].isRelative;}
    public float GetAmount() {return flexibleElem.actuals[0].amount;}
    public float GetAmount(int index) {return flexibleElem.actuals[index].amount;}
    public float GetRemainAmount() {return flexibleElem.actuals[0].remainAmount;}
    public float GetRemainAmount(int index) {return flexibleElem.actuals[index].remainAmount;}

    public void SetAmount(float amount)
    {
        flexibleElem.actuals[0].amount = amount;
    }

    public void SetAmount(int index, float amount)
    {
        flexibleElem.actuals[index].amount = amount;
    }

    public void SetRemainAmount(float amount)
    {
		flexibleElem.actuals[0].remainAmount = amount;
    }

    public void SetRemainAmount(int index, float amount)
    {
		flexibleElem.actuals[index].remainAmount = amount;
    }

    public void AddRemainPhase(int phase)
	{
		flexibleElem.display.remainPhase += phase;
	}
	
	public void DecreaseRemainPhase()
	{
		flexibleElem.display.remainPhase -= 1;
	}

    public void DecreaseRemainPhase(int phase)
	{
		flexibleElem.display.remainPhase -= phase;
	}
    
    public void SetRemainPhase(int phase)
    {
		flexibleElem.display.remainPhase = phase;
    }
    
    public void AddRemainStack(int stack)
    {
		flexibleElem.display.remainStack += stack;
    }
    
    public void DecreaseRemainStack()
    {
		flexibleElem.display.remainStack -= 1;
    }
    
    public void DecreaseRemainStack(int stack)
    {
		flexibleElem.display.remainStack -= stack;
    }

    public void SetRemainStack(int stack)
    {
		flexibleElem.display.remainStack = stack;
    }

    public void SetToBeRemoved(bool toEnd)
    {
		flexibleElem.display.toBeRemoved = toEnd;
    }

    public bool IsOfType(StatusEffectType statusEffectType)
    {
        bool isOfType = false;
        if (statusEffectType.Equals(this.GetStatusEffectType()))
        {
            isOfType = true;
        }
        
        return isOfType;
    }

    public bool IsOfType(int index, StatusEffectType statusEffectType)
    {
        return statusEffectType.Equals(this.GetStatusEffectType(index));
    }
    
    public bool IsSameStatusEffect(StatusEffect anotherStatusEffect)
    {
        return (this.GetOriginSkillName().Equals(anotherStatusEffect.GetOriginSkillName()) &&
                (this.GetCaster().Equals(anotherStatusEffect.GetCaster())));
    }
}
