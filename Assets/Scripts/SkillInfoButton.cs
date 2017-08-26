using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SkillInfoButton : SkillInfoUI, IPointerDownHandler{
	Image iconSlot;

	public void Initialize(Skill newSkill){
		skill = newSkill;
		nameText = transform.Find("SkillText").GetComponent<Text>();
		iconSlot = transform.Find("SkillImage").GetComponent<Image>();
		
		if(newSkill == null){
			gameObject.SetActive(false);
		}else{
			nameText.text = skill.korName;
			if(skill.icon != null){
				iconSlot.sprite = skill.icon;
			}
		}

		//조건 체크는 패널 지우는 동일 작업을 11번 반복하지 않기 위함
		if(skill.requireLevel == 0){
			base.nameText.text = "";
			costText.text = "";
			cooldownText.text = "";
			rangeType.sprite = Resources.Load<Sprite>("transparent");
			rangeText.text = "";
			explainText.text = "";
		}
	}

	void IPointerDownHandler.OnPointerDown(PointerEventData eventData){
		SetNameText();
		SetCommonSkillInfoUI();
	}
}