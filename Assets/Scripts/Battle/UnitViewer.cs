using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Enums;
using System.Collections.Generic;

// This component is used in two UI.
// SelectedUnitViewer and UnitViewer.
public class UnitViewer : MonoBehaviour {

	TileManager tileManager;

	Image unitImage;
	Text nameText;
	Image classImage;
	Image elementImage;
	GameObject elementBuffIcon;
	GameObject elementDebuffIcon;
	Image celestialImage;
	GameObject celestialBuffIcon;
	GameObject celestialDebuffIcon;

	Text hpText;

	Text apText;

	Text powerText;
	Text defenseText;
	Text resistanceText;
	// FIXME : 버프/디버프 표시 임시로 텍스트로.
	Text statusEffectText;

	public void UpdateUnitViewer(Unit unit)
	{
		unitImage.sprite = unit.GetDefaultSprite();
		nameText.text = unit.GetName();
		SetClassImage(unit.GetUnitClass());
		SetElementImage(unit.GetElement());
		CheckElementBuff(unit);
		SetCelestialImage(unit.GetCelestial());
		UpdateHp(unit);
		UpdateAp(unit);
		UpdatePower(unit);
		UpdateDefense(unit);
		UpdateResistance(unit);

		UpdateEffect(unit);
	}

	public void UpdateUnitViewer(string unitName){
		Debug.Assert(unitName != "unselected");
		string hpString = UnitInfo.GetStat(unitName, UnitInfo.StatType.Health).ToString();
		hpText.text = hpString + "/" + hpString;
		powerText.text = UnitInfo.GetStat(unitName, UnitInfo.StatType.Power).ToString();
		defenseText.text = UnitInfo.GetStat(unitName, UnitInfo.StatType.Defense).ToString();
		resistanceText.text = UnitInfo.GetStat(unitName, UnitInfo.StatType.Resist).ToString();

		int Agility = UnitInfo.GetStat(unitName, UnitInfo.StatType.Agility);
		int level = Save.SaveDataCenter.GetSaveData().party.partyLevel;
		apText.text = level+60+(Agility/2) + "(+" + Agility + ")";

		SetClassImage(UnitInfo.GetUnitClass(unitName));
		SetElementImage(UnitInfo.GetElement(unitName));
		SetCelestialImage(UnitInfo.GetCelestial(unitName));
	}

	public void Clear(){
		Sprite transparentSprite = Resources.Load<Sprite>("Icon/transparent");

		hpText.text = "--/--";
		powerText.text = "";
		defenseText.text = "";
		resistanceText.text = "";
		apText.text = "--(+--)";
		nameText.text = "--";
		classImage.sprite = transparentSprite;
		elementImage.sprite = transparentSprite;
		celestialImage.sprite = transparentSprite;
		unitImage.sprite = transparentSprite;
	}

	void UpdateEffect(Unit unit)
	{
		List<StatusEffect> effectList = unit.GetStatusEffectList();
		// Debug.Log(unit.GetName() + " has " + effectList.Count + " se");
		int numberOfEffects = effectList.Count;
		string concattedText = "";
		for (int i = 0; i < numberOfEffects; i++)
		{
			if (!effectList[i].GetIsBuff())
				concattedText += "d_";
			concattedText += effectList[i].GetDisplayName();
			if (effectList[i].GetIsStackable())
				concattedText += "[" + effectList[i].GetRemainStack() +"]";
			if (effectList[i].GetRemainPhase() < 500)
				concattedText += "(" + effectList[i].GetRemainPhase() + ")";
			else
				concattedText += "(--)";
            for (int j = 0; j < effectList[i].fixedElem.actuals.Count; j++) {
                if(effectList[i].GetStatusEffectType(j) == StatusEffectType.Shield) {
                    concattedText += (int)effectList[i].GetRemainAmount(j);
                    concattedText += "/";
                }
                concattedText += (int)effectList[i].GetAmount(j);
                if(j < effectList[i].fixedElem.actuals.Count - 1)   concattedText += ",";
            }
			if (i < numberOfEffects-1)
				concattedText += " ";
		}
		statusEffectText.text = concattedText;
	}

	void CheckElementBuff(Unit unit)
	{
		elementBuffIcon.SetActive(false);
		elementDebuffIcon.SetActive(false);

		if (unit.GetElement() == tileManager.GetTile(unit.GetPosition()).GetTileElement())
		{
			elementBuffIcon.SetActive(true);
		}
	}

	void UpdateHp(Unit unit)
	{
		hpText.text = unit.GetCurrentHealth() + " / " + unit.GetStat(Stat.MaxHealth);
	}

	void UpdateAp(Unit unit)
	{
		apText.text = unit.GetCurrentActivityPoint() + " (+" + unit.GetStat(Stat.Dexturity) + ")";
	}

