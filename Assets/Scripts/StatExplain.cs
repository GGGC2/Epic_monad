using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Enums;

public class StatExplain : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler{
	public Stat stat;
	public GameObject ExplainPanel;
	Text DataSource;
	Text textUI;
	
	void Awake(){
		DataSource = transform.Find("Text").GetComponent<Text>();
		textUI = ExplainPanel.transform.Find("Text").GetComponent<Text>();
		ExplainPanel.SetActive(false);
	}

	void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData){
		Debug.Log("OnPointerEnter");
		ExplainPanel.SetActive(true);
		float statPoint = float.Parse(DataSource.text);
		if(stat == Stat.Defense){
			textUI.text = "물리 피해 " + Math.Round(statPoint*100/(statPoint+200), 1) + "% 감소";
		}else if(stat == Stat.Resistance){
			textUI.text = "마법 피해 " + Math.Round(statPoint*100/(statPoint+200), 1) + "% 감소";
		}else if(stat == Stat.Agility){ //원래 Speed인데 목록에 없어서 대체
			if(statPoint > 100){
				textUI.text = "소모 행동력 " + Math.Round((statPoint-100)*100/(statPoint), 1) + "% 감소";
			}else if(statPoint < 100){
				textUI.text = "소모 행동력 " + Math.Round((100-statPoint)*100/(statPoint), 1) + "% 증가";
			}else{
				textUI.text = "소모 행동력 불변";
			}
		}
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData eventData){
		ExplainPanel.SetActive(false);
	}
}