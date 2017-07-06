using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AvailableUnitPanel : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler{
	public string unitName;

	void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
	{
		Debug.Log("PointerEnter");
		GameObject.Find("CharacterIllust").GetComponent<Image>().sprite = Resources.Load<Sprite>("StandingImage/"+unitName+"_standing");
	}

	void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
	{
		
	}

	public void ChangeIllust()
	{
		Debug.Log("ChangeIllust");
		GameObject.Find("CharacterIllust").GetComponent<Image>().sprite = Resources.Load<Sprite>("StandingImage/"+unitName+"_standing");
	}
}