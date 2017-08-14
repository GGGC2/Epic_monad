using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using GameData;

public class TutorialManager : MonoBehaviour {
	public Image image;
	public Button NextButton;
	public int index;
	CameraMover cm;
	string usedSceneName;
	List<TutorialScenario> scenarioList;

	public void OnEnable(){
		TutorialScenario.tutorialManager = this;
		usedSceneName = SceneManager.GetActiveScene().name;
		TextAsset searchData = Resources.Load<TextAsset>("Tutorial/" + usedSceneName + SceneData.stageNumber.ToString());
		cm = FindObjectOfType<CameraMover>();

		if(searchData == null || SceneData.isTestMode || SceneData.isStageMode){
			if(usedSceneName == "Battle"){
				cm.mouseMoveActive = true;
				cm.keyboardMoveActive = true;
			}
			EndTutorial ();
		}
		else{
			scenarioList = Parser.GetParsedData<TutorialScenario>(searchData, Parser.ParsingDataType.TutorialScenario);
			BattleManager battleManager = FindObjectOfType<BattleManager>();
			battleManager.onTutorial = true;
			ToNextStep();
		}
	}
	public void EndTutorial(){
		gameObject.SetActive(false);
	}

	void TryNewSprite(){
		if(SearchSprite() != null) {image.sprite = SearchSprite();}
		else {image.sprite = Resources.Load<Sprite>("transparent");}
	}

	public void ToNextStep(){
		TutorialScenario previousScenario = scenarioList.Find (data => data.index == index);
		if (previousScenario != null) {previousScenario.ResetMissionCondition ();}

		index++;
		Debug.Log("Tutorial Step "+index);
		TutorialScenario currentScenario = scenarioList.Find (data => data.index == index);
		TryNewSprite();
		if (currentScenario != null){
			if (currentScenario.IsEndMission) {EndTutorial ();}
			currentScenario.SetMissionCondition ();
			SetControl(true);
		}else {SetControl(false);}
	}

	Sprite SearchSprite(){
		return Resources.Load<Sprite>("Tutorial/"+SceneManager.GetActiveScene().name + SceneData.stageNumber.ToString() + "_" + index);
	}

	//able이 true이면 통제권을 주고, false이면 빼앗음
	void SetControl(bool able){
		NextButton.enabled = !able;
		image.raycastTarget = !able;
	}
}