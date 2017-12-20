﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameData;

public class BattleReadyPanel : MonoBehaviour{
	public enum PanelType{Ether, Party, Stage}
	public PanelType panelType;
	public RightScreen_BattleReady RightPanel;
	public SkillViewer skillViewer;
	List<AvailableUnitButton> UnitButtons;
	public GameObject UnitPart;
	public GameObject SkillPart;
	public List<SkillSelectButton> skillButtonList = new List<SkillSelectButton>();
	public SkillSelectButton SkillButtonPrefab;
    public Scrollbar scrollbar;
	int buttonDist = 90;

	//ReadyManager.Start()가 끝난 직후 넘어온다
	public void Initialize(){
		UnitButtons = Utility.ArrayToList(transform.Find("UnitList").Find("CharacterButtons").GetComponentsInChildren<AvailableUnitButton>());

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

		SetPanelType(PanelType.Party);

		if(SceneData.stageNumber < Setting.unitSelectOpenStage){
			UnitButtons.ForEach(button => button.OnClicked());
			transform.Find("TopButtons").Find("PartySelect").gameObject.SetActive(false);
			transform.Find("TopButtons").Find("Ether").gameObject.SetActive(false);
			SetPanelType(PanelType.Ether);
		}
        SetScrollbarValues();
	}
    public void Update() {
        SetScrollbarValues();
    }

	//이 함수는 TopButtons에서 사용한다.
	public void SetPanelType(string typeName){
		SetPanelType((PanelType)Enum.Parse(typeof(PanelType), typeName));
	}

	public void SetPanelType(PanelType type){
		panelType = type;
		if(panelType == PanelType.Party){
			UnitPart.SetActive(true);
			SkillPart.SetActive(false);
			RightPanel.SetCommonUnitInfoUI(ReadyManager.Instance.RecentUnitButton.nameString);
			UnitButtons.ForEach(button => {
				button.ActivatePropertyIcon();
				button.NameText.text = UnitInfo.ConvertToKoreanName(button.nameString);
			});
		}else if(panelType == PanelType.Ether){
			UnitPart.SetActive(false);
			SkillPart.SetActive(true);
			UnitButtons.ForEach(button => {
				button.ActivatePropertyIcon(false);
				button.NameText.text = UnitInfo.ConvertToKoreanName(button.nameString);
			});

			SetAllSkillSelectButtons();
		}
	}

	public void SetAllSkillSelectButtons(){
		skillButtonList.ForEach(button => {
			button.gameObject.SetActive(true);
			button.Initialize();
		});
	}

    public void SetScrollbarValues() {
        scrollbar.size = 0.1f;
        scrollbar.numberOfSteps = 11;
    }
}