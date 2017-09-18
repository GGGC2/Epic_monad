using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlacedUnitCheckPanel : MonoBehaviour {

	List<Image> unitPortraitList = new List<Image>();

	public void SetUnitPortraits (List<string> unitStringList) {
		for (int i = 0; i < unitStringList.Count; i++){
			Sprite unitPortraitSprite = Resources.Load<Sprite>("UnitImage/portrait_" + unitStringList[i]);
			unitPortraitList[i].sprite = unitPortraitSprite;
		} 
	}

	void Awake() {
		for (int i = 1; i <= 8; i++) {
			unitPortraitList.Add(GameObject.Find("PlacedUnitImage (" + i + ")").GetComponent<Image>());
		}
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
