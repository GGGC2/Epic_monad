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
	string visualEffectName;
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
		visualEffectName = commaParser.Consume();
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
				 string visualEffectName, EffectVisualType effectVisualType, EffectMoveType effectMoveType, string soundEffectName,
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
		this.effectName = visualEffectName;
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
		var firstRange = battleData.tileManager.GetTilesInRange (firstRangeForm,
			                 casterPos,
			                 firstMinReach,
			                 firstMaxReach,
			                 firstWidth,
			                 direction);

		//투사체 스킬이면 직선경로상에서 유닛이 가로막은 지점까지를 1차 범위로 함. 범위 끝까지 가로막은 유닛이 없으면 직선 전체가 1차 범위
		if (skillType == SkillType.Route) {
			firstRange = TileManager.GetRouteTiles(firstRange);
		}

		return firstRange;
	}
	public Tile GetRealTargetTileForAI(Vector2 casterPos, Direction direction, Tile targetTile=null){	
		if (skillType == SkillType.Route) {
			List<Tile> firstRange = GetTilesInFirstRange (casterPos, direction);
			Tile routeEnd = TileManager.GetRouteEndForAI (firstRange);
			return routeEnd;
		}
		return targetTile;
	}
	public Tile GetRealTargetTileForPC(Vector2 casterPos, Direction direction, Tile targetTile=null){	
		if (skillType == SkillType.Route) {
			List<Tile> firstRange = GetTilesInFirstRange (casterPos, direction);
			Tile routeEnd = TileManager.GetRouteEndForPC (firstRange);
			return routeEnd;
		}
		return targetTile;
	}
	public List<Tile> GetTilesInSecondRange(Tile targetTile, Direction direction)
	{
		List<Tile> secondRange = battleData.tileManager.GetTilesInRange (secondRangeForm,
			                           targetTile.GetTilePos (),
			                           secondMinReach,
			                           secondMaxReach,
			                           secondWidth,
			                           direction);
		if (skillType == SkillType.Auto)
		{
			secondRange.Remove(targetTile);
		}
		return secondRange;
	}
	public List<Tile> GetTilesInRealEffectRange(Tile targetTile, Direction direction){
		List<Tile> secondRange = GetTilesInSecondRange (targetTile, direction);
		List<Tile> realEffectRange = secondRange;
		if (battleData.SelectedSkill.skillType == SkillType.Route) {
			if (!targetTile.IsUnitOnTile ())
				realEffectRange = new List<Tile>();
		}
		return realEffectRange;
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

	public void ApplySoundEffect(){
		if(soundEffectName != null || soundEffectName != "-")
			SoundManager.Instance.PlaySE (soundEffectName);
	}
	public IEnumerator ApplyVisualEffect(Unit unit, List<Tile> secondRange) {
		if (visualEffectName == "-") {
			Debug.Log("There is no visual effect for " + korName);
			yield break;
		}

		if ((effectVisualType == EffectVisualType.Area) && (effectMoveType == EffectMoveType.Move)) {
			// 투사체, 범위형 이펙트.
			Vector3 startPos = unit.gameObject.transform.position;
			Vector3 endPos = new Vector3(0, 0, 0);
			foreach (var tile in secondRange) {
				endPos += tile.gameObject.transform.position;
			}
			endPos = endPos / (float)secondRange.Count;

			GameObject particle = GameObject.Instantiate(Resources.Load("Particle/" + visualEffectName)) as GameObject;
			particle.transform.position = startPos - new Vector3(0, -0.5f, 0.01f);
			yield return new WaitForSeconds(0.2f);
			// 타일 축 -> 유닛 축으로 옮기기 위해 z축으로 5만큼 앞으로 빼준다.
			// 유닛의 중앙 부분을 공격하기 위하여 y축으고 0.5 올린다.
			iTween.MoveTo(particle, endPos - new Vector3(0, 0, 0.01f) - new Vector3(0, -0.5f, 5f), 0.5f);
			yield return new WaitForSeconds(0.3f);
			GameObject.Destroy(particle, 0.5f);
			yield return null;
		} else if ((effectVisualType == EffectVisualType.Area) && (effectMoveType == EffectMoveType.NonMove)) {
			// 고정형, 범위형 이펙트.
			Vector3 targetPos = new Vector3(0, 0, 0);
			foreach (var tile in secondRange) {
				targetPos += tile.transform.position;
			}
			targetPos = targetPos / (float)secondRange.Count;
			targetPos = targetPos - new Vector3(0, -0.5f, 5f); // 타일 축 -> 유닛 축으로 옮기기 위해 z축으로 5만큼 앞으로 빼준다.

			GameObject particlePrefab = Resources.Load("Particle/" + visualEffectName) as GameObject;
			if (particlePrefab == null) {
				Debug.LogError("Cannot load particle " + visualEffectName);
			}
			GameObject particle = GameObject.Instantiate(particlePrefab) as GameObject;
			particle.transform.position = targetPos - new Vector3(0, -0.5f, 0.01f);
			yield return new WaitForSeconds(0.5f);
			GameObject.Destroy(particle, 0.5f);
			yield return null;
		} else if ((effectVisualType == EffectVisualType.Individual) && (effectMoveType == EffectMoveType.NonMove)) {
			// 고정형, 개별 대상 이펙트.
			List<Vector3> targetPosList = new List<Vector3>();
			foreach (var tileObject in secondRange) {
				Tile tile = tileObject;
				if (tile.IsUnitOnTile()) {
					targetPosList.Add(tile.GetUnitOnTile().transform.position);
				}
			}

			foreach (var targetPos in targetPosList) {
				GameObject particle = GameObject.Instantiate(Resources.Load("Particle/" + visualEffectName)) as GameObject;
				particle.transform.position = targetPos - new Vector3(0, -0.5f, 0.01f);
				GameObject.Destroy(particle, 0.5f + 0.3f); // 아랫줄에서의 지연시간을 고려한 값이어야 함.
			}

			if (targetPosList.Count == 0) // 대상이 없을 경우. 일단 가운데 이펙트를 띄운다.
			{
				Vector3 midPos = new Vector3(0, 0, 0);
				foreach (var tile in secondRange) {
					midPos += tile.transform.position;
				}
				midPos = midPos / (float)secondRange.Count;

				GameObject particle = GameObject.Instantiate(Resources.Load("Particle/" + visualEffectName)) as GameObject;
				particle.transform.position = midPos - new Vector3(0, -0.5f, 0.01f);
				GameObject.Destroy(particle, 0.5f + 0.3f); // 아랫줄에서의 지연시간을 고려한 값이어야 함.
			}
			yield return new WaitForSeconds(0.5f);
		}
	}

	public bool IsChainable(){
		return skillApplyType == SkillApplyType.DamageHealth
			|| skillApplyType == SkillApplyType.Debuff;
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
	public string GetSkillDataText() {return skillDataText;}
    public List<StatusEffect.FixedElement> GetStatusEffectList() {return statusEffectList;}
    public List<TileStatusEffect.FixedElement> GetTileStatusEffectList() { return tileStatusEffectList; }
}