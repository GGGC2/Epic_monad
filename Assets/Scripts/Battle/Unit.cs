using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Enums;
using Battle;
using Battle.Skills;
using GameData;
using Save;

public class HitInfo{
	public readonly Unit caster;
	public readonly ActiveSkill skill;
    public readonly int finalDamage;

	public HitInfo(Unit caster, ActiveSkill skill, int finalDamage){
		this.caster = caster;
		this.skill = skill;
        this.finalDamage = finalDamage;
	}
}

public class Unit : MonoBehaviour{
	GameObject damageTextObject;
	GameObject recoverTextObject;
	GameObject activeArrowIcon;
	public HealthViewer healthViewer;
	GameObject chainAttackerIcon;
	List<HitInfo> latelyHitInfos;

	bool isAI = false; // AI 유닛인지 여부인데, 지형지물은 AI로 분류되지 않으므로 PC인지 확인하려면 !IsAI가 아니라 IsPC(=!isAI && !isObject로 아래에 get 함수로 있음)의 return 값을 받아야 한다
	bool isAlreadyBehavedObject; //지형지물(오브젝트)일 때만 의미있는 값. 그 페이즈에 이미 행동했는가
	int movedTileCount;
	Battle.Turn.AI _AI;
	public UnitInfo myInfo;

	// 스킬리스트
	public List<ActiveSkill> activeSkillList = new List<ActiveSkill>();
	public List<PassiveSkill> passiveSkillList = new List<PassiveSkill>();
	// 사용한 스킬 정보 저장(쿨타임 산정용)
	Dictionary<string, int> usedSkillDict = new Dictionary<string, int>();

    // 효과 리스트
    List<UnitStatusEffect> statusEffectList = new List<UnitStatusEffect>();

    class ActualStat {
        public int value;
        public Stat stat;
        public List<UnitStatusEffect> appliedStatusEffects;

        public ActualStat(int value, Stat stat) {
            this.value = value;
            this.stat = stat;
            this.appliedStatusEffects = new List<UnitStatusEffect>();
        }
    }
    Dictionary<Stat, ActualStat> actualStats;

	// Variable values.
	Vector2 position;
	// 유닛이 해당 페이즈에서 처음 있었던 위치 - 영 패시브에서 체크
	Vector2 startPositionOfPhase;
    //이 유닛이 이 턴에 움직였을 경우에 true - 큐리 스킬 '재결정'에서 체크
    int notMovedTurnCount;
    //이 유닛이 이 턴에 스킬을 사용했을 경우에 true - 유진 스킬 '여행자의 발걸음', '길잡이'에서 체크
    bool hasUsedSkillThisTurn;

	Direction direction;
	int currentHealth;
	int activityPoint;

	GameObject chargeEffect;

	Sprite spriteLeftUp;
	Sprite spriteLeftDown;
	Sprite spriteRightUp;
	Sprite spriteRightDown;

	public SpriteRenderer bindIcon;

