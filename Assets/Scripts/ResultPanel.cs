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
		StartCoroutine(IClicked());
	}

	public IEnumerator IClicked(){
		if(!alreadyClicked){
			alreadyClicked = true;
			while(Checker.battleData.rewardPoint > 0){
				GameData.AddExp(1);
				Checker.battleData.rewardPoint -= 1;
				UpdateText();
				yield return null;
			}
		}
		else
			Checker.sceneLoader.LoadNextDialogueScene(Checker.nextScriptName);
	}

	void UpdateText(){
		LevelText.GetComponent<Text>().text = "레벨 : " + GameData.level;
		ExpText.GetComponent<Text>().text = "경험치 : " + GameData.exp;
	}
}