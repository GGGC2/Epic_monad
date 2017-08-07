using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour {

	int index;
	public Image image;
	CameraMover cm;
	string usedSceneName;
	public Button ReverseButton;

	void Start () {
		usedSceneName = SceneManager.GetActiveScene().name;
		index = 0;

        if (usedSceneName == "Battle"){
            cm = FindObjectOfType<CameraMover>();
            cm.mouseMoveActive = false;
            cm.keyboardMoveActive = false;
        }
        OnClick();
        if (GameData.SceneData.isTestMode || GameData.SceneData.isStageMode) {
            Skip();
        }
    }
	
	void OnClick(){
		index += 1;
		CheckReverseButtonActive();
		Sprite newSprite = Resources.Load<Sprite>("Tutorial/" + usedSceneName + GameData.SceneData.stageNumber.ToString() + "_" + index.ToString());

		if(newSprite == null){
			if(usedSceneName == "Battle"){
				cm.mouseMoveActive = true;
				cm.keyboardMoveActive = true;
			}
			gameObject.SetActive(false);
		}
		else
			image.sprite = newSprite;
	}

	void CheckReverseButtonActive(){
		if(index <= 1)
			ReverseButton.gameObject.SetActive(false);
		else
			ReverseButton.gameObject.SetActive(true);
	}

	public void Skip(){
		gameObject.SetActive(false);
	}

	public void Reverse(){
		index -= 1;
		CheckReverseButtonActive();
		image.sprite = Resources.Load<Sprite>("Tutorial/" + usedSceneName + GameData.SceneData.stageNumber.ToString() + "_" + index.ToString());
	}
}