﻿using Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Battle;
using Battle.Skills;
using Battle.Damage;
using UnityEngine;

public class ActiveSkill : Skill{	
	int requireAP;
	int cooldown;
	
	// (보통)공격력에 곱해서 기술 위력을 결정하는 계수
	float powerFactor;
	
	// reach & range
	SkillType skillType;
	// 1차범위
	RangeForm firstRangeForm;
	int firstMinReach;
	int firstMaxReach;
	int firstWidth;
	// 2차범위   ** 자동형의 경우 반드시 1차범위 = 2차범위! **
	RangeForm secondRangeForm;
	int secondMinReach;
	int secondMaxReach;
	int secondWidth;
	
	SkillApplyType skillApplyType; // DamageHealth,DamageAP,HealHealth,HealAP,Buff,Debuff,Move,Tile,Etc
	
	// 시각적 이펙트
	public string visualEffectName;
	public EffectVisualType effectVisualType;
	public EffectMoveType effectMoveType;
	public bool effectDirectionality;

	// 효과음
	public string soundEffectName;
    
    // 상태이상 관련 정보
    List<UnitStatusEffect.FixedElement> unitStatusEffectList = new List<UnitStatusEffect.FixedElement>();
    List<TileStatusEffect.FixedElement> tileStatusEffectList = new List<TileStatusEffect.FixedElement>();

	public BaseSkillLogic SkillLogic {
		get { return SkillLogicFactory.Get (this); }
	}

	public ActiveSkill(string skillData){
		StringParser commaParser = new StringParser(skillData, '\t');
		
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
		effectDirectionality = commaParser.ConsumeBool();

		soundEffectName = commaParser.Consume ();

		skillDataText = commaParser.Consume();
		firstTextValueType = commaParser.ConsumeEnum<Stat>();
		firstTextValueCoef = commaParser.ConsumeFloat();
        firstTextValueBase = commaParser.ConsumeFloat();
		secondTextValueType = commaParser.ConsumeEnum<Stat>();
		secondTextValueCoef = commaParser.ConsumeFloat();
        secondTextValueBase = commaParser.ConsumeFloat();
	}

