using Enums;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class StatusEffect {

    // 공유목록
    class DisplayElement
    {
        class FixedElement
        {
            string name; // 유저에게 보일 이름
            bool isHidden; // 효과 아이콘이 표시될지 (특성으로 얻은 효과의 경우 표시되지 않음(true))
            bool isBuff; // 버프일 경우 true
            bool isInfinite; // 페이즈 지속 제한이 없을 경우 true
            bool isStackable; // 상태 이상 중첩이 가능한 경우 true
            int maxStack; // 최대 가능한 스택 수
            bool isRemovable; // 다른 기술에 의해 해제 가능할 경우 true

            // 이펙트 관련 정보
            string effectName;
            EffectVisualType effectVisualType;
            EffectMoveType effectMoveType;

            public FixedElement(string name,  
                  bool isHidden, bool isBuff, bool isInfinite, 
                  bool isStackable, int maxStack, bool isRemovable, 
                  string effectName, EffectVisualType effectVisualType, EffectMoveType effectMoveType)
            {
                this.name = name;
                this.isHidden = isHidden;
                this.isBuff = isBuff;
                this.isInfinite = isInfinite;
                this.isStackable = isStackable;
                this.maxStack = maxStack;
                this.isRemovable = isRemovable;
                this.effectName = effectName;
                this.effectVisualType = effectVisualType;
                this.effectMoveType = effectMoveType;
            }

            public string GetName() {return name;}
            public bool GetIsBuff() {return isBuff;}
            public bool GetIsInfinite() {return isInfinite;}
            public bool GetIsStackable() {return isStackable;}
            public bool GetIsRemovable() {return isRemovable;}
            public string GetEffectName() {return effectName;}
            public EffectVisualType GetEffectVisualType() {return effectVisualType;}
            public EffectMoveType GetEffectMoveType() {return effectMoveType;}
        }
        
        class FlexibleElement
        {
            GameObject caster; // 시전자
            int remainStack; // 지속 단위가 적용 횟수 단위인 경우 사용
            int remainPhase; // 지속 단위가 페이즈 단위인 경우 사용
            // 없애면 터져서 일단 넣어놓음
            bool toBeRemoved; // 지속 단위가 0일 때, 또는 특정 조건에 의해 효과가 사라져야 할 경우 true로 바뀜

            public FlexibleElement(GameObject caster, int remainStack, int remainPhase, bool toBeRemoved)
            {
                this.caster = caster;
                this.remainStack = remainStack;
                this.remainPhase = remainPhase;
                this.toBeRemoved = toBeRemoved; 
            }

            public int GetRemainPhase() {return remainPhase;}
            public int GetRemainStack() {return remainStack;}
            public bool GetToBeRemoved() {return toBeRemoved;}

            public void AddRemainPhase(int phase)
            {
                remainPhase += phase;
            }
            
            public void SubRemainPhase(int phase)
            {
                remainPhase -= phase;
            }
            
            public void DecreaseRemainPhase()
            {
                remainPhase --;
            }
            
            public void SetRemainPhase(int phase)
            {
                remainPhase = phase;
            }
            
            public void AddRemainStack(int stack)
            {
                remainStack += stack;
            }
            
            public void SubRemainStack(int stack)
            {
                remainStack -= stack;
            }
            
            public void DecreaseRemainStack()
            {
                remainStack --; 
            }
            
            public void SetRemainStack(int stack)
            {
                remainStack = stack; 
            }

            public void SetToBeRemoved(bool toEnd)
            {
                toBeRemoved = toEnd;
            }
        }
    
        FixedElement fixedElement;
        FlexibleElement flexibleElement;

        public DisplayElement(string name,  
                  bool isHidden, bool isBuff, bool isInfinite, 
                  bool isStackable, int maxStack, bool isRemovable,
                  string effectName, EffectVisualType effectVisualType, EffectMoveType effectMoveType)
        {
            this.fixedElement = new FixedElement(name,  
                  isHidden, isBuff, isInfinite, 
                  isStackable, maxStack, isRemovable, 
                  effectName, effectVisualType, effectMoveType);

            this.flexibleElement = new FlexibleElement(null, 0, 0, false);
        }

        public string GetName() {return fixedElement.GetName();}
        public bool GetIsBuff() {return fixedElement.GetIsBuff();}
        public bool GetIsInfinite() {return fixedElement.GetIsInfinite();}
        public bool GetIsStackable() {return fixedElement.GetIsStackable();}
        public bool GetIsRemovable() {return fixedElement.GetIsRemovable();}
        public string GetEffectName() {return fixedElement.GetEffectName();}
        public EffectVisualType GetEffectVisualType() {return fixedElement.GetEffectVisualType();}
        public EffectMoveType GetEffectMoveType() {return fixedElement.GetEffectMoveType();}

        public int GetRemainPhase() {return flexibleElement.GetRemainPhase();}
        public int GetRemainStack() {return flexibleElement.GetRemainStack();}
        public bool GetToBeRemoved() {return flexibleElement.GetToBeRemoved();}

        public void AddRemainPhase(int phase)
        {
            flexibleElement.AddRemainPhase(phase);
        }
        
        public void SubRemainPhase(int phase)
        {
            flexibleElement.SubRemainPhase(phase);
        }
        
        public void DecreaseRemainPhase()
        {
            flexibleElement.DecreaseRemainPhase();
        }
        
        public void SetRemainPhase(int phase)
        {
            flexibleElement.SetRemainPhase(phase);
        }
        
        public void AddRemainStack(int stack)
        {
            flexibleElement.AddRemainStack(stack);
        }
        
        public void SubRemainStack(int stack)
        {
            flexibleElement.SubRemainStack(stack);
        }
        
        public void DecreaseRemainStack()
        {
            flexibleElement.DecreaseRemainStack(); 
        }
        
        public void SetRemainStack(int stack)
        {
            flexibleElement.SetRemainStack(stack); 
        }

        public void SetToBeRemoved(bool toEnd)
        {
            flexibleElement.SetToBeRemoved(toEnd);
        }
    }
    	
    // 비공유 목록
    public class ActualElement
    {
        class FixedElement
        {
            StatusEffectType statusEffectType; // 시스템 상으로 구분하는 상태이상의 종류        
            Stat amountStat; // 영향을 주는 수치(절대수치)의 스탯값

            public FixedElement(StatusEffectType statusEffectType, Stat amountStat)
            {
                this.statusEffectType = statusEffectType;
                this.amountStat = amountStat;   
            }

            public StatusEffectType GetStatusEffectType() {return statusEffectType;}
            public Stat GetAmountStat() {return amountStat;}
        }

        class FlexibleElement
        {
            int amount; // 영향을 주는 수치
            int remainAmount; // 남은 수치 (실드 등)

            public FlexibleElement(int amount, int remainAmount)
            {
                this.amount = amount;
                this.remainAmount = remainAmount;
            }

            public int GetAmount() {return amount;}
            public int GetRemainAmount() {return remainAmount;}

            public void SetRemainAmount(int newRemainAmount)
            {
                remainAmount = newRemainAmount;
            }
        }

        FixedElement fixedElement;
        FlexibleElement flexibleElement;

        public ActualElement(StatusEffectType statusEffectType, 
                Stat amountStat, bool isRelative)
        {
            this.fixedElement = new FixedElement(statusEffectType, amountStat);
            this.flexibleElement = new FlexibleElement(0, 0);                
        }

        public StatusEffectType GetStatusEffectType() {return fixedElement.GetStatusEffectType();}
        public Stat GetAmountStat() {return fixedElement.GetAmountStat();}
        
        public int GetAmount() {return GetAmount();}
        public int GetRemainAmount() {return GetRemainAmount();}

        public void SetRemainAmount(int newRemainAmount)
        {
            flexibleElement.SetRemainAmount(newRemainAmount);
        }
    }

    DisplayElement displayElement;
    List<ActualElement> actualElements;
	
	public StatusEffect(string name,   
                  bool isHidden, bool isBuff, bool isInfinite, 
                  bool isStackable, int maxStack, bool isRemovable,
                  string effectName, EffectVisualType effectVisualType, EffectMoveType effectMoveType,
                  List<ActualElement> newActualEffects)
	{
		this.displayElement = new DisplayElement(name,  
                                                isHidden, isBuff, isInfinite, 
                                                isStackable, maxStack, isRemovable,
                                                effectName, effectVisualType, effectMoveType);

        this.actualElements = newActualEffects;
    }
	
    public string GetName() {return displayElement.GetName();}
    public bool GetIsBuff() {return displayElement.GetIsBuff();}
    public bool GetIsInfinite() {return displayElement.GetIsInfinite();}
    public bool GetIsStackable() {return displayElement.GetIsStackable();}
    public bool GetIsRemovable() {return displayElement.GetIsRemovable();}
    public int GetRemainPhase() {return displayElement.GetRemainPhase();}
    public int GetRemainStack() {return displayElement.GetRemainStack();}
    public bool GetToBeRemoved() {return displayElement.GetToBeRemoved();}
    public string GetEffectName() {return displayElement.GetEffectName();}
    public EffectVisualType GetEffectVisualType() {return displayElement.GetEffectVisualType();}
    public EffectMoveType GetEffectMoveType() {return displayElement.GetEffectMoveType();}

    public StatusEffectType GetStatusEffectType() {return actualElements[0].GetStatusEffectType();}
    public Stat GetAmountStat() {return actualElements[0].GetAmountStat();}
    public float GetAmount() {return actualElements[0].GetAmount();}
    public int GetRemainAmount() {return actualElements[0].GetRemainAmount();}

    public void SetRemainAmount(int amount)
    {
        actualElements[0].SetRemainAmount(amount);
    }

    public void AddRemainPhase(int phase)
	{
        displayElement.SetRemainPhase(phase);
	}
	
	public void SubRemainPhase(int phase)
	{
        displayElement.SetRemainPhase(phase);
	}
	
	public void DecreaseRemainPhase()
	{
		displayElement.DecreaseRemainPhase();
	}
    
    public void SetRemainPhase(int phase)
    {
        displayElement.SetRemainPhase(phase);
    }
    
    public void AddRemainStack(int stack)
    {
        displayElement.AddRemainStack(stack);
    }
    
    public void SubRemainStack(int stack)
    {
        displayElement.SubRemainStack(stack);
    }
    
    public void DecreaseRemainStack()
    {
        displayElement.DecreaseRemainStack(); 
    }
    
    public void SetRemainStack(int stack)
    {
        displayElement.SetRemainStack(stack); 
    }

    public void SetToBeRemoved(bool toEnd)
    {
        displayElement.SetToBeRemoved(toEnd);
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
    
    public bool IsSameStatusEffect(StatusEffect statusEffect)
    {
        bool isSameStatusEffect = false;
        if(this.GetName().Equals(statusEffect.GetName()))
        {
            if(this.IsOfType(statusEffect.GetStatusEffectType()))
            {
                isSameStatusEffect = true;
            }
        }
        
        return isSameStatusEffect;
    }
}
