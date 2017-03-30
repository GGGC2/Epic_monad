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

	// 스킬리스트.
	List<Skill> skillList = new List<Skill>();
	List<PassiveSkill> passiveSkillList = new List<PassiveSkill>();
	// 사용한 스킬 정보 저장.
	Dictionary<string, int> usedSkillDict = new Dictionary<string, int>();

    // 상태이상 리스트
    List<StatusEffect> statusEffectList = new List<StatusEffect>();

	// FIXME : temp values
	Vector2 initPosition;

	// Base stats. FIXME : 지금은 수동으로 셋팅.
	float baseHealth; // 체력
	float basePower; // 공격력
	float baseDefense; // 방어력
	float baseResistance; // 저항력
	float baseDexturity; // 행동력

	// Applied stats.
	int maxHealth;
	int power;
	int defense;
	int resistance;
	int dexturity;

	// type.
	UnitClass unitClass;
	Element element;
	Celestial celestial;

	// Variable values.
	public Vector2 position;
	public Direction direction;
	public int currentHealth;
	public int activityPoint;

	GameObject chargeEffect;

	Sprite spriteLeftUp;
	Sprite spriteLeftDown;
	Sprite spriteRightUp;
	Sprite spriteRightDown;

	public Sprite GetCurrentSprite()
	{
		return GetComponent<SpriteRenderer>().sprite;
	}

	public Sprite GetDefaultSprite()
	{
		return spriteLeftDown;
	}

	public void SetChargeEffect(GameObject effect)
	{
		if (chargeEffect != null) RemoveChargeEffect();
		chargeEffect = effect;
		effect.transform.position = gameObject.transform.position - new Vector3(0, 0, 0.01f);
	}

	public void RemoveChargeEffect()
	{
		Destroy(chargeEffect);
	}

    public int GetStat(Stat stat)
    {
        if(stat == Stat.MaxHealth) {return maxHealth;}
        else if(stat == Stat.Power) {return power;}
        else if(stat == Stat.Defense) {return defense;}
        else if(stat == Stat.Resistance) {return resistance;}
        else if (stat == Stat.Dexturity) {return dexturity;}
        else
        {
          Debug.LogWarning("Cannot get stat of " + stat);
          return 1;
        }
    }

	// 이걸 이용해서 코드를 바꾸자
	class ActualStat
	{
		int statValue;
		List<StatusEffect> appliedStatusEffects;
	}

    public int GetActualStat(Stat stat)
    {
        int actualStat = GetStat(stat);
        StatusEffectType statusChange = (StatusEffectType)Enum.Parse(typeof(StatusEffectType), stat.ToString() + "Change");

		// 능력치 증감 효과 적용
		actualStat = (int) GetActualEffect(actualStat, statusChange);
        
        return actualStat;
    }

	public void SetActive()
	{
		activeArrowIcon.SetActive(true);
	}

	public void SetInactive()
	{
		activeArrowIcon.SetActive(false);
	}

	public Vector2 GetInitPosition()
	{
		return initPosition;
	}

	public List<Skill> GetSkillList()
	{
		return skillList;
	}

	public List<Skill> GetLearnedSkillList()
	{
		var learnedSkills =
			 from skill in skillList
			// where SkillDB.IsLearned(nameInCode, skill.GetName())
			 select skill;

		Debug.LogWarning(GetNameInCode() +  " Learnedskils" + learnedSkills.Count());
		return learnedSkills.ToList();
	}

	public List<PassiveSkill> GetLearnedPassiveSkillList()
	{
		// Learn passive skill is not implemented yet
		return passiveSkillList;
	}

    public List<StatusEffect> GetStatusEffectList()
    {
        return statusEffectList;
    }

	public void SetStatusEffectList(List<StatusEffect> newStatusEffectList)
	{
		statusEffectList = newStatusEffectList;
	}

	public int GetMaxHealth()
	{
		return maxHealth;
	}

    public int GetCurrentHealth()
	{
		return currentHealth;
	}

	public int GetCurrentActivityPoint()
	{
		return activityPoint;
	}

	public void SetUnitClass(UnitClass unitClass)
	{
		this.unitClass = unitClass;
	}

	public UnitClass GetUnitClass()
	{
		return unitClass;
	}

	public void SetElement(Element element)
	{
		this.element = element;
	}

	public Element GetElement()
	{
		return element;
	}

	public void SetCelestial(Celestial celestial)
	{
		this.celestial = celestial;
	}

	public Celestial GetCelestial()
	{
		return celestial;
	}

	public int GetHeight()
	{
		int height = 0;
		GameObject tile = FindObjectOfType<TileManager>().GetTile(this.GetPosition());
		height = tile.GetComponent<Tile>().GetTileHeight();

		return height;
	}

	public string GetNameInCode()
	{
		return nameInCode;
	}

	public string GetName()
	{
		return name;
	}

	public void SetName(string name)
	{
		this.name = name;
	}

	public Side GetSide()
	{
		return side;
	}

	public void SetDirection(Direction direction)
	{
		this.direction = direction;
		UpdateSpriteByDirection();
	}

	public Direction GetDirection()
	{
		return direction;
	}

	public void SetPosition(Vector2 position)
	{
		this.position = position;
	}

	public void ApplySnapshot(Tile before, Tile after, Direction direction, int snapshotAp)
	{
		before.SetUnitOnTile(null);
		transform.position = after.transform.position + new Vector3(0, 0, -0.05f);
		SetPosition(after.GetTilePos());
		SetDirection(direction);
		after.SetUnitOnTile(this.gameObject);
		this.activityPoint = snapshotAp;
	}

	public void ApplyMove(Tile before, Tile after, Direction direction, int costAp)
	{
		before.SetUnitOnTile(null);
		transform.position = after.transform.position + new Vector3(0, 0, -0.05f);
		SetPosition(after.GetTilePos());
		SetDirection(direction);
		after.SetUnitOnTile(this.gameObject);
		UseActivityPoint(costAp);
	}

	public Vector2 GetPosition()
	{
		return position;
	}

	public Dictionary<string, int> GetUsedSkillDict()
	{
		return usedSkillDict;
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
			ChainList.RemoveChainsFromUnit(gameObject);
		}
	}

	public void UpdateStatusEffect()
	{
		int count = statusEffectList.Count;
		List<StatusEffect> newStatusEffectList = new List<StatusEffect>();

		for(int i = 0; i < count; i++)
		{
			if(!statusEffectList[i].GetToBeRemoved())
			{
				newStatusEffectList.Add(statusEffectList[i]);
			}
		}
		statusEffectList = newStatusEffectList;
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
		if (statusEffectList.Any(k => k.IsOfType(statusEffectType)))
			hasStatusEffect = true;

		return hasStatusEffect;
	}

	public float GetActualEffect(float data, StatusEffectType statusEffectType)
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
					if (statusEffect.GetIsRelative(i)) // 상대값 합산
					{
						totalRelativeValue *= statusEffect.GetAmount(i);	
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
		
		this.UpdateStatusEffect();

		return data * totalRelativeValue + totalAbsoluteValue;
	}

	public void UpdateRemainPhaseAtPhaseEnd()
	{
		List<StatusEffect> newStatusEffectList = new List<StatusEffect>();
		foreach (var statusEffect in statusEffectList)
		{
			if (!statusEffect.GetIsInfinite())
				statusEffect.DecreaseRemainPhase();
			if (statusEffect.GetRemainPhase() > 0)
				newStatusEffectList.Add(statusEffect);
		}

		statusEffectList = newStatusEffectList;
	}

	public void RemoveStatusEffect(Enums.StatusEffectCategory category, int num)
	{
		List<StatusEffect> newStatusEffectList = new List<StatusEffect>();
		int remainNum = num;
		foreach (var statusEffect in statusEffectList)
		{
			if (remainNum == 0)
			{
				newStatusEffectList.Add(statusEffect);
				continue;				
			}

			// 자신이 건 효과는 해제할 수 없다 - 기획문서 참조
			if (statusEffect.GetCaster() == this.gameObject)
			{
				newStatusEffectList.Add(statusEffect);
				continue;
			}

			if (!statusEffect.GetIsRemovable())
			{
				newStatusEffectList.Add(statusEffect);
				continue;
			}

			bool matchIsBuff = (category == Enums.StatusEffectCategory.Buff) && (statusEffect.GetIsBuff());
			bool matchIsDebuff = (category == Enums.StatusEffectCategory.Debuff) && (!statusEffect.GetIsBuff());
			bool matchAll = category == Enums.StatusEffectCategory.All;
			if (matchIsBuff || matchIsDebuff || matchAll)
			{
				num -= 1;
			}
		}

		statusEffectList = newStatusEffectList;
	}

	public IEnumerator Damaged(Skill appliedSkill, Unit caster, float amount, float penetration, bool isDot, bool isHealth)
	{
		int finalDamage = 0; // 최종 대미지 (정수로 표시되는)

		// 체력 깎임
		// 체인 해제
		if (isHealth == true)
		{
			finalDamage = (int)Battle.DamageCalculator.GetActualDamage(appliedSkill, this, caster, amount, penetration, isDot, isHealth);

			if (finalDamage > 0)
			{
				currentHealth -= finalDamage;
				latelyHitInfos.Add(new HitInfo(caster, appliedSkill));
			}
			if (currentHealth < 0)
				currentHealth = 0;

			Debug.Log("Damage dealt : "+finalDamage);

			damageTextObject.SetActive(true);
			damageTextObject.GetComponent<CustomWorldText>().text = finalDamage.ToString();

			healthViewer.UpdateCurrentHealth(currentHealth, maxHealth);

			if (!isDot) // 도트데미지가 아니면 체인이 해제됨.
				ChainList.RemoveChainsFromUnit(gameObject);

			// 데미지 표시되는 시간.
			yield return new WaitForSeconds(1);
			damageTextObject.SetActive(false);
		}

		else
		{
			finalDamage = (int) amount;
			if (activityPoint >= finalDamage)
			{
				activityPoint -= finalDamage;
			}
			else activityPoint = 0;
			Debug.Log(GetName() + " loses " + finalDamage + "AP.");
		}
	}

	public void ApplyDamageOverPhase()
	{
		float totalAmount = 0.0f;

		if (this.HasStatusEffect(StatusEffectType.ContinuousDamage))
		{
			foreach (var statusEffect in statusEffectList)
			{
				if (statusEffect.IsOfType(StatusEffectType.ContinuousDamage))
				{
					totalAmount += statusEffect.GetAmount();
				}
			}

			// FIXME : 도트데미지 처리 메소드 이거 말고 따로 만들 것.
			// Damaged(UnitClass.None, totalAmount, 0f, true, false);
		}
	}

	public void ApplyHealOverPhase()
	{
		float totalAmount = 0.0f;

		if (this.HasStatusEffect(StatusEffectType.ContinuousHeal))
		{
			foreach (var statusEffect in statusEffectList)
			{
				if (statusEffect.IsOfType(StatusEffectType.ContinuousHeal))
				{
					totalAmount += statusEffect.GetAmount();
				}
			}
		}

		RecoverHealth(totalAmount);
	}

	public IEnumerator RecoverHealth(float amount)
	{
		// 회복량 증감 효과 적용
		if (this.HasStatusEffect(StatusEffectType.HealChange))
		{
			amount = GetActualEffect(amount, StatusEffectType.HealChange);
		}

		// 초과회복량 차감
		if (currentHealth + (int)amount > maxHealth)
		{
			amount = (int)amount - (currentHealth + (int)amount - maxHealth);
		}

		currentHealth += (int) amount;
		if (currentHealth > maxHealth)
			currentHealth = maxHealth;

		recoverTextObject.SetActive(true);
		recoverTextObject.GetComponent<TextMesh>().text = amount.ToString();

		healthViewer.UpdateCurrentHealth(currentHealth, maxHealth);

		// 회복량 표시되는 시간. (회복량이 0일때는 딜레이 없음)
		if (amount > 0)
			yield return new WaitForSeconds(1);
		else
			yield return null;
		recoverTextObject.SetActive(false);
	}

	public IEnumerator RecoverAP(int amount)
	{
		activityPoint += amount;

		recoverTextObject.SetActive(true);
		recoverTextObject.GetComponent<TextMesh>().text = amount.ToString();

		// healthViewer.UpdateCurrentActivityPoint(currentHealth, maxHealth);

		// 회복량 표시되는 시간.
		yield return new WaitForSeconds(1);
		recoverTextObject.SetActive(false);

	}

	public void RegenerateActivityPoint()
	{
		activityPoint = GetRegeneratedActivityPoint();
		Debug.Log(name + " recover " + dexturity + "AP. Current AP : " + activityPoint);
	}

	public int GetRegeneratedActivityPoint()
	{
		return activityPoint + GetRegenerationAmount(); // 페이즈당 행동력 회복량 = 민첩성 * 보정치(버프/디버프)
	}

	public int GetRegenerationAmount()
	{
		return GetActualStat(Stat.Dexturity);
	}

    public int GetActualRequireSkillAP(Skill selectedSkill)
    {
        int requireSkillAP = selectedSkill.GetRequireAP();

		// 기술 자체에 붙은 행동력 소모 증감효과 적용
		requireSkillAP = SkillLogicFactory.Get(selectedSkill).CalculateAP(requireSkillAP, this);

        // 행동력(기술) 소모 증감 효과 적용
        if (this.HasStatusEffect(StatusEffectType.RequireSkillAPChange))
		{
			requireSkillAP = (int) GetActualEffect((float)requireSkillAP, StatusEffectType.RequireSkillAPChange);
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
	}

	public void GetKnockedBack(BattleData battleData, GameObject destTile)
	{
		GameObject currentTile = battleData.tileManager.GetTile(GetPosition());
		currentTile.GetComponent<Tile>().SetUnitOnTile(null);
		transform.position = destTile.transform.position + new Vector3(0, 0, -5f);
		SetPosition(destTile.GetComponent<Tile>().GetTilePos());
		destTile.GetComponent<Tile>().SetUnitOnTile(gameObject);
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

	public void ApplyUnitInfo(UnitInfo unitInfo)
	{
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
		this.unitClass = unitInfo.unitClass;
		this.element = unitInfo.element;
		this.celestial = unitInfo.celestial;
	}

	public void ApplySkillList(List<SkillInfo> skillInfoList, List<StatusEffectInfo> statusEffectInfoList, List<PassiveSkillInfo> passiveSkillInfoList)
	{
		float partyLevel = (float)FindObjectOfType<BattleManager>().GetPartyLevel();

		foreach (var skillInfo in skillInfoList)
		{
			if ((skillInfo.GetOwner() == this.nameInCode)) //&&
			//	(skillInfo.GetRequireLevel() <= partyLevel))
                {
                    Skill skill = skillInfo.GetSkill();
					// if(SkillDB.IsLearned(this.nameInCode, skill.GetName()))
					{
						skill.ApplyStatusEffectList(statusEffectInfoList);
                    	skillList.Add(skill);
					}
                }

		}
		// 비어있으면 디폴트 스킬로 채우도록.
		if (skillList.Count() == 0)
		{
			foreach (var skillInfo in skillInfoList)
			{
				if ((skillInfo.GetOwner() == "default") &&
					(skillInfo.GetRequireLevel() <= partyLevel))
					skillList.Add(skillInfo.GetSkill());
			}
		}

		foreach (var passiveSkillInfo in passiveSkillInfoList)
		{
			//Debug.LogError("Passive skill name " + passiveSkillInfo.name);
			if ((passiveSkillInfo.GetOwner() == this.nameInCode) &&
				(passiveSkillInfo.GetRequireLevel() <= partyLevel))
			{
				PassiveSkill passiveSkill = passiveSkillInfo.GetSkill();
				passiveSkill.ApplyStatusEffectList(statusEffectInfoList);
				passiveSkillList.Add(passiveSkill);
			}
		}
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

		maxHealth = (int)baseHealth;
		power = (int)basePower;
		defense = (int)baseDefense;
		resistance = (int)baseResistance;
		dexturity = (int)baseDexturity;

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

	void Initialize()
	{
		gameObject.name = nameInCode;

		position = initPosition;
		UpdateSpriteByDirection();
		currentHealth = maxHealth;
		activityPoint = (int)(dexturity * 0.5f) + FindObjectOfType<UnitManager>().GetStandardActivityPoint();
		if (dexturity == 0)
		{
			// Manastone is not move
			activityPoint = 0;
		}
		// skillList = SkillLoader.MakeSkillList();

		statusEffectList = new List<StatusEffect>();
		latelyHitInfos = new List<HitInfo>();

		healthViewer.SetInitHealth(maxHealth, side);
	}

	void LoadSprite()
	{
		UnityEngine.Object[] sprites = Resources.LoadAll("UnitImage/" + nameInCode);

		if (sprites.Length == 0)
		{
			Debug.LogError("Cannot find sprite for " + nameInCode);
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

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.R))
		{
			RegenerateActivityPoint();
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
