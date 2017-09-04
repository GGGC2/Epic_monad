using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Enums;

public class AvailableUnitButton : MonoBehaviour {

	Image standingImage;
	Text unitName;
	Image classImage;
	Image celestialImage;
	Image elementImage;

	// Use this for initialization
	void Start () {
		standingImage = transform.Find("CharacterImageMask").Find("CharacterImage").GetComponent<Image>();
		unitName = transform.Find("NameText").GetComponent<Text>();
		classImage = transform.Find("ClassImageMask").Find("ClassImage").GetComponent<Image>();
		celestialImage = transform.Find("CelestialImageMask").Find("CelestialImage").GetComponent<Image>();
		elementImage = transform.Find("ElementImageMask").Find("ElementImage").GetComponent<Image>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SetNameAndSprite(string nameString) {
		unitName.text = UnitInfo.ConvertToKoreanName(nameString);

		if(nameString == "unselected")
			standingImage.sprite = Resources.Load<Sprite>("transparent");
		else
			standingImage.sprite = Resources.Load<Sprite>("StandingImage/"+nameString+"_standing");		
	
		Utility.SetClassImage(classImage, UnitInfo.GetUnitClass(nameString));
		Utility.SetElementImage(elementImage, UnitInfo.GetElement(nameString));
		Utility.SetCelestialImage(celestialImage, UnitInfo.GetCelestial(nameString));

        // 첫번째 버튼에 있는 캐릭터 정보를 기본으로 띄우게 함
        if (gameObject.name == "CharacterButton1")
            FindObjectOfType<DetailInfoPanelInPartySelect>().SetCommonUnitInfoUI(nameString);       
	}
}
