﻿using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using GameData;

public class DialogueManager : MonoBehaviour{
	public TextAsset dialogueData;
	public Sprite transparent;
	
    public GameObject dialogueUI;
    public GameObject adventureUI;
    public GameObject menuPanel;

	public Image leftPortrait;
	public Image rightPortrait; 
	public Image namePanel;

	public Image clickIcon;
	public Text nameText;
	public Text dialogueText;
	
	string leftUnit;
	string rightUnit;
	
	List<DialogueData> dialogueDataList;
	int line;
	int endLine;
	bool isWaitingMouseInput;
	
	public bool isLeftUnitOld;
	
	public SceneLoader sceneLoader;
	public GameObject skipQuestionUI;

    GameObject[] objects;

	public void SkipDialogue(){
		InactiveSkipQuestionUI();

		int newLine = line;
		for (int i = newLine; i < endLine; i++){
			DialogueData data = dialogueDataList[i];
			if(data.Command == DialogueData.CommandType.Glos){
				SetGlossaryLevel(data);
			}
			if(HandleSceneChange(dialogueDataList[i])) {return;}
		}
	}

	bool HandleSceneChange (DialogueData Data){
		//뭔가 명령이 인식됐으면 true, 아무것도 하지 않았으면 false
		if (Data.Command == DialogueData.CommandType.Script){
			string nextScriptName = Data.GetCommandSubType ();
			sceneLoader.LoadNextDialogueScene (nextScriptName);
			return true;
		}
		else if (Data.Command == DialogueData.CommandType.Battle){
			Debug.Log("Loading Battle : Stage " + Data.GetCommandSubType());
			GameData.SceneData.stageNumber = int.Parse(Data.GetCommandSubType());
			sceneLoader.LoadNextBattleScene();
			return true;
		}
		else if(Data.Command == DialogueData.CommandType.Map){
			string nextStoryName = Data.GetCommandSubType();
			sceneLoader.LoadNextWorldMapScene(nextStoryName);
			return true;
		}
		else if(Data.Command == DialogueData.CommandType.Title){
			SceneManager.LoadScene("Title");
			return true;
		}
		return false;
	}

	IEnumerator HandleCommand(){
		DialogueData data = dialogueDataList[line];
		DialogueData.CommandType Command = data.Command;
		string subType = data.GetCommandSubType();

		if(HandleSceneChange(data))
			yield break;
		else if(Command == DialogueData.CommandType.App){
			Sprite loadedSprite = Utility.IllustOf(data.GetNameInCode());
			if (subType == "left"){
				if (loadedSprite != null) {
					leftUnit = dialogueDataList[line].GetNameInCode();               
					leftPortrait.sprite = loadedSprite;
					isLeftUnitOld = false;
				}
			}else if (subType == "right"){
				if (loadedSprite != null) {      
					rightUnit = dialogueDataList[line].GetNameInCode();         
					rightPortrait.sprite = loadedSprite;
					isLeftUnitOld = true;
				}
			}else{
				Debug.LogError("Undefined effectSubType : " + subType);
			}
		}else if (Command == DialogueData.CommandType.Disapp){
			string commandSubType = subType;
			if (commandSubType == "left" || commandSubType == leftUnit){
				leftUnit = null;
				leftPortrait.sprite = transparent;
				isLeftUnitOld = false;
			}else if (commandSubType == "right" || commandSubType == rightUnit){
				rightUnit = null;
				rightPortrait.sprite = transparent;
				isLeftUnitOld = true;
			}
			// 양쪽을 동시에 제거할경우 다음 유닛은 무조건 왼쪽에서 등장. 오른쪽 등장 명령어 사용하는 경우는 예외.
			else if (subType == "all"){
				leftUnit = null;
				leftPortrait.sprite = transparent;
				rightUnit = null;
				rightPortrait.sprite = transparent;
				isLeftUnitOld = false;
			}else {Debug.LogError("Undefined effectSubType : " + subType);}
		}else if(Command == DialogueData.CommandType.BGM){
			string bgmName = dialogueDataList [line].GetCommandSubType ();
			StartCoroutine (SoundManager.Instance.PlayBGM (bgmName));
		}else if(Command == DialogueData.CommandType.BG){
			Sprite bgSprite = Resources.Load<Sprite>("Background/" + subType);
			GameObject.Find("Background").GetComponent<Image>().sprite = bgSprite;
		}else if(Command == DialogueData.CommandType.SE){
			string SEName = dialogueDataList [line].GetCommandSubType ();
			SoundManager.Instance.PlaySE (SEName);
		}else if(Command == DialogueData.CommandType.FO){
			yield return StartCoroutine(sceneLoader.Fade(true));
		}else if(Command == DialogueData.CommandType.FI){
			ResetPortraits();
			StartCoroutine(sceneLoader.Fade(false));
		}else if(Command == DialogueData.CommandType.Gray){
			Material grayscale=Resources.Load<Material> ("Shader/grayscale");
			GameObject.Find ("Background").GetComponent<Image> ().material = grayscale;
			leftPortrait.material = grayscale;
			rightPortrait.material = grayscale;
		}else if(Command == DialogueData.CommandType.Colorful){
			GameObject.Find ("Background").GetComponent<Image> ().material = null;
			leftPortrait.material = null;
			rightPortrait.material = null;
		}else if(Command == DialogueData.CommandType.Glos){
			SetGlossaryLevel(data);
		}else
			Debug.LogError("Undefined effectType : " + dialogueDataList[line].Command);

		if (dialogueDataList [line + 1].IsEffect()) {
			line += 1;
			yield return StartCoroutine(HandleCommand ());
		}
	}

