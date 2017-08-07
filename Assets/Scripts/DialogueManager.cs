using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;
using GameData;

public class DialogueManager : MonoBehaviour{
	public TextAsset dialogueData;

	public Sprite transparent;
	
    public GameObject dialogueUI;
    public GameObject adventureUI;

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
	
	bool isLeftUnitOld;
	
	public SceneLoader sceneLoader;
	public GameObject skipQuestionUI;

    GameObject[] objects;

	public void SkipDialogue(){
		InactiveSkipQuestionUI();

		int newLine = line;
		for (int i = newLine; i < endLine; i++){
			if (dialogueDataList[i].IsAdventureObject()){
				SetActiveAdventureUI(true);
				break;
			}
			
			if(HandleSceneChange(dialogueDataList[i]) != DialogueData.CommandType.Else){
				return;
			}
		}
		SetActiveAdventureUI(true);
	}

	DialogueData.CommandType HandleSceneChange (DialogueData Data){
		if (Data.Command == DialogueData.CommandType.Adv){
			SetActiveAdventureUI(true);
			LoadAdventureObjects ();
			return Data.Command;
		}else if (Data.Command == DialogueData.CommandType.Script){
			string nextScriptName = Data.GetCommandSubType ();
			sceneLoader.LoadNextDialogueScene (nextScriptName);
			return Data.Command;
		}
		else if (Data.Command == DialogueData.CommandType.Battle){
			Debug.Log(Data.GetCommandSubType());
			GameData.SceneData.stageNumber = int.Parse(Data.GetCommandSubType());
			sceneLoader.LoadNextBattleScene();
			return Data.Command;
		}
		else if(Data.Command == DialogueData.CommandType.Map){
			string nextStoryName = Data.GetCommandSubType();
			sceneLoader.LoadNextWorldMapScene(nextStoryName);
			return Data.Command;
		}
		else if(Data.Command == DialogueData.CommandType.Title){
			SceneManager.LoadScene("Title");
			return Data.Command;
		}
		return DialogueData.CommandType.Else;
	}

	IEnumerator HandleCommand(){
		DialogueData.CommandType Command = HandleSceneChange(dialogueDataList[line]);
		if(Command != DialogueData.CommandType.Else)
			yield break;

		else if (dialogueDataList[line].Command == DialogueData.CommandType.App){
			if (dialogueDataList[line].GetCommandSubType() == "left"){
				Sprite loadedSprite = Resources.Load("StandingImage/" + dialogueDataList[line].GetNameInCode() + "_standing", typeof(Sprite)) as Sprite;
				if (loadedSprite != null) {
					leftUnit = dialogueDataList[line].GetNameInCode();               
					leftPortrait.sprite = loadedSprite;
					isLeftUnitOld = false;
				}
			}else if (dialogueDataList[line].GetCommandSubType() == "right"){
				Sprite loadedSprite = Resources.Load("StandingImage/" + dialogueDataList[line].GetNameInCode() + "_standing", typeof(Sprite)) as Sprite;
				if (loadedSprite != null) {      
					rightUnit = dialogueDataList[line].GetNameInCode();         
					rightPortrait.sprite = loadedSprite;
					isLeftUnitOld = true;
				}
			}else
				Debug.LogError("Undefined effectSubType : " + dialogueDataList[line].GetCommandSubType());
		}else if (dialogueDataList[line].Command == DialogueData.CommandType.Disapp){
			string commandSubType = dialogueDataList[line].GetCommandSubType();
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
			else if (dialogueDataList[line].GetCommandSubType() == "all"){
				leftUnit = null;
				leftPortrait.sprite = transparent;
				rightUnit = null;
				rightPortrait.sprite = transparent;
				isLeftUnitOld = false;
			}else
				Debug.LogError("Undefined effectSubType : " + dialogueDataList[line].GetCommandSubType());
		}else if (dialogueDataList[line].Command == DialogueData.CommandType.BGM){
			string bgmName = dialogueDataList [line].GetCommandSubType ();
			SoundManager.Instance.PlayBgm(bgmName);
		}else if (dialogueDataList[line].Command == DialogueData.CommandType.BG){
			Sprite bgSprite = Resources.Load<Sprite>("Background/" + dialogueDataList[line].GetCommandSubType());
			GameObject.Find("Background").GetComponent<Image>().sprite = bgSprite;
		}else if (dialogueDataList[line].Command == DialogueData.CommandType.SE){
			string SEName = dialogueDataList [line].GetCommandSubType ();
			SoundManager.Instance.PlaySE (SEName);
		}else if(dialogueDataList[line].Command == DialogueData.CommandType.FO){
			yield return StartCoroutine(sceneLoader.Fade(true));
		}else if(dialogueDataList[line].Command == DialogueData.CommandType.FI){
			ResetPortraits();
			StartCoroutine(sceneLoader.Fade(false));
		}else
			Debug.LogError("Undefined effectType : " + dialogueDataList[line].Command);
	}
	public void ReadEndLine(){
		StartCoroutine (PrintLinesFrom (endLine-1));
	}

