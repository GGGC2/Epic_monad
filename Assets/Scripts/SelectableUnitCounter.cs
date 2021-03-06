﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectableUnitCounter : MonoBehaviour {
	public Text text;
	public int maxSelectableUnitNumber = 0;

	public void SetMaxSelectableUnitNumber(int num) {
		maxSelectableUnitNumber = num;
	}

	public bool IsPartyFull() {
		return ReadyManager.Instance.selectedUnits.Count >= maxSelectableUnitNumber;
	}

	void Update () {
		text.text = "출전 가능 인원 : " + ReadyManager.Instance.selectedUnits.Count + " / " + maxSelectableUnitNumber;
	}
}
