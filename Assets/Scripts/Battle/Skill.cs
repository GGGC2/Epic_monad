using Enums;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Skill {

	// base info.
	string name;
	int level;
	int[] requireAPArray;
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
    
    // 상태이상 관련 정보
    List<StatusEffect> statusEffectList = new List<StatusEffect>();
    
	public Skill(string name, int level, int[] requireAPArray, int[] cooldownArray, 
                 Dictionary<string, float[]> powerFactor,
				 SkillType skillType,
				 RangeForm firstRangeForm, int firstMinReach, int firstMaxReach, int firstWidth,
				 RangeForm secondRangeForm, int secondMinReach, int secondMaxReach, int secondWidth,
				 SkillApplyType skillApplyType, float[] penetrationArray, 
				 string effectName, EffectVisualType effectVisualType, EffectMoveType effectMoveType)
	{
		this.name = name;
		this.level = level;
		this.requireAPArray = requireAPArray;
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
	}
      
    public void ApplyStatusEffectList(List<StatusEffectInfo> statusEffectInfoList)
    {
        foreach (var statusEffectInfo in statusEffectInfoList)
        {
            StatusEffect statusEffect = statusEffectInfo.GetStatusEffect();
			statusEffect.SetLevel(level);
            if(statusEffectInfo.GetSkillName().Equals(this.name))
            {
                statusEffectList.Add(statusEffectInfo.GetStatusEffect());
            }
        }
    }
	
	public string GetName() {return name;}
	public int GetLevel() {return level;}
	public int[] GetRequireAP() {return requireAPArray;}
	public int GetRequireAP(int level) {return requireAPArray[level-1];}
	public int[] GetCooldown() {return cooldownArray;}	
	public int GetCooldown(int level) {return cooldownArray[level-1];}
    public Dictionary<string, float[]> GetPowerFactorDict() {return powerFactor;}
	public float[] GetPowerFactor(Stat status) {return powerFactor[status.ToString()];} 
	public float GetPowerFactor(Stat status, int level) {return powerFactor[status.ToString()][level-1];}
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
	public float[] GetPenetration() {return penetrationArray;}
	public float GetPenetration(int level) {return penetrationArray[level-1];}
	public string GetEffectName() {return effectName;}
	public EffectVisualType GetEffectVisualType() {return effectVisualType;}
	public EffectMoveType GetEffectMoveType() {return effectMoveType;}
    public List<StatusEffect> GetStatusEffectList() {return statusEffectList;}

	public void SetLevel(int num) {level = num;}
}
