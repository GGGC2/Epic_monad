using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;
using GameData;

public class DialogueManager : MonoBehaviour {

	public TextAsset dialogueData;

	Sprite transparent;
	
    public GameObject dialogueUI;
    public GameObject adventureUI;

	public Image leftPortrait;
	public Image rightPortrait; 
	public Image namePanel;
	public Text nameText;
	public Text dialogueText;
	
	string leftUnit;
	string rightUnit;
	
	List<DialogueData> dialogueDataList;
	int line;
	int endLine;
	bool isWaitingMouseInput;
	
	bool isLeftUnitOld;
	
	SceneLoader sceneLoader;
	GameObject skipQuestionUI;

    GameObject[] objects;

	public void SkipDialogue()
	{
		InactiveSkipQuestionUI();

		int newLine = line;
		for (int i = newLine; i < endLine; i++)
		{
			if (dialogueDataList[i].IsAdventureObject())
			{
				ActiveAdventureUI();
				break;
			}
			
			if(HandleSceneChange(dialogueDataList[i]) != "else")
			{
				return;
			}
		}
		ActiveAdventureUI();
	}

	string HandleSceneChange (DialogueData Data)
	{
		if (Data.GetCommandType () == "adv_start") 
		{
			ActiveAdventureUI ();
			LoadAdventureObjects ();
			return Data.GetCommandType();
		}
		else if (Data.GetCommandType () == "load_script") 
		{
			string nextScriptName = Data.GetCommandSubType ();
			FindObjectOfType<SceneLoader> ().LoadNextDialogueScene (nextScriptName);
			return Data.GetCommandType();
		}
		else if (Data.GetCommandType () == "load_battle")
		{
			string nextSceneName = Data.GetCommandSubType();
			FindObjectOfType<SceneLoader>().LoadNextBattleScene(nextSceneName);
			return Data.GetCommandType();
		}
		else if(Data.GetCommandType() == "load_worldmap")
		{
			string nextStoryName = Data.GetCommandSubType();
			FindObjectOfType<SceneLoader>().LoadNextWorldMapScene(nextStoryName);
			return Data.GetCommandType();
		}
		else if(Data.GetCommandType() == "load_title")
		{
			SceneManager.LoadScene("title");
			return Data.GetCommandType();
		}
		return "else";
	}

	void HandleCommand()
	{
		string CommandType = HandleSceneChange(dialogueDataList[line]);
		if(CommandType != "else")
		{
			return;
		}
		else if (dialogueDataList[line].GetCommandType() == "appear")
		{
			if (dialogueDataList[line].GetCommandSubType() == "left")
			{
				Sprite loadedSprite = Resources.Load("StandingImage/" + dialogueDataList[line].GetNameInCode() + "_standing", typeof(Sprite)) as Sprite;
				if (loadedSprite != null) 
				{
					leftUnit = dialogueDataList[line].GetNameInCode();               
					leftPortrait.sprite = loadedSprite;
					isLeftUnitOld = false;
				}
			}
			else if (dialogueDataList[line].GetCommandSubType() == "right")
			{
				Sprite loadedSprite = Resources.Load("StandingImage/" + dialogueDataList[line].GetNameInCode() + "_standing", typeof(Sprite)) as Sprite;
				if (loadedSprite != null) 
				{      
					rightUnit = dialogueDataList[line].GetNameInCode();         
					rightPortrait.sprite = loadedSprite;
					isLeftUnitOld = true;
				}
			}
			else
			{
				Debug.LogError("Undefined effectSubType : " + dialogueDataList[line].GetCommandSubType());
			}
		}
		else if (dialogueDataList[line].GetCommandType() == "disappear")
		{
			string commandSubType = dialogueDataList[line].GetCommandSubType();
			if (commandSubType == "left" || commandSubType == leftUnit)
			{
				leftUnit = null;
				leftPortrait.sprite = Resources.Load("StandingImage/" + "transparent", typeof(Sprite)) as Sprite;
				isLeftUnitOld = false;
			}
			else if (commandSubType == "right" || commandSubType == rightUnit)
			{
				rightUnit = null;
				rightPortrait.sprite = Resources.Load("StandingImage/" + "transparent", typeof(Sprite)) as Sprite;
				isLeftUnitOld = true;
			}
			// 양쪽을 동시에 제거할경우 다음 유닛은 무조건 왼쪽에서 등장. 오른쪽 등장 명령어 사용하는 경우는 예외.
			else if (dialogueDataList[line].GetCommandSubType() == "all")
			{
				leftUnit = null;
				leftPortrait.sprite = Resources.Load("StandingImage/" + "transparent", typeof(Sprite)) as Sprite;
				rightUnit = null;
				rightPortrait.sprite = Resources.Load("StandingImage/" + "transparent", typeof(Sprite)) as Sprite;
				isLeftUnitOld = false;
			}
			else
			{
				Debug.LogError("Undefined effectSubType : " + dialogueDataList[line].GetCommandSubType());
			}
		}
		else if (dialogueDataList[line].GetCommandType() == "bgm")
		{
			FindObjectOfType<SoundManager>().PlayBgm(dialogueDataList[line].GetCommandSubType());
		}
		else if (dialogueDataList[line].GetCommandType() == "bg")
		{
			Sprite bgSprite = Resources.Load("Background/" + dialogueDataList[line].GetCommandSubType(), typeof(Sprite)) as Sprite;
			GameObject.Find("Background").GetComponent<Image>().sprite = bgSprite;
		}
		else if (dialogueDataList[line].GetCommandType() == "se")
		{
			// Not implement yet.
		}
		else
		{
			Debug.LogError("Undefined effectType : " + dialogueDataList[line].GetCommandType());
		}
	}

