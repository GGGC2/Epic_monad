using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour {

	int index;
	readonly int maxIndex = 9;

	public Image image;

	// Use this for initialization
	void Start () {
		index = 1;
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
			gameObject.SetActive(false);
		}
			
	}
}