	void UpdatePower(Unit unit)
	{
		int actualPower = unit.GetStat(Stat.Power);
		int originPower = unit.GetBaseStat(Stat.Power);

		powerText.color = Color.white;
		powerText.text = actualPower.ToString();
		if (actualPower > originPower)
			powerText.color = Color.green;
		else if (actualPower < originPower)
			powerText.color = Color.red;
	}

	void UpdateDefense(Unit unit)
	{
		int actualDefense = unit.GetStat(Stat.Defense);
		int originDefense = unit.GetBaseStat(Stat.Defense);

		defenseText.color = Color.white;
		defenseText.text = actualDefense.ToString();
		if (actualDefense > originDefense)
			defenseText.color = Color.green;
		else if (actualDefense < originDefense)
			defenseText.color = Color.red;
	}

	void UpdateResistance(Unit unit)
	{
		int actualResistance = unit.GetStat(Stat.Resistance);
		int originResistance = unit.GetBaseStat(Stat.Resistance);

		resistanceText.color = Color.white;
		resistanceText.text = actualResistance.ToString();
		if (actualResistance > originResistance)
			resistanceText.color = Color.green;
		else if (actualResistance < originResistance)
			resistanceText.color = Color.red;
	}

	void SetClassImage(UnitClass unitClass)
	{
		if (unitClass == UnitClass.Melee)
			classImage.sprite = Resources.Load<Sprite>("Icon/Stat/meleeClass");
		else if (unitClass == UnitClass.Magic)
			classImage.sprite = Resources.Load<Sprite>("Icon/Stat/magicClass");
		else
			classImage.sprite = Resources.Load<Sprite>("Icon/transparent");
	}

	void SetElementImage(Element element)
	{
		if (element == Element.Fire)
			elementImage.sprite = Resources.Load("Icon/Element/fire", typeof(Sprite)) as Sprite;
		else if (element == Element.Water)
			elementImage.sprite = Resources.Load("Icon/Element/water", typeof(Sprite)) as Sprite;
		else if (element == Element.Plant)
			elementImage.sprite = Resources.Load("Icon/Element/plant", typeof(Sprite)) as Sprite;
		else if (element == Element.Metal)
			elementImage.sprite = Resources.Load("Icon/Element/metal", typeof(Sprite)) as Sprite;
		else
			elementImage.sprite = Resources.Load("Icon/transparent", typeof(Sprite)) as Sprite;
	}

	void SetCelestialImage(Celestial celestial)
	{
		if (celestial == Celestial.Sun)
			celestialImage.sprite = Resources.Load("Icon/Celestial/sun", typeof(Sprite)) as Sprite;
		else if (celestial == Celestial.Moon)
			celestialImage.sprite = Resources.Load("Icon/Celestial/moon", typeof(Sprite)) as Sprite;
		else if (celestial == Celestial.Earth)
			celestialImage.sprite = Resources.Load("Icon/Celestial/earth", typeof(Sprite)) as Sprite;
		else
			celestialImage.sprite = Resources.Load("Icon/transparent", typeof(Sprite)) as Sprite;
	}

	void Awake () {
		tileManager = FindObjectOfType<TileManager>();

		unitImage = transform.Find("UnitImage").GetComponent<Image>();
		nameText = transform.Find("NameText").GetComponent<Text>();
		classImage = transform.Find("ClassImage").GetComponent<Image>();

		elementImage = transform.Find("ElementImage").GetComponent<Image>();
		elementBuffIcon = transform.Find("ElementImage").Find("BuffImage").gameObject;
		elementDebuffIcon = transform.Find("ElementImage").Find("DebuffImage").gameObject;

		celestialImage = transform.Find("CelestialImage").GetComponent<Image>();
		celestialBuffIcon = transform.Find("CelestialImage").Find("BuffImage").gameObject;
		celestialDebuffIcon = transform.Find("CelestialImage").Find("DebuffImage").gameObject;

		hpText = transform.Find("HP").Find("HPText").GetComponent<Text>();
		apText = transform.Find("AP").Find("APText").GetComponent<Text>();

		powerText = transform.Find("Power").Find("PowerText").GetComponent<Text>();
		defenseText = transform.Find("Defense").Find("DefenseText").GetComponent<Text>();
		resistanceText = transform.Find("Resistance").Find("ResistanceText").GetComponent<Text>();

		statusEffectText = transform.Find("buffs").GetComponent<Text>();
	}

	void Start()
	{
		elementBuffIcon.SetActive(false);
		elementDebuffIcon.SetActive(false);

		celestialBuffIcon.SetActive(false);
		celestialDebuffIcon.SetActive(false);
	}
}
