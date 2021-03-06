﻿using System.Collections;
using Enums;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ActionButton : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler{
	public ActiveSkill skill;
	public Image icon;
    public ActionButtonType type;
    public GameObject standByOrRestExplanation;
	public SkillViewer viewer;
	public bool clickable;
	public bool onOffLock = false;
	public UnityEvent clicked = new UnityEvent();

	Image frameImage;
	Material grayscale;

	void Awake(){
		icon = GetComponent<Image>();
		frameImage = transform.Find(name+"Frame").GetComponent<Image>();
	}

	public void InitializeWithSkill(ActiveSkill newSkill){
        type = ActionButtonType.Skill;
		skill = newSkill;
		icon.sprite = skill.icon;
		frameImage.enabled = true;
	}

	public void Absent() {
		type = ActionButtonType.Absent;
		skill = null;
        clickable = false;
        standByOrRestExplanation.SetActive(false);
		icon.sprite = Resources.Load<Sprite> ("transparent");
		frameImage.enabled = false;
	}

	public void Activate(bool isActive){
		if (UIManager.Instance.ActionButtonOnOffLock || type == ActionButtonType.Absent)
			return;
		clickable = isActive;
		if (isActive) {
			icon.material = null;
		}
		else {
			icon.material = grayscale;
		}
		frameImage.enabled = true;
	}

	void IPointerDownHandler.OnPointerDown(PointerEventData eventData){
		StartCoroutine(OnClick());
	}

	//굳이 OnPointerDown을 거쳐서 오는 건 public으로 선언해서 UIManager에서도 부를 수 있기 위함
	public IEnumerator OnClick(){
		if(clickable){
			yield return StartCoroutine(Utility.WaitForFewFrames(3));

			if(type == ActionButtonType.Skill){
				BattleManager.Instance.CallbackSkillSelect(skill);
			}else if(type == ActionButtonType.Collect) {
                BattleManager.Instance.CallbackCollectCommand();
            }else if(icon.sprite != Resources.Load<Sprite>("transparent")){
				BattleManager.Instance.CallbackStandbyCommand();
			}		
			clicked.Invoke();
		}
	}

	void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData){
		if(skill != null){
			viewer.gameObject.SetActive(true);
			viewer.UpdateSkillViewer (skill, BattleData.turnUnit);
			return;
		}
        else if(clickable){
            standByOrRestExplanation.SetActive(true);
            Text text = standByOrRestExplanation.GetComponentInChildren<Text>();
            if (type == ActionButtonType.Standby) text.text = "턴을 종료합니다.";
            else if(type == ActionButtonType.Rest) text.text = GetRestExplanationText();
            else if(type == ActionButtonType.Collect)   text.text = "오브젝트를 수집합니다.";
        }
		if(icon.sprite != Resources.Load<Sprite>("transparent")){
			viewer.gameObject.SetActive(false);
		}
	}

    string GetRestExplanationText() {
        Unit unit = BattleData.turnUnit;
        LogManager.Instance.Record(new RestLog(unit));
        RestAndRecover.Run();
        EventLog log = LogManager.Instance.PopLastEventLog();
        int predictedUsingAP = 0;
        int predictedRecoveringHP = 0;
        foreach (var effectLog in log.getEffectLogList()) {
            if (effectLog is APChangeLog) {
                APChangeLog apChangeLog = (APChangeLog)effectLog;
                if (apChangeLog.unit == unit) predictedUsingAP = -apChangeLog.amount;
            } else if (effectLog is HPChangeLog) {
                HPChangeLog hpChangeLog = (HPChangeLog)effectLog;
                if (hpChangeLog.target == unit) predictedRecoveringHP = hpChangeLog.amount;
            }
        }
        return "<color=cyan>" + predictedUsingAP + "</color>" + "의 행동력을 소모하여 " + "\n" +
               "<color=green>" + predictedRecoveringHP + "</color>" + "의 체력을 회복합니다.";
    }

	void Start(){
		grayscale = Resources.Load<Material> ("Shader/grayscale");

		if(viewer.gameObject.activeSelf){
			viewer.gameObject.SetActive(false);
		}
	}
	void IPointerExitHandler.OnPointerExit(PointerEventData eventData){
		viewer.gameObject.SetActive(false);
        standByOrRestExplanation.SetActive(false);
	}
}