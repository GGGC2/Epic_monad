using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Enums;

public class AvailableUnitButton : MonoBehaviour {

	public Image highlightImage;
	Image standingImage;
	Text unitName;
	Image classImage;
	Image celestialImage;
	Image elementImage;
	public string nameString; // 영어이름: 유닛정보 찾을때 필요

	DetailInfoPanelInPartySelect detailInfoPanelInPartySelect;
	ReadyManager readyManager;

	// Use this for initialization
	void Awake () {
		InactiveHighlight();

		standingImage = transform.Find("CharacterImageMask").Find("CharacterImage").GetComponent<Image>();
		unitName = transform.Find("NameText").GetComponent<Text>();
		classImage = transform.Find("ClassImageMask").Find("ClassImage").GetComponent<Image>();
		celestialImage = transform.Find("CelestialImageMask").Find("CelestialImage").GetComponent<Image>();
		elementImage = transform.Find("ElementImageMask").Find("ElementImage").GetComponent<Image>();

		detailInfoPanelInPartySelect = FindObjectOfType<DetailInfoPanelInPartySelect>();
		readyManager = FindObjectOfType<ReadyManager>();
	}

	public void SetNameAndSprite(string nameString) {
		this.nameString = nameString;
		unitName.text = UnitInfo.ConvertToKoreanName(nameString);

		if(nameString == "unselected")
			standingImage.sprite = Resources.Load<Sprite>("transparent");
		else
			standingImage.sprite = Resources.Load<Sprite>("StandingImage/"+nameString+"_standing");		
	
		Utility.SetClassImage(classImage, UnitInfo.GetUnitClass(nameString));
		Utility.SetElementImage(elementImage, UnitInfo.GetElement(nameString));
		Utility.SetCelestialImage(celestialImage, UnitInfo.GetCelestial(nameString));

        // 첫번째 버튼에 있는 캐릭터 정보를 기본으로 띄우게 함
        if (gameObject.name == "CharacterButton1"){
            detailInfoPanelInPartySelect.SetCommonUnitInfoUI(nameString);
		}
	}

	public void ActiveHighlight() {
		highlightImage.enabled = true;
	}

	public void InactiveHighlight() {
		highlightImage.enabled = false;
	}

	public void SetUnitInfoToDetailPanel() {
		detailInfoPanelInPartySelect.SetCommonUnitInfoUI(nameString);
		SelectUnitIfUnselected();
	}

	void SelectUnitIfUnselected() {
		// 출전중이 아니라면
		if (!readyManager.IsAlreadySelected(nameString)) {
			// 풀팟이면
			if (readyManager.selectableUnitCounter.IsPartyFull()) return;
			else {
				readyManager.AddUnitToSelectedUnitList(this);
			}
		}
		// 출전중일때 누르면 빠짐
		else {
			readyManager.SubUnitToSelectedUnitList(this);
		}
	}
}
