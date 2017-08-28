using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ActionButton : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler{
	public ActiveSkill skill;
	public Image icon;
	public SkillViewer viewer;
	public bool clickable;
	public bool onOffLock = false;
	public UnityEvent clicked = new UnityEvent();

	void Awake(){
		icon = GetComponent<Image>();
	}

	public void Initialize(ActiveSkill newSkill){
		skill = newSkill;
		icon.sprite = skill.icon;
	}

	public void Activate(bool isActive){
		if (UIManager.Instance.ActionButtonOnOffLock)
			return;
		clickable = isActive;
		if(isActive){
			icon.color = Color.white;
		}else{
			icon.color = new Color (0.3f, 0.3f, 0.3f);
		}
	}

	void IPointerDownHandler.OnPointerDown(PointerEventData eventData){
		if (clickable) {
			StartCoroutine(OnClick());
		}
	}

	//굳이 OnPointerDown을 거쳐서 오는 건 public으로 선언해서 UIManager에서도 부를 수 있기 위함
	public IEnumerator OnClick(){
		if(BattleData.currentState == CurrentState.SelectSkillApplyDirection || BattleData.currentState == CurrentState.SelectSkillApplyPoint){
			BattleData.triggers.rightClicked.Trigger();
		}

		yield return StartCoroutine(Utility.WaitForFewFrames(3));

		if(skill != null){
			BattleManager.Instance.CallbackSkillSelect(skill);
		}else if(icon.sprite != Resources.Load<Sprite>("transparent")){
			BattleManager.Instance.CallbackStandbyCommand();
		}		
		clicked.Invoke();
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