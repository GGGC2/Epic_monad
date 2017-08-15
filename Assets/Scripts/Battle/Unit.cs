using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enums;
using Battle.Skills;
using Battle;
using GameData;

using Save;

public class HitInfo{
	public readonly Unit caster;
	public readonly ActiveSkill skill;
    public readonly int finalDamage;

	public HitInfo(Unit caster, ActiveSkill skill, int finalDamage)
	{
		this.caster = caster;
		this.skill = skill;
        this.finalDamage = finalDamage;
	}
}

public class Unit : MonoBehaviour{
	GameObject damageTextObject;
	GameObject recoverTextObject;
	GameObject activeArrowIcon;
	GameObject chainBonusTextObject;
	GameObject celestialBonusTextObject;
	GameObject directionBonusTextObject;
	GameObject heightBonusTextObject;
	HealthViewer healthViewer;
	GameObject chainAttackerIcon;
	List<HitInfo> latelyHitInfos;

	int index; // 유닛의 고유 인덱스
	new string name; // 한글이름
	string nameInCode; // 영어이름

	public Side side; // 진영. 적/아군
	bool isAI = false; // AI 유닛인지 여부인데, 지형지물은 AI로 분류되지 않으므로 PC인지 확인하려면 !IsAI가 아니라 IsPC(=!isAI && !isObject로 아래에 get 함수로 있음)의 return 값을 받아야 한다
	bool isObject; // '지형지물' 여부. 지형지물은 방향에 의한 추가피해를 받지 않으며 기술이 있을 경우 매 페이즈 모든 유닛의 턴이 끝난 후에 1회 행동한다
	bool isAlreadyBehavedObject; //지형지물(오브젝트)일 때만 의미있는 값. 그 페이즈에 이미 행동했는가

	// 스킬리스트
	public List<ActiveSkill> activeSkillList = new List<ActiveSkill>();
	List<PassiveSkill> passiveSkillList = new List<PassiveSkill>();
	// 사용한 스킬 정보 저장(쿨타임 산정용)
	Dictionary<string, int> usedSkillDict = new Dictionary<string, int>();

    // 효과 리스트
    List<UnitStatusEffect> statusEffectList = new List<UnitStatusEffect>();

	// 유닛 배치할때만 사용
	Vector2 initPosition;
    
    Dictionary<Stat, int> baseStats;

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
    ActualStat actualHealth;
    ActualStat actualPower;
    ActualStat actualDefense;
    ActualStat actualResistance;
    ActualStat actualAgility;
    Dictionary<Stat, ActualStat> actualStats;

	// type.
	UnitClass unitClass;
	Element element;
	Celestial celestial;

	// Variable values.
	Vector2 position;
	// 유닛이 해당 페이즈에서 처음 있었던 위치 - 영 패시브에서 체크
	Vector2 startPositionOfPhase;
    //이 유닛이 이 턴에 움직였을 경우에 true - 큐리 스킬 '재결정'에서 체크
    int notMovedTurnCount;
    //이 유닛이 이 턴에 스킬을 사용했을 경우에 true - 유진 스킬 '여행자의 발걸음', '길잡이'에서 체크
    bool hasUsedSkillThisTurn;

	Direction direction;
	public int currentHealth;
	int activityPoint;

	GameObject chargeEffect;

