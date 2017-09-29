using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectableUnitCounter : MonoBehaviour {

	public Text text;
	int maxSelectableUnitNumber = 0;
	int currentSelectedUnitNumber = 0;

	public void SetMaxSelectableUnitNumber(int num) {
		maxSelectableUnitNumber = num;
	}

	public bool IsPartyFull() {
		return currentSelectedUnitNumber >= maxSelectableUnitNumber;
	}

	public void PartyNumberChange(int num) {
		currentSelectedUnitNumber += num;
	}
	
	void Update () {
		text.text = "출전 가능 인원 : " + currentSelectedUnitNumber + " / " + maxSelectableUnitNumber;
	}
}
