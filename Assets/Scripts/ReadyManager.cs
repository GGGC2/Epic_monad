using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Linq;
using GameData;
public class ReadyManager : MonoBehaviour{
	TextAsset csvFile;
	public List<UnitPanel> selected = new List<UnitPanel>();
	public string currentUnitName;

	List<GameObject> availableUnitButtons = new List<GameObject>();

	public Button readyButton;

	void Start(){
		csvFile = Resources.Load<TextAsset>("Data/StageAvailablePC");
		// string[] stageData = Parser.FindRowDataOf(csvFile.text, SceneData.stageNumber.ToString());
		string[] stageData = Parser.FindRowDataOf(csvFile.text, "99");


		for (int i = 1; i <= 20; i++){
			GameObject availableUnitButton = GameObject.Find("CharacterButton" + i);
			availableUnitButtons.Add(availableUnitButton);
			if (i <= Int32.Parse(stageData[2]))
				availableUnitButton.GetComponent<AvailableUnitButton>().SetNameAndSprite(stageData[i+2]);
			else
				availableUnitButton.SetActive(false);
		}

		// for (int i = 1; i <= 8; i++){
		// 	UnitPanel Panel = GameObject.Find("SelectedUnit"+i).GetComponent<UnitPanel>();
		// 	if (i <= Int32.Parse(stageData[1])){
		// 		selected.Add(Panel);
		// 		Panel.unitName = "unselected";
		// 	}
		// 	else
		// 		Panel.gameObject.SetActive(false);
		// }

		DontDestroyOnLoad(gameObject);
	}

	void Update(){
		// if (IsThereAnyReadiedUnit())
		// 	readyButton.interactable = true;
		// else
		// 	readyButton.interactable = false;

		if(Input.GetKeyDown(KeyCode.A))
			GameObject.Find("SceneLoader").GetComponent<SceneLoader>().LoadNextBattleScene();
	}

	bool IsThereAnyReadiedUnit(){
		List<GameObject> selectedUnitPanels = FindObjectOfType<SelectedUnits>().SelectedUnitPanels;
		return selectedUnitPanels.Any(panel => panel.GetComponent<UnitPanel>().unitName != "unselected");
	}

	public void ReadyButtonDown(){
		GameObject.Find("SceneLoader").GetComponent<SceneLoader>().LoadNextBattleScene();
	}
}