	public void ReadEndLine()
	{
		StartCoroutine (PrintLinesFrom (endLine-1));
	}

	public void ActiveSkipQuestionUI()
	{
		skipQuestionUI.SetActive(true);
	}

	public void InactiveSkipQuestionUI()
	{
		skipQuestionUI.SetActive(false);
	}

    public void ActiveAdventureUI()
    {
        adventureUI.SetActive(true);
        dialogueUI.SetActive(false);
    }

    public void InactiveAdventureUI()
    {
		Debug.Log("InactiveAdventureUI");
        adventureUI.SetActive(false);
    }

    public void ActiveDialogueUI()
    {
        adventureUI.SetActive(false);
        dialogueUI.SetActive(true);
    }

    public void LoadAdventureObjects()
    {
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

    public void PrintLinesFromObjectIndex(int objectIndex)
    {
        GameObject buttonPanel = objects[objectIndex];
        buttonPanel.transform.Find("New").gameObject.SetActive(false);
        buttonPanel.transform.Find("Highlight").gameObject.SetActive(false);
        string objectName = buttonPanel.transform.Find("ObjectNameText").gameObject.GetComponent<Text>().text;
        int startLine = 0;

        for (int i = 0; i < endLine; i++)
        {
            if (dialogueDataList[i].GetObjectName() == objectName)
            {
                startLine = i+1;
                break;
            }
        }

        ActiveDialogueUI();

        StartCoroutine(PrintLinesFrom(startLine));
    }

	void Initialize()
	{
		Debug.Log(SceneData.dialogueName);
        if (SceneData.dialogueName != null)
        {
            TextAsset nextScriptFile = Resources.Load("Data/" + SceneData.dialogueName, typeof(TextAsset)) as TextAsset;
            dialogueData = nextScriptFile;
        }

		sceneLoader = FindObjectOfType<SceneLoader>();
		skipQuestionUI = GameObject.Find("SkipQuestion");
		InactiveSkipQuestionUI();
		
		transparent = Resources.Load("StandingImage/" + "transparent", typeof(Sprite)) as Sprite;
		
		dialogueDataList = Parser.GetParsedDialogueData(dialogueData);
		
		line = 0;
		endLine = dialogueDataList.Count; 
		
        adventureUI.SetActive(false);
		
		StartCoroutine(PrintLinesFrom(0));
	}

    IEnumerator PrintLinesFrom(int startLine)
	{
		// Initialize.
		leftPortrait.sprite = transparent;
		rightPortrait.sprite = transparent;
		
		leftUnit = null;
		rightUnit = null;

		isLeftUnitOld = true;

		line = startLine;
		while (line < endLine)
		{
			leftPortrait.color = Color.gray;
			rightPortrait.color = Color.gray;

			// Previous adventure dialogue is end
			// If adventure object is clicked, PrintAllLine is called.
			if (dialogueDataList[line].IsAdventureObject())
			{
				ActiveAdventureUI();
				yield break;
			}
			else if (dialogueDataList[line].IsEffect())
			{
				HandleCommand();
				line++;
			}
			else
			{
				HandleDialogue();

				isWaitingMouseInput = true;
				while (isWaitingMouseInput)
				{
					yield return null;
				}
				line++;
			}
		}

		yield return new WaitForSeconds(0.01f);

		ActiveAdventureUI();
	}

	void HandleDialogue()
	{
		if ((dialogueDataList[line].GetNameInCode() != leftUnit) &&
			(dialogueDataList[line].GetNameInCode() != rightUnit) &&
			(Resources.Load("StandingImage/" + dialogueDataList[line].GetNameInCode() + "_standing", typeof(Sprite)) as Sprite != null))
		{
			Sprite sprite = Resources.Load("StandingImage/" + dialogueDataList[line].GetNameInCode() + "_standing", typeof(Sprite)) as Sprite;
			if (isLeftUnitOld)
			{
				leftUnit = dialogueDataList[line].GetNameInCode();
				leftPortrait.sprite = sprite;
				isLeftUnitOld = false;
			}
			else
			{
				rightUnit = dialogueDataList[line].GetNameInCode();
				rightPortrait.sprite = sprite;
				isLeftUnitOld = true;
			}
		}

		if (leftUnit == dialogueDataList[line].GetNameInCode())
			leftPortrait.color = Color.white;
		else if (rightUnit == dialogueDataList[line].GetNameInCode())
			rightPortrait.color = Color.white;

		if (dialogueDataList[line].GetName() != "-")
		{
			nameText.text = dialogueDataList[line].GetName();
			namePanel.enabled = true;
		}
		else
		{
			nameText.text = null;
			namePanel.enabled = false;
		}
		dialogueText.text = dialogueDataList[line].GetDialogue();
	}

	public void OnClickDialogue()
	{
		if (isWaitingMouseInput)
		{
			isWaitingMouseInput = false;
		}
	}

	void Start () 
	{
		Initialize();
	}

	void Update()
	{
		if(Input.GetMouseButtonDown(1) && skipQuestionUI.active)
		{
			skipQuestionUI.SetActive(false);
		}
	}
}