using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillInfoButton : MonoBehaviour{
	Skill skill;
	Text nameText;
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
	}
}