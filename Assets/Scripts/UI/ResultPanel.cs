using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameData;

public class ResultPanel : MonoBehaviour{
	public Text LevelText;
	public Text ExpText;
	public Text ScoreText;
	public Text TriggerIndex;
	public Image ExpBar;
	private bool alreadyClicked;
	public BattleTriggerManager Checker;
	public int runningFrame;

	public void Clicked(){
		if (!alreadyClicked) {
			alreadyClicked = true;
			StartCoroutine (IClicked ());
		}
	}

	public IEnumerator IClicked(){
		foreach(BattleTrigger trigger in Checker.battleTriggers){
			//Debug.Log("TriggerName : " + trigger.korName + ", acquired : " + trigger.acquired);
			if(trigger.acquired){
				TriggerIndex.text += trigger.korName + " " + trigger.reward + MultiplierText(trigger);
				TriggerIndex.text += "\n";
				yield return new WaitForSeconds(0.5f);
			}
		}

		ScoreText.text = "점수 : " + BattleData.rewardPoint;
		yield return new WaitForSeconds(0.5f);

		int expTick = BattleData.rewardPoint/runningFrame;
		while(BattleData.rewardPoint > 0){
			if(expTick == 0)
				UpdateExp(1);
			else if(BattleData.rewardPoint >= expTick)
				UpdateExp(expTick);
			else
				UpdateExp(BattleData.rewardPoint);
			yield return null;
		}

		//다 출력된 후 클릭을 해야 넘어감
		yield return new WaitUntil (() => Input.GetMouseButtonDown(0));

		Checker.sceneLoader.LoadNextDialogueScene(Checker.nextScriptName);
	}

	string MultiplierText(BattleTrigger trigger){
		if(!trigger.repeatable || trigger.count == 1)
			return "";
		else
			return " x"+trigger.count;
	}

	void UpdateExp(int point){
		PartyData.AddExp(point);
		BattleData.rewardPoint -= point;
		UpdatePanel(BattleData.rewardPoint);
	}

	public void UpdatePanel(int remainScore){
		Debug.Log("PartyLevel = " + PartyData.GetLevel());
		ScoreText.text = "점수 : " + remainScore;
		LevelText.text = "레벨 : " + PartyData.GetLevel();
		ExpText.text = "경험치 : " + PartyData.exp + " / " + PartyData.reqExp;
		ExpBar.fillAmount = (float)PartyData.exp / (float)PartyData.reqExp;
	}
}