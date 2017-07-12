using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultPanel : MonoBehaviour
{
	public GameObject LevelText;
	public GameObject ExpText;
	public bool alreadyClicked;
	public BattleTriggerChecker Checker;

	void OnEnable(){
		UpdateText();
	}
	public void Clicked(){
		if(!alreadyClicked){
			GameData.AddExp(Checker.battleData.rewardPoint);
			alreadyClicked = true;
			UpdateText();
		}
		else
			Checker.sceneLoader.LoadNextDialogueScene(Checker.nextScriptName);
	}

	void UpdateText(){
		LevelText.GetComponent<Text>().text = "레벨 : " + GameData.level;
		ExpText.GetComponent<Text>().text = "경험치 : " + GameData.exp;
	}
}