	public List<Tile> GetTilesInFirstRange(Vector2 casterPos, Direction direction) {
		var firstRange = BattleData.tileManager.GetTilesInRange (firstRangeForm,
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
	public List<Tile> GetTilesInFirstRange(SkillLocation location) {
		return GetTilesInFirstRange (location.CasterPos, location.Direction);
	}
	public Tile GetRealTargetTileForAI(Vector2 casterPos, Direction direction, Tile targetTile=null){	
		if (skillType == SkillType.Route) {
			List<Tile> firstRange = GetTilesInFirstRange (casterPos, direction);
			Tile routeEnd = TileManager.GetRouteEndForAI (firstRange);
			return routeEnd;
		}
		return targetTile;
	}
	public Tile GetRealTargetTileForPC(SkillLocation skillLocation){
		if (skillType == SkillType.Route) {
			List<Tile> firstRange = GetTilesInFirstRange (skillLocation.CasterPos, skillLocation.Direction);
			Tile routeEnd = TileManager.GetRouteEndForPC (firstRange);
			return routeEnd;
		}
		return skillLocation.TargetTile;
	}
	public void SetRealTargetTileForSkillLocation(SkillLocation skillLocation){
		if (skillType == SkillType.Route) {
			List<Tile> firstRange = GetTilesInFirstRange (skillLocation.CasterPos, skillLocation.Direction);
			Tile routeEnd = TileManager.GetRouteEndForPC (firstRange);
			skillLocation.SetTargetTile (routeEnd);
		}
	}
	public List<Tile> GetTilesInSecondRange(SkillLocation skillLocation)
	{
		List<Tile> secondRange = new List<Tile>();
		if (skillLocation.TargetTile != null) {
            secondRange = BattleData.tileManager.GetTilesInRange(secondRangeForm,
                skillLocation.TargetPos,
                secondMinReach,
                secondMaxReach,
                secondWidth,
                skillLocation.Direction);
            if (skillType == SkillType.Auto) {
                secondRange.Remove(skillLocation.TargetTile);
            }
		}
		return secondRange;
	}
	public List<Tile> GetTilesInRealEffectRange(SkillLocation skillLocation){
		if (skillType == SkillType.Route && (skillLocation.TargetTile == null || !skillLocation.TargetTile.IsUnitOnTile ()))
			return new List<Tile> ();
		else
			return GetTilesInSecondRange (skillLocation);
	}
	public Dictionary<Vector2, Color> RangeColorsForSecondRangeDisplay(int rowNum){
		Vector2 center = new Vector2 ((rowNum - 1) / 2, (rowNum - 1) / 2);
		Dictionary<Vector2, Color> rangeColors = new Dictionary<Vector2, Color> ();

		for (int x = 0; x < rowNum; x++)
			for (int y = 0; y < rowNum; y++)
				rangeColors [new Vector2 (x, y)] = Color.white;
		
		List<Vector2> redRange = new List<Vector2> ();
		if (secondRangeForm == RangeForm.Global){
			for (int x = 0; x < rowNum; x++)
				for (int y = 0; y < rowNum; y++)
					redRange.Add (new Vector2 (x, y));
		}
		else
			redRange = Utility.GetRange (secondRangeForm, center, secondMinReach, secondMaxReach, secondWidth, Direction.RightUp);

		foreach (var pos in redRange)
			rangeColors [pos] = Color.red;

		/*
		if (skillType == SkillType.Auto || skillType == SkillType.Self)
			rangeColors [center] = Color.black;
		if (skillType == SkillType.Point || skillType == SkillType.Route)
			rangeColors [center] = new Color (1, 0.6f, 0);
			*/
		
		return rangeColors;
	}

	public Casting GetBestCasting(Unit caster, Tile casterTile){
		if (!caster.IsThisSkillUsable (this)) {
			return null;
		}

		// just default values
		Tile targetTile = casterTile;
		Direction direction = caster.GetDirection ();

		Casting bestCasting = null;
		float maxReward = 0;

		if (skillType == SkillType.Point) {
			Dictionary<Tile, List<Tile>> attackableTiles = GetAttackableTilesOfPointSkill (caster, casterTile);
			foreach(var pair in attackableTiles){
				targetTile = pair.Key;
				direction = Utility.GetDirectionToTarget (caster, targetTile.GetTilePos ());
				SkillLocation location = new SkillLocation (casterTile, targetTile, direction);
				Casting casting = new Casting (caster, this, location);

				float reward = GetRewardByCasting (casting);
				if (reward > maxReward) {
					bestCasting = casting;
					maxReward = reward;
				}
			}
		}
		else{
			Dictionary<Direction, List<Tile>> attackableTiles = GetAttackableTilesOfDirectionSkill (caster, casterTile);
			foreach(var pair in attackableTiles){
				if (skillType == SkillType.Route)
					targetTile = GetRealTargetTileForAI (casterTile.GetTilePos (), direction, targetTile);
				direction = pair.Key;
				SkillLocation location = new SkillLocation (casterTile, targetTile, direction);
				Casting casting = new Casting (caster, this, location);

				float reward = GetRewardByCasting (casting);
				if (reward > maxReward) {
					bestCasting = casting;
					maxReward = reward;
				}
				else if (reward == maxReward && direction == caster.GetDirection()) {
					// 자동형 스킬 중 4방향으로 범위가 똑같은 애들을 쓸 때는 괜히 방향 바꾸지 않도록 함
					bestCasting = casting;
				}
			}
		}
		return bestCasting;
	}
	public float GetRewardByCasting(Casting casting){
		Chain chain = new Chain (casting);

		float totalReward = 0;
		foreach (Unit target in chain.CurrentTargets) {
			totalReward += target.CalculateRewardByCastingToThisUnit (casting);
		}
		return totalReward;
	}

	private Dictionary<Direction, List<Tile>> GetAttackableTilesOfDirectionSkill(Unit caster, Tile casterTile){
		Dictionary<Direction, List<Tile>> attackAbleTilesGroups = new Dictionary<Direction, List<Tile>> ();

		//투사체 스킬이면 4방향 직선경로상에서 유닛이 가로막은 지점들을 targetTiles로 함.
		//그리고 각 targetTile에서 2차범위 적용해서 각 범위를 attackAbleTilesGroups에 넣는다
		if (skillType == SkillType.Route) {
			Dictionary<Direction, Tile> targetTiles = new Dictionary<Direction, Tile> ();
			Vector2 casterPos = casterTile.GetTilePos ();

			foreach (Direction direction in EnumUtil.directions) {
				Tile routeEnd = GetRealTargetTileForAI (casterPos, direction);
				if (routeEnd != null)
					targetTiles [direction] = routeEnd;
			}

			foreach (var pair in targetTiles) {
				Direction direction = pair.Key;
				Tile targetTile = pair.Value;
				SkillLocation skillLocation = new SkillLocation (casterTile, targetTile, direction);
				attackAbleTilesGroups [direction] = GetTilesInSecondRange (skillLocation);
			}
		}
		//Auto 스킬은 시전자가 위치한 타일이 유일한 targetTile이며 네 방향의 범위를 각각 attackAbleTilesGroups에 넣는다
		else if (skillType == SkillType.Auto) {
			Tile targetTile = casterTile;

			foreach (Direction direction in EnumUtil.directions) {
				SkillLocation skillLocation = new SkillLocation (casterTile, targetTile, direction);
				attackAbleTilesGroups [direction] = GetTilesInSecondRange (skillLocation);
			}
		}
		//skillType==SkillType.Self
		//Self 스킬은 방향을 바꾸지 말고 써야 해서 시전자의 방향을 그대로 받아옴
		else {
			Tile targetTile = casterTile;
			Direction direction = caster.GetDirection ();
			SkillLocation skillLocation = new SkillLocation (casterTile, targetTile, direction);
			attackAbleTilesGroups [direction] = GetTilesInSecondRange (skillLocation);
		}

		/*
		Dictionary<Direction, List<Tile>> attackAbleEnemyTilesGroups = new Dictionary<Direction, List<Tile>> ();
		foreach (var pair in attackAbleTilesGroups) {
			Direction direction = pair.Key;
			List<Tile> tilesGroup = pair.Value;
			List<Tile> enemyTilesGroup = Battle.Turn.AIUtil.FindEnemyTilesInTheseTiles (tilesGroup, caster);
			if(enemyTilesGroup.Count != 0)
				attackAbleEnemyTilesGroups [direction] = enemyTilesGroup;
		}*/
		return attackAbleTilesGroups;
	}
	private Dictionary<Tile, List<Tile>> GetAttackableTilesOfPointSkill(Unit caster, Tile casterTile){
		Dictionary<Tile, List<Tile>> attackAbleTilesGroups = new Dictionary<Tile, List<Tile>> ();
		Vector2 casterPos = casterTile.GetTilePos ();
		List<Tile> targetTiles = GetTilesInFirstRange (casterPos, caster.GetDirection ());
		foreach (Tile targetTile in targetTiles) {
			SkillLocation skillLocation = new SkillLocation (casterPos, targetTile, caster.GetDirection ());
			attackAbleTilesGroups [targetTile] = GetTilesInSecondRange (skillLocation);
		}

		/*
		Dictionary<Tile, List<Tile>> attackAbleEnemyTilesGroups = new Dictionary<Tile, List<Tile>> ();
		foreach (var pair in attackAbleTilesGroups) {
			Tile targetTile = pair.Key;
			List<Tile> tilesGroup = pair.Value;
			List<Tile> enemyTilesGroup = Battle.Turn.AIUtil.FindEnemyTilesInTheseTiles (tilesGroup, caster);
			if(enemyTilesGroup.Count != 0)
				attackAbleEnemyTilesGroups [targetTile] = enemyTilesGroup;
		}*/
		return attackAbleTilesGroups;
	}

	public void Apply(Casting casting, int chainCombo) {
        LogManager logManager = LogManager.Instance;
		BattleManager battleManager = BattleData.battleManager;
		Unit caster = casting.Caster;
		List<Tile> secondRange = casting.SecondRange;
		List<Tile> realEffectRange = casting.RealEffectRange;
		List<Unit> targets = TileManager.GetUnitsOnTiles(realEffectRange);
		List<PassiveSkill> passiveSkillsOfCaster = caster.GetLearnedPassiveSkillList();
		ListPassiveSkillLogic passiveSkillLogicsOfCaster = SkillLogicFactory.Get (passiveSkillsOfCaster);

        //secondRange -> 스킬 이펙트용으로만 쓰인다(투사체가 아무 효과 없이 사라져도 이펙트가 날아갈 목표점은 있어야 하니까)
        //realEffectRange -> 효과와 데미지 적용 등 모든 곳에 쓰이는 실제 범위

        if (IsChainable()) 
            ChainList.RemoveChainOfThisUnit(caster);

		// ApplySoundEffect();
        // yield return battleManager.StartCoroutine(ApplyVisualEffect (casting));
        LogManager.Instance.Record(new SoundEffectLog(this));
        LogManager.Instance.Record(new VisualEffectLog(casting));

        foreach (var tile in realEffectRange) {
			if (tile.IsUnitOnTile()) {
				Unit target = tile.GetUnitOnTile();

                // AI 유닛에게 뭔가 기술이 날아오면, 그 유닛이 활성화조건 5번(기술 날아온 순간 활성화)을 가지고 있는지 확인하고 맞으면 활성화시킨다
                if (target.GetComponent<AIData>() != null) 
                    target.GetComponent<AIData>().SetActiveByExternalFactor();

				//공격/약화계 스킬이면 회피 체크를 하고 아니라면 무조건 효과를 가한다
				if (!IsChainable() || !CheckEvasion(caster, target)) {
					CastingApply castingApply = new CastingApply(casting, target);

                    // 효과 외의 부가 액션 (AP 감소 등)
                    SkillLogic.ActionInDamageRoutine(castingApply);
                    passiveSkillLogicsOfCaster.ActionInDamageRoutine(castingApply);

                    // 데미지 적용
                    if (SkillLogic.MayDisPlayDamageCoroutine(castingApply)) {
						if (skillApplyType == SkillApplyType.DamageHealth) {
							ApplyDamage(castingApply, chainCombo, target == targets.Last());
						} else {
							Battle.DamageCalculator.CalculateAmountOtherThanAttackDamage(castingApply);
							float amount = castingApply.GetDamage().resultDamage;
							if (skillApplyType == SkillApplyType.DamageAP) {
								target.ApplyDamageByCasting(castingApply, false);
								BattleData.uiManager.UpdateApBarUI();
							} else if (skillApplyType == SkillApplyType.HealHealth) {
								target.RecoverHealth(amount);
								passiveSkillLogicsOfCaster.TriggerApplyingHeal(castingApply);
							} else if (skillApplyType == SkillApplyType.HealAP) {
								target.RecoverActionPoint((int)amount);
							}
						}
					}

					// 기술의 상태이상은 기술이 적용된 후에 붙인다.
					if (unitStatusEffectList.Count > 0) {
						bool ignored = false;
						foreach (var tileStatusEffect in tile.GetStatusEffectList()) {
							Skill originSkill = tileStatusEffect.GetOriginSkill();
							if (originSkill is ActiveSkill) {
								if (!((ActiveSkill)originSkill).SkillLogic.TriggerTileStatusEffectWhenStatusEffectAppliedToUnit(castingApply, tile, tileStatusEffect))
									ignored = true;
							}
						}
						if (!ignored)
							StatusEffector.AttachStatusEffect (caster, this, target, realEffectRange);
					}
				}
			}
			StatusEffector.AttachStatusEffect(caster, this, tile);
		}

		// 기술 사용 시 적용되는 특성
		passiveSkillLogicsOfCaster.TriggerUsingSkill(casting, targets);
		foreach(var statusEffect in caster.StatusEffectList) {
			Skill originPassiveSkill = statusEffect.GetOriginSkill();
			if(originPassiveSkill is PassiveSkill)
				((PassiveSkill)originPassiveSkill).SkillLogic.TriggerStatusEffectsOnUsingSkill(caster, targets, statusEffect);
		}

		// 공격스킬 시전시 관련 효과중 1회용인 효과 제거 (공격할 경우 - 공격력 변화, 데미지 변화, 강타)
		if (skillApplyType == SkillApplyType.DamageHealth) {
			List<UnitStatusEffect> statusEffectsToRemove = caster.StatusEffectList.FindAll (x => (x.GetIsOnce () &&
				(x.IsOfType (StatusEffectType.PowerChange) ||
					x.IsOfType (StatusEffectType.DamageChange) ||
					x.IsOfType (StatusEffectType.Smite) ||
                    x.IsOfType (StatusEffectType.RequireSkillAPChange))));
			foreach (var statusEffect in statusEffectsToRemove)
				caster.RemoveStatusEffect (statusEffect);
		}
	}

	private static bool CheckEvasion(Unit caster, Unit target) {
		List<PassiveSkill> passiveSkillsOfTarget = target.GetLearnedPassiveSkillList();
		ListPassiveSkillLogic passiveSkillLogicsOfTarget = SkillLogicFactory.Get (passiveSkillsOfTarget);
		float totalEvasionChance = target.GetEvasionChance();
		float randomNumber = UnityEngine.Random.Range(0, 1.0f);

		// 회피에 성공했는지 아닌지에 상관 없이 회피 효과 해제
		List<UnitStatusEffect> statusEffectsToRemove =  target.StatusEffectList.FindAll(x => x.IsOfType(StatusEffectType.EvasionChange));
		foreach(var statusEffect in statusEffectsToRemove)
			target.RemoveStatusEffect(statusEffect);

		if (totalEvasionChance > randomNumber) {
			BattleData.uiManager.AppendNotImplementedLog ("EVASION SUCCESS");
			// (타겟이) 회피 성공했을 경우 추가 효과
			passiveSkillLogicsOfTarget.TriggerOnEvasionEvent (caster, target);
			return true;
		} else {
			return false;
		}
	}

	private static void ApplyDamage(CastingApply castingApply, int chainCombo, bool isLastTarget) {
		Unit caster = castingApply.GetCaster();
		Unit target = castingApply.GetTarget();
		ActiveSkill skill = castingApply.GetSkill();
		int targetCount = castingApply.GetTargetCount();

		DamageCalculator.CalculateAttackDamage(castingApply, chainCombo);
		DamageCalculator.AttackDamage attackDamage = castingApply.GetDamage();
		UIManager UIM = BattleData.uiManager;
        LogManager logManager = LogManager.Instance;
        if (attackDamage.directionBonus > 1) {
            if(attackDamage.attackDirection == DirectionCategory.Back)
                LogManager.Instance.Record(new PrintBonusTextLog("DirectionBack", attackDamage.directionBonus, true));
            else if(attackDamage.attackDirection == DirectionCategory.Side)
                LogManager.Instance.Record(new PrintBonusTextLog("DirectionSide", attackDamage.directionBonus, true));
        }
		if (attackDamage.celestialBonus != 1)   logManager.Record(new PrintBonusTextLog("Celestial", attackDamage.celestialBonus, true));
		if (attackDamage.chainBonus > 1) logManager.Record(new PrintBonusTextLog("Chain", chainCombo, true));
        if (attackDamage.heightBonus != 1) logManager.Record(new PrintBonusTextLog("Height", attackDamage.heightBonus, true));

        BattleManager battleManager = BattleData.battleManager;
		UnitClass damageType = caster.GetUnitClass();
		bool canReflect = target.HasStatusEffect(StatusEffectType.Reflect) ||
			(target.HasStatusEffect(StatusEffectType.MagicReflect) && damageType == UnitClass.Magic) ||
			(target.HasStatusEffect(StatusEffectType.MeleeReflect) && damageType == UnitClass.Melee);
		float reflectAmount = 0;
		if (canReflect) {
			reflectAmount = DamageCalculator.CalculateReflectDamage(attackDamage.resultDamage, target, caster, damageType);
			attackDamage.resultDamage -= reflectAmount;
		}
        
        target.ApplyDamageByCasting(castingApply, true);
		if(canReflect)  reflectDamage(caster, target, reflectAmount);
	}
	private static void reflectDamage(Unit caster, Unit target, float reflectAmount) {
		UnitClass damageType = caster.GetUnitClass();
		BattleManager battleManager = BattleData.battleManager;
		caster.ApplyDamageByNonCasting(reflectAmount, target, 0, 0, true, false, false);

		foreach (var statusEffect in target.StatusEffectList) {
			bool canReflect = statusEffect.IsOfType(StatusEffectType.Reflect) ||
				(statusEffect.IsOfType(StatusEffectType.MagicReflect) && damageType == UnitClass.Magic) ||
				(statusEffect.IsOfType(StatusEffectType.MeleeReflect) && damageType == UnitClass.Melee);
			if (canReflect) {
                Skill originSkill = statusEffect.GetOriginSkill();
				if (originSkill is ActiveSkill)
					((ActiveSkill)originSkill).SkillLogic.TriggerStatusEffectAtReflection(target, statusEffect, caster);
				if (statusEffect.GetIsOnce() == true)
					target.RemoveStatusEffect(statusEffect);
			}
		}
	}

    public void ApplyUnitStatusEffectList(List<UnitStatusEffectInfo> statusEffectInfoList, int partyLevel)
    {
        UnitStatusEffect.FixedElement previousStatusEffect = null;
        foreach (var statusEffectInfo in statusEffectInfoList) {
            UnitStatusEffect.FixedElement statusEffectToAdd = statusEffectInfo.GetStatusEffect();
            if(statusEffectInfo.GetRequireLevel() <= partyLevel) {
                if (previousStatusEffect != null && previousStatusEffect.display.toBeReplaced) { //이전의 previousStatusEffect에 대해서만 대체 여부를 확인함.
                                                                                                 //즉, 대체되어야 하는 StatusEffect는 csv 파일에서 바로 다음 줄에 만들어야 함.
                    unitStatusEffectList.Remove(previousStatusEffect);
                }
                if(statusEffectInfo.GetOriginSkillName().Equals(korName) &&
                    !unitStatusEffectList.Contains(statusEffectToAdd)) {    //같은 스킬을 가진 유닛이 여러 개일 때 중복으로 들어가는 것 방지
                    unitStatusEffectList.Add(statusEffectToAdd);
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

	public IEnumerator AIUseSkill(Casting casting){
        LogManager logManager = LogManager.Instance;
		Unit caster = casting.Caster;
		ActiveSkill skill = casting.Skill;
		SkillLocation location = casting.Location;

        logManager.Record(new CastLog(casting));
        caster.SetDirection (location.Direction);

        BattleManager.MoveCameraToUnit (caster);
        SetSkillNamePanelUI ();

		List<Tile> firstRange = casting.FirstRange;

        if (Configuration.NPCBehaviourDuration >= 0.05f) {
            logManager.Record(new PaintTilesLog(firstRange, TileColor.Red));
            logManager.Record(new WaitForSecondsLog(Configuration.NPCBehaviourDuration));
            logManager.Record(new DepaintTilesLog(TileColor.Red));
        }

		caster.UseActivityPoint (casting.RequireAP);

        if (skill.GetCooldown () > 0) {
            logManager.Record(new CoolDownLog(caster, skill.GetName(), skill.GetCooldown()));
		}

		List<Tile> secondRange = casting.SecondRange;
        TileManager.Instance.PaintTiles(secondRange, TileColor.Red);
		yield return Battle.Turn.SkillAndChainStates.ApplyAllTriggeredChains (casting);
        TileManager.Instance.DepaintAllTiles(TileColor.Red);

		HideSkillNamePanelUI ();
	}

	public void SetSkillNamePanelUI(){
		BattleData.uiManager.SetSkillNamePanelUI(korName);
	}
	public void HideSkillNamePanelUI(){
		BattleData.uiManager.HideSkillNamePanelUI ();
	}

	public bool IsChainable(){
		return skillApplyType == SkillApplyType.DamageHealth
		|| skillApplyType == SkillApplyType.Debuff;
	}

    public string GetOwner(){return owner;}
	public int GetColumn() { return row; }
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
    public List<UnitStatusEffect.FixedElement> GetUnitStatusEffectList() {return unitStatusEffectList;}
    public List<TileStatusEffect.FixedElement> GetTileStatusEffectList() { return tileStatusEffectList; }
}