    public List<HitInfo> GetLatelyHitInfos() { return latelyHitInfos; }
	public Sprite GetCurrentSprite() { return GetComponent<SpriteRenderer>().sprite; }
	public Sprite GetDefaultSprite(){ return spriteLeftDown; }
	public void SetChargeEffect(GameObject effect){
		if (chargeEffect != null) RemoveChargeEffect();
		chargeEffect = effect;
		effect.transform.position = gameObject.transform.position - new Vector3(0, 0, 0.01f);
	}
	public void RemoveChargeEffect(){ Destroy(chargeEffect); }
    public int GetStat(Stat stat) {
        if (actualStats.ContainsKey(stat))
            return actualStats[stat].value;
        return 0;
    }
    public int GetBaseStat(Stat stat) {
        if (myInfo.baseStats.ContainsKey(stat)){
            return myInfo.baseStats[stat];
		}
        return 0;
    }
	public float GetSpeed(){
		float speed = 100;
		if (HasStatusEffect(StatusEffectType.SpeedChange))
			speed = CalculateActualAmount(100, StatusEffectType.SpeedChange);
		return speed;
	}
    public float GetEvasionChance() {
        float evasionChance = 0;
        if (HasStatusEffect(StatusEffectType.EvasionChange))
            evasionChance = CalculateActualAmount(0, StatusEffectType.EvasionChange);
        return evasionChance;
    }
    public int GetRegenerationAmount() { return GetStat(Stat.Agility); }
    public void SetActive() { activeArrowIcon.SetActive(true); }
	public void SetInactive() { activeArrowIcon.SetActive(false); }
	public bool IsAlreadyBehavedObject(){ return isAlreadyBehavedObject; }
	public void SetNotAlreadyBehavedObject() { isAlreadyBehavedObject = false; }
	public void SetAlreadyBehavedObject() { isAlreadyBehavedObject = true; }
	public void AddMovedTileCount(int count) {
		movedTileCount += count;
	}
	public void ResetMovedTileCount() {movedTileCount = 0;}
	public int GetMovedTileCount() {return movedTileCount;}
	public Vector2 GetInitPosition() { return myInfo.initPosition; }
	public List<ActiveSkill> GetSkillList() { return activeSkillList; }
	public List<ActiveSkill> GetLearnedSkillList(){
		var learnedSkills =
			 from skill in activeSkillList
			// where SkillDB.IsLearned(nameInCode, skill.GetName())
			 select skill;
		return learnedSkills.ToList();
	}
	public List<PassiveSkill> GetLearnedPassiveSkillList() { return passiveSkillList; }
	public List<UnitStatusEffect> StatusEffectList{ get{return statusEffectList;} }
	public void SetStatusEffectList(List<UnitStatusEffect> newStatusEffectList) { statusEffectList = newStatusEffectList; }
	public void AddStatusEffectList(UnitStatusEffect effect){
		statusEffectList.Add(effect);
	}
	public int GetMaxHealth() { return actualStats[Stat.MaxHealth].value; }
    public int GetCurrentHealth() { return currentHealth; }
	public float GetHpRatio() {return (float)GetCurrentHealth()/(float)GetMaxHealth();}
	public float GetApRatio(int number){
		int MaxAp = actualStats[Stat.Agility].value + PartyData.level + 60;
		float ratio = (float)number/(float)MaxAp;
		if(ratio < 1) {return ratio;}
		else {return 1;}
	}
	public int GetModifiedHealth(UnitClass casterClass){
		if(casterClass == UnitClass.Melee)
			return currentHealth*(GetStat(Stat.Defense)+200);
		else if(casterClass == UnitClass.Magic)
			return currentHealth*(GetStat(Stat.Resistance)+200);
		else //casterClass == None
			Debug.LogError("Invalid Input");
			return 0;
	}
	public int GetCurrentActivityPoint() {return activityPoint;}
	public UnitClass GetUnitClass() {return myInfo.unitClass;}
	public Element GetElement() {return myInfo.element;}
	public Celestial GetCelestial() {return myInfo.celestial;}
    public Tile GetTileUnderUnit() { return TileManager.Instance.GetTile(position); }
	public int GetHeight() { return GetTileUnderUnit().GetHeight(); }
	public string GetNameEng() { return myInfo.nameEng; }
	public int GetIndex() {return myInfo.index;}
	public string GetNameKor() { return myInfo.nameKor; }
	public Side GetSide() { return myInfo.side; }
	public bool IsAlly(Unit unit) { return myInfo.side == unit.GetSide (); }
	public bool IsSeenAsEnemyToThisAIUnit(Unit unit) { return Battle.Turn.AIUtil.IsSecondUnitEnemyToFirstUnit (unit, this); } //은신 상태에선 적으로 인식되지 않음
	public void SetAsAI() { isAI = true; }
	public void SetAI(Battle.Turn.AI _AI) { this._AI = _AI; }
	public Battle.Turn.AI GetAI() { return _AI; }
	public bool IsAI { get { return isAI; } }
	public bool IsPC { get { return (!isAI) && (!myInfo.isObject); } }
	public bool IsObject { get { return myInfo.isObject; } }
    public Vector2 GetPosition() {return position;}
    public void SetPosition(Vector2 position) { this.position = position; }
	public Vector3 realPosition {
		get { return transform.position; }
	}
    public Vector2 GetStartPositionOfPhase() { return startPositionOfPhase; }
    public int GetNotMovedTurnCount() { return notMovedTurnCount; }
    public bool GetHasUsedSkillThisTurn() { return hasUsedSkillThisTurn; }
    public void SetHasUsedSkillThisTurn(bool hasUsedSkillThisTurn) { this.hasUsedSkillThisTurn = hasUsedSkillThisTurn; }
    public Dictionary<string, int> GetUsedSkillDict() {return usedSkillDict;}
    public Direction GetDirection() { return direction; }
    public void SetDirection(Direction direction) {
		this.direction = direction;
		UpdateSpriteByDirection();
	}
	public int GetStandardAP(){
		return unitManager.GetStandardActivityPoint ();
	}
	public bool HasEnoughAPToUseSkill(ActiveSkill skill){
		return activityPoint >= GetActualRequireSkillAP (skill);
	}
	public int MinAPUseForStandbyForAI(){
		return activityPoint - (GetStandardAP () - 1);
	}
	public int MinAPUseForStandbyForPC(){
		int myAP = activityPoint;
		int maxOtherUnitAP = -1;
		foreach (var anyUnit in BattleData.unitManager.GetUpdatedReadiedUnits()){
			int unitAP = anyUnit.GetCurrentActivityPoint ();
			if (anyUnit != this && unitAP > maxOtherUnitAP) {
				maxOtherUnitAP = anyUnit.GetCurrentActivityPoint ();
			}
		}
		int minAPUse;
		if (maxOtherUnitAP == -1) { // 현 페이즈 행동가능 유닛이 본인밖에 없다면 AP가 턴 기준값보다 낮도록 만들어야 넘길 수 있음
			minAPUse = myAP - (GetStandardAP () - 1);
		}
		else {
			minAPUse = myAP - (maxOtherUnitAP - 1);
		}
		return minAPUse;
	}
	public bool IsStandbyPossible(){
		bool isPossible = (MinAPUseForStandbyForPC () <= 0);
		return isPossible;
	}
	public bool IsMovePossibleState(){
		return (!HasStatusEffect(StatusEffectType.Bind) && !HasStatusEffect(StatusEffectType.Faint));
	}
	public bool IsSkillUsePossibleState(){
		bool isPossible = false;

		isPossible = !(HasStatusEffect(StatusEffectType.Silence) ||
			HasStatusEffect(StatusEffectType.Faint));

		Tile tileUnderCaster = GetTileUnderUnit();
		foreach (var tileStatusEffect in tileUnderCaster.GetStatusEffectList()) {
			Skill originSkill = tileStatusEffect.GetOriginSkill();
			if (originSkill.GetType() == typeof(ActiveSkill)) {
				if (!((ActiveSkill)originSkill).SkillLogic.TriggerTileStatusEffectWhenUnitTryToUseSkill(tileUnderCaster, tileStatusEffect)) {
					isPossible = false;
				}
			}
		}
		return isPossible;
	}
	public bool IsThisSkillUsable(ActiveSkill skill){
		bool isPossible = IsSkillUsePossibleState ();
		isPossible = isPossible && HasEnoughAPToUseSkill (skill);
		isPossible = isPossible && !usedSkillDict.ContainsKey (skill.GetName());
		return isPossible;
	}
	public void ApplySnapshot(){
		BattleData.MoveSnapshot snapshot = BattleData.moveSnapshot;
		BattleData.tileManager.DepaintAllTiles(TileColor.Blue);
		GetTileUnderUnit().SetUnitOnTile(null);
		transform.position = snapshot.tile.transform.position + new Vector3(0, 0, -0.05f);
		SetPosition(snapshot.tile.GetTilePos());
		SetDirection(snapshot.direction);
		snapshot.tile.SetUnitOnTile(this);
		activityPoint = snapshot.ap;
		movedTileCount = snapshot.movedTileCount;
		SetStatusEffectList(snapshot.statEffectList);
		BattleData.uiManager.UpdateUnitViewer(this);
		unitManager.UpdateUnitOrder();
	}
    private void ChangePosition(Tile destTile) {
        Tile tileBefore = GetTileUnderUnit();
        tileBefore.SetUnitOnTile(null);
        transform.position = destTile.transform.position + new Vector3(0, 0, -0.05f);
        SetPosition(destTile.GetTilePos());
        destTile.SetUnitOnTile(this);
        notMovedTurnCount = 0;

		ChainList.RemoveChainOfThisUnit (this);
        SkillLogicFactory.Get(passiveSkillList).TriggerOnMove(this);
        foreach (var statusEffect in StatusEffectList) {
            Skill originPassiveSkill = statusEffect.GetOriginSkill();
            if (originPassiveSkill.GetType() == typeof(PassiveSkill))
                ((PassiveSkill)originPassiveSkill).SkillLogic.TriggerStatusEffectsOnMove(this, statusEffect);
        }
        BattleTriggerManager.CheckBattleTrigger(this, destTile);
        UpdateStats();
    }

