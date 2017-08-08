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
	public TutorialScenario currentScenario;

	public void OnEnable(){
		usedSceneName = SceneManager.GetActiveScene().name;
		TextAsset searchData = Resources.Load<TextAsset>("Tutorial/" + usedSceneName + SceneData.stageNumber.ToString());
		cm = FindObjectOfType<CameraMover>();

		if(searchData == null || SceneData.isTestMode || SceneData.isStageMode){
			if(usedSceneName == "Battle"){
				cm.mouseMoveActive = true;
				cm.keyboardMoveActive = true;
			}
			gameObject.SetActive(false);
		}
		else{
			scenarioList = Parser.GetParsedData<TutorialScenario>(searchData, Parser.ParsingDataType.TutorialScenario);
			BattleManager battleManager = FindObjectOfType<BattleManager>();
			battleManager.onTutorial = true;
			NextStep();
		}
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

	public void NextStep(){
		TutorialScenario previousScenario = scenarioList.Find(data => data.index == index);
		if(previousScenario != null){
			previousScenario.ResetMissionCondition ();
		}

		index++;
		TutorialScenario findCurrentScenario = scenarioList.Find(data => data.index == index);
		if(findCurrentScenario == null){
			SetNewSprite();
		}else{
			//Debug.Log("Step " + index);
			currentScenario = findCurrentScenario;
			currentScenario.SetMissionCondition ();
			image.enabled = false;
			DarkBG.enabled = false;
			if(currentScenario.mission == TutorialScenario.Mission.End){
				gameObject.SetActive(false);
			}
		}
	}

	public void Skip(){
		gameObject.SetActive(false);
	}
}