using Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Battle;
using Battle.Skills;
using Battle.Damage;
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

	public BaseSkillLogic SkillLogic {
		get { return SkillLogicFactory.Get (this); }
	}

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
		List<Tile> secondRange = battleData.tileManager.GetTilesInRange (secondRangeForm,
			                           skillLocation.TargetPos,
			                           secondMinReach,
			                           secondMaxReach,
			                           secondWidth,
			                           skillLocation.Direction);
		if (skillType == SkillType.Auto)
		{
			secondRange.Remove(skillLocation.TargetTile);
		}
		return secondRange;
	}
	public List<Tile> GetTilesInRealEffectRange(SkillLocation skillLocation){
		if (skillType == SkillType.Route && !skillLocation.TargetTile.IsUnitOnTile ())
			return new List<Tile>();
		else
			return GetTilesInSecondRange (skillLocation);
	}
		
	public bool IsAttackableOnTheTile (Unit caster, Tile casterTile){
		if (skillType == SkillType.Point) {
			return GetAttackableEnemyTilesOfPointSkill (caster, casterTile).Count != 0;
		}
		else {
			return GetAttackableEnemyTilesOfDirectionSkill (caster, casterTile).Count != 0;
		}
	}
	public Casting GetBestAttack(Unit caster, Tile casterTile){
		int minCurrHP = Int32.MaxValue;
		Tile targetTile = casterTile;
		Direction direction = caster.GetDirection ();
		if (skillType == SkillType.Point) {
			Dictionary<Tile, List<Tile>> attackableEnemyTiles = GetAttackableEnemyTilesOfPointSkill (caster, casterTile);
			foreach(var pair in attackableEnemyTiles){
				int minCurrHPInThisRange = GetMinModifiedHP (caster, pair.Value);
				if (minCurrHPInThisRange < minCurrHP) {
					minCurrHP = minCurrHPInThisRange;
					targetTile = pair.Key;
					direction = Utility.GetDirectionToTarget (caster, targetTile.GetTilePos ());
				}
			}
		}else{
			Dictionary<Direction, List<Tile>> attackableEnemyTiles = GetAttackableEnemyTilesOfDirectionSkill (caster, casterTile);
			foreach(var pair in attackableEnemyTiles){
				int minCurrHPInThisRange = GetMinModifiedHP (caster, pair.Value);
				if (minCurrHPInThisRange < minCurrHP) {
					direction = pair.Key;
					if (skillType == SkillType.Route)
						targetTile = GetRealTargetTileForAI (casterTile.GetTilePos (), direction, targetTile);
				}
			}
		}
		Casting casting = new Casting (caster, this, new SkillLocation (casterTile, targetTile, direction));
		return casting;
	}
	private static int GetMinModifiedHP(Unit caster, List<Tile> unitTiles){
		int minCurrHP = Int32.MaxValue;
		foreach (Tile tile in unitTiles) {
			int currHP = tile.GetUnitOnTile().GetModifiedHealth(caster.GetUnitClass());
			if (currHP < minCurrHP) {
				minCurrHP = currHP;
			}
		}
		return minCurrHP;
	}
	private Dictionary<Direction, List<Tile>> GetAttackableEnemyTilesOfDirectionSkill(Unit caster, Tile casterTile){
		Dictionary<Direction, List<Tile>> attackAbleTilesGroups = new Dictionary<Direction, List<Tile>> ();

		//투사체 스킬이면 4방향 직선경로상에서 유닛이 가로막은 지점들을 targetTiles로 함.
		//그리고 각 targetTile에서 2차범위 적용해서 각 범위를 attackAbleTilesGroups에 넣는다
		if (skillType == SkillType.Route) {
			Dictionary<Direction, Tile> targetTiles = new Dictionary<Direction, Tile> ();
			Vector2 casterPos = casterTile.GetTilePos ();

			Tile routeEnd;
			Direction direction;

			direction= Direction.LeftDown;
			routeEnd = GetRealTargetTileForAI (casterPos, direction);
			if (routeEnd != null)
				targetTiles [direction] = routeEnd;
			direction = Direction.LeftUp;
			routeEnd = GetRealTargetTileForAI (casterPos, direction);
			if (routeEnd != null)
				targetTiles [direction] = routeEnd;
			direction = Direction.RightDown;
			routeEnd = GetRealTargetTileForAI (casterPos, direction);
			if (routeEnd != null)
				targetTiles [direction] = routeEnd;
			direction = Direction.RightUp;
			routeEnd = GetRealTargetTileForAI (casterPos, direction);
			if (routeEnd != null)
				targetTiles [direction] = routeEnd;

			foreach (var pair in targetTiles) {
				direction = pair.Key;
				Tile targetTile = pair.Value;
				SkillLocation skillLocation = new SkillLocation (casterTile, targetTile, direction);
				attackAbleTilesGroups [direction] = GetTilesInSecondRange (skillLocation);
			}
		}
		//Auto 스킬은 시전자가 위치한 타일이 유일한 targetTile이며 네 방향의 범위를 각각 attackAbleTilesGroups에 넣는다
		else if (skillType == SkillType.Auto) {
			Tile targetTile = casterTile;

			Direction direction;
			direction= Direction.LeftDown;
			SkillLocation skillLocation = new SkillLocation (casterTile, targetTile, direction);
			attackAbleTilesGroups [direction] = GetTilesInSecondRange (skillLocation);
			direction = Direction.LeftUp;
			skillLocation = new SkillLocation (casterTile, targetTile, direction);
			attackAbleTilesGroups [direction] = GetTilesInSecondRange (skillLocation);
			direction = Direction.RightDown;
			skillLocation = new SkillLocation (casterTile, targetTile, direction);
			attackAbleTilesGroups [direction] = GetTilesInSecondRange (skillLocation);
			direction = Direction.RightUp;
			skillLocation = new SkillLocation (casterTile, targetTile, direction);
			attackAbleTilesGroups [direction] = GetTilesInSecondRange (skillLocation);
		}
		//skillType==SkillType.Self
		//Self 스킬은 방향을 바꾸지 말고 써야 해서 시전자의 방향을 그대로 받아옴
		else {
			Tile targetTile = casterTile;
			Direction direction = caster.GetDirection ();
			SkillLocation skillLocation = new SkillLocation (casterTile, targetTile, direction);
			attackAbleTilesGroups [direction] = GetTilesInSecondRange (skillLocation);
		}

		Dictionary<Direction, List<Tile>> attackAbleEnemyTilesGroups = new Dictionary<Direction, List<Tile>> ();
		foreach (var pair in attackAbleTilesGroups) {
			Direction direction = pair.Key;
			List<Tile> tilesGroup = pair.Value;
			List<Tile> enemyTilesGroup = Battle.Turn.AIUtil.FindEnemyTilesInTheseTiles (tilesGroup, caster);
			if(enemyTilesGroup.Count != 0)
				attackAbleEnemyTilesGroups [direction] = enemyTilesGroup;
		}
		return attackAbleEnemyTilesGroups;
	}
	private Dictionary<Tile, List<Tile>> GetAttackableEnemyTilesOfPointSkill(Unit caster, Tile casterTile){
		Dictionary<Tile, List<Tile>> attackAbleTilesGroups = new Dictionary<Tile, List<Tile>> ();
		Vector2 casterPos = casterTile.GetTilePos ();
		List<Tile> targetTiles = GetTilesInFirstRange (casterPos, caster.GetDirection ());
		foreach (Tile targetTile in targetTiles) {
			SkillLocation skillLocation = new SkillLocation (casterPos, targetTile, caster.GetDirection ());
			attackAbleTilesGroups [targetTile] = GetTilesInSecondRange (skillLocation);
		}

		Dictionary<Tile, List<Tile>> attackAbleEnemyTilesGroups = new Dictionary<Tile, List<Tile>> ();
		foreach (var pair in attackAbleTilesGroups) {
			Tile targetTile = pair.Key;
			List<Tile> tilesGroup = pair.Value;
			List<Tile> enemyTilesGroup = Battle.Turn.AIUtil.FindEnemyTilesInTheseTiles (tilesGroup, caster);
			if(enemyTilesGroup.Count != 0)
				attackAbleEnemyTilesGroups [targetTile] = enemyTilesGroup;
		}
		return attackAbleEnemyTilesGroups;
	}

	public IEnumerator Apply(Casting casting, int chainCombo) {
		BattleManager battleManager = battleData.battleManager;
		Unit caster = casting.Caster;
		List<Tile> secondRange = casting.SecondRange;
		List<Tile> realEffectRange = casting.RealEffectRange;
		List<Unit> targets = TileManager.GetUnitsOnTiles(realEffectRange);
		List<PassiveSkill> passiveSkillsOfCaster = caster.GetLearnedPassiveSkillList();
		ListPassiveSkillLogic passiveSkillLogicsOfCaster = SkillLogicFactory.Get (passiveSkillsOfCaster);

		//secondRange -> 스킬 이펙트용으로만 쓰인다(투사체가 아무 효과 없이 사라져도 이펙트가 날아갈 목표점은 있어야 하니까)
		//realEffectRange -> 효과와 데미지 적용 등 모든 곳에 쓰이는 실제 범위

		if (caster == battleData.selectedUnit) {
			caster.UseActivityPoint(casting.RequireAP); // 즉시시전을 한 유닛만 AP를 차감. 나머지는 연계대기할 때 이미 차감되었으므로 패스.
			// 스킬 쿨다운 기록
			if (cooldown > 0)
				caster.GetUsedSkillDict().Add(korName, cooldown);
		}

		if (IsChainable())
			ChainList.RemoveChainOfThisUnit(caster);

		ApplySoundEffect();
		yield return battleManager.StartCoroutine(ApplyVisualEffect (caster, secondRange));

		foreach (var tile in realEffectRange) {
			if (tile.IsUnitOnTile()) {
				Unit target = tile.GetUnitOnTile();

				// AI 유닛에게 뭔가 기술이 날아오면, 그 유닛이 활성화조건 5번(기술 날아온 순간 활성화)을 가지고 있는지 확인하고 맞으면 활성화시킨다
				if (target.GetComponent<AIData>() != null) 
					target.GetComponent<AIData>().SetActiveByExternalFactor();

				//공격/약화계 스킬이면 회피 체크를 하고 아니라면 무조건 효과를 가한다
				if (!IsChainable() || !CheckEvasion(caster, target)) {
					SkillInstanceData skillInstanceData = new SkillInstanceData(new Battle.DamageCalculator.AttackDamage(), this, caster, realEffectRange, target, targets.Count);
					// 데미지 적용
					if (SkillLogic.MayDisPlayDamageCoroutine(skillInstanceData)) {
						if (skillApplyType == SkillApplyType.DamageHealth) {
							yield return battleManager.StartCoroutine(ApplyDamage(skillInstanceData, chainCombo, target == targets.Last()));
						} else {
							Battle.DamageCalculator.CalculateAmountOtherThanAttackDamage(skillInstanceData);
							float amount = skillInstanceData.GetDamage().resultDamage;
							if (skillApplyType == SkillApplyType.DamageAP) {
								yield return battleManager.StartCoroutine(target.DamagedBySkill(skillInstanceData, false));
								battleData.uiManager.UpdateApBarUI(battleData, battleData.unitManager.GetAllUnits());
							} else if (skillApplyType == SkillApplyType.HealHealth) {
								yield return battleManager.StartCoroutine(target.RecoverHealth(amount));
								yield return battleManager.StartCoroutine(passiveSkillLogicsOfCaster.TriggerApplyingHeal(skillInstanceData));
							} else if (skillApplyType == SkillApplyType.HealAP) {
								yield return battleManager.StartCoroutine(target.RecoverActionPoint((int)amount));
							}
						}
					}

					// 효과 외의 부가 액션 (AP 감소 등)
					yield return battleManager.StartCoroutine(SkillLogic.ActionInDamageRoutine(skillInstanceData));
					yield return battleManager.StartCoroutine(passiveSkillLogicsOfCaster.ActionInDamageRoutine(skillInstanceData));

					// 기술의 상태이상은 기술이 적용된 후에 붙인다.
					if (statusEffectList.Count > 0) {
						bool ignored = false;
						foreach (var tileStatusEffect in tile.GetStatusEffectList()) {
							ActiveSkill originSkill = tileStatusEffect.GetOriginSkill();
							if (originSkill != null) {
								if (!originSkill.SkillLogic.TriggerTileStatusEffectWhenStatusEffectAppliedToUnit(skillInstanceData, tile, tileStatusEffect))
									ignored = true;
							}
						}
						if(!ignored)
							StatusEffector.AttachStatusEffect(caster, this, target, realEffectRange);
					}
				}
				caster.ActiveFalseAllBonusText();
				// 사이사이에도 특성 발동 조건을 체크해준다.
				battleData.unitManager.TriggerPassiveSkillsAtActionEnd();
				yield return battleManager.StartCoroutine(battleData.unitManager.TriggerStatusEffectsAtActionEnd());
				battleData.unitManager.UpdateStatusEffectsAtActionEnd();
				battleData.tileManager.UpdateTileStatusEffectsAtActionEnd();

				target.UpdateHealthViewer();
			}
			StatusEffector.AttachStatusEffect(caster, this, tile);
		}

		// 기술 사용 시 적용되는 특성
		passiveSkillLogicsOfCaster.TriggerUsingSkill(caster, targets);
		foreach(var statusEffect in caster.GetStatusEffectList()) {
			PassiveSkill originPassiveSkill = statusEffect.GetOriginPassiveSkill();
			if(originPassiveSkill != null)
				originPassiveSkill.SkillLogic.TriggerStatusEffectsOnUsingSkill(caster, targets, statusEffect);
		}
		caster.SetHasUsedSkillThisTurn(true);

		// 공격스킬 시전시 관련 효과중 1회용인 효과 제거 (공격할 경우 - 공격력 변화, 데미지 변화, 강타)
		if (skillApplyType == SkillApplyType.DamageHealth) {
			List<StatusEffect> statusEffectsToRemove = caster.GetStatusEffectList ().FindAll (x => (x.GetIsOnce () &&
				(x.IsOfType (StatusEffectType.PowerChange) ||
					x.IsOfType (StatusEffectType.DamageChange) ||
					x.IsOfType (StatusEffectType.Smite))));
			foreach (StatusEffect statusEffect in statusEffectsToRemove)
				caster.RemoveStatusEffect (statusEffect);
		}
		battleData.indexOfSelectedSkillByUser = 0; // return to init value.

		yield return new WaitForSeconds(0.5f);
		battleData.alreadyMoved = false;
	}

	private static bool CheckEvasion(Unit caster, Unit target) {
		List<PassiveSkill> passiveSkillsOfTarget = target.GetLearnedPassiveSkillList();
		ListPassiveSkillLogic passiveSkillLogicsOfTarget = SkillLogicFactory.Get (passiveSkillsOfTarget);
		int totalEvasionChance = 0;
		totalEvasionChance = passiveSkillLogicsOfTarget.GetEvasionChance();

		int randomNumber = UnityEngine.Random.Range(0, 100);

		// 회피에 성공했는지 아닌지에 상관 없이 회피 효과 해제
		List<StatusEffect> statusEffectsToRemove =  caster.GetStatusEffectList().FindAll(x => x.IsOfType(StatusEffectType.EvasionChange));
		foreach(StatusEffect statusEffect in statusEffectsToRemove)
			caster.RemoveStatusEffect(statusEffect);

		if (totalEvasionChance > randomNumber) {
			battleData.uiManager.AppendNotImplementedLog("EVASION SUCCESS");
			// (타겟이) 회피 성공했을 경우 추가 효과
			passiveSkillLogicsOfTarget.TriggerOnEvasionEvent(battleData, caster, target);
			return true;
		} else return false;
	}

	private static IEnumerator ApplyDamage(SkillInstanceData skillInstanceData, int chainCombo, bool isLastTarget) {
		Unit caster = skillInstanceData.GetCaster();
		Unit target = skillInstanceData.GetMainTarget();
		ActiveSkill skill = skillInstanceData.GetSkill();
		int targetCount = skillInstanceData.GetTargetCount();

		DamageCalculator.CalculateAttackDamage(skillInstanceData, chainCombo);
		DamageCalculator.AttackDamage attackDamage = skillInstanceData.GetDamage();

		if (attackDamage.directionBonus > 1) caster.PrintDirectionBonus(attackDamage);
		if (attackDamage.celestialBonus != 1) caster.PrintCelestialBonus(attackDamage.celestialBonus);
		if (attackDamage.chainBonus > 1) caster.PrintChainBonus(chainCombo);
		if (attackDamage.heightBonus != 1) caster.PrintHeightBonus(attackDamage.heightBonus);

		BattleManager battleManager = battleData.battleManager;
		// targetUnit이 반사 효과를 지니고 있을 경우 반사 대미지 코루틴 준비
		// FIXME : 반사데미지는 다른 데미지 함수로 뺄 것! Damaged 함수 쓰면 원 공격자 스킬의 부가효과도 적용됨.
		UnitClass damageType = caster.GetUnitClass();
		bool canReflect = target.HasStatusEffect(StatusEffectType.Reflect) ||
			(target.HasStatusEffect(StatusEffectType.MagicReflect) && damageType == UnitClass.Magic) ||
			(target.HasStatusEffect(StatusEffectType.MeleeReflect) && damageType == UnitClass.Melee);
		float reflectAmount = 0;
		if (canReflect) {
			reflectAmount = DamageCalculator.CalculateReflectDamage(attackDamage.resultDamage, target, caster, damageType);
			attackDamage.resultDamage -= reflectAmount;
		}

		var damageCoroutine = target.DamagedBySkill(skillInstanceData, true);
		if (isLastTarget) {
			yield return battleManager.StartCoroutine(damageCoroutine);
		} else {
			battleManager.StartCoroutine(damageCoroutine);
			yield return null;
		}

		if(canReflect)  yield return battleManager.StartCoroutine(reflectDamage(caster, target, reflectAmount));
	}
	private static IEnumerator reflectDamage(Unit caster, Unit target, float reflectAmount) {
		UnitClass damageType = caster.GetUnitClass();
		BattleManager battleManager = battleData.battleManager;
		yield return battleManager.StartCoroutine(caster.Damaged(reflectAmount, target, 0, 0, true, false, false));

		foreach (var statusEffect in target.GetStatusEffectList()) {
			bool canReflect = statusEffect.IsOfType(StatusEffectType.Reflect) ||
				(statusEffect.IsOfType(StatusEffectType.MagicReflect) && damageType == UnitClass.Magic) ||
				(statusEffect.IsOfType(StatusEffectType.MeleeReflect) && damageType == UnitClass.Melee);
			if (canReflect) {
				if (statusEffect.GetOriginSkill() != null)
					yield return battleManager.StartCoroutine(statusEffect.GetOriginSkill().SkillLogic.
						TriggerStatusEffectAtReflection(target, statusEffect, caster));
				if (statusEffect.GetIsOnce() == true)
					target.RemoveStatusEffect(statusEffect);
			}
		}
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
			Vector3 startPos = unit.realPosition;
			Vector3 endPos = new Vector3(0, 0, 0);
			foreach (var tile in secondRange) {
				endPos += tile.realPosition;
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
				targetPos += tile.realPosition;
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
					targetPosList.Add(tile.GetUnitOnTile().realPosition);
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
					midPos += tile.realPosition;
				}
				midPos = midPos / (float)secondRange.Count;

				GameObject particle = GameObject.Instantiate(Resources.Load("Particle/" + visualEffectName)) as GameObject;
				particle.transform.position = midPos - new Vector3(0, -0.5f, 0.01f);
				GameObject.Destroy(particle, 0.5f + 0.3f); // 아랫줄에서의 지연시간을 고려한 값이어야 함.
			}
			yield return new WaitForSeconds(0.5f);
		}
	}

	public IEnumerator AIUseSkill(Casting casting){
		Unit caster = casting.Caster;
		SkillLocation location = casting.Location;

		caster.SetDirection (location.Direction);
		BattleManager.MoveCameraToUnit (caster);
		SetSkillNamePanelUI ();

		List<Tile> firstRange = casting.FirstRange;
		battleData.tileManager.PaintTiles(firstRange, TileColor.Red);
		yield return new WaitForSeconds (0.4f);
		battleData.tileManager.DepaintTiles(firstRange, TileColor.Red);

		List<Tile> secondRange = casting.SecondRange;
		battleData.tileManager.PaintTiles (secondRange, TileColor.Red);
		yield return Battle.Turn.SkillAndChainStates.ApplyAllTriggeredChains (casting);
		battleData.tileManager.DepaintTiles(secondRange, TileColor.Red);

		BattleManager.MoveCameraToUnit (caster);
		HideSkillNamePanelUI ();
	}

	public void SetSkillNamePanelUI(){
		battleData.uiManager.SetSkillNamePanelUI(korName);
	}
	public void HideSkillNamePanelUI(){
		battleData.uiManager.HideSkillNamePanelUI ();
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