    public void ForceMove(Tile destTile) { //강제이동
		if (!IsObject && SkillLogicFactory.Get(passiveSkillList).TriggerOnForceMove(this, destTile)) {
			ChangePosition (destTile);
        }
    }
    
	public void ApplyMove(Tile destTile, Direction finalDirection, int totalAPCost, int tileCount) {
        Tile beforeTile = GetTileUnderUnit();
        UseActivityPoint (totalAPCost);
		ChangePosition (destTile);
		SetDirection (finalDirection);
		AddMovedTileCount(tileCount);

        foreach (var statusEffect in StatusEffectList) {
            if ((statusEffect.IsOfType(StatusEffectType.RequireMoveAPChange) ||
                statusEffect.IsOfType(StatusEffectType.SpeedChange)) && statusEffect.GetIsOnce() == true) {
                RemoveStatusEffect(statusEffect);
            }
        }
        SkillLogicFactory.Get(passiveSkillList).TriggerAfterMove(this, beforeTile, destTile);
    }

    private void updateCurrentHealthRelativeToMaxHealth() {
        if(currentHealth > GetMaxHealth())
            currentHealth = GetMaxHealth();
    }

    public void UpdateStats(){
        foreach (var actualStat in actualStats.Values) {
            Stat statType = actualStat.stat;
            StatusEffectType statusEffectType = EnumConverter.GetCorrespondingStatusEffectType(statType);
            if (statusEffectType != StatusEffectType.Etc)
                actualStat.value = (int)CalculateActualAmount(myInfo.baseStats[statType], statusEffectType);
        }
        updateCurrentHealthRelativeToMaxHealth();
    }
    public void UpdateStats(UnitStatusEffect statusEffect, bool isApplied, bool isRemoved) {
        List<ActualStat> statsToUpdate = new List<ActualStat>();
        for (int i = 0; i < statusEffect.fixedElem.actuals.Count; i++) {
            StatusEffectType type = statusEffect.fixedElem.actuals[i].statusEffectType;
            Stat statTypeToUpdate = EnumConverter.GetCorrespondingStat(type);
            if (statTypeToUpdate != Stat.None)
                statsToUpdate.Add(actualStats[statTypeToUpdate]);
        }
        for (int i = 0; i < statsToUpdate.Count; i++) {
            if (isApplied) statsToUpdate[i].appliedStatusEffects.Add(statusEffect);
            else if (isRemoved) statsToUpdate[i].appliedStatusEffects.Remove(statusEffect);
            
            StatusEffectType statusEffectType = EnumConverter.GetCorrespondingStatusEffectType(statsToUpdate[i].stat);
            statsToUpdate[i].value = (int)CalculateActualAmount(myInfo.baseStats[statsToUpdate[i].stat], statusEffectType);
        }
        updateCurrentHealthRelativeToMaxHealth();
    }
    
    public void UpdateHealthViewer() {
		healthViewer.gameObject.SetActive(true);
        healthViewer.UpdateCurrentHealth(currentHealth, GetRemainShield(), GetMaxHealth());
		CheckAndHideObjectHealth();
    }
    public void UpdateRealPosition(int direction) { //direction = 1 : 반시계방향으로 회전, direction = -1 : 시계방향으로 회전 
        transform.position = GetTileUnderUnit().transform.position + new Vector3(0, 0, -0.05f);
		SetDirection (this.direction);
    }

    public void AddSkillCooldown(int phase){
		Dictionary<string, int> newUsedSkillDict = new Dictionary<string, int>();
		foreach (var skill in activeSkillList)
		{
			int cooldown = 0;
			if (usedSkillDict.ContainsKey(skill.GetName()))
				cooldown = usedSkillDict[skill.GetName()];
			newUsedSkillDict.Add(skill.GetName(), cooldown + phase);
		}
		usedSkillDict = newUsedSkillDict;
	}

	public void SubSkillCooldown(int phase){
		Dictionary<string, int> newUsedSkillDict = new Dictionary<string, int>();
		foreach (var skill in usedSkillDict)
		{
			int updatedCooldown = skill.Value - phase;
			if (updatedCooldown > 0)
				newUsedSkillDict.Add(skill.Key, updatedCooldown);
		}
		usedSkillDict = newUsedSkillDict;
	}

	public void UpdateSkillCooldown(){
		Dictionary<string, int> newUsedSkillDict = new Dictionary<string, int>();
		foreach (var skill in usedSkillDict)
		{
			int updatedCooldown = skill.Value - 1;
			if (updatedCooldown > 0)
				newUsedSkillDict.Add(skill.Key, updatedCooldown);
		}
		usedSkillDict = newUsedSkillDict;
	}

	public void SetStatusEffect(UnitStatusEffect statusEffect){
		statusEffectList.Add(statusEffect);
		// 침묵이나 기절상태가 될 경우 체인 해제.
		// FIXME : 넉백 추가할 것. (넉백은 디버프가 아니라서 다른 곳에서 적용할 듯?)
		if (statusEffect.IsOfType(StatusEffectType.Faint) ||
			statusEffect.IsOfType(StatusEffectType.Silence))
		{
			ChainList.RemoveChainOfThisUnit(this);
		}
	}

	// searching certain StatusEffect
	public bool HasStatusEffect(UnitStatusEffect statusEffect)
	{
		bool hasStatusEffect = false;
		if (statusEffectList.Any(k => k.GetOriginSkillName().Equals(statusEffect.GetOriginSkillName())))
			hasStatusEffect = true;
		return hasStatusEffect;
	}

	// searching certain StatusEffectType
	public bool HasStatusEffect(StatusEffectType statusEffectType){
		bool hasStatusEffect = false;
		if (statusEffectList.Any(se => se.fixedElem.actuals.Any(elem => elem.statusEffectType == statusEffectType)))
			hasStatusEffect = true;

		return hasStatusEffect;
	}

