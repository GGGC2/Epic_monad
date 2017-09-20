using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Enums;

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

		if(triggers.FindAll(trigger => trigger.resultType == TrigResultType.Win).Count > 1){
			SetAllOrOneTrigger(Win, triggers[triggers.Count-1].winTriggerRelation);
		}
		if(triggers.FindAll(trigger => trigger.resultType == TrigResultType.Lose).Count > 1){
			SetAllOrOneTrigger(Lose, triggers[triggers.Count-1].loseTriggerRelation);
		}

		foreach(BattleTrigger trigger in triggers){
			if(trigger.resultType == TrigResultType.Win)
				Win.text += "- " + trigger.korName + "\n";
			else if(trigger.resultType == TrigResultType.Lose)
				Lose.text += "- " + trigger.korName + "\n";
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