using UnityEngine;
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
	public GameObject chainBonusTextObject;
	GameObject damageTextObject;
	GameObject recoverTextObject;
	GameObject activeArrowIcon;
	public GameObject celestialBonusTextObject;
	public GameObject directionBonusTextObject;
	public GameObject heightBonusTextObject;
	HealthViewer healthViewer;
	GameObject chainAttackerIcon;

	public List<HitInfo> latelyHitInfos;

	new string name; // 한글이름
	string nameInCode; // 영어이름

	Side side; // 진영. 적/아군
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
	public Vector2 position;
	// 유닛이 해당 페이즈에서 처음 있었던 위치 - 영 패시브에서 체크
	public Vector2 startPositionOfPhase;
    //이 유닛이 이 턴에 움직였을 경우에 true - 큐리 스킬 '재결정'에서 체크
    public bool hasMovedThisTurn;

	public Direction direction;
	public int currentHealth;
	public int activityPoint;

	GameObject chargeEffect;

	Sprite spriteLeftUp;
	Sprite spriteLeftDown;
	Sprite spriteRightUp;
	Sprite spriteRightDown;

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

		Debug.LogWarning(GetNameInCode() +  " Learnedskils" + learnedSkills.Count());
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
        foreach (StatusEffect statusEffect in GetStatusEffectList()) {
            if (statusEffect.GetStatusEffectType() == StatusEffectType.RequireMoveAPChange && statusEffect.GetIsOnce() == true) {
                RemoveStatusEffect(statusEffect);
            }
        }
        updateStats();
    }

    public void updateStats() {
        foreach (var actualStat in actualStats.Values) {
            actualStat.value = (int)CalculateActualStats(actualStat.stat);
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

            statsToUpdate[i].value = (int)CalculateActualStats(statsToUpdate[i].stat);
        }
    }
    public float CalculateActualStats(Stat statType) {
        float result = ApplyTileElement(baseStats[statType], statType);
        result = ApplyTileStatusEffect(result, statType);
        result = CalculateActualAmount(result, EnumConverter.GetCorrespondingStatusEffectType(statType));
        return result;
    }
    public float ApplyTileElement(float statValue, Stat stat) {
        // 불속성 유닛이 불타일 위에 있을경우 공격력 +20%
        if (GetTileUnderUnit() == null) {
            Debug.Log("Null tile Unit's " + transform.position);
        }
        if (element == Element.Fire && GetTileUnderUnit().GetTileElement() == Element.Fire) {
            if (stat == Stat.Power)
                statValue *= 1.2f;
        }

        // 금속성 유닛이 금타일 위에 있을경우 방어/저항 +30 
        if (element == Element.Metal && GetTileUnderUnit().GetTileElement() == Element.Metal) {
            if (stat == Stat.Defense || stat == Stat.Resistance)
                statValue += 30;
        }
        return statValue;
    }
    public float ApplyTileStatusEffect(float statValue, Stat stat) {
        Tile tile = GetTileUnderUnit();
        List<TileStatusEffect> tileStatusEffectList = tile.GetStatusEffectList();
        foreach (var tileStatusEffect in tileStatusEffectList) {
            for (int i = 0; i < tileStatusEffect.fixedElem.actuals.Count; i++) {
                StatusEffectType type = tileStatusEffect.fixedElem.actuals[i].statusEffectType;
                bool isMatch = ((type == StatusEffectType.PowerChange && stat == Stat.Power) ||
                                (type == StatusEffectType.DefenseChange && stat == Stat.Defense) ||
                                (type == StatusEffectType.ResistanceChange && stat == Stat.Resistance));
                if (isMatch && tileStatusEffect.fixedElem.actuals[i].isMultiply)
                    statValue *= 1 + tileStatusEffect.GetAmount(i) / 100;
                if (isMatch && !tileStatusEffect.fixedElem.actuals[i].isMultiply)
                    statValue += tileStatusEffect.GetAmount(i);
            }
        }
        return statValue;
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
        }
    }
    public void RemoveStatusEffect(StatusEffectCategory category, int num)  //해당 category의 statusEffect를 num개 까지 제거
    {
        foreach (var statusEffect in statusEffectList) {
            if (num == 0) break;

            // 자신이 건 효과는 해제할 수 없다 - 기획문서 참조
            if (statusEffect.GetCaster() == this) continue;

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

    public float GetSpeed()
	{
		int speedValue = 100;
		foreach (var statusEffect in statusEffectList)
		{
			int num = statusEffect.fixedElem.actuals.Count;
			for (int i = 0; i < num; i++)
			{
				if (statusEffect.IsOfType(i, StatusEffectType.SpeedChange))
				{
					speedValue += (int)statusEffect.GetAmount(i);	
				}
			}
		}

		return (float)speedValue / 100;
	}

	public float CalculateActualAmount(float data, StatusEffectType statusEffectType)
	{
		float totalAbsoluteValue = 0.0f; // 절대값
		float totalRelativeValue = 1.0f; // 상대값

		// 효과로 인한 변동값 계산
		foreach (var statusEffect in statusEffectList)
		{
			int num = statusEffect.fixedElem.actuals.Count;
			for (int i = 0; i < num; i++)
			{
				if (statusEffect.IsOfType(i, statusEffectType))
				{
					if (statusEffect.GetIsMultiply(i)) // 상대값 합산
					{
						totalRelativeValue *= 1 + statusEffect.GetAmount(i)/100;
					}
					else // 절대값 합산
					{
						totalAbsoluteValue += statusEffect.GetAmount(i);
					}
				}
			}
		}

		// 상대값 공격력 변동 특성 영향 합산
		float additionalPowerBonus = 1.0f;					
		if (statusEffectType == StatusEffectType.PowerChange)
		{
			List<PassiveSkill> passiveSkills = this.GetLearnedPassiveSkillList();
			additionalPowerBonus = SkillLogicFactory.Get(passiveSkills).GetAdditionalRelativePowerBonus(this);
		}
		totalRelativeValue *= additionalPowerBonus;

		// 절대값 방어력 변동 특성 영향 합산
		float additionalDefenseBouns = 0;
		if (statusEffectType == StatusEffectType.DefenseChange)
		{
			List<PassiveSkill> passiveSkills = this.GetLearnedPassiveSkillList();
			additionalDefenseBouns = SkillLogicFactory.Get(passiveSkills).GetAdditionalAbsoluteDefenseBonus(this);
		}
		totalAbsoluteValue += additionalDefenseBouns;

		// 데미지, 저항력, 민첩성, 기타등등...추가할 것			
		
		// this.UpdateStatusEffect();

		return data * totalRelativeValue + totalAbsoluteValue;
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
	}

	// 반사데미지
	public IEnumerator DamagedByReflection()
	{
		yield return null;
	}

	// 지속데미지
	public IEnumerator DamagedByDot(float damage, Unit caster)
	{
		float originDotDamage = damage; // 최종 대미지 (정수로 표시되는)
        
		float defense = GetStat(Stat.Defense);
		float resistance = GetStat(Stat.Resistance);

		if (caster.GetUnitClass() == UnitClass.Melee)
		{
			// 실제 피해 = 원래 피해 x 200/(200+방어력)
			originDotDamage = originDotDamage * 200.0f / (200.0f + defense);
			Debug.Log("Actual melee DOT damage : " + originDotDamage);
		}
		else if (caster.GetUnitClass() == UnitClass.Magic)
		{
			originDotDamage = originDotDamage * 200.0f / (200.0f + resistance);
			Debug.Log("Actual magic DOT damage effect: " + originDotDamage);
		}
		else if (caster.GetUnitClass() == UnitClass.None)
		{
			// actualDamage = actualDamage;
		}	

		// 실드 차감. 먼저 걸린 실드부터 차감.
		foreach (var se in statusEffectList)
		{
			int actuals = se.fixedElem.actuals.Count;
			for (int i = 0; i < actuals; i++)
			{
				if (se.GetStatusEffectType(i) == StatusEffectType.Shield)
				{
					float remainShieldAmount = se.GetRemainAmount(i);
					if (remainShieldAmount >= originDotDamage)
					{
						se.SubAmount(i, originDotDamage);
						originDotDamage = 0;
					}
					else
					{
						se.SubAmount(i, remainShieldAmount);
						originDotDamage -= remainShieldAmount;
					}
				}
			}

			if (originDotDamage == 0) break;
		}

		// 0이 된 실드 제거
		List<StatusEffect> statusEffectsToRemove = new List<StatusEffect>();
		foreach (StatusEffect se in statusEffectList)
		{
			bool isEmptyShield = false;
			int actuals = se.fixedElem.actuals.Count;
			for (int i = 0; i < actuals; i++)
			{
				if (se.GetStatusEffectType(i) == StatusEffectType.Shield &&
					se.GetRemainAmount(i) == 0) {
					isEmptyShield = true;
				}
			}
			if (isEmptyShield)
				statusEffectsToRemove.Add(se);
		}
        foreach(StatusEffect statusEffect in statusEffectsToRemove) 
		    RemoveStatusEffect(statusEffect);

		int finalDamage = (int)originDotDamage;

		if (finalDamage > 0)
		{
			currentHealth -= finalDamage;
			latelyHitInfos.Add(new HitInfo(caster, null));
		}
		if (currentHealth < 0)
			currentHealth = 0;

		Debug.Log("Damage dealt by DOT : " + finalDamage);

		damageTextObject.SetActive(true);
		damageTextObject.GetComponent<CustomWorldText>().text = finalDamage.ToString();

		healthViewer.UpdateCurrentHealth(currentHealth, GetMaxHealth());

		// 데미지 표시되는 시간.
		yield return new WaitForSeconds(0.5f);
		damageTextObject.SetActive(false);

		yield return null;
	}

	public IEnumerator Damaged(SkillInstanceData skillInstanceData, bool isHealth)
	{
		int finalDamage = 0; // 최종 대미지 (정수로 표시되는)
        Unit caster = skillInstanceData.GetCaster();
        Skill appliedSkill = skillInstanceData.GetSkill();
		// 체력 깎임
		// 체인 해제
		if (isHealth == true)
		{
			finalDamage = (int)Battle.DamageCalculator.GetActualDamage(skillInstanceData, isHealth);

			if (finalDamage > 0)
			{
				currentHealth -= finalDamage;

				if (currentHealth < 0)
					currentHealth = 0;
				
				latelyHitInfos.Add(new HitInfo(caster, appliedSkill));
		
				// 대상에게 데미지를 줄때 발동하는 공격자 특성
				var passiveSkillsOfAttacker = caster.GetLearnedPassiveSkillList();
				SkillLogicFactory.Get(passiveSkillsOfAttacker).TriggerActiveSkillDamageApplied(caster, this);

				// 데미지를 받을 때 발동하는 피격자 특성
                SkillLogicFactory.Get(passiveSkillList).TriggerDamaged(this, finalDamage, skillInstanceData.GetCaster());
			}

			Debug.Log("Damage dealt : "+finalDamage);

			damageTextObject.SetActive(true);
			damageTextObject.GetComponent<CustomWorldText>().ActWhenOnEnable();
			damageTextObject.GetComponent<CustomWorldText>().text = finalDamage.ToString();

			healthViewer.UpdateCurrentHealth(currentHealth, GetMaxHealth());

			// 체인 해제
			ChainList.RemoveChainsFromUnit(this);

			// 데미지 표시되는 시간.
			yield return new WaitForSeconds(1);
			damageTextObject.SetActive(false);
		}

		else
		{
			finalDamage = (int) skillInstanceData.GetDamage().resultDamage;
			if (activityPoint >= finalDamage)
			{
				activityPoint -= finalDamage;
			}
			else activityPoint = 0;
			Debug.Log(GetName() + " loses " + finalDamage + "AP.");
		}
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
					yield return StartCoroutine(DamagedByDot(damage, caster));
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
					totalAmount += statusEffect.GetAmount();
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

		healthViewer.UpdateCurrentHealth(currentHealth, maxHealth);

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
        if (this.HasStatusEffect(StatusEffectType.RequireSkillAPChange))
		{
			requireSkillAP = (int) CalculateActualAmount((float)requireSkillAP, StatusEffectType.RequireSkillAPChange);
		}

		// 스킬 시전 유닛의 모든 행동력을 요구하는 경우
		if (selectedSkill.GetRequireAP() == 9999)
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

	public IEnumerator ApplyTriggerOnPhaseStart()
	{
		yield return SkillLogicFactory.Get(passiveSkillList).TriggerOnPhaseStart(this);
        foreach (StatusEffect statusEffect in statusEffectList) {
            if (statusEffect.GetOriginSkill() != null) {
                SkillLogicFactory.Get(statusEffect.GetOriginSkill()).TriggerStatusEffectsAtPhaseStart(this, statusEffect);
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
		float partyLevel = (float)FindObjectOfType<BattleManager>().GetPartyLevel();

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

        // 절대값으로 그대로 넣도록 변경.
        // float actualHealthAcceleration = healthAcceleration + (healthAccelerationInterval * baseHealth);
        // float actualHealthInitialGrowth = healthInitialGrowth + (healthInitialGrowthInterval * baseHealth);
        // float actualHealthStandardValue = healthStandardValue + (healthStandardValueInterval * baseHealth);
        // maxHealth = (int)((actualHealthAcceleration * partyLevel * (partyLevel - 1f) / 2f)
        // 				   + (actualHealthInitialGrowth * partyLevel) + actualHealthStandardValue);
        // float actualPowerAcceleration = powerAcceleration + (powerAccelerationInterval * basePower);
        // float actualPowerInitialGrowth = powerInitialGrowth + (powerInitialGrowthInterval * basePower);
        // float actualPowerStandardValue = powerStandardValue + (powerStandardValueInterval * basePower);
        // power = (int)((actualPowerAcceleration * partyLevel * (partyLevel - 1f) / 2f)
        // 				   + (actualPowerInitialGrowth * partyLevel) + actualPowerStandardValue);
        // float actualDefenseAcceleration = defenseAcceleration + (defenseAccelerationInterval * baseDefense);
        // float actualDefenseInitialGrowth = defenseInitialGrowth + (defenseInitialGrowthInterval * baseDefense);
        // float actualDefenseStandardValue = defenseStandardValue + (defenseStandardValueInterval * baseDefense);
        // defense = (int)((actualDefenseAcceleration * partyLevel * (partyLevel - 1f) / 2f)
        // 				   + (actualDefenseInitialGrowth * partyLevel) + actualDefenseStandardValue);
        // float actualResistanceAcceleration = resistanceAcceleration + (resistanceAccelerationInterval * baseResistance);
        // float actualResistanceInitialGrowth = resistanceInitialGrowth + (resistanceInitialGrowthInterval * baseResistance);
        // float actualResistanceStandardValue = resistanceStandardValue + (resistanceStandardValueInterval * baseResistance);
        // resistance = (int)((actualResistanceAcceleration * partyLevel * (partyLevel - 1f) / 2f)
        // 				   + (actualResistanceInitialGrowth * partyLevel) + actualResistanceStandardValue);
        // float actualDexturityAcceleration = dexturityAcceleration + (dexturityAccelerationInterval * baseDexturity);
        // float actualDexturityInitialGrowth = dexturityInitialGrowth + (dexturityInitialGrowthInterval * baseDexturity);
        // float actualDexturityStandardValue = dexturityStandardValue + (dexturityStandardValueInterval * baseDexturity);
        // dexturity = (int)((actualDexturityAcceleration * partyLevel * (partyLevel - 1f) / 2f)
        // 				   + (actualDexturityInitialGrowth * partyLevel) + actualDexturityStandardValue);
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
        this.baseDexturity = unitInfo.baseDexturity;
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
        int partyLevel = FindObjectOfType<BattleManager>().GetPartyLevel();

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
		if (actualDexturity.value == 0)
		{
			// Manastone is not move
			activityPoint = 0;
		}
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
		GetComponent<SpriteRenderer>().sprite = spriteLeftUp; // FIXME : 초기 방향에 따라 스프라이트 지정되도록 기능 추가.
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
		damageTextObject.SetActive(false);
		recoverTextObject.SetActive(false);
		activeArrowIcon.SetActive(false);
		chainAttackerIcon.SetActive(false);

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