    public void RemoveStatusEffect(UnitStatusEffect statusEffect) {
        bool toBeRemoved = true;

        toBeRemoved = SkillLogicFactory.Get(passiveSkillList).TriggerStatusEffectRemoved(statusEffect, this);
        Skill originSkill = statusEffect.GetOriginSkill();
        if (originSkill.GetType() == typeof(ActiveSkill)) {
            toBeRemoved = ((ActiveSkill)originSkill).SkillLogic.TriggerStatusEffectRemoved(statusEffect, this);
        }
        if (toBeRemoved) {
            Debug.Log(statusEffect.GetDisplayName() + " is removed from " + myInfo.nameKor);
            statusEffectList = statusEffectList.FindAll(se => se != statusEffect);
            UpdateStats(statusEffect, false, true);
            UpdateSpriteByStealth();
            if(statusEffect.IsOfType(StatusEffectType.Shield)) {
                UpdateHealthViewer();
            }
        }
    }
    public void RemoveStatusEffect(Unit caster, StatusEffectCategory category, int num)  //해당 category의 statusEffect를 num개 까지 제거
	{
        foreach (var statusEffect in statusEffectList) {
            if (num == 0) break;

            // 자신이 자신에게 건 효과는 스스로 해제할 수 없다 - 기획문서 참조
            if (caster == this && statusEffect.GetCaster() == this) continue;

            if (!statusEffect.GetIsRemovable()) continue;

            bool matchIsBuff = (category == StatusEffectCategory.Buff) && (statusEffect.GetIsBuff());
            bool matchIsDebuff = (category == StatusEffectCategory.Debuff) && (!statusEffect.GetIsBuff());
            bool matchAll = (category == StatusEffectCategory.All);
            if (matchIsBuff || matchIsDebuff || matchAll) {
                RemoveStatusEffect(statusEffect);
                num -= 1;
            }
        }
    }

    float CalculateThroughChangeList(float data, List<StatChange> appliedChangeList) {    //<isMultiply, value>
        float totalAdditiveValue = 0.0f;
        float totalMultiplicativeValue = 1.0f;
        foreach (var change in appliedChangeList) {
            if(change.isMultiply == true) {
                totalMultiplicativeValue *= 1 + change.value;
            }
            else {
                totalAdditiveValue += change.value;
            }
		}
        return data * totalMultiplicativeValue + totalAdditiveValue;
    }

    class StatChange {
        public bool isMultiply;
        public float value;
        public StatChange(bool isMultiply, float value) {
            this.isMultiply = isMultiply;
            this.value = value;
        }
    }
	public float CalculateActualAmount(float data, StatusEffectType statusEffectType){
        List<StatChange> appliedChangeList = new List<StatChange>();

        // 효과로 인한 변동값 계산
		foreach (var statusEffect in statusEffectList) {
			for (int i = 0; i < statusEffect.fixedElem.actuals.Count; i++) {
				if (statusEffect.IsOfType (i, statusEffectType)) {
					float amount = statusEffect.GetAmount (i);
					if (statusEffect.GetIsPercent (i)) {
						amount = amount / 100;
					}
					appliedChangeList.Add (new StatChange (statusEffect.GetIsMultiply (i), amount));
				}
			}
        
			// TileStatusEffect로 인한 변동값 계산
			Tile tile = GetTileUnderUnit ();
			foreach (var tileStatusEffect in tile.GetStatusEffectList())
				for (int i = 0; i < tileStatusEffect.fixedElem.actuals.Count; i++)
					if (tileStatusEffect.IsOfType (i, statusEffectType))
						appliedChangeList.Add (new StatChange (tileStatusEffect.GetIsMultiply (i), tileStatusEffect.GetAmount (i)));

			// 상대값 공격력 변동 특성 영향 합산 (무조건 곱연산)			
			if (statusEffectType == StatusEffectType.PowerChange) {
				List<PassiveSkill> passiveSkills = this.GetLearnedPassiveSkillList ();
				float relativePowerBonus = SkillLogicFactory.Get (passiveSkills).GetAdditionalRelativePowerBonus (this);
				relativePowerBonus = (relativePowerBonus - 1) * 100;
				appliedChangeList.Add (new StatChange (true, relativePowerBonus));
			}

			// 방어/저항력 변동 특성 영향 합산
			if (statusEffectType == StatusEffectType.DefenseChange || statusEffectType == StatusEffectType.ResistanceChange) {
				List<PassiveSkill> passiveSkills = this.GetLearnedPassiveSkillList ();
				if (statusEffectType == StatusEffectType.DefenseChange) {
					float additiveDefenseBouns = SkillLogicFactory.Get (passiveSkills).GetAdditionalAbsoluteDefenseBonus (this);
					appliedChangeList.Add (new StatChange (false, additiveDefenseBouns));
				} else if (statusEffectType == StatusEffectType.ResistanceChange) {
					float additiveResistanceBouns = SkillLogicFactory.Get (passiveSkills).GetAdditionalAbsoluteResistanceBonus (this);
					appliedChangeList.Add (new StatChange (false, additiveResistanceBouns));
				}
			}

			if (statusEffectType == StatusEffectType.SpeedChange) {
				if (myInfo.element == Element.Water && GetTileUnderUnit ().GetTileElement () == Element.Water) {
					appliedChangeList.Add (new StatChange (false, 15));
				}
			}
		}

		// 불속성 유닛이 불 타일 위에 있을 경우 공격력 * 1.2
		if(statusEffectType == StatusEffectType.PowerChange && myInfo.element == Element.Fire && GetTileUnderUnit ().GetTileElement () == Element.Fire){
			appliedChangeList.Add (new StatChange (true, 1.2f));
		}
		// 금속성 유닛이 금타일 위에 있을경우 방어/저항 상승
		else if((statusEffectType == StatusEffectType.DefenseChange || statusEffectType == StatusEffectType.ResistanceChange) && myInfo.element == Element.Metal && GetTileUnderUnit ().GetTileElement () == Element.Metal){
			appliedChangeList.Add (new StatChange (false, 0.7f*PartyData.GetLevel()+53));
		}

		return CalculateThroughChangeList(data, appliedChangeList);
	}

    public int GetRemainShield() {
        float remainShieldAmount = 0;
        foreach(var statusEffect in statusEffectList) {
            remainShieldAmount += statusEffect.GetRemainAmountOfType(StatusEffectType.Shield);
        }
        return (int)remainShieldAmount;
    }

	public void UpdateRemainPhaseAtPhaseEnd()
	{
		foreach (var statusEffect in statusEffectList)
		{
			if (!statusEffect.GetIsInfinite())
				statusEffect.DecreaseRemainPhase();
			if (statusEffect.GetRemainPhase() <= 0)
				RemoveStatusEffect(statusEffect);
		}
	}

	public void UpdateStartPosition(){
		startPositionOfPhase = this.GetPosition();
        notMovedTurnCount ++;
        hasUsedSkillThisTurn = false;
	}

