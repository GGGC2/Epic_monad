using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameData;

public class ResultPanel : MonoBehaviour{
	public GameObject LevelText;
	public GameObject ExpText;
	public GameObject ScoreText;
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

	void UpdateExp(int point){
		PartyData.AddExp(point);
		Checker.battleData.rewardPoint -= point;
		UpdatePanel();
	}

	public void UpdatePanel(){
		ScoreText.GetComponent<Text>().text = "점수 : " + Checker.battleData.rewardPoint;
		LevelText.GetComponent<Text>().text = "레벨 : " + PartyData.level;
		ExpText.GetComponent<Text>().text = "경험치 : " + PartyData.exp;

		if (PartyData.reqExp != 0)
			ExpBar.fillAmount = (float)PartyData.exp / (float)PartyData.reqExp;
	}
}