	//유니티 씬에서 쓰는 것이므로 레퍼런스 없더라도 지우지 말 것
	public void ActiveSkipQuestionUI(){
		skipQuestionUI.SetActive(true);
	}
	public void InactiveSkipQuestionUI(){
		skipQuestionUI.SetActive(false);
	}

	public void SetActiveAdventureUI(bool active){
		adventureUI.SetActive(active);
		if(active) { dialogueUI.SetActive(false); }
	}

    public void ActiveDialogueUI(){
        adventureUI.SetActive(false);
        dialogueUI.SetActive(true);
    }

    public void LoadAdventureObjects(){
        objects = adventureUI.GetComponent<AdventureManager>().objects;

		objects.ToList().ForEach(x => x.SetActive(true));
        
        int objectIndex = 0;

        for (int i = line; i < endLine; i++) {
            if (dialogueDataList[i].IsAdventureObject())
            {
                objects[objectIndex].transform.Find("ObjectNameText").gameObject.GetComponent<Text>().text = dialogueDataList[i].GetObjectName();
                objects[objectIndex].transform.Find("ObjectSubNameText").gameObject.GetComponent<Text>().text = dialogueDataList[i].GetObjectSubName();
                objects[objectIndex].transform.Find("New").gameObject.SetActive(true);
                objects[objectIndex].transform.Find("Highlight").gameObject.SetActive(true);
                objectIndex++;
            }
        }

		for (int i = objectIndex; i < objects.Length; i++)
		{
			objects[i].SetActive(false);
		}
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
        if (SceneData.dialogueName != null){
            TextAsset nextScriptFile = Resources.Load<TextAsset>("Data/" + SceneData.dialogueName);
            dialogueData = nextScriptFile;
        }

		dialogueDataList = Parser.GetParsedData<DialogueData>(dialogueData, Parser.ParsingDataType.DialogueData);

		InactiveSkipQuestionUI();
		
		line = 0;
		endLine = dialogueDataList.Count;
		
        adventureUI.SetActive(false);
		
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
				SetActiveAdventureUI(true);
				yield break;
			}else if (dialogueDataList[line].IsEffect()){
				/*yield return*/StartCoroutine(HandleCommand());
				line++;
			}else{
				HandleDialogue();

				while (isWaitingMouseInput)
					yield return null;
				line++;
			}
		}

		yield return new WaitForSeconds(0.01f);

		SetActiveAdventureUI(true);
	}

	void HandleDialogue(){
		if ((dialogueDataList[line].GetNameInCode() != leftUnit) &&
			(dialogueDataList[line].GetNameInCode() != rightUnit) &&
			(Resources.Load<Sprite>("StandingImage/" + dialogueDataList[line].GetNameInCode() + "_standing") != null))
		{
			Sprite sprite = Resources.Load<Sprite>("StandingImage/" + dialogueDataList[line].GetNameInCode() + "_standing");
			if (isLeftUnitOld){
				leftUnit = dialogueDataList[line].GetNameInCode();
				leftPortrait.sprite = sprite;
				isLeftUnitOld = false;
			}else{
				rightUnit = dialogueDataList[line].GetNameInCode();
				rightPortrait.sprite = sprite;
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
		if (isWaitingMouseInput)
			isWaitingMouseInput = false;
	}

	void Start() {
		Initialize();

		if(dialogueData.name == "Scene#1-1")
			StartCoroutine(BlinkClickIcon());
	}

	IEnumerator BlinkClickIcon(){		
		for(int i = 0; i < 3; i++){
			clickIcon.gameObject.SetActive(true);
			yield return new WaitForSeconds(1.0f);
			clickIcon.gameObject.SetActive(false);
			yield return new WaitForSeconds(1.0f);
		}
	}

	void Update(){
		if(Input.GetMouseButtonDown(1) && skipQuestionUI.activeSelf)
			skipQuestionUI.SetActive(false);
	}
}