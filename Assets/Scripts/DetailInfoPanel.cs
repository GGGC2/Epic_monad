using Enums;
using GameData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.EventSystems;

public class DetailInfoPanel : UnitInfoUI{
	public Image illust;
    List<SkillInfoButton> skillButtons = new List<SkillInfoButton>();

    void Awake(){
        //0번이 고유 특성 자리
        for(int i = 0; i <= 10; i++){
            skillButtons.Add(GameObject.Find("SkillPrevButton"+i).GetComponent<SkillInfoButton>());
        }
    }

	public void Initialize(){
		Sprite sprite = Utility.IllustOf(unit.GetNameEng());
		if(sprite != null){
			illust.sprite = sprite;
		}else{
			illust.sprite = Resources.Load<Sprite>("transparent");
        }

		SetCommonUnitInfoUI();

        Debug.Log("Passive : " + unit.passiveSkillList.Count + ", Active : " + unit.activeSkillList.Count);
        
        skillButtons.ForEach(button => button.gameObject.SetActive(true));

		// 고유특성 (렙제 0인 특성) 은 무조건 앞으로
		if (unit.passiveSkillList.Any(pSkill => pSkill.requireLevel == 0)) {
			PassiveSkill uniquePassive = unit.passiveSkillList.Find(pSkill => pSkill.requireLevel == 0);
			skillButtons.First ().Initialize (uniquePassive, unit);
		}
		else {
			skillButtons.First().gameObject.SetActive(false);
		}
		
		// 나머지 스킬을 액티브 -> 패시브 순서로 표시
		for (int i = 1; i <= 10; i++){
            if (i <= unit.activeSkillList.Count){
				skillButtons [i].Initialize (unit.activeSkillList [i - 1], unit);
            }else if(i < unit.activeSkillList.Count + unit.passiveSkillList.Count){
				skillButtons [i].Initialize (unit.passiveSkillList [i - unit.activeSkillList.Count], unit);
            }else{
                skillButtons[i].gameObject.SetActive(false);
            }
        }

		// 스킬 상세설명 초기화
		SkillInfoButton skillButton = skillButtons.Find(button => button.isActiveAndEnabled);
		if (skillButton != null) {
			skillButton.GetComponent<SkillInfoButton> ().SetViewer (unit);
			EventSystem.current.SetSelectedGameObject(skillButton.gameObject);
		}
		else {
			FindObjectOfType<SkillViewer>().Initialize();
		}
	}
}

public class UnitInfoUI : MonoBehaviour{
	public Unit unit;
	public Text unitName;
	public Text hpText;
	public Text apText;
	public Text powerText;
	public Text defenseText;
	public Text resistText;
	public Text speedText;

	public Image classImage;
    public Image elementImage;
	public Image celestialImage;

	public void SetCommonUnitInfoUI(){
		unitName.text = unit.GetNameKor();
		hpText.text = unit.GetCurrentHealth() + " / " + unit.GetStat(Stat.MaxHealth);
		apText.text = unit.GetCurrentActivityPoint() + " (+" + unit.GetStat(Stat.Agility) + ")";
		UpdatePower();
		UpdateDefense();
		UpdateResistance();
		UpdateSpeed();
		Utility.SetClassImage(classImage, unit.GetUnitClass());
        Utility.SetElementImage(elementImage, unit.GetElement());
        Utility.SetCelestialImage(celestialImage, unit.GetCelestial());
	}

	void UpdatePower() {
        int actualPower = unit.GetStat(Stat.Power);
        int originPower = unit.GetBaseStat(Stat.Power);
        powerText.color = Color.white;
        powerText.text = actualPower.ToString();
        if (actualPower > originPower)
            powerText.color = Color.green;
        else if (actualPower < originPower)
            powerText.color = Color.red;
        }

    void UpdateDefense() {
        int actualDefense = unit.GetStat(Stat.Defense);
        int originDefense = unit.GetBaseStat(Stat.Defense);

        defenseText.color = Color.white;
        defenseText.text = actualDefense.ToString();
        if (actualDefense > originDefense)
            defenseText.color = Color.green;
        else if (actualDefense < originDefense)
            defenseText.color = Color.red;
    }

    void UpdateResistance() {
        int actualResistance = unit.GetStat(Stat.Resistance);
        int originResistance = unit.GetBaseStat(Stat.Resistance);

        resistText.color = Color.white;
        resistText.text = actualResistance.ToString();
        if (actualResistance > originResistance)
            resistText.color = Color.green;
        else if (actualResistance < originResistance)
            resistText.color = Color.red;
    }

	void UpdateSpeed() {
		int actualSpeed = (int)unit.GetSpeed ();
		int originSpeed = 100;

		speedText.color = Color.white;
		speedText.text = actualSpeed.ToString ();
		if (actualSpeed > originSpeed)
			speedText.color = Color.green;
		else if (actualSpeed < originSpeed)
			speedText.color = Color.red;
	}
}