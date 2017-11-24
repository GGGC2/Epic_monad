using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using GameData;

public class TutorialManager : MonoBehaviour {
	private static TutorialManager instance;
	public static TutorialManager Instance{ get { return instance; } }
	void Awake (){
		if (instance != null && instance != this) {
			Destroy (this.gameObject);
			return;
		} else {
			instance = this;
		}
	}

	public Image image;
	public Button NextButton;
	CameraMover cm;
	string usedSceneName;
	List<TutorialScenario> scenarioList;
	public int index;
	List<AIScenario> AIscenarioList;
	int AIscenarioIndex = 0;
	public MouseMark markPrefab;
	MouseMark mark;

	public void OnEnable(){
		TutorialScenario.tutorialManager = this;
		usedSceneName = SceneManager.GetActiveScene().name;
		TextAsset searchData = Resources.Load<TextAsset>("Tutorial/" + usedSceneName + SceneData.stageNumber.ToString());
		cm = FindObjectOfType<CameraMover>();

		if(searchData == null || SceneData.isTestMode || SceneData.isStageMode){
			if(usedSceneName == "Battle"){
				cm.SetMovable(true);
			}
			EndTutorial ();
		} else{
			scenarioList = Parser.GetParsedData<TutorialScenario>();
			AIscenarioList = Parser.GetParsedData<AIScenario> ();
			AIscenarioIndex = 0;
			BattleManager battleManager = BattleData.battleManager;
			BattleData.onTutorial = true;
			BattleData.rightClickLock = true;
			ToNextStep();
		}
	}
	public void EndTutorial(){
		Debug.Log ("Tutorial ended");
		BattleData.rightClickLock = false;
		BattleData.onTutorial = false;
		gameObject.SetActive(false);
	}
    public void RemoveSpriteAndMark() {
        if(scenarioList == null)
            return;
        image.sprite = Resources.Load<Sprite>("transparent");
        TutorialScenario lastScenario = scenarioList.Find(data => data.index == index);
        if (lastScenario != null && lastScenario.mouseMarkPos != Vector3.zero && mark != null) {
            Destroy(mark.gameObject);
        }
    }
	void TryNewSprite(){
        Sprite sprite = SearchSprite();
		if(sprite != null) {image.sprite = sprite;}
		else image.sprite = Resources.Load<Sprite>("transparent");
    }

	public void ToNextStep(){
		TutorialScenario previousScenario = scenarioList.Find (data => data.index == index);
		if (previousScenario != null) {
			previousScenario.ResetMissionCondition ();
			if(previousScenario.mouseMarkPos != Vector3.zero && mark != null){
				Destroy (mark.gameObject);
			}
		}

		index++;
		Debug.Log("Tutorial Step "+index);
		TutorialScenario currentScenario = scenarioList.Find (data => data.index == index);
		TryNewSprite();
		if (currentScenario != null){
			if (currentScenario.IsEndMission) {EndTutorial ();}
			currentScenario.SetMissionCondition ();
			SetControl(true);
			if(currentScenario.mouseMarkPos != Vector3.zero){
				mark = Instantiate(markPrefab, currentScenario.mouseMarkPos, Quaternion.identity, GameObject.Find("FixedUICanvas").transform);
			}
		}
		else {SetControl(false);}
	}

	Sprite SearchSprite(){
		return Resources.Load<Sprite>("Tutorial/"+SceneManager.GetActiveScene().name + SceneData.stageNumber.ToString() + "_" + index);
	}

	//able이 true이면 통제권을 주고, false이면 빼앗음
	void SetControl(bool able){
		NextButton.enabled = !able;
		image.raycastTarget = !able;
		Setting.shortcutEnable = able;
	}

	public AIScenario GetNextAIScenario(){
		return AIscenarioList [AIscenarioIndex++];
	}
}