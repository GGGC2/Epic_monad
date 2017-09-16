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

		if(triggers.FindAll(trigger => trigger.resultType == BattleTrigger.ResultType.Win).Count > 1){
			SetAllOrOneTrigger(Win, triggers[triggers.Count-1].winTriggerAll);
		}
		if(triggers.FindAll(trigger => trigger.resultType == BattleTrigger.ResultType.Lose).Count > 1){
			SetAllOrOneTrigger(Lose, triggers[triggers.Count-1].loseTriggerAll);
		}

		foreach(BattleTrigger trigger in triggers){
			if(trigger.resultType == BattleTrigger.ResultType.Win)
				Win.text += "- " + trigger.korName + "\n";
			else if(trigger.resultType == BattleTrigger.ResultType.Lose)
				Lose.text += "- " + trigger.korName + "\n";
		}
	}

	void SetAllOrOneTrigger(Text target, bool isAll){
		if(isAll){
			target.text += "다음 조건을 모두 충족 :\n";
		}else{
			target.text += "다음 중 하나를 충족 :\n";
		}
	}

	public void OnClicked(){
		cm.SetMovable(true);
		gameObject.SetActive(false);

		if (FindObjectOfType<ReadyManager>() == null) {
			BattleData.battleManager.StartTurnManager();
		}
		else {
			BattleData.uiManager.EnablePlacedUnitCheckUI();
		}
		//tutorial.gameObject.SetActive(true);
	}
}