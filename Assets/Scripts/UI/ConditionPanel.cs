using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConditionPanel : MonoBehaviour{
	public Text Win;
	public Text Lose;

	CameraMover cm;

	public void Initialize(List<BattleTrigger> triggers){

		cm = FindObjectOfType<CameraMover>();
		cm.mouseMoveActive = false;

		Win.text = "";
		Lose.text = "";

		foreach(BattleTrigger trigger in triggers){
			if(trigger.resultType == BattleTrigger.ResultType.Win)
				Win.text += "- " + trigger.korName + "\n";
			else if(trigger.resultType == BattleTrigger.ResultType.Lose)
				Lose.text += "- " + trigger.korName + "\n";
		}
	}

	public void OnClicked(){
		cm.mouseMoveActive = true;
		gameObject.SetActive(false);
	}
}