	public IEnumerator ApplyDamageByCasting(CastingApply castingApply, bool isHealth) {
		Unit caster = castingApply.GetCaster();
		ActiveSkill skill = castingApply.GetSkill();
		bool ignoreShield = SkillLogicFactory.Get(skill).IgnoreShield(castingApply);

		// 대상에게 스킬로 데미지를 줄때 발동하는 공격자 특성
		var passiveSkillsOfAttacker = caster.GetLearnedPassiveSkillList();
		SkillLogicFactory.Get(passiveSkillsOfAttacker).TriggerActiveSkillDamageApplied(caster, this);

		int realDamage = (int)CalculateDamageByCasting (castingApply, isHealth);
		yield return StartCoroutine (ApplyDamage (realDamage, caster, isHealth, ignoreShield));
	}
	public int CalculateDamageByCasting(CastingApply castingApply, bool isHealth){
		Unit caster = castingApply.GetCaster();
		ActiveSkill appliedSkill = castingApply.GetSkill();
		float damage = castingApply.GetDamage().resultDamage;
		if (isHealth == true) {
			float reflectDamage = DamageCalculator.CalculateReflectDamage(damage, this, caster, caster.GetUnitClass());
			damage -= reflectDamage;

			// 피격자의 효과/특성으로 인한 대미지 증감 효과 적용 - 아직 미완성
			damage = CalculateActualAmount (damage, StatusEffectType.TakenDamageChange);

			float defense = DamageCalculator.CalculateDefense(appliedSkill, this, caster);
			float resistance = DamageCalculator.CalculateResistance(appliedSkill, this, caster);
			damage = DamageCalculator.ApplyDefenseAndResistance (damage, caster.GetUnitClass (), defense, resistance);
		}
		return (int)damage;
	}
	public IEnumerator ApplyDamageByNonCasting(float originalDamage, Unit caster, float additionalDefense, float additionalResistance, bool isHealth, bool ignoreShield, bool isSourceTrap){
		int realDamage = (int)CalculateDamageByNonCasting (originalDamage, caster, additionalDefense, additionalResistance, isHealth, isSourceTrap);
		yield return StartCoroutine (ApplyDamage (realDamage, caster, isHealth, ignoreShield));
	}
	public int CalculateDamageByNonCasting(float originalDamage, Unit caster, float additionalDefense, float additionalResistance, bool isHealth, bool isSourceTrap){
		float damage = originalDamage;

		if (isHealth) {
			// 피격자의 효과/특성으로 인한 대미지 증감 효과 적용 - 아직 미완성
			damage = CalculateActualAmount (damage, StatusEffectType.TakenDamageChange);
			// 방어력 및 저항력 적용
			float defense = GetStat(Stat.Defense) + additionalDefense;
			float resistance = GetStat(Stat.Resistance) + additionalResistance;
			damage = DamageCalculator.ApplyDefenseAndResistance (damage, caster.GetUnitClass (), defense, resistance);

			// 비앙카 고유스킬 - 덫 데미지 안 받음
			if (!SkillLogicFactory.Get (GetLearnedPassiveSkillList ()).TriggerDamaged (this, damage, caster, isSourceTrap)) {
				damage = 0;
			}
		}
		return (int)damage;
	}
    
	IEnumerator ApplyDamage(int damage, Unit caster, bool isHealth, bool ignoreShield) {
		if (isHealth) {
			int damageAfterShieldApply = damage;
			// 보호막 차감(먼저 적용된 것 우선).
			Dictionary<UnitStatusEffect, int> attackedShieldDict = new Dictionary<UnitStatusEffect, int>();
			if (!ignoreShield) {
				foreach (var se in statusEffectList) {
					int num = se.fixedElem.actuals.Count;
					for (int i = 0; i < num; i++) {
						if (se.GetStatusEffectType(i) == StatusEffectType.Shield) {
							int remainShieldAmount = (int)se.GetRemainAmount(i);
							if (remainShieldAmount >= damageAfterShieldApply) {
								se.SubAmount(i, damageAfterShieldApply);
								attackedShieldDict.Add(se, damageAfterShieldApply);
								damageAfterShieldApply = 0;
							}
							else {
								se.SubAmount(i, remainShieldAmount);
								attackedShieldDict.Add(se, remainShieldAmount);
								RemoveStatusEffect(se);
								damageAfterShieldApply -= remainShieldAmount;
							}
						}
					}
					if (damage == 0) break;
				}
			}
			currentHealth -= damageAfterShieldApply;

			if (currentHealth < 0)
				currentHealth = 0;

			Debug.Log(damageAfterShieldApply + " damage applied to " + GetNameKor());

			DisplayDamageText(damageAfterShieldApply);

			UpdateHealthViewer();

			yield return new WaitForSeconds(1);

			damageTextObject.SetActive(false);

			if (damageAfterShieldApply > 0) {
				latelyHitInfos.Add(new HitInfo(caster, null, damageAfterShieldApply));

				// 데미지를 받을 때 발동하는 피격자 특성
				SkillLogicFactory.Get(passiveSkillList).TriggerAfterDamaged(this, damageAfterShieldApply, caster);
			}
			foreach (var kv in attackedShieldDict) {
				BattleManager battleManager = FindObjectOfType<BattleManager>();
				UnitStatusEffect statusEffect = kv.Key;
				Skill originSkill = statusEffect.GetOriginSkill();
				if (originSkill.GetType() == typeof(ActiveSkill))
					yield return battleManager.StartCoroutine(((ActiveSkill)originSkill).SkillLogic.TriggerShieldAttacked(this, kv.Value));
				Unit shieldCaster = statusEffect.GetCaster();
				List<PassiveSkill> shieldCastersPassiveSkills = shieldCaster.GetLearnedPassiveSkillList();
				yield return battleManager.StartCoroutine(SkillLogicFactory.Get(shieldCastersPassiveSkills).
					TriggerWhenShieldWhoseCasterIsOwnerIsAttacked(caster, shieldCaster, this, kv.Value));
			}

		}
		else {
			if (activityPoint >= damage)
				activityPoint -= damage;
			else
				activityPoint = 0;
			Debug.Log(GetNameKor() + " loses " + damage + "AP.");
		}
	}

