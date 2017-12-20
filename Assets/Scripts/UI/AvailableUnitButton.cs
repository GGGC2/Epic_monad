using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Enums;

public class AvailableUnitButton : MonoBehaviour, IPointerDownHandler{
	public Image highlightImage;
	Image standingImage;
	Text unitName;
	Image classImage;
	Image celestialImage;
	Image elementImage;
	public string nameString; // 영어이름: 유닛정보 찾을때 필요

	public BattleReadyPanel ReadyPanel;
	public RightScreen_BattleReady BattleReadyRightPanel; //오른쪽 절반 화면(RightScreen)을 담당
	ReadyManager RM;

	void Awake (){
		ActiveHighlight(false);

		standingImage = transform.Find("CharacterImageMask").Find("CharacterImage").GetComponent<Image>();
		unitName = transform.Find("NameText").GetComponent<Text>();
		classImage = transform.Find("ClassImageMask").Find("ClassImage").GetComponent<Image>();
		celestialImage = transform.Find("CelestialImageMask").Find("CelestialImage").GetComponent<Image>();
		elementImage = transform.Find("ElementImageMask").Find("ElementImage").GetComponent<Image>();

		RM = FindObjectOfType<ReadyManager>();
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
	}

	public void ActiveHighlight(bool onoff = true) {
		highlightImage.enabled = onoff;
	}

	void IPointerDownHandler.OnPointerDown(PointerEventData eventData){
		OnClicked();
	}

	public void OnClicked() {
		SelectUnitIfUnselected();
		BattleReadyRightPanel.unitName.text = UnitInfo.ConvertToKoreanFullName(nameString);
		RM.RecentUnitButton = this;
		if(ReadyPanel.panelType == BattleReadyPanel.PanelType.Party){
			BattleReadyRightPanel.SetCommonUnitInfoUI(nameString);
		}else if(ReadyPanel.panelType == BattleReadyPanel.PanelType.Ether){
			ReadyPanel.SetPanelType(BattleReadyPanel.PanelType.Ether);
		}
	}

	void SelectUnitIfUnselected() {
		// 출전중이 아니라면
		if (!RM.IsAlreadySelected(nameString)) {
			// 풀팟이면
			if (RM.selectableUnitCounter.IsPartyFull()) return;
			else {
				RM.AddUnitToSelectedUnitList(this);
			}
		}
		// 출전중일때 누르면 빠짐(파티 화면 한정)
		else if(RM.RecentUnitButton == this){
			RM.SubUnitToSelectedUnitList(this);
		}
	}
}