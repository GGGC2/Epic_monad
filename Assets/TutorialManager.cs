using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using GameData;

public class TutorialManager : MonoBehaviour {
	public Image image;
	public Image DarkBG;
	public int index;
	CameraMover cm;
	string usedSceneName;
	public Button ReverseButton;
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

	void SetNewSprite(){
		image.enabled = true;
		DarkBG.enabled = true;
		Sprite searchResult = Resources.Load<Sprite>("Tutorial/"+SceneManager.GetActiveScene().name + GameData.SceneData.stageNumber.ToString() + "_" + index);
		if(searchResult != null)
			image.sprite = searchResult;
		else
			Debug.LogError("Sprite NOT found!");
	}

	public void ToNextStep(){
		TutorialScenario previousScenario = scenarioList.Find (data => data.index == index);
		if (previousScenario != null)
			previousScenario.ResetMissionCondition ();

		index++;
		Debug.Log("Tutorial Step "+index);
		TutorialScenario currentScenario = scenarioList.Find (data => data.index == index);
		if (currentScenario == null)
			SetNewSprite ();
		else{
			currentScenario.SetMissionCondition ();
			image.enabled = false;
			DarkBG.enabled = false;
			if (currentScenario.IsEndMission) {
				EndTutorial ();
			}
		}
	}
}