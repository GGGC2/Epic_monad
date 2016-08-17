using Enums;
using System.Collections;
using UnityEngine;

public class StatusEffect {

	string name; // 유저에게 보일 이름
    StatusEffectType statusEffectType; // 시스템 상으로 구분하는 상태이상의 종류
    
    bool isBuff; // 버프일 경우 true
    bool isInfinite; // 페이즈 지속 제한이 없을 경우 true
    bool isStackable; // 상태 이상 중첩이 가능한 경우 true
    bool isRemovable; // 다른 기술에 의해 해제 가능할 경우 true
    
    float degree; // 영향을 주는 수치(상대수치)
    int amount; // 영향을 주는 수치(절대수치)
    int remainPhase; // 지속 단위가 페이즈 단위인 경우 사용
    int remainStack; // 지속 단위가 적용 횟수 단위인 경우 사용
    int cooldown; // 효과가 적용되는 시점
	
	// 이펙트 관련 정보
	string effectName;
	EffectVisualType effectVisualType;
	EffectMoveType effectMoveType;
	
	public StatusEffect(string name, StatusEffectType statusEffectType,  
                  bool isBuff, bool isInfinite, bool isStackable, bool isRemovable, 
                  float degree, int amount, int remainPhase, int remainStack, int cooldown,
                  string effectName, EffectVisualType effectVisualType, EffectMoveType effectMoveType)
	{
		this.name = name;
		this.statusEffectType = statusEffectType;
        this.isBuff = isBuff;
        this.isInfinite = isInfinite;
        this.isStackable = isStackable;
        this.isRemovable = isRemovable;
        this.degree = degree;
        this.amount = amount;
        this.remainPhase = remainPhase;
        this.remainStack = remainStack;
        this.cooldown = cooldown;
        this.effectName = effectName;
		this.effectVisualType = effectVisualType;
		this.effectMoveType = effectMoveType;
    }
	
	public string GetName() {return name;}
    public StatusEffectType GetStatusEffectType() {return statusEffectType;}
    public bool GetIsBuff() {return isBuff;}
    public bool GetIsInfinite() {return isInfinite;}
    public bool GetIsStackable() {return isStackable;}
    public bool GetIsRemovable() {return isRemovable;}
    public float GetDegree() {return degree;}
    public int GetAmount() {return amount;}
    public int GetRemainPhase() {return remainPhase;}
    public int GetRemainStack() {return remainStack;}
	public int GetCooldown() {return cooldown;}	
	public string GetEffectName() {return effectName;}
	public EffectVisualType GetEffectVisualType() {return effectVisualType;}
	public EffectMoveType GetEffectMoveType() {return effectMoveType;}
    
    public void SetRemainAmount(int remainAmount)
    {
        amount = remainAmount;
    }

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
        if(this.name.Equals(statusEffect.GetName()))
        {
            if(this.IsOfType(statusEffect.GetStatusEffectType()))
            {
                isSameStatusEffect = true;
            }
        }
        
        return isSameStatusEffect;
    }
}
