using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class PlacedUnitCheckPanel : MonoBehaviour {
	List<Image> unitPortraitList = new List<Image>();
	List<SelectedUnit> selectedUnitList = new List<SelectedUnit>();
	Text text;

	public void SetUnitPortraits (List<SelectedUnit> unitList) {
		this.selectedUnitList = unitList;
		for (int i = 0; i < unitList.Count; i++){
			Sprite unitPortraitSprite = Resources.Load<Sprite>("UnitImage/portrait_" + unitList[i].name);
			unitPortraitList[i].sprite = unitPortraitSprite;
		} 
	}

	public void HighlightPortrait (string unitString) {
		unitPortraitList.ForEach(img => img.color = Color.gray);
		var targetPortraitIndex = selectedUnitList.IndexOf(selectedUnitList.Find(unit => unit.name == unitString));
		if (targetPortraitIndex != -1)
			unitPortraitList[targetPortraitIndex].color = Color.white;
	}

	public void SetText(string inputText) {
		text.text = inputText;
	}

	void Awake() {
		for (int i = 1; i <= 8; i++) {
			unitPortraitList.Add(GameObject.Find("PlacedUnitImage (" + i + ")").GetComponent<Image>());
		}

		text = GameObject.Find("PlacedUIText").GetComponent<Text>();
	}

	// Use this for initialization
	void Start () {
		text.text = "시작 위치를 선택해주세요";
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