	public void DisplayDamageText(int damage){
		damageTextObject.SetActive(true);
		damageTextObject.GetComponent<CustomWorldText>().text = damage.ToString();
		damageTextObject.GetComponent<CustomWorldText>().ApplyText(CustomWorldText.Font.DAMAGE);
	}
	public void DisplayRecoverText(int recover){
		recoverTextObject.SetActive(true);
		recoverTextObject.GetComponent<CustomWorldText>().text = recover.ToString();
		recoverTextObject.GetComponent<CustomWorldText>().ApplyText(CustomWorldText.Font.RECOVER);
	}

    private bool checkIfSourceIsTrap(UnitStatusEffect se) {
        List<TileStatusEffect.FixedElement> statusEffectList = new List<TileStatusEffect.FixedElement>();
        Skill originSkill = se.GetOriginSkill();
        if(originSkill.GetType() == typeof(ActiveSkill)) 
            statusEffectList = ((ActiveSkill)originSkill).GetTileStatusEffectList();
        bool isSourceTrap = false;
        foreach(var fixedElem in statusEffectList) {
            foreach(var actual in fixedElem.actuals) {
                if(actual.statusEffectType == StatusEffectType.Trap)
                    isSourceTrap = true;
            }
        }
        return isSourceTrap;
    }
    public IEnumerator ApplyDamageOverPhase() {
        foreach (var se in statusEffectList) {
            int actuals = se.fixedElem.actuals.Count;
            for (int i = 0; i < actuals; i++) {
                if (se.IsOfType(i, StatusEffectType.DamageOverPhase)) {
                    BattleManager.MoveCameraToUnit(this);

                    float damage = se.GetAmount(i);
                    Unit caster = se.GetCaster();

                    bool isSourceTrap = checkIfSourceIsTrap(se);
					yield return StartCoroutine(ApplyDamageByNonCasting(damage, caster, 0, 0, true, false, isSourceTrap));
                }
            }
        }

        yield return null;
    }

	public IEnumerator ApplyHealOverPhase(){
		float totalAmount = 0.0f;

		if (this.HasStatusEffect(StatusEffectType.HealOverPhase)){
			foreach (var statusEffect in statusEffectList){
				if (statusEffect.IsOfType(StatusEffectType.HealOverPhase)){
                    BattleManager.MoveCameraToUnit(this);
                    totalAmount += statusEffect.GetAmountOfType(StatusEffectType.HealOverPhase);
				}
			}
		}
        if(totalAmount != 0)
		    yield return RecoverHealth(totalAmount);
	}

	public IEnumerator RecoverHealth(float amount)
	{
        int maxHealth = GetMaxHealth();
		// 회복량 증감 효과 적용
		amount = CalculateActualAmount(amount, StatusEffectType.TakenHealChange);

		// 초과회복시 최대체력까지만 올라감
		int actualAmount = (int)amount;
		if (currentHealth + actualAmount > maxHealth)
		{
			actualAmount = maxHealth - currentHealth;
		}

		currentHealth += actualAmount;

		DisplayRecoverText (actualAmount);
		UpdateHealthViewer();
		yield return new WaitForSeconds(1);
		recoverTextObject.SetActive(false);
	}

	public IEnumerator RecoverActionPoint(int amount)
	{
		activityPoint += amount;

		//AP 회복인데 체력 회복과 똑같은 폰트로 나오면 헷갈리지 않을까
		DisplayRecoverText (amount);

		// 회복량 표시되는 시간.
		yield return new WaitForSeconds(1);
		recoverTextObject.SetActive(false);

		unitManager.UpdateUnitOrder();
	}

	public void RegenerateActionPoint(){
		if (!myInfo.isObject) {
			activityPoint = GetRegeneratedActionPoint ();
			Debug.Log (name + " recover " + GetStat(Stat.Agility) + "AP. Current AP : " + activityPoint);
		}
	}

	public int GetRegeneratedActionPoint()
	{
		return activityPoint + GetRegenerationAmount(); // 페이즈당 행동력 회복량 = 민첩성 * 보정치(버프/디버프)
	}

    public int GetActualRequireSkillAP(ActiveSkill skill){
        int requireSkillAP = skill.GetRequireAP();

		// 기술 자체에 붙은 행동력 소모 증감효과 적용
		requireSkillAP = SkillLogicFactory.Get(skill).CalculateAP(requireSkillAP, this);

        // 기술의 행동력 소모 증감 효과 적용
        if (HasStatusEffect(StatusEffectType.RequireSkillAPChange)) {
			requireSkillAP = (int) CalculateActualAmount(requireSkillAP, StatusEffectType.RequireSkillAPChange);
		}
		float speed = GetSpeed ();
		requireSkillAP = (int)(requireSkillAP * (100f / speed));

		// 스킬 시전 유닛의 모든 행동력을 요구하는 경우
		if (skill.GetRequireAP() == 1000)
		{
			requireSkillAP = activityPoint;
		}

        return requireSkillAP;
    }

	// 아래 - AI용 함수들

	// FIXME : AI가 공격, 회복 외의 기술을 갖게 되는 시점이 오면 reward 함수를 확장해야 한다
	public float CalculateRewardByCastingToThisUnit(Casting casting){
		float sideFactor = 0;
		Unit caster = casting.Caster;
		if (IsAlly (caster)) {
			sideFactor = -0.6f;
		}
		if (IsSeenAsEnemyToThisAIUnit (caster)) {
			sideFactor = 1;
		}

		float PCFactor = 1;
		if (IsPC) {
			PCFactor = 2;
		}

		float reward = 0;
		if (casting.Skill.GetSkillApplyType() == SkillApplyType.DamageHealth) {
			float killNeedCount = CalculateFloatKillNeedCount (casting);
			if (killNeedCount <= 1) {
				killNeedCount = 0.4f;
			}
			reward = sideFactor * PCFactor * GetStat (Stat.Power) / killNeedCount;
		} else if(casting.Skill.GetSkillApplyType() == SkillApplyType.HealHealth) {
			sideFactor = -sideFactor;
			float healNeedCount = CalculateFloatHealNeedCount (casting);
			reward = sideFactor * PCFactor * GetStat (Stat.Power) * healNeedCount;
		}
		//Debug.Log ("Skill apply to " + name + " is " + ((int)reward) + " point");
		return reward;
	}

