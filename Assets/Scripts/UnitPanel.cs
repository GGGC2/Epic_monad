using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UnitPanel : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerExitHandler{
	public string unitName;
	public bool AvailableOrSelected;
	public ReadyManager Manager;

	void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData){
		if(unitName != "unselected"){
			GameObject.Find("CharacterIllust").GetComponent<Image>().sprite = Resources.Load<Sprite>("StandingImage/"+unitName+"_standing");
			GameObject.Find("UnitImage").GetComponent<Image>().sprite = Resources.Load<Sprite>("UnitImage/"+unitName+"_2");
			GameObject.Find("UnitViewerPanel").GetComponent<UnitViewer>().UpdateUnitViewer(unitName);
		}
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData eventData){
		GameObject.Find("UnitViewerPanel").GetComponent<UnitViewer>().Clear();
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

	public void ChangeIllust()
	{
		GameObject.Find("CharacterIllust").GetComponent<Image>().sprite = Resources.Load<Sprite>("StandingImage/"+unitName+"_standing");
	}

	public void SetNameAndSprite(string name)
	{
		unitName = name;

		if(name == "unselected")
			gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("UnitImage/portrait_placeholder");
		else
			gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("UnitImage/portrait_" + unitName);		
	}
}