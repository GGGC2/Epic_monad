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

	// FIXME : temp values
	Vector2 initPosition;

	// Base stats. FIXME : 지금은 수동으로 셋팅.
	float baseHealth; // 체력
	float basePower; // 공격력
	float baseDefense; // 방어력
	float baseResistance; // 저항력
	float baseDexturity; // 행동력

	// 계산 관련 값들 - 절대값으로 변경해서 현재는 쓰지 않음.
	// float healthAcceleration = 0.91f;
	// float healthAccelerationInterval = 0.09f;
	// float healthInitialGrowth = 31.4f;
	// float healthInitialGrowthInterval = 2f;
	// float healthStandardValue = 400f;
	// float healthStandardValueInterval = 25f;
	// float powerAcceleration = 0.14f;
	// float powerAccelerationInterval = 0.025f;
	// float powerInitialGrowth = 4.9f;
	// float powerInitialGrowthInterval = 0.4f;
	// float powerStandardValue = 69f;
	// float powerStandardValueInterval = 4.25f;
	// float defenseAcceleration = 0f;
	// float defenseAccelerationInterval = 0f;
	// float defenseInitialGrowth = 4.4f;
	// float defenseInitialGrowthInterval = 0.75f;
	// float defenseStandardValue = 40f;
	// float defenseStandardValueInterval = 10f;
	// float resistanceAcceleration = 0f;
	// float resistanceAccelerationInterval = 0f;
	// float resistanceInitialGrowth = 4.4f;
	// float resistanceInitialGrowthInterval = 0.75f;
	// float resistanceStandardValue = 40f;
	// float resistanceStandardValueInterval = 10f;
	// float dexturityAcceleration = 0f;
	// float dexturityAccelerationInterval = 0f;
	// float dexturityInitialGrowth = 0.8f;
	// float dexturityInitialGrowthInterval = 0.05f;
	// float dexturityStandardValue = 50f;
	// float dexturityStandardValueInterval = 2.5f;

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

	List<StatusEffect> statusEffectList;

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
        StatusEffectType statusIncrease = (StatusEffectType)Enum.Parse(typeof(StatusEffectType), stat.ToString()+"Increase");
        StatusEffectType statusDecrease = (StatusEffectType)Enum.Parse(typeof(StatusEffectType), stat.ToString()+"Decrease");
        
        // stat decrease by debuffs
		if (statusEffectList.Any(k => k.GetStatusEffectType() == statusDecrease))
		{
			// 상대치 곱연산
			float totalDegree = 1.0f;
			foreach (var statusEffect in statusEffectList)
			{
				if (statusEffect.GetStatusEffectType() == statusDecrease)
				{
					totalDegree *= (100.0f - statusEffect.GetDegree()) / 100.0f;
				}
			}
			actualStat = (int)((float)actualStat * totalDegree);

			// 절대치 합연산
			int totalAmount = 0;
			foreach (var statusEffect in statusEffectList)
			{
				if (statusEffect.GetStatusEffectType() == statusDecrease)
				{
					totalAmount += statusEffect.GetAmount();
				}
			}
			actualStat -= totalAmount;
        }
        
        // stat increase by buffs
		if (statusEffectList.Any(k => k.GetStatusEffectType() == statusIncrease))
		{
			// 상대치 곱연산
			float totalDegree = 1.0f;
			foreach (var statusEffect in statusEffectList)
			{
				if (statusEffect.GetStatusEffectType() == statusIncrease)
				{
					totalDegree *= (100.0f + statusEffect.GetDegree()) / 100.0f;
				}
			}
			actualStat = (int)((float)actualStat * totalDegree);

			// 절대치 합연산
			int totalAmount = 0;
			foreach (var statusEffect in statusEffectList)
			{
				if (statusEffect.GetStatusEffectType() == statusIncrease)
				{
					totalAmount += statusEffect.GetAmount();
				}
			}
			actualStat += totalAmount;
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

	public bool IsBound()
	{
		return statusEffectList.Any(k => k.GetStatusEffectType() == StatusEffectType.Bind);
	}

	public bool IsSilenced()
	{
		return statusEffectList.Any(k => k.GetStatusEffectType() == StatusEffectType.Silence);
	}

	public bool IsFainted()
	{
		return statusEffectList.Any(k => k.GetStatusEffectType() == StatusEffectType.Bind) &&
			   statusEffectList.Any(k => k.GetStatusEffectType() == StatusEffectType.Silence);
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
		if (statusEffect.GetStatusEffectType() == StatusEffectType.Faint ||
			statusEffect.GetStatusEffectType() == StatusEffectType.Silence)
		{
			ChainList.RemoveChainsFromUnit(gameObject);
		}
	}

	public void RemainStatusEffect()
	{
		statusEffectList.Remove(statusEffectList[0]);
	}

	public void RemainAllStatusEffect()
	{
		statusEffectList.Clear();
	}

	public void DecreaseRemainPhaseStatusEffect()
	{
		List<StatusEffect> newStatusEffectList = new List<StatusEffect>();
		foreach (var statusEffect in statusEffectList)
		{
			statusEffect.DecreaseRemainPhase();
			if (statusEffect.GetRemainPhase() > 0)
			{
				newStatusEffectList.Add(statusEffect);
			}
		}
		statusEffectList = newStatusEffectList;
	}

	public IEnumerator Damaged(UnitClass unitClass, int amount, bool isDot)
	{
		int actualDamage = 0;
		// 공격이 물리인지 마법인지 체크
		// 방어력 / 저항력 중 맞는 값을 적용
		// 방어 증가/감소 / 저항 증가/감소 적용			 // FIXME : 증가분 미적용
		// 체력 깎임
		// 체인 해제
		if (unitClass == UnitClass.Melee)
		{
			// 실제 피해 = 원래 피해 x 200/(200+방어력)
			actualDamage = amount * 200 / (200 + GetActualStat(Stat.Defense));
			Debug.Log("Actual melee damage : " + actualDamage);
		}
		else if (unitClass == UnitClass.Magic)
		{
			actualDamage = amount * 200 / (200 + GetActualStat(Stat.Resistance));
			Debug.Log("Actual magic damage : " + actualDamage);
		}
		else if (unitClass == UnitClass.None)
		{
			actualDamage = amount;
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

		if (statusEffectList.Any(k => k.GetStatusEffectType() == StatusEffectType.DamageOverPhase))
		{
			foreach (var statusEffect in statusEffectList)
			{
				if (statusEffect.GetStatusEffectType() == StatusEffectType.DamageOverPhase)
				{
					totalAmount += statusEffect.GetAmount();
				}
			}

			// FIXME : 도트데미지는 물뎀인가 마뎀인가? 기획서대로 적용할 것. 언제? 일단 보류중.
			Damaged(UnitClass.None, totalAmount, true);
		}
	}

	public IEnumerator RecoverHealth(int amount)
	{
		// FIXME : 치유량 증가 효과

		// 내상 효과
		if (statusEffectList.Any(k => k.GetStatusEffectType() == StatusEffectType.Wound))
		{
			// 상대치 곱연산
			float totalDegree = 1.0f;
			foreach (var statusEffect in statusEffectList)
			{
				if (statusEffect.GetStatusEffectType() == StatusEffectType.Exhaust)
				{
					totalDegree *= (100.0f - statusEffect.GetDegree()) / 100.0f;
				}
			}
			amount = (int)((float)amount * totalDegree);
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

	public void RegenerateActionPoint()
	{
		activityPoint = GetRegeneratedActionPoint();
		Debug.Log(name + " recover " + dexturity + "AP. Current AP : " + activityPoint);
	}

	public int GetRegeneratedActionPoint()
	{
		return activityPoint + GetActualStat(Stat.Dexturity); // 페이즈당 행동력 회복량 = 민첩성 * 보정치(버프/디버프)
	}

	public void UseActionPoint(int amount)
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

	public void ApplySkillList(List<SkillInfo> skillInfoList)
	{
		float partyLevel = (float)FindObjectOfType<BattleManager>().GetPartyLevel();

		foreach (var skillInfo in skillInfoList)
		{
			if ((skillInfo.GetOwner() == this.nameInCode) &&
				(skillInfo.GetRequireLevel() <= partyLevel))
				skillList.Add(skillInfo.GetSkill());
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
		activityPoint = (int)(dexturity * 0.5f) + FindObjectOfType<UnitManager>().GetStandardActionPoint();
		// skillList = SkillLoader.MakeSkillList();

		statusEffectList = new List<StatusEffect>();

		healthViewer.SetInitHealth(maxHealth, side);
	}

	void LoadSprite()
	{
		spriteLeftUp = Resources.Load("UnitImage/" + nameInCode + "_LeftUp", typeof(Sprite)) as Sprite;
		spriteLeftDown = Resources.Load("UnitImage/" + nameInCode + "_LeftDown", typeof(Sprite)) as Sprite;
		spriteRightUp = Resources.Load("UnitImage/" + nameInCode + "_RightUp", typeof(Sprite)) as Sprite;
		spriteRightDown = Resources.Load("UnitImage/" + nameInCode + "_RightDown", typeof(Sprite)) as Sprite;
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
