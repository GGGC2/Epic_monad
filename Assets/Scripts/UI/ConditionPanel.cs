using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Enums;

public class ConditionPanel : MonoBehaviour{
	public Text Win;
	public Text Lose;
	public TutorialManager tutorial;

	public void Initialize(List<BattleTrigger> triggers){
		Win.text = "";
		Lose.text = "";

		if(triggers.FindAll(trigger => trigger.result == TrigResultType.Win).Count > 1){
			SetAllOrOneTrigger(Win, triggers[triggers.Count-1].winTriggerRelation);
		}
		if(triggers.FindAll(trigger => trigger.result == TrigResultType.Lose).Count > 1){
			SetAllOrOneTrigger(Lose, triggers[triggers.Count-1].loseTriggerRelation);
		}

		foreach(BattleTrigger trigger in triggers){
            Text text = Win;
            if (trigger.result == TrigResultType.Win)
                text = Win;
            else if (trigger.result == TrigResultType.Lose)
                text = Lose;
			else{
				continue;
			}

			if (trigger.korName != null) {
				text.text += "- " + trigger.korName;
				if (trigger.reqCount > 0) {
					text.text += "(";
					if (trigger.count < trigger.reqCount)
						text.text += "<color=red>" + trigger.count + "</color>";
					else
						text.text = "<color=green>" + trigger.count + "</color>"; 
					text.text += "/" + trigger.reqCount + ")";
				}
				text.text += "\n";
			}
		}
	}

	void SetAllOrOneTrigger(Text target, BattleTrigger.TriggerRelation relation){
		if(relation == BattleTrigger.TriggerRelation.All){
			target.text += "다음 조건을 모두 충족 :\n";
		}else if(relation == BattleTrigger.TriggerRelation.Sequence){
			target.text += "다음 조건을 순서대로 충족 :\n";
		}else{
			target.text += "다음 중 하나를 충족 :\n";
		}
	}

	public void OnClicked(){
		FindObjectOfType<CameraMover>().SetMovable(true);
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