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
		selectedSkills.Add(Parser.GetSkillsByUnit(name).Find(skill => skill.requireLevel == 0));
	}

	public int CurrentEther{
		get{
			int result = 0;
			selectedSkills.ForEach(skill => {
				result += skill.ether;
			});
			return result;
		}
	}
}

public class ReadyManager : MonoBehaviour{
	public static ReadyManager Instance;
	TextAsset csvFile;
	public SelectableUnitCounter selectableUnitCounter;
	public BattleReadyPanel ReadyPanel;
	public AvailableUnitButton RecentUnitButton;
	List<AvailableUnitButton> UnitButtons;
	public List<SelectedUnit> selectedUnits = new List<SelectedUnit>();
    public GameObject CharacterButtons;
	public GameObject WarningPanel;

	List<GameObject> availableUnitButtons = new List<GameObject>();

	public Button readyButton;
	
	public bool IsAlreadySelected(string unitName) {
		return selectedUnits.Any(unit => unit.name == unitName);
	}

	public void AddUnitToSelectedUnitList(AvailableUnitButton button) {
		selectedUnits.Add(new SelectedUnit(button.nameString));
		button.ActiveHighlight();
	}

	public void SubUnitToSelectedUnitList(AvailableUnitButton button) {
		selectedUnits.Remove(selectedUnits.Find(unit => unit.name == button.nameString)); 
		button.ActiveHighlight(false);;
	}

	void Start(){
		Instance = this;
		csvFile = Resources.Load<TextAsset>("Data/StageAvailablePC");
		string[] stageData = Parser.FindRowDataOf(csvFile.text, SceneData.stageNumber.ToString());

		int numberOfSelectableUnit = Int32.Parse(stageData[1]);
		int numberOfAvailableUnit = Int32.Parse(stageData[2]);

		selectableUnitCounter = FindObjectOfType<SelectableUnitCounter>();
		selectableUnitCounter.SetMaxSelectableUnitNumber(numberOfSelectableUnit);

		UnitButtons = Utility.ArrayToList<AvailableUnitButton>(CharacterButtons.transform.GetComponentsInChildren<AvailableUnitButton>());
		for(int i = 0; i < UnitButtons.Count; i++){
			if (i < numberOfAvailableUnit){
				//이쪽 실행 전에 AvailableUnitButton.Awake의 UI 참조가 완료돼야 함
				UnitButtons[i].GetComponent<AvailableUnitButton>().SetNameAndSprite(stageData[i+3]);
			}else{
				UnitButtons[i].gameObject.SetActive(false);
			}
		}

		DontDestroyOnLoad(gameObject);
		RecentUnitButton = UnitButtons.First();
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

	//UI 버튼으로 구현.
	public void StartBattle(){
		//기존 능력을 포기하지 않고 추가로 가져갈 수 있는 능력이 있을 경우
		bool isEfficient = selectedUnits.All(unit => {
			int leastEther = 100;
			var unselectedList = new List<Skill>();
			ReadyPanel.skillButtonList.FindAll(button => !button.selected && button.mySkill != null).ForEach(button => {
				unselectedList.Add(button.mySkill);
			});

			unselectedList.ForEach(skill => {
				if(PartyData.level >= skill.requireLevel && skill.ether < leastEther){
					leastEther = skill.ether;
				}
			});

			//남은 에테르가 가장 저렴한 능력의 에테르보다 적은지(추가 선택 불가) 여부. true이면 효율적.
			return PartyData.MaxEther - unit.CurrentEther < leastEther;
		});

		if(selectedUnits.Count < selectableUnitCounter.maxSelectableUnitNumber){
			WarningPanel.SetActive(true);
			WarningPanel.transform.Find("Text").GetComponent<Text>().text = "캐릭터를 더 선택할 수 있습니다.\n\n정말 시작하시겠습니까?";
		}else if(!isEfficient){
			WarningPanel.SetActive(true);
			WarningPanel.transform.Find("Text").GetComponent<Text>().text = "능력을 더 선택할 수 있습니다.\n\n정말 시작하시겠습니까?";
		}else{
			FindObjectOfType<SceneLoader>().LoadNextBattleScene();
		}
	}
}
