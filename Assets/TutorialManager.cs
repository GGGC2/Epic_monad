using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour {

	int index;
	public Image image;
	CameraMover cm;

	void Start () {
		index = 0;

		cm = FindObjectOfType<CameraMover>();
		cm.mouseMoveActive = false;
		cm.keyboardMoveActive = false;

		OnClick();
	}
	
	void OnClick(){
		index += 1;
		Sprite newSprite = Resources.Load<Sprite>("Tutorial/" + GameData.SceneData.stageNumber.ToString() + "_" + index.ToString());

		if(newSprite == null){
			cm.mouseMoveActive = true;
			cm.keyboardMoveActive = true;
			gameObject.SetActive(false);
		}
		else
			image.sprite = newSprite;
	}
}