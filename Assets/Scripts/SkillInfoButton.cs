using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SkillInfoButton : SkillUI{
	Text myNameText;

	public void Awake(){
		myNameText = transform.Find("SkillText").GetComponent<Text>();
		iconSlot = transform.Find("SkillImage").GetComponent<Image>();
	}

	public void Start(){
		if(gameObject.name == "SkillPrevButton0"){
			viewer.Initialize();
		}
	}

	public void Initialize(Skill newSkill, Unit owner){
		mySkill = newSkill;
		if(newSkill == null){
			gameObject.SetActive(false);
		}else{
			myNameText.text = mySkill.korName;
			if(mySkill.icon != null){
				iconSlot.sprite = mySkill.icon;
			}
		}
		this.owner = owner;
	}
}