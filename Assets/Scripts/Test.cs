using System;
using UnityEngine.UI;
using UnityEngine;
using GameData;

public class Test : MonoBehaviour {
    GameObject panel;
    InputField levelInputField;
    InputField stageInputField;

    void Awake() {
        panel = GameObject.Find("panel");

        levelInputField = GameObject.Find("levelInput").GetComponent<InputField>();
        stageInputField = GameObject.Find("stageInput").GetComponent<InputField>();
    }

    void Start() {
        panel.SetActive(false);
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Return) && panel.activeSelf) {
            string partyLevel = levelInputField.text;
            if(partyLevel == "") PartyData.level = 1;
            else PartyData.level = Convert.ToInt32(partyLevel);

            string stageNumber = stageInputField.text;
            if (stageNumber == "") SceneData.stageNumber = 1;
            else SceneData.stageNumber = Convert.ToInt32(stageNumber);

            SceneLoader sceneLoader = FindObjectOfType<SceneLoader>();
            sceneLoader.LoadNextBattleScene();
        }
    }
    public void OnTestButtonClicked() {
        SceneData.isTestMode = true;
        panel.SetActive(true);
        stageInputField.gameObject.SetActive(false);
        GameObject.Find("stageText").SetActive(false);
    }
    public void OnStageButtonClicked() {
        SceneData.isStageMode = true;
        panel.SetActive(true);
    }

}
