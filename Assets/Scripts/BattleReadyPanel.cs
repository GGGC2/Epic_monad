using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameData;

public class BattleReadyPanel : MonoBehaviour{
	public enum PanelType{Ether, Party, Stage}
	public PanelType panelType;
	public DetailInfoPanelInBattleReady RightPanel;
	public SkillViewer skillViewer;
	public List<AvailableUnitButton> Buttons;
	public GameObject UnitPart;
	public GameObject SkillPart;
	List<SkillSelectButton> skillButtonList = new List<SkillSelectButton>();
	public SkillSelectButton SkillButtonPrefab;
	int buttonDist = 90;

	//ReadyManager.Start()가 끝난 직후 넘어온다
	public void Initialize(){
		Buttons = Utility.ArrayToList(transform.Find("CharacterButtonMask").Find("CharacterButtons").GetComponentsInChildren<AvailableUnitButton>());

		var firstButton = Instantiate(SkillButtonPrefab);
		skillButtonList.Add(firstButton);
		firstButton.transform.parent = SkillPart.transform;
		firstButton.transform.localPosition = new Vector3(400, 205, 0);
		
		for(int column = 1; column <= 8; column++){
			for(int row = 1; row <= 3; row++){
				var button = Instantiate(SkillButtonPrefab);
				skillButtonList.Add(button);
				button.row = row;
				button.level = column*7-6;
				button.transform.parent = SkillPart.transform;
				button.transform.localPosition = new Vector3(-320 + column*buttonDist, 170 - row*buttonDist, 0);
			}
		}

		skillButtonList.ForEach(button => {
			button.viewer = skillViewer;
			button.RightPanel = RightPanel;
		});

		Reset();

		if(SceneData.stageNumber < Setting.unitSelectOpenStage){
			Buttons.ForEach(button => button.SetUnitInfoToDetailPanel());
			panelType = PanelType.Ether;
			Reset();
		}
	}

	public void Reset(){
		if(panelType == PanelType.Party){
			UnitPart.SetActive(true);
			SkillPart.SetActive(false);
			RightPanel.SetCommonUnitInfoUI(Buttons[0].nameString);
		}
		if(panelType == PanelType.Ether){
			UnitPart.SetActive(false);
			SkillPart.SetActive(true);

			skillButtonList.ForEach(button => {
				button.gameObject.SetActive(true);
				button.Initialize();
			});
		}
	}
}