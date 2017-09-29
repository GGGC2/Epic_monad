using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using GameData;

public class SelectedUnit{
	public string name;
	public List<Skill> selectedSkills = new List<Skill>();

	public SelectedUnit(string name){
		this.name = name;
	}
}

public class ReadyManager : MonoBehaviour{
	TextAsset csvFile;
	public SelectableUnitCounter selectableUnitCounter;
	public BattleReadyPanel ReadyPanel;
	public List<AvailableUnitButton> unitButtons;
	public List<SelectedUnit> selectedUnits = new List<SelectedUnit>();
	public List<UnitPanel> selected = new List<UnitPanel>();
	public string currentUnitName;

	List<GameObject> availableUnitButtons = new List<GameObject>();

	public Button readyButton;
	
	public bool IsAlreadySelected(string unitName) {
		return selectedUnits.Any(unit => unit.name == unitName);
	}

	public void AddUnitToSelectedUnitList(AvailableUnitButton button) {
		selectedUnits.Add(new SelectedUnit(button.nameString));
		selectableUnitCounter.PartyNumberChange(1);
		button.ActiveHighlight();
	}

	public void SubUnitToSelectedUnitList(AvailableUnitButton button) {
		selectedUnits.Remove(selectedUnits.Find(unit => unit.name == button.nameString)); 
		selectableUnitCounter.PartyNumberChange(-1);
		button.InactiveHighlight();
	}

	void Start(){
		csvFile = Resources.Load<TextAsset>("Data/StageAvailablePC");
		string[] stageData = Parser.FindRowDataOf(csvFile.text, SceneData.stageNumber.ToString());

		int numberOfSelectableUnit = Int32.Parse(stageData[1]);
		int numberOfAvailableUnit = Int32.Parse(stageData[2]);

		selectableUnitCounter = FindObjectOfType<SelectableUnitCounter>();
		selectableUnitCounter.SetMaxSelectableUnitNumber(numberOfSelectableUnit);

		unitButtons = Utility.ArrayToList<AvailableUnitButton>(GameObject.Find("CharacterButtons").transform.GetComponentsInChildren<AvailableUnitButton>());
		for(int i = 0; i < unitButtons.Count; i++){
			if (i < numberOfAvailableUnit){
				//이쪽 실행 전에 AvailableUnitButton.Awake의 UI 참조가 완료돼야 함
				unitButtons[i].GetComponent<AvailableUnitButton>().SetNameAndSprite(stageData[i+3]);
			}else{
				unitButtons[i].gameObject.SetActive(false);
			}
		}

		DontDestroyOnLoad(gameObject);
		ReadyPanel.Initialize();
	}

	void Update(){
		if (readyButton == null) return;

		if (IsThereAnyReadiedUnit()) {
			readyButton.interactable = true;
			readyButton.GetComponent<Image>().color = Color.white;
		}else{
			readyButton.interactable = false;
			readyButton.GetComponent<Image>().color = Color.gray;
		}

		if(Input.GetKeyDown(KeyCode.A))
			GameObject.Find("SceneLoader").GetComponent<SceneLoader>().LoadNextBattleScene();
	}

	bool IsThereAnyReadiedUnit(){
		return selectedUnits.Count > 0;
	}

	public void ReadyButtonDown(){
		GameObject.Find("SceneLoader").GetComponent<SceneLoader>().LoadNextBattleScene();
	}
}
