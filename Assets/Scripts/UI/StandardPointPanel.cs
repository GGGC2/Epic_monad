﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using GameData;


public class StandardPointPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler{
	public GameObject Panel;
	void IPointerEnterHandler.OnPointerEnter(PointerEventData pointerData){
		Panel.SetActive(true);
		Panel.transform.Find("Text").GetComponent<Text>().text = "턴 기준값 : " + (PartyData.level+60);
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData pointerData){
		Panel.SetActive(false);
	}
}