	public float CalculateFloatKillNeedCount(Casting casting){
		CastingApply castingApply = new CastingApply (casting, this);
		List<Chain> allTriggeredChains = ChainList.GetAllChainTriggered (casting);
		int chainCombo = allTriggeredChains.Count;
		DamageCalculator.CalculateAttackDamage(castingApply, chainCombo);
		int damage = CalculateDamageByCasting(castingApply, true);	

		int remainHP = GetCurrentHealth () + GetRemainShield();
		if (!IsObject && SceneData.stageNumber >= Setting.retreatOpenStage) {
			remainHP -= GetStat (Stat.MaxHealth) * Setting.retreatHpPercent / 100;
		}
		
		damage = Math.Min (damage, remainHP);

		float killNeedCount;

		if (damage >= 1) {
			killNeedCount = remainHP / damage;
		} else {
			killNeedCount = 10000;
		}
		return killNeedCount;
	}
	public int CalculateIntKillNeedCount(Casting casting){
		return (int)CalculateFloatKillNeedCount (casting);
	}

	public float CalculateFloatHealNeedCount(Casting casting){
		CastingApply castingApply = new CastingApply (casting, this);
		DamageCalculator.CalculateAmountOtherThanAttackDamage(castingApply);
		float healAmount = castingApply.GetDamage().resultDamage;

		int remainHP = GetCurrentHealth () + GetRemainShield();
		int recoverableHP = Math.Max (0, GetStat (Stat.MaxHealth) - remainHP);

		float healNeedCount = recoverableHP / healAmount;
		return healNeedCount;
	}

	// 1턴 내 공격 못하는 적에 대해 '예상' 가치 구한다(지금은 안 쓰는데 나중에 쓸지도)
	public float CalculatePredictReward(Unit caster, ActiveSkill skill){
		// 접근했을 때 실제로 서로가 위치한 타일도 모르고 어느 방향으로 기술 시전할지도 모르므로 가짜로 넣어둠
		SkillLocation pseudoLocation = new SkillLocation(caster.GetTileUnderUnit(), this.GetTileUnderUnit(), Direction.LeftDown);
		Casting pseudoCasting = new Casting (caster, skill, pseudoLocation);
		return CalculateRewardByCastingToThisUnit (pseudoCasting);
	}

	// 위 - AI용 함수들

	public void SetActivityPoint(int newAP){
		activityPoint = newAP;
		unitManager.UpdateUnitOrder();
	}
	public void UseActivityPoint(int amount){
		activityPoint -= amount;
		//Debug.Log(name + " use " + amount + "AP. Current AP : " + activityPoint);
		unitManager.UpdateUnitOrder();
		UIManager.Instance.UpdateSelectedUnitViewerUI (this);
	}

	public void ApplyTriggerOnPhaseStart(int phase){
		SkillLogicFactory.Get(passiveSkillList).TriggerOnPhaseStart(this, phase);
        foreach (var statusEffect in statusEffectList) {
            Skill originSkill = statusEffect.GetOriginSkill();
            if (originSkill.GetType() == typeof(ActiveSkill)) {
                ((ActiveSkill)originSkill).SkillLogic.TriggerStatusEffectAtPhaseStart(this, statusEffect);
            }
        }
    }
    
    public void ApplyTriggerOnStart() {
        SkillLogicFactory.Get(passiveSkillList).TriggerOnStart(this);
    }

    public void ApplyTriggerOnPhaseEnd()
    {
        SkillLogicFactory.Get(passiveSkillList).TriggerOnPhaseEnd(this);
    }
    public void TriggerTileStatusEffectAtTurnStart() {
		foreach(var tile in BattleData.tileManager.GetAllTiles().Values) {
            foreach (var tileStatusEffect in tile.GetStatusEffectList()) {
                Skill originSkill = tileStatusEffect.GetOriginSkill();
                if (originSkill.GetType() == typeof(ActiveSkill)) {
                    ((ActiveSkill)originSkill).SkillLogic.TriggerTileStatusEffectAtTurnStart(this, tile, tileStatusEffect);
                }
            }
        }
    }
    public void TriggerTileStatusEffectAtTurnEnd() {
		foreach (var tile in BattleData.tileManager.GetAllTiles().Values) {
            foreach (var tileStatusEffect in tile.GetStatusEffectList()) {
                Skill originSkill = tileStatusEffect.GetOriginSkill();
                if (originSkill.GetType() == typeof(ActiveSkill)) {
                    ((ActiveSkill)originSkill).SkillLogic.TriggerTileStatusEffectAtTurnEnd(this, tile, tileStatusEffect);
                }
            }
        }
    }

	public void UpdateSpriteByDirection()
	{
		Direction faceDirection = (Direction)(((int)direction + (int)BattleData.aspect + 4) % 4);
		if (faceDirection == Direction.LeftUp)
			GetComponent<SpriteRenderer>().sprite = spriteLeftUp;
		if (faceDirection == Direction.LeftDown)
			GetComponent<SpriteRenderer>().sprite = spriteLeftDown;
		if (faceDirection == Direction.RightUp)
			GetComponent<SpriteRenderer>().sprite = spriteRightUp;
		if (faceDirection == Direction.RightDown)
			GetComponent<SpriteRenderer>().sprite = spriteRightDown;
	}

    public void UpdateSpriteByStealth() {
        Color color = GetComponent<SpriteRenderer>().color;
        if(HasStatusEffect(StatusEffectType.Stealth))
            color.a = 0.5f;
        else
            color.a = 1;
        GetComponent<SpriteRenderer>().color = color;
    }

	public void ShowChainIcon(){
		chainAttackerIcon.SetActive(true);
	}
	public void HideChainIcon(){
		chainAttackerIcon.SetActive(false);
	}

	void ApplyStats(){
		float partyLevel = (float)GameData.PartyData.level;
        
        actualStats = new Dictionary<Stat, ActualStat>();
        actualStats.Add(Stat.MaxHealth, new ActualStat(myInfo.baseStats[Stat.MaxHealth], Stat.MaxHealth));
        actualStats.Add(Stat.Power, new ActualStat(myInfo.baseStats[Stat.Power], Stat.Power));
		actualStats.Add(Stat.Defense, new ActualStat(myInfo.baseStats[Stat.Defense], Stat.Defense));
        actualStats.Add(Stat.Resistance, new ActualStat(myInfo.baseStats[Stat.Resistance], Stat.Resistance));
        actualStats.Add(Stat.Agility, new ActualStat(myInfo.baseStats[Stat.Agility], Stat.Agility));
    }

	void AddActiveSkill(ActiveSkill skill, List<UnitStatusEffectInfo> statusEffectInfoList, List<TileStatusEffectInfo> tileStatusEffectInfoList){
		skill.ApplyUnitStatusEffectList(statusEffectInfoList, PartyData.level);
        skill.ApplyTileStatusEffectList(tileStatusEffectInfoList, PartyData.level);
        activeSkillList.Add(skill);
	}

