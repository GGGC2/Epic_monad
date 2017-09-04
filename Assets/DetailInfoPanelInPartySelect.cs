using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Enums;
using GameData;

public class DetailInfoPanelInPartySelect : MonoBehaviour {

	public Image unitImage;
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

	public void SetCommonUnitInfoUI(string nameString){
		unitImage.sprite = Utility.IllustOf(nameString);
		unitName.text = UnitInfo.ConvertToKoreanFullName(nameString);
		string hpStatText = UnitInfo.GetStat(nameString, Stat.MaxHealth).ToString();
		hpText.text = hpStatText + " / " + hpStatText;
		int Agility = UnitInfo.GetStat(nameString, Stat.Agility);
		int level = PartyData.level;
		apText.text = level + 60 + (Agility / 2) + " (+" + Agility + ")";
		
		powerText.text = UnitInfo.GetStat(nameString, Stat.Power).ToString();
		defenseText.text = UnitInfo.GetStat(nameString, Stat.Defense).ToString();
		resistText.text = UnitInfo.GetStat(nameString, Stat.Resistance).ToString();
		speedText.text = "100"; // 속도 기본값은 무조건 100

		Utility.SetClassImage(classImage, UnitInfo.GetUnitClass(nameString));
        Utility.SetElementImage(elementImage, UnitInfo.GetElement(nameString));
        Utility.SetCelestialImage(celestialImage, UnitInfo.GetCelestial(nameString));
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}