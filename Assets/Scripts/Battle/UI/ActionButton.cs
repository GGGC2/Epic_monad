using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ActionButton : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler{
	public ActiveSkill skill;
	public Image icon;
	public SkillViewer viewer;

	void Awake(){
		icon = GetComponent<Image>();
	}

	public void Initialize(ActiveSkill newSkill){
		skill = newSkill;
		icon.sprite = skill.icon;
	}

	void IPointerDownHandler.OnPointerDown(PointerEventData eventData){
		OnClick();
	}

	//굳이 OnPointerDown을 거쳐서 오는 건 public으로 선언해서 UIManager에서도 부를 수 있기 위함
	public void OnClick(){
		if(skill != null){
			BattleManager.Instance.CallbackSkillSelect(skill);
			return;
		}
		if(icon.sprite != Resources.Load<Sprite>("transparent")){
			BattleManager.Instance.CallbackStandbyCommand();
		}
	}

	void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData){
		if(skill != null){
			viewer.gameObject.SetActive(true);
			viewer.UpdateSkillViewer(skill);
			return;
		}
		if(icon.sprite != Resources.Load<Sprite>("transparent")){
			viewer.gameObject.SetActive(false);
		}
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData eventData){
		viewer.gameObject.SetActive(false);
	}
}