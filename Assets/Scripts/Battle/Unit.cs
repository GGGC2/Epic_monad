using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using Enums;
using Battle.Skills;

using Save;

public class Unit : MonoBehaviour
{

	public GameObject chainBonusTextObject;
	GameObject damageTextObject;
	GameObject recoverTextObject;
	GameObject activeArrowIcon;
	public GameObject celestialBonusTextObject;
	public GameObject directionBonusTextObject;
	HealthViewer healthViewer;
	GameObject chainAttackerIcon;

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

	public List<StatusEffect> GetAllStatusEffectList()
	{
		return statusEffectList;
	}

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
			actualStat = (int) GetActualEffect(actualStat, statusChange);
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

	public List<Skill> GetLearnedSkillList()
	{
		var learnedSkills =
			 from skill in skillList
			 where SkillDB.IsLearned(nameInCode, skill.GetName())
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

	public void UpdateSkillCooldown()
	{
		Dictionary<string, int> newUsedSkillDict = new Dictionary<string, int>();
		List<string> usedSkillKeys = new List<string>();
		foreach (var skill in usedSkillKeys)
		{
			usedSkillDict[skill]--;
			if (usedSkillDict[skill] != 0)
			{
				newUsedSkillDict.Add(skill, usedSkillDict[skill]);
			}
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

	public void DecreaseRemainPhaseStatusEffect()
	{
		foreach (var statusEffect in statusEffectList)
		{
			if (statusEffect.GetRemainPhase() > 0 && !statusEffect.GetIsInfinite())
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
			if(statusEffectList[i].GetName().Equals("상자에 든 고양이") && statusEffectList[i].GetToBeRemoved())
			{
				float catIsDead = UnityEngine.Random.Range(0.0f, 1.0f);
				if (catIsDead > 0.2f)
				{
					StartCoroutine(Damaged(UnitClass.Magic,statusEffectList[i].GetRemainAmount(), 0.0f, false, true));
				}
			}
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

	public float GetActualEffect(float data, StatusEffectType statusEffectType)
	{
		float totalvalue = 0.0f; // 절대값
		float totalratio = 1.0f; // 상대값

		foreach (var statusEffect in statusEffectList)
		{
			if (statusEffect.IsOfType(statusEffectType))
			{
				if (statusEffect.GetIsRelative()) // 상대값 합산
				{
					float additionalPowerBouns = 0;					
					if (statusEffectType == StatusEffectType.PowerChange)
					{
						List<PassiveSkill> passiveSkills = this.GetLearnedPassiveSkillList();
						additionalPowerBouns = SkillLogicFactory.Get(passiveSkills).GetAdditionalPowerBouns(this);
					}
					
					totalratio *= statusEffect.GetAmount() * additionalPowerBouns;
					if (statusEffect.GetRemainStack() > 0) // 지속 단위가 횟수인 효과의 지속 횟수 감소
					{
						statusEffect.DecreaseRemainStack();
						if(statusEffect.GetRemainStack() == 0) // 지속 횟수 소진 시 효과 제거
						{
							statusEffect.SetToBeRemoved(true);
						}
					}	
				}
				else // 절대값 합산
				{
					totalvalue += statusEffect.GetAmount();
					if (statusEffect.GetRemainStack() > 0) // 지속 단위가 횟수인 효과의 지속 횟수 감소
					{
						statusEffect.DecreaseRemainStack();
						if(statusEffect.GetRemainStack() == 0) // 지속 횟수 소진 시 효과 제거
						{
							statusEffect.SetToBeRemoved(true);
						}
					}
				}
			}
		}
		
		this.UpdateStatusEffect();

		return data * totalratio + totalvalue;
	}

	public float GetActualDamage(UnitClass unitClass, float amount, float penetration, bool isDot, bool isHealth)
	{
		float actualDamage = 0.0f;
		int finalDamage = 0; // 최종 대미지 (정수로 표시되는)

		// 공격이 물리인지 마법인지 체크
		// 방어력 / 저항력 중 맞는 값을 적용 (적용 단계에서 능력치 변동 효과 반영)
		// 대미지 증가/감소 효과 적용
		// 보호막 있을 경우 대미지 삭감
		// 체력 깎임
		// 체인 해제
		if (isHealth == true)
		{
			if (unitClass == UnitClass.Melee)
			{
				// 실제 피해 = 원래 피해 x 200/(200+방어력)
				actualDamage = amount * 200.0f / (200.0f + GetActualStat(Stat.Defense) * (1.0f - penetration));
				Debug.Log("Actual melee damage without status effect : " + actualDamage);
			}
			else if (unitClass == UnitClass.Magic)
			{
				actualDamage = amount * 200.0f / (200.0f + GetActualStat(Stat.Resistance) * (1.0f - penetration));
				Debug.Log("Actual magic damage without status effect: " + actualDamage);
			}
			else if (unitClass == UnitClass.None)
			{
				actualDamage = amount;
			}

			// sisterna_l_1의 저항력 계산
			if (penetration == -1.0f)
			{
				actualDamage = amount - GetActualStat(Stat.Resistance);
			}

			// 대미지 증감 효과 적용
			if (this.HasStatusEffect(StatusEffectType.DamageChange))
			{
				actualDamage = GetActualEffect(actualDamage, StatusEffectType.DamageChange);
			}

			finalDamage = (int) actualDamage;

			// 보호막에 따른 대미지 삭감
			if (this.HasStatusEffect(StatusEffectType.Shield))
			{
				int shieldAmount = 0;
				for (int i = 0; i < statusEffectList.Count; i++)
				{
					if (statusEffectList[i].IsOfType(StatusEffectType.Shield))
					{
						shieldAmount = statusEffectList[i].GetRemainAmount();
						if (shieldAmount > finalDamage)
						{
							statusEffectList[i].SetRemainAmount(shieldAmount - finalDamage);
							finalDamage = 0;
							Debug.Log("Remain Shield Amount : " + statusEffectList[i].GetRemainAmount());
							break;
						}
						else
						{
							finalDamage -= shieldAmount;
							statusEffectList[i].SetToBeRemoved(true);
						}
					}
				}
				this.UpdateStatusEffect();
			}
		}

		else
		{
			// finalDamage = (int) amount;
			// if (activityPoint >= finalDamage)
			// {
			// 	activityPoint -= finalDamage;
			// }
			// else activityPoint = 0;
			// Debug.Log(GetName() + " loses " + finalDamage + "AP.");
		}

		return (float) finalDamage;
	}

	public IEnumerator Damaged(UnitClass unitClass, float amount, float penetration, bool isDot, bool isHealth)
	{
		float actualDamage = 0.0f;
		int finalDamage = 0; // 최종 대미지 (정수로 표시되는)

		// 공격이 물리인지 마법인지 체크
		// 방어력 / 저항력 중 맞는 값을 적용 (적용 단계에서 능력치 변동 효과 반영)
		// 대미지 증가/감소 효과 적용
		// 보호막 있을 경우 대미지 삭감
		// 체력 깎임
		// 체인 해제
		if (isHealth == true)
		{
			if (unitClass == UnitClass.Melee)
			{
				// 실제 피해 = 원래 피해 x 200/(200+방어력)
				actualDamage = amount * 200.0f / (200.0f + GetActualStat(Stat.Defense) * (1.0f - penetration));
				Debug.Log("Actual melee damage without status effect : " + actualDamage);
			}
			else if (unitClass == UnitClass.Magic)
			{
				actualDamage = amount * 200.0f / (200.0f + GetActualStat(Stat.Resistance) * (1.0f - penetration));
				Debug.Log("Actual magic damage without status effect: " + actualDamage);
			}
			else if (unitClass == UnitClass.None)
			{
				actualDamage = amount;
			}

			// sisterna_l_1의 저항력 계산
			if (penetration == -1.0f)
			{
				actualDamage = amount - GetActualStat(Stat.Resistance);
			}

			// 대미지 증감 효과 적용
			if (this.HasStatusEffect(StatusEffectType.DamageChange))
			{
				actualDamage = GetActualEffect(actualDamage, StatusEffectType.DamageChange);
			}

			finalDamage = (int) actualDamage;

			// 보호막에 따른 대미지 삭감
			if (this.HasStatusEffect(StatusEffectType.Shield))
			{
				int shieldAmount = 0;
				for (int i = 0; i < statusEffectList.Count; i++)
				{
					if (statusEffectList[i].IsOfType(StatusEffectType.Shield))
					{
						// sisterna_m_12 발동 조건 체크
						// if (statusEffectList[i].GetName().Equals("파장 분류"))
						// {
						// 	if (finalDamage < (int)(0.2 * maxHealth)) continue;
						// 	else
						// 	{
						// 		int absorbDamage = (int)(statusEffectList[i].GetAmount() * this.GetActualStat(statusEffectList[i].GetAmountStat()));
						// 		finalDamage -= absorbDamage;
						// 		float tempValue = 0; // 강타 정의용 임시 array
						// 		StatusEffect sisternaSmite = new StatusEffect("파장 분류 강타", StatusEffectType.Smite,
						// 														true, false, false, false,
						// 														tempValue, Stat.None, tempValue, absorbDamage,
						// 														0, 1, 0, false, 
						// 														"None", EffectVisualType.None, EffectMoveType.None);
						// 		statusEffectList.Add(sisternaSmite);
						// 		statusEffectList[i].SetToBeRemoved(true);
						// 		break;
						// 	}
						// }
						shieldAmount = statusEffectList[i].GetRemainAmount();
						if (shieldAmount > finalDamage)
						{
							statusEffectList[i].SetRemainAmount(shieldAmount - finalDamage);
							finalDamage = 0;
							Debug.Log("Remain Shield Amount : " + statusEffectList[i].GetRemainAmount());
							break;
						}
						else
						{
							finalDamage -= shieldAmount;
							statusEffectList[i].SetToBeRemoved(true);
						}
					}
				}
				this.UpdateStatusEffect();
			}

			if (finalDamage > -1)
				currentHealth -= finalDamage;
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

			// FIXME : 도트데미지는 물뎀인가 마뎀인가? 기획서대로 적용할 것. 언제? 일단 보류중.
			Damaged(UnitClass.None, totalAmount, 0f, true, false);
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
			if ((skillInfo.GetOwner() == this.nameInCode) &&
				(skillInfo.GetRequireLevel() <= partyLevel))
                {
                    Skill skill = skillInfo.GetSkill();
					if(SkillDB.IsLearned(this.nameInCode, skill.GetName()))
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
			if ((passiveSkillInfo.GetOwner() == this.nameInCode) &&
				(passiveSkillInfo.GetRequireLevel() <= partyLevel))
			{
				PassiveSkill passiveSkill = passiveSkillInfo.GetSkill();
				passiveSkillList.Add(passiveSkill);
			}
		}
	}

	// 보너스 텍스트 표시.
	public void PrintDirectionBonus(float bonus)
	{
		directionBonusTextObject.SetActive(true);
		if (bonus == 1.1f)
			directionBonusTextObject.GetComponentInChildren<Text>().text = "측면 공격 보너스 (x1.1)";
		else
			directionBonusTextObject.GetComponentInChildren<Text>().text = "후면 공격 보너스 (x1.25)";
		// Invoke("ActiveFalseAtDelay", 0.5f);
	}

	public void PrintCelestialBonus()
	{
		celestialBonusTextObject.SetActive(true);
		// bonusTextObject.GetComponent<TextMesh>().text = "Celestial bonus";
		celestialBonusTextObject.GetComponentInChildren<Text>().text = "천체속성 보너스 (x1.2)";
		// Invoke("ActiveFalseAtDelay", 0.5f);
	}

	public void ActiveFalseAllBounsText()
	{
		celestialBonusTextObject.SetActive(false);
		chainBonusTextObject.SetActive(false);
		directionBonusTextObject.SetActive(false);
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
		// chainBonusTextObject.GetComponent<TextMesh>().text = "연계" + chainCount + "단";
		chainBonusTextObject.GetComponentInChildren<Text>().text = "연계" + chainCount + "단 (x" + chainBonus + ")";
		// Invoke("ActiveFalseAtDelay", 0.5f);
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

		healthViewer.SetInitHealth(maxHealth, side);
	}

	void LoadSprite()
	{
		UnityEngine.Object[] sprites = Resources.LoadAll("UnitImage/" + nameInCode);

		if (sprites.Length == 0)
		{
			Debug.LogError("Cannot find sprite for " + nameInCode);
			sprites = Resources.LoadAll("UnitImage/notFound");
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
	}

	void Awake()
	{
		// chainBonusTextObject = transform.Find("ChainText").gameObject;
		chainBonusTextObject = GameObject.Find("ChainBounsPanel");
		damageTextObject = transform.Find("DamageText").gameObject;
		recoverTextObject = transform.Find("RecoverText").gameObject;
		activeArrowIcon = transform.Find("ActiveArrowIcon").gameObject;
		// celestialBonusTextObject = transform.Find("BonusText").gameObject;
		celestialBonusTextObject = GameObject.Find("CelestialBounsPanel");
		chainAttackerIcon = transform.Find("icons/chain").gameObject;
		directionBonusTextObject = GameObject.Find("DirectionBounsPanel");
		// chainBonusTextObject.SetActive(false);
		damageTextObject.SetActive(false);
		recoverTextObject.SetActive(false);
		activeArrowIcon.SetActive(false);
		// celestialBonusTextObject.SetActive(false);
		chainAttackerIcon.SetActive(false);

		healthViewer = transform.Find("HealthBar").GetComponent<HealthViewer>();
	}

	// Update is called once per frame
	void Update()
	{

	}
}
