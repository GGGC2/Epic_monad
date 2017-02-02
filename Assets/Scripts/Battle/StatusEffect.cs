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
            bool isBuff; // 버프일 경우 true
            bool isInfinite; // 페이즈 지속 제한이 없을 경우 true
            bool isStackable; // 상태 이상 중첩이 가능한 경우 true
            bool isRemovable; // 다른 기술에 의해 해제 가능할 경우 true

            // 이펙트 관련 정보
            string effectName;
            EffectVisualType effectVisualType;
            EffectMoveType effectMoveType;

            public FixedElement(string name,  
                  bool isBuff, bool isInfinite, bool isStackable, bool isRemovable, 
                  string effectName, EffectVisualType effectVisualType, EffectMoveType effectMoveType)
            {
                this.name = name;
                this.isBuff = isBuff;
                this.isInfinite = isInfinite;
                this.isStackable = isStackable;
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
            int remainPhase; // 지속 단위가 페이즈 단위인 경우 사용
            int remainStack; // 지속 단위가 적용 횟수 단위인 경우 사용
            int cooldown; // 효과가 적용되는 시점 (사라진 후 추가효과 있을 때)
            bool toBeRemoved; // 지속 단위가 0일 때, 또는 특정 조건에 의해 효과가 사라져야 할 경우 true로 바뀜

            public FlexibleElement(int remainPhase, int remainStack, int cooldown, bool toBeRemoved)
            {
                this.remainPhase = remainPhase;
                this.remainStack = remainStack;
                this.cooldown = cooldown;
                this.toBeRemoved = toBeRemoved; 
            }

            public int GetRemainPhase() {return remainPhase;}
            public int GetRemainStack() {return remainStack;}
            public int GetCooldown() {return cooldown;}	
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

            public void DecreaseCooldown()
            {
                cooldown--;
            }

            public void SetCooldown(int updatedCooldown)
            {
                cooldown = updatedCooldown;
            }

            public void SetToBeRemoved(bool toEnd)
            {
                toBeRemoved = toEnd;
            }
        }
    
        FixedElement fixedElement;
        FlexibleElement flexibleElement;

        public DisplayElement(string name,  
                  bool isBuff, bool isInfinite, bool isStackable, bool isRemovable,
                  int remainPhase, int remainStack, int cooldown, bool toBeRemoved, 
                  string effectName, EffectVisualType effectVisualType, EffectMoveType effectMoveType)
        {
            this.fixedElement = new FixedElement(name,  
                  isBuff, isInfinite, isStackable, isRemovable, 
                  effectName, effectVisualType, effectMoveType);

            this.flexibleElement = new FlexibleElement(remainPhase, remainStack, cooldown, toBeRemoved);
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
        public int GetCooldown() {return flexibleElement.GetCooldown();}	
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

        public void DecreaseCooldown()
        {
            flexibleElement.DecreaseCooldown();
        }

        public void SetCooldown(int updatedCooldown)
        {
            flexibleElement.SetCooldown(updatedCooldown);
        }

        public void SetToBeRemoved(bool toEnd)
        {
            flexibleElement.SetToBeRemoved(toEnd);
        }
    }
    	
    // 비공유 목록
    class ActualElement
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
            float degree; // 영향을 주는 수치(상대수치)
            float amount; // 영향을 주는 수치(절대수치)
            int remainAmount; // 남은 수치

            public FlexibleElement(float degree, float amount, int remainAmount)
            {
                this.degree = degree;
                this.amount = amount;
                this.remainAmount = remainAmount;
            }

            public float GetDegree() {return degree;}
            public float GetAmount() {return amount;}
            public int GetRemainAmount() {return remainAmount;}

            public void SetRemainAmount(int amount)
            {
                remainAmount = amount;
            }
        }

        FixedElement fixedElement;
        FlexibleElement flexibleElement;

        public ActualElement(StatusEffectType statusEffectType, 
                float degree, Stat amountStat, float amount, int remainAmount)
        {
            this.fixedElement = new FixedElement(statusEffectType, amountStat);
            this.flexibleElement = new FlexibleElement(degree, amount, remainAmount);                
        }

        public StatusEffectType GetStatusEffectType() {return fixedElement.GetStatusEffectType();}
        public Stat GetAmountStat() {return fixedElement.GetAmountStat();}
        
        public float GetDegree() {return flexibleElement.GetDegree();}
        public float GetAmount() {return GetAmount();}
        public int GetRemainAmount() {return GetRemainAmount();}

        public void SetRemainAmount(int amount)
        {
            flexibleElement.SetRemainAmount(amount);
        }
    }

    DisplayElement displayElement;
    List<ActualElement> actualElements;
	
	public StatusEffect(string name, StatusEffectType statusEffectType,  
                  bool isBuff, bool isInfinite, bool isStackable, bool isRemovable,
                  float degree, Stat amountStat, float amount, int remainAmount, 
                  int remainPhase, int remainStack, int cooldown, bool toBeRemoved, 
                  string effectName, EffectVisualType effectVisualType, EffectMoveType effectMoveType)
	{
		this.displayElement = new DisplayElement(name,  
                                                isBuff, isInfinite, isStackable, isRemovable,
                                                remainPhase, remainStack, cooldown, toBeRemoved, 
                                                effectName, effectVisualType, effectMoveType);

        this.actualElements = new List<ActualElement>();
        ActualElement actualElement = new ActualElement(statusEffectType, 
                                degree, amountStat, amount, remainAmount);
        actualElements.Add(actualElement);
    }
	
    public string GetName() {return displayElement.GetName();}
    public bool GetIsBuff() {return displayElement.GetIsBuff();}
    public bool GetIsInfinite() {return displayElement.GetIsInfinite();}
    public bool GetIsStackable() {return displayElement.GetIsStackable();}
    public bool GetIsRemovable() {return displayElement.GetIsRemovable();}
    public int GetRemainPhase() {return displayElement.GetRemainPhase();}
    public int GetRemainStack() {return displayElement.GetRemainStack();}
    public int GetCooldown() {return displayElement.GetCooldown();}	
    public bool GetToBeRemoved() {return displayElement.GetToBeRemoved();}
    public string GetEffectName() {return displayElement.GetEffectName();}
    public EffectVisualType GetEffectVisualType() {return displayElement.GetEffectVisualType();}
    public EffectMoveType GetEffectMoveType() {return displayElement.GetEffectMoveType();}

    public StatusEffectType GetStatusEffectType() {return actualElements[0].GetStatusEffectType();}
    public float GetDegree() {return actualElements[0].GetDegree();}
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

    public void DecreaseCooldown()
    {
        displayElement.DecreaseCooldown();
    }

    public void SetCooldown(int updatedCooldown)
    {
        displayElement.SetCooldown(updatedCooldown);
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
