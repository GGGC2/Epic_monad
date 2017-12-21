using System;
using UnityEngine.UI;
using UnityEngine;
using GameData;
using UnityEngine.SceneManagement;

public class Test : MonoBehaviour {
    GameObject panel;
    GameObject stageTextField;
    InputField levelInputField;
    InputField stageInputField;

    void Awake() {
        panel = GameObject.Find("panel");

        levelInputField = GameObject.Find("levelInput").GetComponent<InputField>();
        stageInputField = GameObject.Find("stageInput").GetComponent<InputField>();

        stageTextField = GameObject.Find("stageText");
    }

    void Start() {
        panel.SetActive(false);
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene("Title");

        if (Input.GetKeyDown(KeyCode.Return) && panel.activeSelf) {
            string partyLevel = levelInputField.text;
            if(partyLevel == "") PartyData.SetLevel(1);
            else PartyData.level = Convert.ToInt32(partyLevel);

            string stageNumber = stageInputField.text;
            if (stageNumber == "") SceneData.stageNumber = 10;
            else SceneData.stageNumber = Convert.ToInt32(stageNumber);

            SceneLoader sceneLoader = FindObjectOfType<SceneLoader>();
            sceneLoader.LoadNextBattleScene();
        }
    }
    public void OnTestButtonClicked() {
        SceneData.isTestMode  = true;
        SceneData.isStageMode = false;
        panel.SetActive(true);
        stageInputField.gameObject.SetActive(false);
        stageTextField.SetActive(false);
    }
    public void OnStageButtonClicked() {
        SceneData.isStageMode = true;
        SceneData.isTestMode  = false;
        panel.SetActive(true);
        stageInputField.gameObject.SetActive(true);
        stageTextField.SetActive(true);
    }

    public void OnEachStageButtonClicked(int stageNumber){
        PartyData.level = stageNumber;
        SceneData.stageNumber = stageNumber;

        SceneLoader sceneLoader = FindObjectOfType<SceneLoader>();
        sceneLoader.LoadNextBattleScene();
    }

}