	void SetGlossaryLevel(DialogueData data){
		Debug.Assert(GlobalData.GlossaryDataList.Count != 0);
		GlossaryData glos = GlobalData.GlossaryDataList.Find(x => x.Type.ToString() == data.GetCommandSubType() && x.index == data.GetGlossaryIndex());
		glos.level = Math.Max(glos.level, data.GetGlossarySetLevel());
	}

	//유니티 씬에서 쓰는 것이므로 레퍼런스 없더라도 지우지 말 것
	public void ActiveSkipQuestionUI(){
		skipQuestionUI.SetActive(true);
	}
	public void InactiveSkipQuestionUI(){
		skipQuestionUI.SetActive(false);
	}

	/*public void SetActiveAdventureUI(bool active){
		adventureUI.SetActive(active);
		if(active) { dialogueUI.SetActive(false); }
	}*/

    public void ActiveDialogueUI(){
        adventureUI.SetActive(false);
        dialogueUI.SetActive(true);
    }

    public void PrintLinesFromObjectIndex(int objectIndex){
        GameObject buttonPanel = objects[objectIndex];
        buttonPanel.transform.Find("New").gameObject.SetActive(false);
        buttonPanel.transform.Find("Highlight").gameObject.SetActive(false);
        string objectName = buttonPanel.transform.Find("ObjectNameText").gameObject.GetComponent<Text>().text;
        int startLine = 0;

        for (int i = 0; i < endLine; i++){
            if (dialogueDataList[i].GetObjectName() == objectName){
                startLine = i+1;
                break;
            }
        }

        ActiveDialogueUI();

        StartCoroutine(PrintLinesFrom(startLine));
    }

	void Initialize(){
		dialogueDataList = Parser.GetParsedData<DialogueData>();

		InactiveSkipQuestionUI();

		endLine = dialogueDataList.Count;
		StartCoroutine(PrintLinesFrom(0));
	}

	void ResetPortraits(){
		leftPortrait.sprite = transparent;
		rightPortrait.sprite = transparent;
		
		leftUnit = null;
		rightUnit = null;

		isLeftUnitOld = true;
	}

    IEnumerator PrintLinesFrom(int startLine){
		// Initialize.
		ResetPortraits();

		line = startLine;
		while (line < endLine){
			isWaitingMouseInput = true;
			leftPortrait.color = Color.gray;
			rightPortrait.color = Color.gray;

			if (dialogueDataList[line].IsAdventureObject()){
				yield break;
			}else if (dialogueDataList[line].IsEffect()){
				yield return StartCoroutine(HandleCommand());
				line++;
			}else{
				HandleDialogue();

				while (isWaitingMouseInput)
					yield return null;
				line++;
			}
		}

		yield return new WaitForSeconds(0.01f);
	}

	void HandleDialogue(){
		DialogueData data = dialogueDataList[line];
		Sprite illust = Utility.IllustOf(data.GetNameInCode());
		if (data.GetNameInCode() != leftUnit &&
			data.GetNameInCode() != rightUnit && illust != null)
		{
			if (isLeftUnitOld){
				leftUnit = dialogueDataList[line].GetNameInCode();
				leftPortrait.sprite = illust;
				isLeftUnitOld = false;
			}else{
				rightUnit = dialogueDataList[line].GetNameInCode();
				rightPortrait.sprite = illust;
				isLeftUnitOld = true;
			}
		}

		if (leftUnit == dialogueDataList[line].GetNameInCode())
			leftPortrait.color = Color.white;
		else if (rightUnit == dialogueDataList[line].GetNameInCode())
			rightPortrait.color = Color.white;

		if (dialogueDataList[line].GetName() != "-"){
			nameText.text = dialogueDataList[line].GetName();
			namePanel.enabled = true;
		}else{
			nameText.text = null;
			namePanel.enabled = false;
		}
		dialogueText.text = dialogueDataList[line].GetDialogue();
	}

	public void OnClickDialogue(){
		if (isWaitingMouseInput){
			isWaitingMouseInput = false;
		}
	}
    
    void Awake() {
        menuPanel = GameObject.Find("MenuPanel");
    }

	void Start(){
		Initialize();
		GlobalData.SetGlossaryDataList();

		if(dialogueData.name == "Scene#1"){
			StartCoroutine(BlinkClickIcon());
		}
        menuPanel.SetActive(false);
	}

	IEnumerator BlinkClickIcon(){		
		for(int i = 0; i < 3; i++){
			clickIcon.gameObject.SetActive(true);
			yield return new WaitForSeconds(1.0f);
			clickIcon.gameObject.SetActive(false);
			yield return new WaitForSeconds(1.0f);
		}
	}
    public void ToggleMenuPanelActive() {
        menuPanel.SetActive(!menuPanel.activeSelf);
    }

	int frameWait = 0;
    void Update() {
        if (Input.GetMouseButtonDown(1) && skipQuestionUI.activeSelf) {
            skipQuestionUI.SetActive(false);
        } else if (Input.GetKey(KeyCode.LeftControl)) {
            frameWait += 1;
            if (frameWait == Setting.fastDialogueFrameLag) {
                frameWait = 0;
                OnClickDialogue();
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape)) {
            ToggleMenuPanelActive();
        }
    }
}