﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using Enums;
using Battle.Skills;

using Save;

public class HitInfo
{
	public readonly Unit caster;
	public readonly Skill skill;

	public HitInfo(Unit caster, Skill skill)
	{
		this.caster = caster;
		this.skill = skill;
	}
}

public class Unit : MonoBehaviour
{
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

	new string name; // 한글이름
	string nameInCode; // 영어이름

	public Side side; // 진영. 적/아군
	bool isObject; // '지형지물' 여부. 지형지물은 방향에 의한 추가피해를 받지 않는다.

	// 스킬리스트.
	List<Skill> skillList = new List<Skill>();
	List<PassiveSkill> passiveSkillList = new List<PassiveSkill>();
	// 사용한 스킬 정보 저장(쿨타임 산정용).
	Dictionary<string, int> usedSkillDict = new Dictionary<string, int>();

    // 효과 리스트
    List<StatusEffect> statusEffectList = new List<StatusEffect>();

	// 유닛 배치할때만 사용
	Vector2 initPosition;
    
	int baseHealth; // 체력
	int basePower; // 공격력
	int baseDefense; // 방어력
	int baseResistance; // 저항력
	int baseDexturity; // 행동력
    Dictionary<Stat, int> baseStats;

    class ActualStat {
        public int value;
        public Stat stat;
        public List<StatusEffect> appliedStatusEffects;

