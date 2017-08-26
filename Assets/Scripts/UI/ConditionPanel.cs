using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConditionPanel : MonoBehaviour{
	public Text Win;
	public Text Lose;
	public TutorialManager tutorial;

	CameraMover cm;

	public void Initialize(List<BattleTrigger> triggers){

		cm = FindObjectOfType<CameraMover>();
		cm.SetMovable(false);

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
		cm.SetMovable(true);
		gameObject.SetActive(false);
		//tutorial.gameObject.SetActive(true);
	}
}