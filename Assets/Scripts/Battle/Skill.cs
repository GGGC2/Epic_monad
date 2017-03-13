using Enums;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Skill {

	// base info.
	string owner;
	int column;
	string name;
	int level;
	int requireAP;
	int[] cooldownArray;
	
	// damage factors in datatype Dictionary
	Dictionary<string, float[]> powerFactor = new Dictionary<string, float[]>();
	
	// reach & range
	// 지정/범위/경로. 
	SkillType skillType;
	// 1차범위.
	RangeForm firstRangeForm;
	int firstMinReach;
	int firstMaxReach;
	int firstWidth;
	// 2차범위.   ** 범위형의 경우 반드시 1차범위 = 2차범위! **
	RangeForm secondRangeForm;
	int secondMinReach;
	int secondMaxReach;
	int secondWidth;
	
	SkillApplyType skillApplyType; // 대미지인지 힐인지 아니면 상태이상만 주는지
	float[] penetrationArray; // 관통률. 고정대미지의 경우 1.0f
	
	// 이펙트 관련 정보
	string effectName;
	EffectVisualType effectVisualType;
	EffectMoveType effectMoveType;
	
	// 스킬 설명 텍스트
	string skillDataText;
    
    // 상태이상 관련 정보
    List<StatusEffect.FixedElement> statusEffectList = new List<StatusEffect.FixedElement>();
    
	public Skill(string owner, int column, string name, int requireAP, int[] cooldownArray, 
                 Dictionary<string, float[]> powerFactor,
				 SkillType skillType,
				 RangeForm firstRangeForm, int firstMinReach, int firstMaxReach, int firstWidth,
				 RangeForm secondRangeForm, int secondMinReach, int secondMaxReach, int secondWidth,
				 SkillApplyType skillApplyType, float[] penetrationArray, 
				 string effectName, EffectVisualType effectVisualType, EffectMoveType effectMoveType,
				 string skillDataText)
	{
		this.owner = owner;
		this.column = column;
		this.name = name;
		this.requireAP = requireAP;
		this.cooldownArray = cooldownArray;
		this.powerFactor = powerFactor;
		this.skillType = skillType;
		this.firstRangeForm = firstRangeForm;
		this.firstMinReach = firstMinReach;
		this.firstMaxReach = firstMaxReach;
		this.firstWidth = firstWidth;
		this.secondRangeForm = secondRangeForm;
		this.secondMinReach = secondMinReach;
		this.secondMaxReach = secondMaxReach;
		this.secondWidth = secondWidth;
		this.skillApplyType = skillApplyType;
		this.penetrationArray = penetrationArray;
		this.effectName = effectName;
		this.effectVisualType = effectVisualType;
		this.effectMoveType = effectMoveType;
		this.skillDataText = skillDataText;
	}
      
    public void ApplyStatusEffectList(List<StatusEffectInfo> statusEffectInfoList)
    {
        foreach (var statusEffectInfo in statusEffectInfoList)
        {
            StatusEffect.FixedElement statusEffect = statusEffectInfo.GetStatusEffect();
            if(statusEffectInfo.GetOriginSkillName().Equals(this.name))
            {
                statusEffectList.Add(statusEffectInfo.GetStatusEffect());
            }
        }
    }

	public string GetOwner(){return owner;}
	public int GetColumn() { return column; }
	public string GetName() {return name;}
	public int GetRequireAP() {return requireAP;}
	public int GetCooldown() {return cooldownArray[0];}
    public Dictionary<string, float[]> GetPowerFactorDict() {return powerFactor;}
	public float GetPowerFactor(Stat status) {return powerFactor[status.ToString()][0];} 
	public SkillType GetSkillType() {return skillType;}
	public RangeForm GetFirstRangeForm() {return firstRangeForm;}
	public int GetFirstMinReach() {return firstMinReach;}
	public int GetFirstMaxReach() {return firstMaxReach;}
	public int GetFirstWidth() {return firstWidth;}
	public RangeForm GetSecondRangeForm() {return secondRangeForm;}
	public int GetSecondMinReach() {return secondMinReach;}
	public int GetSecondMaxReach() {return secondMaxReach;}
	public int GetSecondWidth() {return secondWidth;}
	public SkillApplyType GetSkillApplyType() {return skillApplyType;}
	public float GetPenetration() {return penetrationArray[0];}
	public string GetEffectName() {return effectName;}
	public EffectVisualType GetEffectVisualType() {return effectVisualType;}
	public EffectMoveType GetEffectMoveType() {return effectMoveType;}
	public string GetSkillDataText() {return skillDataText;}
    public List<StatusEffect.FixedElement> GetStatusEffectList() {return statusEffectList;}
}
