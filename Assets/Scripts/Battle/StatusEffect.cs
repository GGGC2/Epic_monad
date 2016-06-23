using Enums;
using System.Collections;
using UnityEngine;

public class StatusEffect {

	string name; // 유저에게 보일 이름
    StatusEffectType statusEffectType; // 시스템 상으로 구분하는 상태이상의 종류
    
    float degree; // 영향을 주는 수치(상대수치)
    int amount; // 영향을 주는 수치(절대수치)
    int remainPhase;
    int cooldown; // 효과가 적용되는 시점
	
	// 이펙트 관련 정보
	string effectName;
	EffectVisualType effectVisualType;
	EffectMoveType effectMoveType;
	
	public StatusEffect(string name, StatusEffectType statusEffectType, 
                  float degree, int amount, int remainPhase, int cooldown,
                  string effectName, EffectVisualType effectVisualType, EffectMoveType effectMoveType)
	{
		this.name = name;
		this.statusEffectType = statusEffectType;
        this.degree = degree;
        this.amount = amount;
        this.remainPhase = remainPhase;
        this.cooldown = cooldown;
        this.effectName = effectName;
		this.effectVisualType = effectVisualType;
		this.effectMoveType = effectMoveType;
    }
	
	public string GetName() {return name;}
    public StatusEffectType GetStatusEffectType() {return statusEffectType;}
    public float GetDegree() {return degree;}
    public int GetAmount() {return amount;}
    public int GetRemainPhase() {return remainPhase;}
	public int GetCooldown() {return cooldown;}	
	public string GetEffectName() {return effectName;}
	public EffectVisualType GetEffectVisualType() {return effectVisualType;}
	public EffectMoveType GetEffectMoveType() {return effectMoveType;}
    
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
}
