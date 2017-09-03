using Enums;
using GameData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
        for(int i = 0; i <= 10; i++){
            skillButtons[i].gameObject.SetActive(true);
            if(i < unit.passiveSkillList.Count){
                skillButtons[i].Initialize(unit.passiveSkillList[i]);
            }else if(i < unit.passiveSkillList.Count + unit.activeSkillList.Count){
                skillButtons[i].Initialize(unit.activeSkillList[i - unit.passiveSkillList.Count]);
            }else{
                skillButtons[i].gameObject.SetActive(false);
            }
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