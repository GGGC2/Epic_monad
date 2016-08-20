using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enums;

public class Unit : MonoBehaviour
{

	GameObject chainTextObject;
	GameObject damageTextObject;
	GameObject recoverTextObject;
	GameObject activeArrowIcon;
	GameObject bonusTextObject;
	HealthViewer healthViewer;
	GameObject chainAttackerIcon;

	new string name; // 한글이름
	string nameInCode; // 영어이름

	Side side; // 진영. 적/아군

	// 스킬리스트.
	List<Skill> skillList = new List<Skill>();
    
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
    
    public int GetActualStat(Stat stat)
    {
        int actualStat = GetStat(stat);
        StatusEffectType statusChange = (StatusEffectType)Enum.Parse(typeof(StatusEffectType), stat.ToString()+"Change");
        
		// 능력치 증감 효과 적용
		if (this.HasStatusEffect(statusChange))
		{
			actualStat = GetActualEffect(actualStat, statusChange);
        }
        
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
    
    public List<StatusEffect> GetStatusEffectList()
    {
        return statusEffectList;
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

	public Vector2 GetPosition()
	{
		return position;
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

	public void DecreaseRemainPhaseStatusEffect()
	{
		foreach (var statusEffect in statusEffectList)
		{
			if (statusEffect.GetRemainPhase() > 0)
			{
				statusEffect.DecreaseRemainPhase();
				if (statusEffect.GetRemainPhase() == 0)
					statusEffect.SetToBeRemoved(true);
			}
		}
	}

	public void UpdateStatusEffect()
	{
		int count = statusEffectList.Count;
		List<StatusEffect> newStatusEffectList = new List<StatusEffect>();

		for(int i = 0; i < count; i++)
		{
			if(!statusEffectList[i].GetToBeRemoved())
				newStatusEffectList.Add(statusEffectList[i]);
		}
		statusEffectList = newStatusEffectList;
	}

	// searching certain StatusEffect
	public bool HasStatusEffect(StatusEffect statusEffect)
	{
		bool hasStatusEffect = false;
		if (statusEffectList.Any(k => k.GetName().Equals(statusEffect.GetName())))
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

	public int GetActualEffect(int data, StatusEffectType statusEffectType)
	{
		int totalAmount = 0;
		float totalDegree = 1.0f;

		foreach (var statusEffect in statusEffectList)
		{
			if (statusEffect.IsOfType(statusEffectType))
			{
				totalAmount += statusEffect.GetAmount();
				if (statusEffect.GetRemainStack() > 0) // 지속 단위가 횟수인 효과의 지속 횟수 감소
				{
					statusEffect.DecreaseRemainStack();
					if(statusEffect.GetRemainStack() == 0) // 지속 횟수 소진 시 효과 제거
					{
						statusEffect.SetToBeRemoved(true);
						this.UpdateStatusEffect();
					}
				}
			}
		}
		foreach (var statusEffect in statusEffectList)
		{
			if (statusEffect.IsOfType(statusEffectType))
			{
				totalDegree = (100.0f + statusEffect.GetDegree()) / 100.0f;
				if (statusEffect.GetRemainStack() > 0) // 지속 단위가 횟수인 효과의 지속 횟수 감소
				{
					statusEffect.DecreaseRemainStack();
					if(statusEffect.GetRemainStack() == 0) // 지속 횟수 소진 시 효과 제거
					{
						statusEffect.SetToBeRemoved(true);
						this.UpdateStatusEffect();
					}
				}
			}
		}

		return (int) ((float)data * totalDegree) + totalAmount;
	}

	public IEnumerator Damaged(UnitClass unitClass, int amount, bool isDot)
	{
		int actualDamage = 0;
		// 공격이 물리인지 마법인지 체크
		// 방어력 / 저항력 중 맞는 값을 적용 (적용 단계에서 능력치 변동 효과 반영)
		// 대미지 증가/감소 효과 적용
		// 보호막 있을 경우 대미지 삭감
		// 체력 깎임
		// 체인 해제
		if (unitClass == UnitClass.Melee)
		{
			// 실제 피해 = 원래 피해 x 200/(200+방어력)
			actualDamage = amount * 200 / (200 + GetActualStat(Stat.Defense));
			Debug.Log("Actual melee damage without status effect : " + actualDamage);
		}
		else if (unitClass == UnitClass.Magic)
		{
			actualDamage = amount * 200 / (200 + GetActualStat(Stat.Resistance));
			Debug.Log("Actual magic damage without status effect: " + actualDamage);
		}
		else if (unitClass == UnitClass.None)
		{
			actualDamage = amount;
		}

		// 대미지 증감 효과 적용
		if (this.HasStatusEffect(StatusEffectType.DamageChange))
		{
			actualDamage = GetActualEffect(actualDamage, StatusEffectType.DamageChange); 
		}

		// 보호막에 따른 대미지 삭감
		if (this.HasStatusEffect(StatusEffectType.Shield))
		{
			int shieldAmount = 0;
			for (int i = 0; i < statusEffectList.Count; i++)
			{
				if (statusEffectList[i].IsOfType(StatusEffectType.Shield))
				{
					shieldAmount = statusEffectList[i].GetAmount();
					if (shieldAmount > actualDamage)
					{
						actualDamage = 0;
						statusEffectList[i].SetRemainAmount(shieldAmount - actualDamage);
						break;
					}
					else
					{
						actualDamage -= shieldAmount;
						statusEffectList[i].SetToBeRemoved(true);
						this.UpdateStatusEffect();
					}
				}
			}
		}

		currentHealth -= actualDamage;
		if (currentHealth < 0)
			currentHealth = 0;

		damageTextObject.SetActive(true);
		damageTextObject.GetComponent<CustomWorldText>().text = actualDamage.ToString();

		healthViewer.UpdateCurrentHealth(currentHealth, maxHealth);

		if (!isDot) // 도트데미지가 아니면 체인이 해제됨.
			ChainList.RemoveChainsFromUnit(gameObject);

		// 데미지 표시되는 시간.
		yield return new WaitForSeconds(1);
		damageTextObject.SetActive(false);
	}

	public void ApplyDamageOverPhase()
	{
		int totalAmount = 0;

		if (this.HasStatusEffect(StatusEffectType.ContinuousDamage))
		{
			foreach (var statusEffect in statusEffectList)
			{
				if (statusEffect.IsOfType(StatusEffectType.ContinuousDamage))
				{
					totalAmount += statusEffect.GetAmount();
				}
			}

			// FIXME : 도트데미지는 물뎀인가 마뎀인가? 기획서대로 적용할 것. 언제? 일단 보류중.
			Damaged(UnitClass.None, totalAmount, true);
		}
	}

	public void ApplyHealOverPhase()
	{
		int totalAmount = 0;

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

	public IEnumerator RecoverHealth(int amount)
	{
		// 회복량 증감 효과 적용
		if (this.HasStatusEffect(StatusEffectType.HealChange))
		{
			amount = GetActualEffect(amount, StatusEffectType.HealChange);
		}

		currentHealth += amount;
		if (currentHealth > maxHealth)
			currentHealth = maxHealth;

		recoverTextObject.SetActive(true);
		recoverTextObject.GetComponent<TextMesh>().text = amount.ToString();

		healthViewer.UpdateCurrentHealth(currentHealth, maxHealth);

		// 데미지 표시되는 시간.
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
        int requireSkillAP = selectedSkill.GetRequireAP()[0];
        
        // 행동력(기술) 소모 증감 효과 적용
        if (this.HasStatusEffect(StatusEffectType.RequireSkillAPChange))
		{
			requireSkillAP = GetActualEffect(requireSkillAP, StatusEffectType.RequireSkillAPChange);
		}

		// 스킬 시전 유닛의 모든 행동력을 요구하는 경우
		if (selectedSkill.GetRequireAP()[0] == 9999)
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

	public void ApplySkillList(List<SkillInfo> skillInfoList, List<StatusEffectInfo> statusEffectInfoList)
	{
		float partyLevel = (float)FindObjectOfType<BattleManager>().GetPartyLevel();

		foreach (var skillInfo in skillInfoList)
		{
			if ((skillInfo.GetOwner() == this.nameInCode) &&
				(skillInfo.GetRequireLevel() <= partyLevel))
                {
                    Skill skill = skillInfo.GetSkill();
                    skill.ApplyStatusEffectList(statusEffectInfoList);
                    skillList.Add(skill);
                }
				
		}
		// 비어있으면 디폴트 스킬로 채우도록.
		if (skillList.Count() == 0)
			foreach (var skillInfo in skillInfoList)
			{
				if ((skillInfo.GetOwner() == "default") &&
					(skillInfo.GetRequireLevel() <= partyLevel))
					skillList.Add(skillInfo.GetSkill());
			}
	}

	// using test.
	public void PrintCelestialBonus()
	{
		bonusTextObject.SetActive(true);
		bonusTextObject.GetComponent<TextMesh>().text = "Celestial bonus";
		Invoke("ActiveFalseAtDelay", 0.4f);
	}

	void ActiveFalseAtDelay()
	{
		bonusTextObject.SetActive(false);
	}

	public void PrintChainText(int chainCount)
	{
		chainTextObject.SetActive(true);
		chainTextObject.GetComponent<TextMesh>().text = "연계" + chainCount + "단";
	}

	public void DisableChainText()
	{
		chainTextObject.SetActive(false);
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
		// skillList = SkillLoader.MakeSkillList();

		statusEffectList = new List<StatusEffect>();

		healthViewer.SetInitHealth(maxHealth, side);
	}

	void LoadSprite()
	{
		UnityEngine.Object[] sprites = Resources.LoadAll("UnitImage/" + nameInCode);
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
	}

	void Awake()
	{
		chainTextObject = transform.Find("ChainText").gameObject;
		damageTextObject = transform.Find("DamageText").gameObject;
		recoverTextObject = transform.Find("RecoverText").gameObject;
		activeArrowIcon = transform.Find("ActiveArrowIcon").gameObject;
		bonusTextObject = transform.Find("BonusText").gameObject;
		chainAttackerIcon = transform.Find("icons/chain").gameObject;
		chainTextObject.SetActive(false);
		damageTextObject.SetActive(false);
		recoverTextObject.SetActive(false);
		activeArrowIcon.SetActive(false);
		bonusTextObject.SetActive(false);
		chainAttackerIcon.SetActive(false);

		healthViewer = transform.Find("HealthBar").GetComponent<HealthViewer>();
	}

	// Update is called once per frame
	void Update()
	{

	}
}
