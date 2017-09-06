using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SkillInfoButton : SkillInfoUI, IPointerDownHandler{
	Image iconSlot;
	Text myNameText;

	public void Awake(){
		myNameText = transform.Find("SkillText").GetComponent<Text>();
		iconSlot = transform.Find("SkillImage").GetComponent<Image>();
	}

	public void Start() { 
		if(gameObject.name == "SkillPrevButton0"){
			// GetComponent<Button>().onClick.Invoke();
			viewerNameText.text = "";
			costText.text = "";
			cooldownText.text = "";
			rangeType.sprite = Resources.Load<Sprite>("transparent");
			rangeText.text = "";
			explainText.text = "";
		}
	}

	public void Initialize(Skill newSkill){
		Debug.Log("Setting Skill : " + newSkill.korName);
		skill = newSkill;		
		if(newSkill == null){
			gameObject.SetActive(false);
		}else{
			myNameText.text = skill.korName;
			if(skill.icon != null){
				iconSlot.sprite = skill.icon;
			}
		}
	}

	void IPointerDownHandler.OnPointerDown(PointerEventData eventData){ //Debug.Log(eventData);
		SetCommonSkillInfoUI();
	}
}