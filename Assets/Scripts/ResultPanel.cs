﻿using System.Collections;
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
	public bool alreadyClicked;
	public BattleTriggerChecker Checker;
	public int runningFrame;

	public void Clicked(){
		StartCoroutine(IClicked());
	}

	public IEnumerator IClicked(){
		if(!alreadyClicked){
			alreadyClicked = true;

			foreach(BattleTrigger trigger in Checker.battleTriggers){
				//Debug.Log("TriggerName : " + trigger.korName + ", acquired : " + trigger.acquired);
				if(trigger.acquired){
					TriggerIndex.text += trigger.korName + " " + trigger.reward + MultiplierText(trigger);
					TriggerIndex.text += "\n";
					yield return new WaitForSeconds(0.5f);
				}
			}

			ScoreText.text = "점수 : " + Checker.battleData.rewardPoint;
			yield return new WaitForSeconds(0.5f);
		
			int expTick = Checker.battleData.rewardPoint/runningFrame;
			while(Checker.battleData.rewardPoint > 0){
				if(Checker.battleData.rewardPoint >= expTick)
					UpdateExp(expTick);
				else
					UpdateExp(Checker.battleData.rewardPoint);
				yield return null;
			}
		}
		else
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
		Checker.battleData.rewardPoint -= point;
		UpdatePanel(Checker.battleData.rewardPoint);
	}

	public void UpdatePanel(int remainScore){
		ScoreText.text = "점수 : " + remainScore;
		LevelText.text = "레벨 : " + PartyData.level;
		ExpText.text = "경험치 : " + PartyData.exp + " / " + PartyData.reqExp;
		ExpBar.fillAmount = (float)PartyData.exp / (float)PartyData.reqExp;
	}
}