        public ActualStat(int value, Stat stat) {
            this.value = value;
            this.stat = stat;
            this.appliedStatusEffects = new List<StatusEffect>();
        }
    }
    ActualStat actualHealth;
    ActualStat actualPower;
    ActualStat actualDefense;
    ActualStat actualResistance;
    ActualStat actualDexturity;
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
    bool hasMovedThisTurn;
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
	public void SetChargeEffect(GameObject effect)
	{
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
    public int GetRegenerationAmount() { return GetStat(Stat.Dexturity); }
    public void SetActive() { activeArrowIcon.SetActive(true); }
	public void SetInactive() { activeArrowIcon.SetActive(false); }
	public Vector2 GetInitPosition() { return initPosition; }
	public List<Skill> GetSkillList() { return skillList; }
	public List<Skill> GetLearnedSkillList()
	{
		var learnedSkills =
			 from skill in skillList
			// where SkillDB.IsLearned(nameInCode, skill.GetName())
			 select skill;
		return learnedSkills.ToList();
	}
	public List<PassiveSkill> GetLearnedPassiveSkillList() { return passiveSkillList; }
    public List<StatusEffect> GetStatusEffectList() { return statusEffectList; }
	public void SetStatusEffectList(List<StatusEffect> newStatusEffectList) { statusEffectList = newStatusEffectList; }
	public int GetMaxHealth() { return actualHealth.value; }
    public int GetCurrentHealth() { return currentHealth; }
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
	public string GetName() { return name; }
	public void SetName(string name) { this.name = name; }
	public Side GetSide() { return side; }	
	public bool IsObject() { return isObject; }
    public Vector2 GetPosition() { return position; }
    public void SetPosition(Vector2 position) { this.position = position; }
    public Vector2 GetStartPositionOfPhase() { return startPositionOfPhase; }
    public bool GetHasMovedThisTurn() { return hasMovedThisTurn; }
    public bool GetHasUsedSkillThisTurn() { return hasUsedSkillThisTurn; }
    public void SetHasUsedSkillThisTurn(bool hasUsedSkillThisTurn) { this.hasUsedSkillThisTurn = hasUsedSkillThisTurn; }
    public Dictionary<string, int> GetUsedSkillDict() {return usedSkillDict;}
    public Direction GetDirection() { return direction; }
    public void SetDirection(Direction direction) {
		this.direction = direction;
		UpdateSpriteByDirection();
	}
	public void ApplySnapshot(Tile before, Tile after, Direction direction, int snapshotAp)
	{
		before.SetUnitOnTile(null);
		transform.position = after.transform.position + new Vector3(0, 0, -0.05f);
		SetPosition(after.GetTilePos());
		SetDirection(direction);
		after.SetUnitOnTile(this);
		this.activityPoint = snapshotAp;
		unitManager.UpdateUnitOrder();
	}

	public void ApplyMove(Tile tileBefore, Tile tileAfter, Direction direction, int costAp)
	{
        hasMovedThisTurn = true;
		tileBefore.SetUnitOnTile(null);
		transform.position = tileAfter.transform.position + new Vector3(0, 0, -0.05f);
		SetPosition(tileAfter.GetTilePos());
		SetDirection(direction);
		tileAfter.SetUnitOnTile(this);
		UseActivityPoint(costAp);

		BattleTriggerChecker.CountBattleCondition(this, tileAfter);

        foreach (StatusEffect statusEffect in GetStatusEffectList()) {
            if ((statusEffect.IsOfType(StatusEffectType.RequireMoveAPChange) ||
                statusEffect.IsOfType(StatusEffectType.SpeedChange)) && statusEffect.GetIsOnce() == true) {
                RemoveStatusEffect(statusEffect);
            }
        }
        SkillLogicFactory.Get(passiveSkillList).TriggerOnMove(this);
        foreach (var statusEffect in GetStatusEffectList()) {
            PassiveSkill originPassiveSkill = statusEffect.GetOriginPassiveSkill();
            if (originPassiveSkill != null)
                SkillLogicFactory.Get(originPassiveSkill).TriggerStatusEffectsOnMove(this, statusEffect);
        }
        updateStats();
    }

    public void updateStats() {
        foreach (var actualStat in actualStats.Values) {
            Stat statType = actualStat.stat;
            StatusEffectType statusEffectType = EnumConverter.GetCorrespondingStatusEffectType(statType);
            if (statusEffectType != StatusEffectType.Etc)
                actualStat.value = (int)CalculateActualAmount(baseStats[statType], statusEffectType);
        }
    }
    public void updateStats(StatusEffect statusEffect, bool isApplied, bool isRemoved) {
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
    }
    
    public void UpdateHealthViewer() {
        healthViewer.UpdateCurrentHealth(currentHealth, GetRemainShield(), GetMaxHealth());
    }
	public void AddSkillCooldown(int phase)
	{
		Dictionary<string, int> newUsedSkillDict = new Dictionary<string, int>();
		foreach (var skill in skillList)
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

	public void SetStatusEffect(StatusEffect statusEffect)
	{
		statusEffectList.Add(statusEffect);
		// 침묵이나 기절상태가 될 경우 체인 해제.
		// FIXME : 넉백 추가할 것. (넉백은 디버프가 아니라서 다른 곳에서 적용할 듯?)
		if (statusEffect.IsOfType(StatusEffectType.Faint) ||
			statusEffect.IsOfType(StatusEffectType.Silence))
		{
			ChainList.RemoveChainsFromUnit(this);
		}
	}

	// searching certain StatusEffect
	public bool HasStatusEffect(StatusEffect statusEffect)
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

    public void RemoveStatusEffect(StatusEffect statusEffect) {
        bool toBeRemoved = true;

        toBeRemoved = SkillLogicFactory.Get(passiveSkillList).TriggerStatusEffectRemoved(statusEffect, this);
        Skill originSkill = statusEffect.GetOriginSkill();
        if (originSkill != null) {
            toBeRemoved = SkillLogicFactory.Get(originSkill).TriggerStatusEffectRemoved(statusEffect, this);
        }
        if (toBeRemoved) {
            Debug.Log(statusEffect.GetDisplayName() + " is removed from " + this.nameInCode);
            statusEffectList = statusEffectList.FindAll(se => se != statusEffect);
            updateStats(statusEffect, false, true);
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
        foreach (var statusEffect in statusEffectList)
            for (int i = 0; i < statusEffect.fixedElem.actuals.Count; i++)
                if (statusEffect.IsOfType(i, statusEffectType)) {
                    float amount = statusEffect.GetAmount(i);
                    if(statusEffect.GetIsPercent(i)) {
                        amount = amount/100;
                    }
                    appliedChangeList.Add(new StatChange(statusEffect.GetIsMultiply(i), amount));
                }
        
        // TileStatusEffect로 인한 변동값 계산
        Tile tile = GetTileUnderUnit();
        foreach (var tileStatusEffect in tile.GetStatusEffectList()) 
            for (int i = 0; i < tileStatusEffect.fixedElem.actuals.Count; i++)
                if(tileStatusEffect.IsOfType(i, statusEffectType))
                    appliedChangeList.Add(new StatChange(tileStatusEffect.GetIsMultiply(), tileStatusEffect.GetAmount(i)));

		// 상대값 공격력 변동 특성 영향 합산 (무조건 곱연산)			
		if (statusEffectType == StatusEffectType.PowerChange)
		{
			List<PassiveSkill> passiveSkills = this.GetLearnedPassiveSkillList();
			float relativePowerBonus = SkillLogicFactory.Get(passiveSkills).GetAdditionalRelativePowerBonus(this);
            relativePowerBonus = (relativePowerBonus - 1) * 100;
            appliedChangeList.Add(new StatChange(true, relativePowerBonus));

            // 불속성 유닛이 불 타일 위에 있을 경우 공격력 * 1.2
            if (element == Element.Fire && GetTileUnderUnit().GetTileElement() == Element.Fire) {
                appliedChangeList.Add(new StatChange(true, 1.2f));
            }
        }

		// 방어력 변동 특성 영향 합산
		if (statusEffectType == StatusEffectType.DefenseChange || statusEffectType == StatusEffectType.ResistanceChange) {
            List<PassiveSkill> passiveSkills = this.GetLearnedPassiveSkillList();
            if (statusEffectType == StatusEffectType.DefenseChange) {
                float additiveDefenseBouns = SkillLogicFactory.Get(passiveSkills).GetAdditionalAbsoluteDefenseBonus(this);
                appliedChangeList.Add(new StatChange(false, additiveDefenseBouns));
            }
            else if(statusEffectType == StatusEffectType.ResistanceChange) {
                float additiveResistanceBouns = SkillLogicFactory.Get(passiveSkills).GetAdditionalAbsoluteResistanceBonus(this);
                appliedChangeList.Add(new StatChange(false, additiveResistanceBouns));
            }

            // 금속성 유닛이 금타일 위에 있을경우 방어/저항 +30 
            if (element == Element.Metal && GetTileUnderUnit().GetTileElement() == Element.Metal) {
                appliedChangeList.Add(new StatChange(false, 30));
            }
        }

        if (statusEffectType == StatusEffectType.SpeedChange) {
            if (element == Element.Water && GetTileUnderUnit().GetTileElement() == Element.Water) {
                appliedChangeList.Add(new StatChange(false, 15));
            }
        }

		return CalculateThroughChangeList(data, appliedChangeList);
	}

    public int GetRemainShield() {
        float remainShieldAmount = 0;
        foreach(StatusEffect statusEffect in statusEffectList) {
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

	public void UpdateStartPosition()
	{
		startPositionOfPhase = this.GetPosition();
        hasMovedThisTurn = false;
        hasUsedSkillThisTurn = false;
	}
    
	public IEnumerator Damaged(float damage, Unit caster, float additionalDefense, float additionalResistance, bool isHealth) {
        int finalDamage = (int) damage;
        float defense = GetStat(Stat.Defense) + additionalDefense;
        float resistance = GetStat(Stat.Resistance) + additionalResistance;

        if (isHealth) {
            // 피격자의 효과/특성으로 인한 대미지 증감 효과 적용 - 아직 미완성
            damage = CalculateActualAmount(damage, StatusEffectType.TakenDamageChange);
            // 방어력 및 저항력 적용
            damage = Battle.DamageCalculator.ApplyDefenseAndResistance(damage, caster.GetUnitClass(), defense, resistance);

            // 실드 차감. 먼저 걸린 실드부터 차감.
            Dictionary<StatusEffect, int> attackedShieldDict = new Dictionary<StatusEffect, int>();
            foreach (var se in statusEffectList) {
                int num = se.fixedElem.actuals.Count;
                for (int i = 0; i < num; i++) {
                    if (se.GetStatusEffectType(i) == StatusEffectType.Shield) {
                        float remainShieldAmount = se.GetRemainAmount(i);
                        if (remainShieldAmount >= (int)damage) {
                            se.SubAmount(i, (int)damage);
                            attackedShieldDict.Add(se, (int)damage);

                            damage = 0;
                        } else {
                            se.SubAmount(i, remainShieldAmount);
                            attackedShieldDict.Add(se, (int)remainShieldAmount);

                            RemoveStatusEffect(se);
                            damage -= remainShieldAmount;
                        }
                    }
                }
                if (damage == 0) break;
            }

            finalDamage = (int)damage;

            if (finalDamage > 0) {
                currentHealth -= finalDamage;
                latelyHitInfos.Add(new HitInfo(caster, null));

                // 데미지를 받을 때 발동하는 피격자 특성
                SkillLogicFactory.Get(passiveSkillList).TriggerDamaged(this, finalDamage, caster);
            }


            if (currentHealth < 0)
                currentHealth = 0;

            Debug.Log(finalDamage + " damage applied to " + GetName());

            damageTextObject.SetActive(true);
            damageTextObject.GetComponent<CustomWorldText>().text = finalDamage.ToString();
            damageTextObject.GetComponent<CustomWorldText>().ApplyText();

            UpdateHealthViewer();

            yield return new WaitForSeconds(1);

            damageTextObject.SetActive(false);

            foreach (var kv in attackedShieldDict) {
                StatusEffect statusEffect = kv.Key;
                Skill originSkill = statusEffect.GetOriginSkill();
                if (originSkill != null)
                    yield return StartCoroutine(SkillLogicFactory.Get(originSkill).TriggerShieldAttacked(this, kv.Value));
            }

        } else {
            if (activityPoint >= finalDamage) {
                activityPoint -= finalDamage;
            } else activityPoint = 0;
            Debug.Log(GetName() + " loses " + finalDamage + "AP.");
        }
    }

	public IEnumerator DamagedBySkill(SkillInstanceData skillInstanceData, bool isHealth) {
        Unit caster = skillInstanceData.GetCaster();
        Unit target = skillInstanceData.GetMainTarget();
        Skill appliedSkill = skillInstanceData.GetSkill();
        float damage = skillInstanceData.GetDamage().resultDamage;
        // 체력 깎임
        // 체인 해제
        float defense = Battle.DamageCalculator.CalculateDefense(appliedSkill, target, caster);
        float resistance = Battle.DamageCalculator.CalculateResistance(appliedSkill, target, caster);
        if (isHealth == true) {
			// 대상에게 스킬로 데미지를 줄때 발동하는 공격자 특성
			var passiveSkillsOfAttacker = caster.GetLearnedPassiveSkillList();
			SkillLogicFactory.Get(passiveSkillsOfAttacker).TriggerActiveSkillDamageApplied(caster, this);
		}
        yield return StartCoroutine(Damaged(damage, caster, defense - GetStat(Stat.Defense), resistance - GetStat(Stat.Resistance), isHealth));
		unitManager.UpdateUnitOrder();
	}

	public IEnumerator ApplyDamageOverPhase()
	{
		foreach (var se in statusEffectList)
		{
			int actuals = se.fixedElem.actuals.Count;
			for (int i = 0; i < actuals; i++)
			{
				if (se.IsOfType(i, StatusEffectType.DamageOverPhase))
				{
					BattleManager.MoveCameraToUnit(this);

					float damage = se.GetAmount(i);
					Unit caster = se.GetCaster();
					yield return StartCoroutine(Damaged(damage, caster, 0, 0, true));
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

		// 초과회복량 차감
		int actualAmount = (int)amount;
		if (currentHealth + actualAmount > maxHealth)
		{
			actualAmount = actualAmount - (currentHealth + actualAmount - maxHealth);
		}

		currentHealth += actualAmount;
		if (currentHealth > maxHealth)
			currentHealth = maxHealth;

		recoverTextObject.SetActive(true);
		recoverTextObject.GetComponent<TextMesh>().text = ((int)amount).ToString();

		UpdateHealthViewer();

		// 회복량 표시되는 시간. (회복량이 0일때는 딜레이 없음)
		if (amount > 0)
			yield return new WaitForSeconds(1);
		else
			yield return null;
		recoverTextObject.SetActive(false);
	}

	public IEnumerator RecoverActionPoint(int amount)
	{
		activityPoint += amount;

		damageTextObject.SetActive(true);
		damageTextObject.GetComponent<CustomWorldText>().text = amount.ToString();

		// recoverTextObject.SetActive(true);
		// recoverTextObject.GetComponent<TextMesh>().text = amount.ToString();

		// healthViewer.UpdateCurrentActivityPoint(currentHealth, maxHealth);

		// 회복량 표시되는 시간.
		yield return new WaitForSeconds(1);
		// recoverTextObject.SetActive(false);
		damageTextObject.SetActive(false);
		unitManager.UpdateUnitOrder();
	}

	public void RegenerateActionPoint()
	{
		activityPoint = GetRegeneratedActionPoint();
		Debug.Log(name + " recover " + actualDexturity.value + "AP. Current AP : " + activityPoint);
	}

	public int GetRegeneratedActionPoint()
	{
		return activityPoint + GetRegenerationAmount(); // 페이즈당 행동력 회복량 = 민첩성 * 보정치(버프/디버프)
	}

    public int GetActualRequireSkillAP(Skill selectedSkill)
    {
        int requireSkillAP = selectedSkill.GetRequireAP();

		// 기술 자체에 붙은 행동력 소모 증감효과 적용
		requireSkillAP = SkillLogicFactory.Get(selectedSkill).CalculateAP(requireSkillAP, this);

        // 행동력(기술) 소모 증감 효과 적용
        if (HasStatusEffect(StatusEffectType.RequireSkillAPChange) || HasStatusEffect(StatusEffectType.SpeedChange)) {
			requireSkillAP = (int) CalculateActualAmount(requireSkillAP, StatusEffectType.RequireSkillAPChange);
            float speed = CalculateActualAmount(100, StatusEffectType.SpeedChange);
            requireSkillAP = (int)(requireSkillAP * (100f / speed));
        }

		// 스킬 시전 유닛의 모든 행동력을 요구하는 경우
		if (selectedSkill.GetRequireAP() == 1000)
		{
			requireSkillAP = GetCurrentActivityPoint();
		}

        return requireSkillAP;
    }

	public void UseActivityPoint(int amount)
	{
		activityPoint -= amount;
		Debug.Log(name + " use " + amount + "AP. Current AP : " + activityPoint);
		unitManager.UpdateUnitOrder();
	}

	public IEnumerator ApplyTriggerOnPhaseStart(int phase)
	{
		yield return SkillLogicFactory.Get(passiveSkillList).TriggerOnPhaseStart(this, phase);
        foreach (StatusEffect statusEffect in statusEffectList) {
            if (statusEffect.GetOriginSkill() != null) {
                SkillLogicFactory.Get(statusEffect.GetOriginSkill()).TriggerStatusEffectAtPhaseStart(this, statusEffect);
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

    public void TriggerTileStatusEffectAtTurnEnd() {
        Tile tile = GetTileUnderUnit();
        foreach (var tileStatusEffect in tile.GetStatusEffectList()) {
            Skill originSkill = tileStatusEffect.GetOriginSkill();
            if (originSkill != null) {
                SkillLogicFactory.Get(originSkill).TriggerTileStatusEffectAtTurnEnd(this, tile, tileStatusEffect);
            }
        }
    }

	public void GetKnockedBack(BattleData battleData, Tile destTile)
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
    
	// 보너스 텍스트 표시.
	public void PrintDirectionBonus(Battle.DamageCalculator.AttackDamage attackDamage)
	{
		directionBonusTextObject.SetActive(true);
		if (attackDamage.attackDirection == DirectionCategory.Side)
			directionBonusTextObject.GetComponentInChildren<Text>().text = "측면 공격 (x" + attackDamage.directionBonus + ")";
		else if (attackDamage.attackDirection == DirectionCategory.Back)
			directionBonusTextObject.GetComponentInChildren<Text>().text = "후면 공격 (x" + attackDamage.directionBonus + ")";
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

	void ApplyStats()
	{
		float partyLevel = (float)GameData.PartyData.level;

        actualHealth = new ActualStat(baseHealth, Stat.MaxHealth);
        actualPower  = new ActualStat(basePower, Stat.Power);
        actualDefense = new ActualStat(baseDefense, Stat.Defense);
        actualResistance = new ActualStat(baseResistance, Stat.Resistance);
        actualDexturity = new ActualStat(baseDexturity, Stat.Dexturity);
        actualStats = new Dictionary<Stat, ActualStat>();
        actualStats.Add(Stat.MaxHealth, actualHealth);
        actualStats.Add(Stat.Power, actualPower);
        actualStats.Add(Stat.Defense, actualDefense);
        actualStats.Add(Stat.Resistance, actualResistance);
        actualStats.Add(Stat.Dexturity, actualDexturity);
    }
    public void ApplyUnitInfo(UnitInfo unitInfo) {
        this.name = unitInfo.name;
        this.nameInCode = unitInfo.nameInCode;
        this.side = unitInfo.side;
        this.initPosition = unitInfo.initPosition;
        this.direction = unitInfo.initDirection;
        this.baseHealth = unitInfo.baseHealth;
        this.basePower = unitInfo.basePower;
        this.baseDefense = unitInfo.baseDefense;
        this.baseResistance = unitInfo.baseResistance;
        this.baseDexturity = unitInfo.baseAgility;
        this.baseStats = new Dictionary<Stat, int>();
        baseStats.Add(Stat.MaxHealth, baseHealth);
        baseStats.Add(Stat.Power, basePower);
        baseStats.Add(Stat.Defense, baseDefense);
        baseStats.Add(Stat.Resistance, baseResistance);
        baseStats.Add(Stat.Dexturity, baseDexturity);
        this.unitClass = unitInfo.unitClass;
        this.element = unitInfo.element;
        this.celestial = unitInfo.celestial;
        this.isObject = unitInfo.isObject;
    }
    public void ApplySkillList(List<SkillInfo> skillInfoList,
                               List<StatusEffectInfo> statusEffectInfoList,
                               List<TileStatusEffectInfo> tileStatusEffectInfoList,
                               List<PassiveSkillInfo> passiveSkillInfoList) {
        int partyLevel = GameData.PartyData.level;

        foreach (var skillInfo in skillInfoList) {
            if ((skillInfo.GetOwner() == nameInCode) &&
                (skillInfo.GetRequireLevel() <= partyLevel)) {
                Skill skill = skillInfo.GetSkill();
                // if(SkillDB.IsLearned(this.nameInCode, skill.GetName()))
                {
                    skill.ApplyStatusEffectList(statusEffectInfoList, partyLevel);
                    skill.ApplyTileStatusEffectList(tileStatusEffectInfoList, partyLevel);
                    skillList.Add(skill);
                }
            }

        }
        // 비어있으면 디폴트 스킬로 채우도록.
        if (skillList.Count() == 0) {
            foreach (var skillInfo in skillInfoList) {
                if ((skillInfo.GetOwner() == "default") &&
                    (skillInfo.GetRequireLevel() <= partyLevel))
                    skillList.Add(skillInfo.GetSkill());
            }
        }

        foreach (var passiveSkillInfo in passiveSkillInfoList) {
            //Debug.LogError("Passive skill name " + passiveSkillInfo.name);
            if ((passiveSkillInfo.GetOwner() == nameInCode) &&
                (passiveSkillInfo.GetRequireLevel() <= partyLevel)) {
                PassiveSkill passiveSkill = passiveSkillInfo.GetSkill();
                passiveSkill.ApplyStatusEffectList(statusEffectInfoList, partyLevel);
                passiveSkillList.Add(passiveSkill);
            }
        }
    }
    UnitManager unitManager;
	void Initialize()
	{
		gameObject.name = nameInCode;

		position = initPosition;
		startPositionOfPhase = position;
		UpdateSpriteByDirection();
		currentHealth = GetMaxHealth();
		unitManager = FindObjectOfType<UnitManager>();
		activityPoint = (int)(actualDexturity.value * 0.5f) + unitManager.GetStandardActivityPoint();
		
		// 기본민첩성이 0인 유닛은 시작시 행동력이 0
		if (baseDexturity == 0)
			activityPoint = 0;
		
		// skillList = SkillLoader.MakeSkillList();

		statusEffectList = new List<StatusEffect>();
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
		// FIXME : 초기 방향에 따라 스프라이트 지정되도록 기능 추가. -> 필요없음. 아래의 Initialize에서 해결.
		// GetComponent<SpriteRenderer>().sprite = spriteLeftUp;
	}

	// Use this for initialization
	void Start()
	{
		ApplyStats();
		LoadSprite();
		Initialize();

		chainBonusTextObject.SetActive(false);
		celestialBonusTextObject.SetActive(false);
		directionBonusTextObject.SetActive(false);
		heightBonusTextObject.SetActive(false);
	}

	void Awake()
	{
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

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.R))
		{
			RegenerateActionPoint();
		}
		if (Input.GetKeyDown(KeyCode.L))
		{
			String log = name + "\n";
			foreach (var skill in skillList)
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