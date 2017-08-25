using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ActionButton : MonoBehaviour, IPointerDownHandler{
	public ActiveSkill skill;
	public Image icon;

	void Awake(){
		icon = GetComponent<Image>();
	}

	public void Initialize(ActiveSkill newSkill){
		skill = newSkill;
		icon.sprite = skill.icon;
	}

	void IPointerDownHandler.OnPointerDown(PointerEventData eventData){
		if(skill != null){
			FindObjectOfType<BattleManager>().CallbackSkillSelect(skill);
			return;
		}
		if(icon != null){
			FindObjectOfType<BattleManager>().CallbackStandbyCommand();
		}
	}
}