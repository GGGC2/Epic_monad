using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class SkillIcon : MonoBehaviour, IPointerEnterHandler{
	public SkillUIManager InfoPanel;
	public Text InfoText;
	public Skill skill;
	public int column;
	public int level;
	void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData){
		if(GetComponent<Image>().sprite != Resources.Load<Sprite>("Icon/Empty"))
			InfoPanel.UpdateSkillInfoPanel(skill, FindObjectOfType<ReadyManager>().currentUnitName);
	}
}