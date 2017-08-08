using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using GameData;

public class TutorialManager : MonoBehaviour {
	public Image image;
	public Image DarkBG;
	CameraMover cm;
	string usedSceneName;
	public Button ReverseButton;
	public TutorialScenario scenarioPrefab;
	public TutorialScenario scenario;

	public void OnEnable(){
		usedSceneName = SceneManager.GetActiveScene().name;

		Sprite searchedSprite = Resources.Load<Sprite>("Tutorial/" + usedSceneName + SceneData.stageNumber.ToString() + "_1");

		if(searchedSprite == null || SceneData.isTestMode || SceneData.isStageMode){
			if(usedSceneName == "Battle"){
				cm.mouseMoveActive = true;
				cm.keyboardMoveActive = true;
			}
			gameObject.SetActive(false);
		}
		else{
			FindObjectOfType<BattleManager>().onTutorial = true;
			scenario = Instantiate(scenarioPrefab);
			scenario.Manager = this;
			scenario.SetNewSprite();
		}
    }

	public void NextStep(){
		scenario.NextStep();	
	}
	
	/*void CheckReverseButtonActive(){
		if(index <= 1)
			ReverseButton.gameObject.SetActive(false);
		else
			ReverseButton.gameObject.SetActive(true);
	}*/

	public void Skip(){
		gameObject.SetActive(false);
	}

	/*public void Reverse(){
		index -= 1;
		CheckReverseButtonActive();
		image.sprite = Resources.Load<Sprite>("Tutorial/" + usedSceneName + GameData.SceneData.stageNumber.ToString() + "_" + index.ToString());
	}*/
}