	Sprite spriteLeftUp;
	Sprite spriteLeftDown;
	Sprite spriteRightUp;
	Sprite spriteRightDown;

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
        if (baseStats.ContainsKey(stat))
            return baseStats[stat];
        return 0;
    }
	public float GetSpeed(){
		float speed = 100;
		if (HasStatusEffect(StatusEffectType.SpeedChange))
			speed = CalculateActualAmount(100, StatusEffectType.SpeedChange);
		return speed;
	}
    public int GetRegenerationAmount() { return GetStat(Stat.Agility); }
    public void SetActive() { activeArrowIcon.SetActive(true); }
	public void SetInactive() { activeArrowIcon.SetActive(false); }
	public bool IsAlreadyBehavedObject(){ return isAlreadyBehavedObject; }
	public void SetNotAlreadyBehavedObject() { isAlreadyBehavedObject = false; }
	public void SetAlreadyBehavedObject() { isAlreadyBehavedObject = true; }
	public Vector2 GetInitPosition() { return initPosition; }
	public List<ActiveSkill> GetSkillList() { return activeSkillList; }
	public List<ActiveSkill> GetLearnedSkillList(){
		var learnedSkills =
			 from skill in activeSkillList
			// where SkillDB.IsLearned(nameInCode, skill.GetName())
			 select skill;
		return learnedSkills.ToList();
	}
	public List<PassiveSkill> GetLearnedPassiveSkillList() { return passiveSkillList; }
    public List<UnitStatusEffect> GetStatusEffectList() { return statusEffectList; }
	public void SetStatusEffectList(List<UnitStatusEffect> newStatusEffectList) { statusEffectList = newStatusEffectList; }
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
	public int GetCurrentActivityPoint() { return activityPoint; }
	public void SetUnitClass(UnitClass unitClass) { this.unitClass = unitClass; }
	public UnitClass GetUnitClass() { return unitClass; }
	public void SetElement(Element element) { this.element = element; }
	public Element GetElement() { return element; }
	public void SetCelestial(Celestial celestial) { this.celestial = celestial; }
	public Celestial GetCelestial() { return celestial; }
    public Tile GetTileUnderUnit() { return FindObjectOfType<TileManager>().GetTile(position); }
	public int GetHeight() { return GetTileUnderUnit().GetTileHeight(); }
	public string GetNameInCode() { return nameInCode; }
	public int GetIndex() { return index; }
	public string GetName() { return name; }
	public void SetName(string name) { this.name = name; }
	public Side GetSide() { return side; }
	public bool IsAlly(Unit unit) { return side == unit.GetSide (); }
	public bool IsSeenAsEnemyToThisAIUnit(Unit unit) { return Battle.Turn.AIUtil.IsSecondUnitEnemyToFirstUnit (unit, this); } //은신 상태에선 적으로 인식되지 않음
	public void SetAsAI() { isAI = true; }
	public bool IsAI { get { return isAI; } }
	public bool IsPC { get { return (!isAI) && (!isObject); } }
	public bool IsObject { get { return isObject; } }
    public Vector2 GetPosition() { return position; }
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
		return activityPoint - GetStandardAP ();
	}
	public int MinAPUseForStandbyForPC(){
		int myAP = activityPoint;
		int maxOtherUnitAP = -1;
		foreach (var anyUnit in BattleData.unitManager.GetAllUnits()){
			int unitAP = anyUnit.GetCurrentActivityPoint ();
			if (anyUnit != this && unitAP > maxOtherUnitAP) {
				maxOtherUnitAP = anyUnit.GetCurrentActivityPoint ();
			}
		}
		int minAPUse;
		if (maxOtherUnitAP == -1) { // 현 페이즈 행동가능 유닛이 본인밖에 없을 경우
			minAPUse = myAP - (GetStandardAP () - 1);
		}
		else if (maxOtherUnitAP == myAP) {
			minAPUse = 1;
		}
		else {
			minAPUse = myAP - maxOtherUnitAP;
		}
		Debug.Log (name + "의 대기를 위한 최소 소모 AP " + minAPUse);
		return minAPUse;
	}
	public bool IsStandbyPossible(){
		bool isPossible = (MinAPUseForStandbyForPC () <= 0);
		return isPossible;
	}
	public bool IsMovePossibleState(){
		bool isPossible =  !(HasStatusEffect(StatusEffectType.Bind) ||
			HasStatusEffect(StatusEffectType.Faint))
			&& !(BattleData.alreadyMoved);
		return isPossible;
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
	public void ApplySnapshot(Tile before, Tile after, Direction direction, int snapshotAp){
		before.SetUnitOnTile(null);
		transform.position = after.transform.position + new Vector3(0, 0, -0.05f);
		SetPosition(after.GetTilePos());
		SetDirection(direction);
		after.SetUnitOnTile(this);
		this.activityPoint = snapshotAp;
		unitManager.UpdateUnitOrder();
	}
    private void ChangePosition(Tile destTile) {
        Tile tileBefore = GetTileUnderUnit();
        tileBefore.SetUnitOnTile(null);
        transform.position = destTile.transform.position + new Vector3(0, 0, -0.05f);
        SetPosition(destTile.GetTilePos());
        destTile.SetUnitOnTile(this);
        notMovedTurnCount = 0;

        SkillLogicFactory.Get(passiveSkillList).TriggerOnMove(this);
        foreach (var statusEffect in GetStatusEffectList()) {
            Skill originPassiveSkill = statusEffect.GetOriginSkill();
            if (originPassiveSkill.GetType() == typeof(PassiveSkill))
                ((PassiveSkill)originPassiveSkill).SkillLogic.TriggerStatusEffectsOnMove(this, statusEffect);
        }
        BattleTriggerManager.CountBattleCondition(this, destTile);
        updateStats();
    }

    public void ForceMove(Tile destTile) { //강제이동
		if (!IsObject && SkillLogicFactory.Get(passiveSkillList).TriggerOnForceMove(this, destTile)) {
			ChangePosition (destTile);
        }
    }

	public void ApplyMove(Tile destTile, Direction finalDirection, int totalAPCost){
		UseActivityPoint (totalAPCost);
		ChangePosition (destTile);
		SetDirection (finalDirection);

        foreach (var statusEffect in GetStatusEffectList()) {
            if ((statusEffect.IsOfType(StatusEffectType.RequireMoveAPChange) ||
                statusEffect.IsOfType(StatusEffectType.SpeedChange)) && statusEffect.GetIsOnce() == true) {
                RemoveStatusEffect(statusEffect);
            }
        }
    }

    private void updateCurrentHealthRelativeToMaxHealth() {
        if(currentHealth > GetMaxHealth())
            currentHealth = GetMaxHealth();
    }

    public void updateStats() {
        foreach (var actualStat in actualStats.Values) {
            Stat statType = actualStat.stat;
            StatusEffectType statusEffectType = EnumConverter.GetCorrespondingStatusEffectType(statType);
            if (statusEffectType != StatusEffectType.Etc)
                actualStat.value = (int)CalculateActualAmount(baseStats[statType], statusEffectType);
        }
        updateCurrentHealthRelativeToMaxHealth();
    }
    public void updateStats(UnitStatusEffect statusEffect, bool isApplied, bool isRemoved) {
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
            statsToUpdate[i].value = (int)CalculateActualAmount(baseStats[statsToUpdate[i].stat], statusEffectType);
        }
        updateCurrentHealthRelativeToMaxHealth();
    }
    
    public void UpdateHealthViewer() {
        healthViewer.UpdateCurrentHealth(currentHealth, GetRemainShield(), GetMaxHealth());
    }
	public void AddSkillCooldown(int phase)
	{
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

	public void SubSkillCooldown(int phase)
	{
		Dictionary<string, int> newUsedSkillDict = new Dictionary<string, int>();
		foreach (var skill in usedSkillDict)
		{
			int updatedCooldown = skill.Value - phase;
			if (updatedCooldown > 0)
				newUsedSkillDict.Add(skill.Key, updatedCooldown);
		}
		usedSkillDict = newUsedSkillDict;
	}

	public void UpdateSkillCooldown()
	{
		Dictionary<string, int> newUsedSkillDict = new Dictionary<string, int>();
		foreach (var skill in usedSkillDict)
		{
			int updatedCooldown = skill.Value - 1;
			if (updatedCooldown > 0)
				newUsedSkillDict.Add(skill.Key, updatedCooldown);
		}
		usedSkillDict = newUsedSkillDict;
	}

	public void SetStatusEffect(UnitStatusEffect statusEffect)
	{
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
	public bool HasStatusEffect(StatusEffectType statusEffectType)
	{
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
            Debug.Log(statusEffect.GetDisplayName() + " is removed from " + this.nameInCode);
            statusEffectList = statusEffectList.FindAll(se => se != statusEffect);
            updateStats(statusEffect, false, true);
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
	public float CalculateActualAmount(float data, StatusEffectType statusEffectType)
	{
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

				// 불속성 유닛이 불 타일 위에 있을 경우 공격력 * 1.2
				if (element == Element.Fire && GetTileUnderUnit ().GetTileElement () == Element.Fire) {
					appliedChangeList.Add (new StatChange (true, 1.2f));
				}
			}

			// 방어력 변동 특성 영향 합산
			if (statusEffectType == StatusEffectType.DefenseChange || statusEffectType == StatusEffectType.ResistanceChange) {
				List<PassiveSkill> passiveSkills = this.GetLearnedPassiveSkillList ();
				if (statusEffectType == StatusEffectType.DefenseChange) {
					float additiveDefenseBouns = SkillLogicFactory.Get (passiveSkills).GetAdditionalAbsoluteDefenseBonus (this);
					appliedChangeList.Add (new StatChange (false, additiveDefenseBouns));
				} else if (statusEffectType == StatusEffectType.ResistanceChange) {
					float additiveResistanceBouns = SkillLogicFactory.Get (passiveSkills).GetAdditionalAbsoluteResistanceBonus (this);
					appliedChangeList.Add (new StatChange (false, additiveResistanceBouns));
				}

				// 금속성 유닛이 금타일 위에 있을경우 방어/저항 +30 
				if (element == Element.Metal && GetTileUnderUnit ().GetTileElement () == Element.Metal) {
					appliedChangeList.Add (new StatChange (false, 30));
				}
			}

			if (statusEffectType == StatusEffectType.SpeedChange) {
				if (element == Element.Water && GetTileUnderUnit ().GetTileElement () == Element.Water) {
					appliedChangeList.Add (new StatChange (false, 15));
				}
			}
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
		yield return BattleData.battleManager.StartCoroutine (ApplyDamage (realDamage, caster, isHealth, ignoreShield));
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
		yield return BattleData.battleManager.StartCoroutine (ApplyDamage (realDamage, caster, isHealth, ignoreShield));
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
			// 실드 차감. 먼저 걸린 실드부터 차감.
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

			Debug.Log(damageAfterShieldApply + " damage applied to " + GetName());

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
			Debug.Log(GetName() + " loses " + damage + "AP.");
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
					yield return FindObjectOfType<BattleManager>().StartCoroutine(ApplyDamageByNonCasting(damage, caster, 0, 0, true, false, isSourceTrap));
                }
            }
        }

        yield return null;
    }

	public IEnumerator ApplyHealOverPhase()
	{
		float totalAmount = 0.0f;

		if (this.HasStatusEffect(StatusEffectType.HealOverPhase))
		{
			foreach (var statusEffect in statusEffectList)
			{
				if (statusEffect.IsOfType(StatusEffectType.HealOverPhase))
				{
                    BattleManager.MoveCameraToUnit(this);
                    totalAmount += statusEffect.GetAmountOfType(StatusEffectType.HealOverPhase);
				}
			}
		}
        if(totalAmount != 0)
		    yield return RecoverHealth(totalAmount);
        else yield return null;
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
		if (!isObject) {
			activityPoint = GetRegeneratedActionPoint ();
			Debug.Log (name + " recover " + GetStat(Stat.Agility) + "AP. Current AP : " + activityPoint);
		}
	}

	public int GetRegeneratedActionPoint()
	{
		return activityPoint + GetRegenerationAmount(); // 페이즈당 행동력 회복량 = 민첩성 * 보정치(버프/디버프)
	}

    public int GetActualRequireSkillAP(ActiveSkill skill)
    {
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

	// FIXME : AI가 공격 외의 기술을 갖게 되는 시점이 오면 reward 함수를 확장해야 한다
	public float CalculateRewardByCastingToThisUnit(Casting casting){
		CastingApply castingApply = new CastingApply (casting, this);
		//FIXME : AI가 연계란 개념을 이용하게 하고 싶으면 아래에서 chainCombo에 1 넣어둔 걸 바꿔야 한다
		DamageCalculator.CalculateAttackDamage(castingApply, 1);
		int damage = CalculateDamageByCasting(castingApply, true);	
		int remainHP = GetCurrentHealth () + GetRemainShield();
		if (!IsObject && SceneData.stageNumber >= Setting.retreatOpenStage)
			remainHP -= GetStat (Stat.MaxHealth) * Setting.retreatHpPercent / 100;
		damage = Math.Min (damage, remainHP);

		float killNeedCount;
		//원턴킬 가능시 보너스로 1이 아니라 작은 값으로 설정
		if (damage == remainHP)
			killNeedCount = 0.3f;
		else
			killNeedCount = remainHP / damage;

		float sideFactor = 0;
		Unit caster = casting.Caster;
		if (IsAlly (caster))
			sideFactor = -0.6f;
		if (IsSeenAsEnemyToThisAIUnit (caster))
			sideFactor = 1;

		float PCFactor = 1;
		if (IsPC)
			PCFactor = 3;

		float reward = 0;
		reward = sideFactor * PCFactor * GetStat (Stat.Power) / killNeedCount;
		//Debug.Log ("Attack to " + name + " is " + ((int)reward) + " point");
		return reward;
	}

	// 1턴 내 공격 못하는 적에 대해 '예상' 가치 구한다
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
		Debug.Log(name + " use " + amount + "AP. Current AP : " + activityPoint);
		unitManager.UpdateUnitOrder();
		UIManager.Instance.UpdateSelectedUnitViewerUI (this);
	}

	public IEnumerator ApplyTriggerOnPhaseStart(int phase){
		yield return SkillLogicFactory.Get(passiveSkillList).TriggerOnPhaseStart(this, phase);
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
        foreach(var tile in FindObjectOfType<TileManager>().GetAllTiles().Values) {
            foreach (var tileStatusEffect in tile.GetStatusEffectList()) {
                Skill originSkill = tileStatusEffect.GetOriginSkill();
                if (originSkill.GetType() == typeof(ActiveSkill)) {
                    ((ActiveSkill)originSkill).SkillLogic.TriggerTileStatusEffectAtTurnStart(this, tile, tileStatusEffect);
                }
            }
        }
    }
    public void TriggerTileStatusEffectAtTurnEnd() {
        foreach (var tile in FindObjectOfType<TileManager>().GetAllTiles().Values) {
            foreach (var tileStatusEffect in tile.GetStatusEffectList()) {
                Skill originSkill = tileStatusEffect.GetOriginSkill();
                if (originSkill.GetType() == typeof(ActiveSkill)) {
                    ((ActiveSkill)originSkill).SkillLogic.TriggerTileStatusEffectAtTurnEnd(this, tile, tileStatusEffect);
                }
            }
        }
    }

	public void GetKnockedBack(Tile destTile)
	{
		Tile currentTile = GetTileUnderUnit();
		currentTile.SetUnitOnTile(null);
		transform.position = destTile.gameObject.transform.position + new Vector3(0, 0, -5f);
		SetPosition(destTile.GetTilePos());
		destTile.SetUnitOnTile(this);
	}

	public void UpdateSpriteByDirection()
	{
		if (direction == Direction.LeftUp)
			GetComponent<SpriteRenderer>().sprite = spriteLeftUp;
		if (direction == Direction.LeftDown)
			GetComponent<SpriteRenderer>().sprite = spriteLeftDown;
		if (direction == Direction.RightUp)
			GetComponent<SpriteRenderer>().sprite = spriteRightUp;
		if (direction == Direction.RightDown)
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
    
	// 보너스 텍스트 표시.
	public void PrintDirectionBonus(DamageCalculator.AttackDamage attackDamage)
	{
		directionBonusTextObject.SetActive (true);
		if (attackDamage.attackDirection == DirectionCategory.Side)
			directionBonusTextObject.GetComponentInChildren<Text> ().text = "측면 공격 (x" + attackDamage.directionBonus + ")";
		else if (attackDamage.attackDirection == DirectionCategory.Back)
			directionBonusTextObject.GetComponentInChildren<Text> ().text = "후면 공격 (x" + attackDamage.directionBonus + ")";
	}
	public void PrintCelestialBonus(float bonus)
	{
		celestialBonusTextObject.SetActive(true);
		// bonusTextObject.GetComponent<TextMesh>().text = "Celestial bonus";
		celestialBonusTextObject.GetComponentInChildren<Text>().text = "천체속성 (x" + bonus + ")";
		// Invoke("ActiveFalseAtDelay", 0.5f);
	}
	public void PrintHeightBonus(float bonus)
	{
		heightBonusTextObject.SetActive(true);
		heightBonusTextObject.GetComponentInChildren<Text>().text = "고저차 (x" + bonus + ")";
	}
	public void ActiveFalseAllBonusText()
	{
		celestialBonusTextObject.SetActive(false);
		chainBonusTextObject.SetActive(false);
		directionBonusTextObject.SetActive(false);
		heightBonusTextObject.SetActive(false);
	}
	public void PrintChainBonus(int chainCount)
	{
		float chainBonus;

		if (chainCount < 2)	chainBonus = 1.0f;
		else if (chainCount == 2) chainBonus = 1.2f;
		else if (chainCount == 3) chainBonus = 1.5f;
		else if (chainCount == 4) chainBonus = 2.0f;
		else chainBonus = 3.0f;

		if (chainCount < 2)	return;

		chainBonusTextObject.SetActive(true);
		chainBonusTextObject.GetComponentInChildren<Text>().text = "연계" + chainCount + "단 (x" + chainBonus + ")";
	}
	public void DisableChainText()
	{
		chainBonusTextObject.SetActive(false);
	}
	public void ShowChainIcon()
	{
		chainAttackerIcon.SetActive(true);
	}
	public void HideChainIcon()
	{
		chainAttackerIcon.SetActive(false);
	}

	void ApplyStats(){
		float partyLevel = (float)GameData.PartyData.level;
        
        actualStats = new Dictionary<Stat, ActualStat>();
        actualStats.Add(Stat.MaxHealth, new ActualStat(baseStats[Stat.MaxHealth], Stat.MaxHealth));
        actualStats.Add(Stat.Power, new ActualStat(baseStats[Stat.Power], Stat.Power));
        actualStats.Add(Stat.Defense, new ActualStat(baseStats[Stat.Defense], Stat.Defense));
        actualStats.Add(Stat.Resistance, new ActualStat(baseStats[Stat.Resistance], Stat.Resistance));
        actualStats.Add(Stat.Agility, new ActualStat(baseStats[Stat.Agility], Stat.Agility));
        actualStats.Add(Stat.Level, new ActualStat(GameData.PartyData.level, Stat.Level));
    }
    public void ApplyUnitInfo(UnitInfo unitInfo) {
        this.index = unitInfo.index;
		this.name = unitInfo.name;
        this.nameInCode = unitInfo.nameInCode;
        this.side = unitInfo.side;
        this.initPosition = unitInfo.initPosition;
        this.direction = unitInfo.initDirection;
        this.baseStats = new Dictionary<Stat, int>();
        baseStats.Add(Stat.MaxHealth, unitInfo.baseHealth);
        baseStats.Add(Stat.Power, unitInfo.basePower);
        baseStats.Add(Stat.Defense, unitInfo.baseDefense);
        baseStats.Add(Stat.Resistance, unitInfo.baseResistance);
        baseStats.Add(Stat.Agility, unitInfo.baseAgility);
        baseStats.Add(Stat.Level, GameData.PartyData.level);
        this.unitClass = unitInfo.unitClass;
        this.element = unitInfo.element;
        this.celestial = unitInfo.celestial;
        this.isObject = unitInfo.isObject;
    }
    public void ApplySkillList(List<ActiveSkill> activeSkills, List<UnitStatusEffectInfo> statusEffectInfoList,
                               List<TileStatusEffectInfo> tileStatusEffectInfoList, List<PassiveSkill> passiveSkills){
        int partyLevel = GameData.PartyData.level;

        foreach (var activeSkill in activeSkills) {
            if (activeSkill.owner == nameInCode && activeSkill.requireLevel <= partyLevel){
                // if(SkillDB.IsLearned(this.nameInCode, skill.GetName()))
                activeSkill.ApplyUnitStatusEffectList(statusEffectInfoList, partyLevel);
                activeSkill.ApplyTileStatusEffectList(tileStatusEffectInfoList, partyLevel);
                activeSkillList.Add(activeSkill);
			}
        }

		foreach (var passiveSkill in passiveSkills) {
            //Debug.LogError("Passive skill name " + passiveSkillInfo.name);
            if (passiveSkill.owner == nameInCode && passiveSkill.requireLevel <= partyLevel){
                passiveSkill.ApplyUnitStatusEffectList(statusEffectInfoList, partyLevel);
                passiveSkillList.Add(passiveSkill);
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
		gameObject.name = nameInCode;

		position = initPosition;
		startPositionOfPhase = position;
		UpdateSpriteByDirection();
		currentHealth = GetMaxHealth();
		unitManager = FindObjectOfType<UnitManager>();
		activityPoint = (int)(GetStat(Stat.Agility) * 0.5f) + unitManager.GetStandardActivityPoint();
		
		// 기본민첩성이 0인 유닛은 시작시 행동력이 0
		if (baseStats[Stat.Agility] == 0)
			activityPoint = 0;
		
		// skillList = SkillLoader.MakeSkillList();

		statusEffectList = new List<UnitStatusEffect>();
		latelyHitInfos = new List<HitInfo>();

		healthViewer.SetInitHealth(GetMaxHealth(), side);
	}

	void LoadSprite()
	{
		UnityEngine.Object[] sprites = Resources.LoadAll("UnitImage/" + nameInCode);

		if (sprites.Length == 0)
		{
			//Debug.LogError("Cannot find sprite for " + nameInCode);
			if (side == Side.Ally)
			{
				sprites = Resources.LoadAll("UnitImage/notFound");
			}
			else
			{
				sprites = Resources.LoadAll("UnitImage/notFound_enemy");
			}
		}
		spriteLeftUp = sprites[1] as Sprite;
		spriteLeftDown = sprites[3] as Sprite;
		spriteRightUp = sprites[4] as Sprite;
		spriteRightDown = sprites[2] as Sprite;
		// GetComponent<SpriteRenderer>().sprite = spriteLeftUp;
	}

	void Awake(){
		chainBonusTextObject = GameObject.Find("ChainBonusPanel");
		damageTextObject = transform.Find("DamageText").gameObject;
		recoverTextObject = transform.Find("RecoverText").gameObject;
		activeArrowIcon = transform.Find("ActiveArrowIcon").gameObject;
		celestialBonusTextObject = GameObject.Find("CelestialBonusPanel");
		chainAttackerIcon = transform.Find("icons/chain").gameObject;
		directionBonusTextObject = GameObject.Find("DirectionBonusPanel");
		heightBonusTextObject = GameObject.Find("HeightBonusPanel");
		healthViewer = transform.Find("HealthBar").GetComponent<HealthViewer>();
	}

	void Start(){
		ApplyStats();
		LoadSprite();
		Initialize();

		chainBonusTextObject.SetActive(false);
		celestialBonusTextObject.SetActive(false);
		directionBonusTextObject.SetActive(false);
		heightBonusTextObject.SetActive(false);
	}

	void Update(){
		if (Input.GetKeyDown(KeyCode.P))
			RegenerateActionPoint();

		if (Input.GetKeyDown(KeyCode.L)){
			String log = name + "\n";
			foreach (var skill in activeSkillList)
			{
				log += skill.GetName() + "\n";
			}
			Debug.LogError(log);

			string passiveLog = name + "\n";
			foreach (var passiveSkill in passiveSkillList)
			{
				passiveLog += passiveSkill.GetName() + "\n";
			}
			Debug.LogError(passiveLog);
		}
	}
}