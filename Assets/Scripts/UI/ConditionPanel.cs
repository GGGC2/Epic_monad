using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConditionPanel : MonoBehaviour{
	public Text Win;
	public Text Lose;

	public void Initialize(List<BattleTrigger> triggers){
		foreach(BattleTrigger trigger in triggers){
			if(trigger.resultType == BattleTrigger.ResultType.Win)
				Win.text += "- " + trigger.korName + "\n";
			else if(trigger.resultType == BattleTrigger.ResultType.Lose)
				Lose.text += "- " + trigger.korName + "\n";
		}
	}

	public void OnClicked(){
		gameObject.SetActive(false);
	}
}