	void AddPassiveSkill(PassiveSkill skill, List<UnitStatusEffectInfo> statusEffectInfoList){
		skill.ApplyUnitStatusEffectList(statusEffectInfoList, PartyData.level);
        passiveSkillList.Add(skill);
	}

	public void ApplySkillList(List<SelectedUnit> selectInfo, List<UnitStatusEffectInfo> statusEffectInfoList, List<TileStatusEffectInfo> tileStatusEffectInfoList){
		int partyLevel = GameData.PartyData.level;
		selectInfo.Find(info => info.name == myInfo.nameEng).selectedSkills.ForEach(skill => {
			if(skill is ActiveSkill){
				AddActiveSkill((ActiveSkill)skill, statusEffectInfoList, tileStatusEffectInfoList);
			}else{
				AddPassiveSkill((PassiveSkill)skill, statusEffectInfoList);
			}
		});
	}
    public void ApplySkillList(List<ActiveSkill> activeSkills, List<UnitStatusEffectInfo> statusEffectInfoList,
                               List<TileStatusEffectInfo> tileStatusEffectInfoList, List<PassiveSkill> passiveSkills){
        int partyLevel = GameData.PartyData.level;

        foreach (var activeSkill in activeSkills) {
            if (activeSkill.owner == myInfo.nameEng && activeSkill.requireLevel <= partyLevel){
				AddActiveSkill(activeSkill, statusEffectInfoList, tileStatusEffectInfoList);
                /*activeSkill.ApplyUnitStatusEffectList(statusEffectInfoList, partyLevel);
                activeSkill.ApplyTileStatusEffectList(tileStatusEffectInfoList, partyLevel);
                activeSkillList.Add(activeSkill);*/
			}
        }

		if(SceneData.stageNumber >= Setting.passiveOpenStage){
			foreach (var passiveSkill in passiveSkills) {
        	    if (passiveSkill.owner == myInfo.nameEng && passiveSkill.requireLevel <= partyLevel){
					AddPassiveSkill(passiveSkill, statusEffectInfoList);
            	    //passiveSkill.ApplyUnitStatusEffectList(statusEffectInfoList, partyLevel);
	                //passiveSkillList.Add(passiveSkill);
    	        }
        	}
		}
		

        // 비어있으면 디폴트 스킬로 채우도록.
        if (activeSkills.Count() == 0) {
            foreach (var activeSkill in activeSkills) {
                if (activeSkill.owner == "default" && activeSkill.requireLevel <= partyLevel)
                    activeSkillList.Add(activeSkill);
            }
		}
    }
    UnitManager unitManager;
	void Initialize(){
		gameObject.name = myInfo.nameEng;
		position = myInfo.initPosition;
		startPositionOfPhase = position;
		direction = myInfo.initDirection;
		UpdateSpriteByDirection();
		currentHealth = GetMaxHealth();
		unitManager = FindObjectOfType<UnitManager>();
		activityPoint = (int)(GetStat(Stat.Agility) * 0.5f) + unitManager.GetStandardActivityPoint();
		//Info에 넣어뒀던 변동사항 적용
		foreach(KeyValuePair<Stat, int> change in myInfo.InitStatChanges){
			if(change.Key == Stat.CurrentHealth){
				currentHealth = GetMaxHealth() * (100-change.Value) / 100;
			}else if(change.Key == Stat.CurrentAP){
				activityPoint += change.Value;
			}
		}
		
		// 기본민첩성이 0인 유닛은 시작시 행동력이 0
		if (myInfo.baseStats[Stat.Agility] == 0){
			activityPoint = 0;
		}
		
		// skillList = SkillLoader.MakeSkillList();

		statusEffectList = new List<UnitStatusEffect>();
		latelyHitInfos = new List<HitInfo>();

		healthViewer.SetInitHealth(GetMaxHealth(), myInfo.side);
	}

	void LoadSprite(){
		UnityEngine.Object[] sprites = Resources.LoadAll("UnitImage/" + myInfo.nameEng);

		if (sprites.Length == 0){
			if (myInfo.side == Side.Ally) {sprites = Resources.LoadAll("UnitImage/notFound");}
			else {sprites = Resources.LoadAll("UnitImage/notFound_enemy");}
		}
		spriteLeftUp = sprites[1] as Sprite;
		spriteLeftDown = sprites[3] as Sprite;
		spriteRightUp = sprites[4] as Sprite;
		spriteRightDown = sprites[2] as Sprite;
		// GetComponent<SpriteRenderer>().sprite = spriteLeftUp;
	}

	void Awake(){
		damageTextObject = transform.Find("DamageText").gameObject;
		recoverTextObject = transform.Find("RecoverText").gameObject;
		activeArrowIcon = transform.Find("ActiveArrowIcon").gameObject;
		chainAttackerIcon = transform.Find("icons/chain").gameObject;
		healthViewer = transform.Find("HealthBar").GetComponent<HealthViewer>();
	}

	void Start(){
		ApplyStats();
		LoadSprite();
		Initialize();

		if(isAI){
			GetComponent<Battle.Turn.AI>().CheckActiveTrigger();
		}
		CheckAndHideObjectHealth();
	}

	void Update(){
		if (Input.GetKeyDown(KeyCode.P))
			RegenerateActionPoint();

		if (Input.GetKeyDown(KeyCode.L)){
			String log = myInfo.nameKor + "\n";
			foreach (var skill in activeSkillList)
			{
				log += skill.GetName() + "\n";
			}
			Debug.LogError(log);

			string passiveLog = myInfo.nameKor + "\n";
			foreach (var passiveSkill in passiveSkillList)
			{
				passiveLog += passiveSkill.GetName() + "\n";
			}
			Debug.LogError(passiveLog);
		}

		if (IsObject) return; // 연산을 최소화하기 위해 오브젝트는 건너뛰고 구현
		else if (bindIcon.enabled == false && HasStatusEffect(StatusEffectType.Bind)){
			bindIcon.enabled = true;
		}
		else if (bindIcon.enabled == true && !HasStatusEffect(StatusEffectType.Bind)){
			bindIcon.enabled = false;
		}
	}

	public void CheckAndHideObjectHealth(){
		if(IsObject && GetSide() == Side.Neutral){
			healthViewer.gameObject.SetActive(false);
		}
	}

	public bool CheckReach(){
		return GetTileUnderUnit().IsReachPoint && FindObjectOfType<BattleTriggerManager>().ActiveTriggers.Any(trig => trig.actionType == TrigActionType.Reach);
	}
}