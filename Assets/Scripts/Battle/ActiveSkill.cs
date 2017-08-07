using Enums;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class ActiveSkill : Skill{
	// base info.
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

	// 스킬 SE 이름
	string soundEffectName;
    
    // 상태이상 관련 정보
    List<StatusEffect.FixedElement> statusEffectList = new List<StatusEffect.FixedElement>();
    List<TileStatusEffect.FixedElement> tileStatusEffectList = new List<TileStatusEffect.FixedElement>();

	public ActiveSkill(string skillData){
		StringParser commaParser = new StringParser(skillData, ',');
		
		GetCommonSkillData(commaParser);
		requireAP = commaParser.ConsumeInt();
		cooldown = commaParser.ConsumeInt();

		string statType = commaParser.Consume();
		powerFactor = commaParser.ConsumeFloat();
		
		skillType = commaParser.ConsumeEnum<SkillType>();

		firstRangeForm = commaParser.ConsumeEnum<RangeForm>();
		firstMinReach = commaParser.ConsumeInt();
		firstMaxReach = commaParser.ConsumeInt();
		firstWidth = commaParser.ConsumeInt();
		
		secondRangeForm = commaParser.ConsumeEnum<RangeForm>();
		secondMinReach = commaParser.ConsumeInt();
		secondMaxReach = commaParser.ConsumeInt();
		secondWidth = commaParser.ConsumeInt();

		skillApplyType = commaParser.ConsumeEnum<SkillApplyType>();
		effectName = commaParser.Consume();
		effectVisualType = commaParser.ConsumeEnum<EffectVisualType>();
		effectMoveType = commaParser.ConsumeEnum<EffectMoveType>();

		soundEffectName = commaParser.Consume ();

		skillDataText = commaParser.Consume();
		firstTextValueType = commaParser.ConsumeEnum<Stat>();
		firstTextValueCoef = commaParser.ConsumeFloat();
        firstTextValueBase = commaParser.ConsumeFloat();
		secondTextValueType = commaParser.ConsumeEnum<Stat>();
		secondTextValueCoef = commaParser.ConsumeFloat();
        secondTextValueBase = commaParser.ConsumeFloat();
	}
    
	/*public ActiveSkill(string owner, int column, string name, int requireLevel, int requireAP, int cooldown, 
                 float powerFactor,
				 SkillType skillType,
				 RangeForm firstRangeForm, int firstMinReach, int firstMaxReach, int firstWidth,
				 RangeForm secondRangeForm, int secondMinReach, int secondMaxReach, int secondWidth,
				 SkillApplyType skillApplyType,  
				 string effectName, EffectVisualType effectVisualType, EffectMoveType effectMoveType, string soundEffectName,
				 string skillDataText, Stat firstTextValueType, float firstTextValueCoef, Stat secondTextValueType, float secondTextValueCoef)
	{
		this.owner = owner;
		this.column = column;
		this.korName = name;
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
		this.firstTextValueType = firstTextValueType;
		this.firstTextValueCoef = firstTextValueCoef;
		this.secondTextValueType = secondTextValueType;
		this.secondTextValueCoef = secondTextValueCoef;
	}*/
	public List<Tile> GetTilesInFirstRange(Vector2 casterPos, Direction direction) {
		var firstRange = battleData.tileManager.GetTilesInRange (GetFirstRangeForm (),
			                 casterPos,
			                 GetFirstMinReach (),
			                 GetFirstMaxReach (),
			                 GetFirstWidth (),
			                 direction);

		//투사체 스킬이면 직선경로상에서 유닛이 가로막은 지점까지를 1차 범위로 함. 범위 끝까지 가로막은 유닛이 없으면 직선 전체가 1차 범위
		if (GetSkillType() == SkillType.Route) {
			firstRange = TileManager.GetRouteTiles(firstRange);
		}

		return firstRange;
	}
	public Tile GetRealTargetTileForAI(Vector2 casterPos, Direction direction, Tile targetTile=null){	
		if (GetSkillType () == SkillType.Route) {
			List<Tile> firstRange = GetTilesInFirstRange (casterPos, direction);
			Tile routeEnd = TileManager.GetRouteEndForAI (firstRange);
			return routeEnd;
		}
		return targetTile;
	}
	public Tile GetRealTargetTileForPC(Vector2 casterPos, Direction direction, Tile targetTile=null){	
		if (GetSkillType () == SkillType.Route) {
			List<Tile> firstRange = GetTilesInFirstRange (casterPos, direction);
			Tile routeEnd = TileManager.GetRouteEndForPC (firstRange);
			return routeEnd;
		}
		return targetTile;
	}
	public List<Tile> GetTilesInSecondRange(Tile targetTile, Direction direction)
	{
		List<Tile> secondRange = battleData.tileManager.GetTilesInRange (GetSecondRangeForm (),
			                           targetTile.GetTilePos (),
			                           GetSecondMinReach (),
			                           GetSecondMaxReach (),
			                           GetSecondWidth (),
			                           direction);
		if (GetSkillType() == SkillType.Auto)
		{
			secondRange.Remove(targetTile);
		}
		return secondRange;
	}
	public List<Tile> GetTilesInRealEffectRange(Tile targetTile, List<Tile> secondRange){
		List<Tile> tilesInRealEffectRange =secondRange;
		//투사체 스킬은 타겟 타일에 유닛이 없으면 아무 효과도 데미지도 없이 이펙트만 나오게 한다. 연계 발동은 안 되고 연계 대기는 된다
		if (battleData.SelectedSkill.GetSkillType() == SkillType.Route) {
			if (!targetTile.IsUnitOnTile ())
				tilesInRealEffectRange = new List<Tile>();
		}
		return tilesInRealEffectRange;
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
                if(statusEffectInfo.GetOriginSkillName().Equals(korName)) {
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
                if (statusEffectInfo.GetOriginSkillName().Equals(korName)) {
                    tileStatusEffectList.Add(statusEffectToAdd);
                }
            }
            previousStatusEffect = statusEffectToAdd;
        }
    }

	private static BattleData battleData;
	public static void SetBattleData(BattleData battleDataInstance){
		battleData=battleDataInstance;
	}

    public string GetOwner(){return owner;}
	public int GetColumn() { return column; }
	public string GetName() {return korName;}
    public int GetRequireLevel() { return requireLevel;}
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