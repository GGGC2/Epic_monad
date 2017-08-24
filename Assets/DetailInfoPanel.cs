using Enums;
using GameData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DetailInfoPanel : UnitInfoUI{
	public Image illust;

	public void Initialize(){
		Sprite sprite = Utility.IllustOf(unit.GetNameEng());
		if(sprite != null){
			illust.sprite = sprite;
		}else{
			illust.sprite = Resources.Load<Sprite>("transparent");
		}

		SetCommonUnitInfoUI();
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
		SetClassImage(unit.GetUnitClass());
        SetElementImage(unit.GetElement());
        SetCelestialImage(unit.GetCelestial());
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

	public void SetClassImage(UnitClass unitClass) {
        if(SceneData.stageNumber < Setting.classOpenStage){
            classImage.sprite = Resources.Load<Sprite>("transparent");
            return;
        }

        if (unitClass == UnitClass.Melee)
            classImage.sprite = Resources.Load<Sprite>("Icon/Stat/meleeClass");
        else if (unitClass == UnitClass.Magic)
            classImage.sprite = Resources.Load<Sprite>("Icon/Stat/magicClass");
        else
            classImage.sprite = Resources.Load<Sprite>("Icon/Empty");
    }

    public void SetElementImage(Element element) {
        if(SceneData.stageNumber < Setting.elementOpenStage){
            elementImage.sprite = Resources.Load<Sprite>("transparent");
            return;
        }

        if (element == Element.Fire)
            elementImage.sprite = Resources.Load("Icon/Element/fire", typeof(Sprite)) as Sprite;
        else if (element == Element.Water)
            elementImage.sprite = Resources.Load("Icon/Element/water", typeof(Sprite)) as Sprite;
        else if (element == Element.Plant)
            elementImage.sprite = Resources.Load("Icon/Element/plant", typeof(Sprite)) as Sprite;
        else if (element == Element.Metal)
            elementImage.sprite = Resources.Load("Icon/Element/metal", typeof(Sprite)) as Sprite;
        else
            elementImage.sprite = Resources.Load("Icon/Empty", typeof(Sprite)) as Sprite;
    }

    public void SetCelestialImage(Celestial celestial) {
        if(SceneData.stageNumber < Setting.celestialOpenStage){
            celestialImage.sprite = Resources.Load<Sprite>("transparent");
            return;
        }

        if (celestial == Celestial.Sun)
            celestialImage.sprite = Resources.Load("Icon/Celestial/sun", typeof(Sprite)) as Sprite;
        else if (celestial == Celestial.Moon)
            celestialImage.sprite = Resources.Load("Icon/Celestial/moon", typeof(Sprite)) as Sprite;
        else if (celestial == Celestial.Earth)
            celestialImage.sprite = Resources.Load("Icon/Celestial/earth", typeof(Sprite)) as Sprite;
        else
            celestialImage.sprite = Resources.Load("Icon/Empty", typeof(Sprite)) as Sprite;
    }
}