﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Linq;
using GameData;
public class ReadyManager : MonoBehaviour{
	TextAsset csvFile;
	public SelectableUnitCounter selectableUnitCounter;
	public List<string> selectedUnitList = new List<string>();
	public List<UnitPanel> selected = new List<UnitPanel>();
	public string currentUnitName;

	List<GameObject> availableUnitButtons = new List<GameObject>();

	public Button readyButton;
	
	public bool IsAlreadySelected(string unitName) {
		return selectedUnitList.Contains(unitName);
	}

	public void AddUnitToSelectedUnitList(AvailableUnitButton abbutton) {
		selectedUnitList.Add(abbutton.nameString);
		selectableUnitCounter.PartyNumberChange(1);
		abbutton.ActiveHighlight();
	}

	public void SubUnitToSelectedUnitList(AvailableUnitButton abbutton) {
		selectedUnitList.Remove(abbutton.nameString);
		selectableUnitCounter.PartyNumberChange(-1);
		abbutton.InactiveHighlight();
	}

	void Start(){
		csvFile = Resources.Load<TextAsset>("Data/StageAvailablePC");
		// string[] stageData = Parser.FindRowDataOf(csvFile.text, SceneData.stageNumber.ToString());
		string[] stageData = Parser.FindRowDataOf(csvFile.text, "99");

		int numberOfSelectableUnit = Int32.Parse(stageData[1]);
		int numberOfAvailableUnit = Int32.Parse(stageData[2]);

		selectableUnitCounter = FindObjectOfType<SelectableUnitCounter>();
		selectableUnitCounter.SetMaxSelectableUnitNumber(numberOfSelectableUnit);

		for (int i = 1; i <= 20; i++){
			GameObject availableUnitButton = GameObject.Find("CharacterButton" + i);
			availableUnitButtons.Add(availableUnitButton);
			if (i <= numberOfAvailableUnit)
				availableUnitButton.GetComponent<AvailableUnitButton>().SetNameAndSprite(stageData[i+2]);
			else
				availableUnitButton.SetActive(false);
		}

		DontDestroyOnLoad(gameObject);
	}

	void Update(){
		if (readyButton == null) return;

		if (IsThereAnyReadiedUnit()) {
			readyButton.interactable = true;
			readyButton.GetComponent<Image>().color = Color.white;
		}
		else {
			readyButton.interactable = false;
			readyButton.GetComponent<Image>().color = Color.gray;
		}

		if(Input.GetKeyDown(KeyCode.A))
			GameObject.Find("SceneLoader").GetComponent<SceneLoader>().LoadNextBattleScene();
	}

	bool IsThereAnyReadiedUnit(){
		return selectedUnitList.Count > 0;
	}

	public void ReadyButtonDown(){
		GameObject.Find("SceneLoader").GetComponent<SceneLoader>().LoadNextBattleScene();
	}
}
