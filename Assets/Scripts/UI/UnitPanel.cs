using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UnitPanel : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler{
	public string unitName;
	public bool AvailableOrSelected;
	public ReadyManager Manager;

	UnitViewer unitViewer;
	Text nameText;
	Image unitImage;
	
	void Start(){
		nameText = GameObject.Find("NameText").GetComponent<Text>();
		unitImage = GameObject.Find("UnitImage").GetComponent<Image>();
		unitViewer = GameObject.Find("UnitViewerPanel").GetComponent<UnitViewer>();
	}

	void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData){
		if(unitName != "unselected"){
			nameText.text = ToKorean(unitName);
			Object[] sprites = Resources.LoadAll("UnitImage/" + unitName);
			unitImage.sprite = sprites[3] as Sprite;
			unitViewer.UpdateUnitViewer(unitName);
			Manager.currentUnitName = unitName;
			FindObjectOfType<SkillEquipPanel>().UpdateIcons(unitName);
		}
	}

	void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
	{ 
		if(AvailableOrSelected){
			bool AlreadySelected = false;
			foreach(UnitPanel Panel in Manager.selected){
				if(Panel.unitName == unitName){
					AlreadySelected = true;
					break;
				}
			}

			if(!AlreadySelected){
				for(int i = 1; i <= 8; i++){
					GameObject Panel = GameObject.Find("SelectedUnit"+i);
					if(Panel == null)
						break;
					else if(Panel.GetComponent<UnitPanel>().unitName == "unselected"){
						Panel.GetComponent<UnitPanel>().SetNameAndSprite(unitName);
						break;
					}
				}
			}	
		}
		else
			SetNameAndSprite("unselected");
	}

	/*public void ChangeIllust()
	{
		characterIllust.sprite = Resources.Load<Sprite>("StandingImage/"+unitName+"_standing");
	}*/

	public void SetNameAndSprite(string name)
	{
		unitName = name;

		if(name == "unselected")
			gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("UnitImage/portrait_placeholder");
		else
			gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("UnitImage/portrait_" + unitName);		
	}

	string ToKorean(string unitName)
	{
		switch (unitName)
		{
			case "noel":		return "노엘";
			case "grenev":		return "그레네브";
			case "sepia":		return "세피아";
			case "arcadia":		return "아르카디아";
			case "darkenir":	return "달케니르";
			case "yeong":		return "영";
			default:			return "--";
		}
	}
}