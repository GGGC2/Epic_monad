using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour {

	int index;
	readonly int maxIndex = 9;

	public Image image;
	CameraMover cm;

	// Use this for initialization
	void Start () {
		index = 1;

		cm = FindObjectOfType<CameraMover>();
		cm.mouseMoveActive = false;
		cm.keyboardMoveActive = false;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void OnClick() {
		if (index < maxIndex)
		{
			index++;
			Sprite newSprite = Resources.Load("Tutorial/tutorial" + index.ToString(), typeof(Sprite)) as Sprite;
			image.sprite = newSprite;
		}
		else if (index == maxIndex)
		{
			cm.mouseMoveActive = true;
			cm.keyboardMoveActive = true;
			gameObject.SetActive(false);
		}
			
	}
}
