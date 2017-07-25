using Enums;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Skill{

	// base info.
	string owner;
	int column;
	string name;
	int requireLevel;
	int requireAP;
	int cooldown;
	
	// damage factors in datatype Dictionary
	float powerFactor;
	
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
	
	// 이펙트 관련 정보
	string effectName;
	EffectVisualType effectVisualType;
	EffectMoveType effectMoveType;

	// 스킬 SE 이름(그냥 그레네브 총질에만 한번 넣어봄)
	string soundEffectName;
	
	// 스킬 설명 텍스트
	string skillDataText;
    
    // 상태이상 관련 정보
    List<StatusEffect.FixedElement> statusEffectList = new List<StatusEffect.FixedElement>();
    List<TileStatusEffect.FixedElement> tileStatusEffectList = new List<TileStatusEffect.FixedElement>();
    
	public Skill(string owner, int column, string name, int requireLevel, int requireAP, int cooldown, 
                 float powerFactor,
				 SkillType skillType,
				 RangeForm firstRangeForm, int firstMinReach, int firstMaxReach, int firstWidth,
				 RangeForm secondRangeForm, int secondMinReach, int secondMaxReach, int secondWidth,
				 SkillApplyType skillApplyType,  
		string effectName, EffectVisualType effectVisualType, EffectMoveType effectMoveType, string soundEffectName,
				 string skillDataText)
	{
		this.owner = owner;
		this.column = column;
		this.name = name;
        this.requireLevel = requireLevel;
		this.requireAP = requireAP;
		this.cooldown = cooldown;
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
		this.effectName = effectName;
		this.effectVisualType = effectVisualType;
		this.effectMoveType = effectMoveType;
		this.soundEffectName = soundEffectName;
		this.skillDataText = skillDataText;
	}
      
    public void ApplyStatusEffectList(List<StatusEffectInfo> statusEffectInfoList, int partyLevel)
    {
        StatusEffect.FixedElement previousStatusEffect = null;
        foreach (var statusEffectInfo in statusEffectInfoList) {
            StatusEffect.FixedElement statusEffectToAdd = statusEffectInfo.GetStatusEffect();
            if(statusEffectInfo.GetRequireLevel() <= partyLevel) {
                if (previousStatusEffect != null && previousStatusEffect.display.toBeReplaced) { //이전의 previousStatusEffect에 대해서만 대체 여부를 확인함.
                                                                                                 //즉, 대체되어야 하는 StatusEffect는 csv 파일에서 바로 다음 줄에 만들어야 함.
                    statusEffectList.Remove(previousStatusEffect);
                }
                if(statusEffectInfo.GetOriginSkillName().Equals(name)) {
                    statusEffectList.Add(statusEffectToAdd);
                }
            }
            previousStatusEffect = statusEffectToAdd;
        }
    }
    public void ApplyTileStatusEffectList(List<TileStatusEffectInfo> statusEffectInfoList, int partyLevel) {
        TileStatusEffect.FixedElement previousStatusEffect = null;
        foreach (var statusEffectInfo in statusEffectInfoList) {
            TileStatusEffect.FixedElement statusEffectToAdd = statusEffectInfo.GetStatusEffect();
            if (statusEffectInfo.GetRequireLevel() <= partyLevel) {
                if (previousStatusEffect != null && previousStatusEffect.display.toBeReplaced) { //이전의 previousStatusEffect에 대해서만 대체 여부를 확인함.
                                                                                                 //즉, 대체되어야 하는 StatusEffect는 csv 파일에서 바로 다음 줄에 만들어야 함.
                    tileStatusEffectList.Remove(previousStatusEffect);
                }
                if (statusEffectInfo.GetOriginSkillName().Equals(name)) {
                    tileStatusEffectList.Add(statusEffectToAdd);
                }
            }
            previousStatusEffect = statusEffectToAdd;
        }
    }
    public string GetOwner(){return owner;}
	public int GetColumn() { return column; }
	public string GetName() {return name;}
    public int GetLequireLevel() { return requireLevel;}
    public void SetRequireAP(int requireAP) { this.requireAP = requireAP;}
	public int GetRequireAP() {return requireAP;}
	public int GetCooldown() {return cooldown;}
	public float GetPowerFactor(Stat status) {return powerFactor;} 
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
	public string GetEffectName() {return effectName;}
	public EffectVisualType GetEffectVisualType() {return effectVisualType;}
	public EffectMoveType GetEffectMoveType() {return effectMoveType;}
	public string GetSoundEffectName() {return soundEffectName;}
	public void SetSoundEffectName(string soundEffectName) { this.soundEffectName = soundEffectName; }
	public string GetSkillDataText() {return skillDataText;}
    public List<StatusEffect.FixedElement> GetStatusEffectList() {return statusEffectList;}
    public List<TileStatusEffect.FixedElement> GetTileStatusEffectList() { return tileStatusEffectList; }
}
