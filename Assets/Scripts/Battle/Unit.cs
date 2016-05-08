using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Enums;
using System.Linq;

public class Unit : MonoBehaviour
{

	GameObject chainTextObject;
	GameObject damageTextObject;
	GameObject recoverTextObject;
	GameObject activeArrowIcon;
	GameObject bounsTextObject;
	HealthViewer healthViewer;

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
	float baseResistence; // 저항력
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
	// float resistenceAcceleration = 0f;
	// float resistenceAccelerationInterval = 0f;
	// float resistenceInitialGrowth = 4.4f;
	// float resistenceInitialGrowthInterval = 0.75f;
	// float resistenceStandardValue = 40f;
	// float resistenceStandardValueInterval = 10f;
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
	int resistence;
	int dexturity;
	int reach;
	int range;

	// type.
	UnitClass unitClass;
	Element element;
	Celestial celestial;

	// Variable values.
	public Vector2 position;
	public Direction direction;
	public int currentHealth;
	public int activityPoint;

	List<Buff> buffList;
	List<Debuff> debuffList;

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

	// FIXME : 임시로 공격력만 외부에서 참조.
	public int GetActualPower()
	{
		int actualPower = power;

		// 공격력 감소 효과 적용.
		if (debuffList.Any(k => k.GetName() == DebuffType.PowerDecrease))
		{
			// 상대치 곱연산
			float totalDegree = 1.0f;
			foreach (var debuff in debuffList)
			{
				if (debuff.GetName() == DebuffType.PowerDecrease)
				{
					totalDegree *= (100.0f - debuff.GetDegree()) / 100.0f;
				}
			}
			actualPower = (int)((float)actualPower * totalDegree);

			// 절대치 합연산
			int totalAmount = 0;
			foreach (var debuff in debuffList)
			{
				if (debuff.GetName() == DebuffType.PowerDecrease)
				{
					totalAmount += debuff.GetAmount();
				}
			}
			actualPower -= totalAmount;
		}

		return actualPower;
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

	public int GetCurrentHealth()
	{
		return currentHealth;
	}

	public int GetMaxHealth()
	{
		return maxHealth;
	}

	public bool IsBound()
	{
		return debuffList.Any(k => k.GetName() == DebuffType.Bind);
	}

	public bool IsSilenced()
	{
		return debuffList.Any(k => k.GetName() == DebuffType.Silence);
	}

	public bool IsFainted()
	{
		return debuffList.Any(k => k.GetName() == DebuffType.Bind) &&
			   debuffList.Any(k => k.GetName() == DebuffType.Silence);
	}

	public int GetTrueDexturity()
	{
		return dexturity;
	}

	public int GetActualDexturity()
	{
		int actualDexturity = dexturity;
		// FIXME : 버프 / 디버프 값 적용

		// 디버프값만 적용.
		if (debuffList.Any(k => k.GetName() == DebuffType.Faint))
		{
			actualDexturity = 0;
		}
		else if (debuffList.Any(k => k.GetName() == DebuffType.Exhaust))
		{
			// 상대치 곱연산
			float totalDegree = 1.0f;
			foreach (var debuff in debuffList)
			{
				if (debuff.GetName() == DebuffType.ResistanceDecrease)
				{
					totalDegree *= (100.0f - debuff.GetDegree()) / 100.0f;
				}
			}
			actualDexturity = (int)((float)actualDexturity * totalDegree);
		}
		return actualDexturity;
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

	public void SetBuff(Buff buff)
	{
		buffList.Add(buff);
	}

	public void RemainBuff()
	{
		buffList.Remove(buffList[0]);
	}

	public void RemainAllBuff()
	{
		buffList.Clear();
	}

	public void SetDebuff(Debuff debuff)
	{
		debuffList.Add(debuff);
		// 침묵이나 기절상태가 될 경우 체인 해제.
		// FIXME : 넉백 추가할 것. (넉백은 디버프가 아니라서 다른 곳에서 적용할 듯?)
		if (debuff.GetName() == DebuffType.Faint ||
			debuff.GetName() == DebuffType.Silence)
		{
			ChainList.RemoveChainsFromUnit(gameObject);
		}
	}

	public void RemainDebuff()
	{
		debuffList.Remove(debuffList[0]);
	}

	public void RemainAllDebuff()
	{
		debuffList.Clear();
	}

	public void DecreaseRemainPhaseBuffAndDebuff()
	{
		List<Buff> newBuffList = new List<Buff>();
		foreach (var buff in buffList)
		{
			buff.DecreaseRemainPhase();
			if (buff.GetRemainPhase() > 0)
			{
				newBuffList.Add(buff);
			}
		}
		buffList = newBuffList;

		List<Debuff> newDebuffList = new List<Debuff>();
		foreach (var debuff in debuffList)
		{
			debuff.DecreaseRemainPhase();
			if (debuff.GetRemainPhase() > 0)
			{
				newDebuffList.Add(debuff);
			}
		}
		debuffList = newDebuffList;
	}

	public int GetActualDefense()
	{
		int actualDefense = defense;

		// 방어력 감소 효과 적용.
		if (debuffList.Any(k => k.GetName() == DebuffType.DefenseDecrease))
		{
			// 상대치 곱연산
			float totalDegree = 1.0f;
			foreach (var debuff in debuffList)
			{
				if (debuff.GetName() == DebuffType.DefenseDecrease)
				{
					totalDegree *= (100.0f - debuff.GetDegree()) / 100.0f;
				}
			}
			actualDefense = (int)((float)actualDefense * totalDegree);

			// 절대치 합연산
			int totalAmount = 0;
			foreach (var debuff in debuffList)
			{
				if (debuff.GetName() == DebuffType.DefenseDecrease)
				{
					totalAmount += debuff.GetAmount();
				}
			}
			actualDefense -= totalAmount;
		}

		return actualDefense;
	}

	public int GetActualResistance()
	{
		int actualResistance = resistence;

		// 저항력 감소 효과 적용.
		if (debuffList.Any(k => k.GetName() == DebuffType.ResistanceDecrease))
		{
			// 상대치 곱연산
			float totalDegree = 1.0f;
			foreach (var debuff in debuffList)
			{
				if (debuff.GetName() == DebuffType.ResistanceDecrease)
				{
					totalDegree *= (100.0f - debuff.GetDegree()) / 100.0f;
				}
			}
			actualResistance = (int)((float)actualResistance * totalDegree);

			// 절대치 합연산
			int totalAmount = 0;
			foreach (var debuff in debuffList)
			{
				if (debuff.GetName() == DebuffType.ResistanceDecrease)
				{
					totalAmount += debuff.GetAmount();
				}
			}
			actualResistance -= totalAmount;
		}

		return actualResistance;
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
			actualDamage = amount * 200 / (200 + GetActualDefense());
			Debug.Log("Actual melee damage : " + actualDamage);
		}
		else if (unitClass == UnitClass.Magic)
		{
			actualDamage = amount * 200 / (200 + GetActualResistance());
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
		damageTextObject.GetComponent<CustomText>().text = actualDamage.ToString();

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

		if (debuffList.Any(k => k.GetName() == DebuffType.DamageOverPhase))
		{
			foreach (var debuff in debuffList)
			{
				if (debuff.GetName() == DebuffType.DamageOverPhase)
				{
					totalAmount += debuff.GetAmount();
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
		if (debuffList.Any(k => k.GetName() == DebuffType.Wound))
		{
			// 상대치 곱연산
			float totalDegree = 1.0f;
			foreach (var debuff in debuffList)
			{
				if (debuff.GetName() == DebuffType.Exhaust)
				{
					totalDegree *= (100.0f - debuff.GetDegree()) / 100.0f;
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
		activityPoint += GetActualDexturity(); // 페이즈당 행동력 회복량 = 민첩성 * 보정치(버프/디버프)
		Debug.Log(name + " recover " + dexturity + "AP. Current AP : " + activityPoint);
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
		this.baseResistence = unitInfo.baseResistence;
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
	public void PrintCelestialBouns()
	{
		bounsTextObject.SetActive(true);
		bounsTextObject.GetComponent<TextMesh>().text = "Celestial bouns";
		Invoke("ActiveFalseAtDelay", 0.4f);
	}

	void ActiveFalseAtDelay()
	{
		bounsTextObject.SetActive(false);
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

	void ApplyStats()
	{
		float partyLevel = (float)FindObjectOfType<BattleManager>().GetPartyLevel();

		maxHealth = (int)baseHealth;
		power = (int)basePower;
		defense = (int)baseDefense;
		resistence = (int)baseResistence;
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
		// float actualResistenceAcceleration = resistenceAcceleration + (resistenceAccelerationInterval * baseResistence);
		// float actualResistenceInitialGrowth = resistenceInitialGrowth + (resistenceInitialGrowthInterval * baseResistence);
		// float actualResistenceStandardValue = resistenceStandardValue + (resistenceStandardValueInterval * baseResistence);
		// resistence = (int)((actualResistenceAcceleration * partyLevel * (partyLevel - 1f) / 2f)
		// 				   + (actualResistenceInitialGrowth * partyLevel) + actualResistenceStandardValue);
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
		buffList = new List<Buff>();
		debuffList = new List<Debuff>();

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
		bounsTextObject = transform.Find("BounsText").gameObject;
		chainTextObject.SetActive(false);
		damageTextObject.SetActive(false);
		recoverTextObject.SetActive(false);
		activeArrowIcon.SetActive(false);
		bounsTextObject.SetActive(false);

		healthViewer = transform.Find("HealthBar").GetComponent<HealthViewer>();
	}

	// Update is called once per frame
	void Update()
	{

	}
}
