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
	
		SetClassImage(UnitInfo.GetUnitClass(nameString));
		SetElementImage(UnitInfo.GetElement(nameString));
		SetCelestialImage(UnitInfo.GetCelestial(nameString));
	}

	public void SetClassImage(UnitClass unitClass) {
        if (unitClass == UnitClass.Melee)
            classImage.sprite = Resources.Load<Sprite>("Icon/Stat/meleeClass");
        else if (unitClass == UnitClass.Magic)
            classImage.sprite = Resources.Load<Sprite>("Icon/Stat/magicClass");
        else
            classImage.sprite = Resources.Load<Sprite>("Icon/Empty");
    }

    public void SetElementImage(Element element) {
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
