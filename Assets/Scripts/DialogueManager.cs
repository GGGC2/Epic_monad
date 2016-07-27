using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour {

	public TextAsset dialogueData;
	public static string nextDialogueName;

	Sprite transparent;
	
    public GameObject dialogueUI;
    public GameObject adventureUI;

	public Image leftPortrait;
	public Image rightPortrait; 
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
		line = endLine-1;
		// FindObjectOfType<SceneLoader>().LoadNextScene();
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

        int tempLine = line;
        int objectIndex = 0;

        for (int i = line; i < endLine; i++) {
            if (dialogueDataList[i].IsAdventureObject())
            {
                objects[objectIndex].transform.Find("NamePanel/ObjectNameText").gameObject.GetComponent<Text>().text = dialogueDataList[i].GetObjectName();
                objects[objectIndex].transform.Find("NamePanel/ObjectSubNameText").gameObject.GetComponent<Text>().text = dialogueDataList[i].GetObjectSubName();
                objectIndex++;
            }
        }
    }

    public void PrintLinesFromObjectIndex(int objectIndex)
    {
        string objectName = objects[objectIndex].transform.Find("NamePanel/ObjectNameText").gameObject.GetComponent<Text>().text;
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
		Debug.Log(nextDialogueName);
        if (nextDialogueName != null)
        {
            TextAsset nextScriptFile = Resources.Load("Data/" + nextDialogueName, typeof(TextAsset)) as TextAsset;
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

		ActiveAdventureUI();

		// FindObjectOfType<SceneLoader>().LoadNextScene();
	}

	void HandleCommand()
	{
		if (dialogueDataList[line].GetCommandType() == "adv_start")
		{
			ActiveAdventureUI();
			LoadAdventureObjects();
			return;
		}
		else if (dialogueDataList[line].GetCommandType() == "load_scene")
		{
			string nextSceneName = dialogueDataList[line].GetCommandSubType();
			FindObjectOfType<SceneLoader>().LoadNextScene(nextSceneName);
		}
		else if (dialogueDataList[line].GetCommandType() == "load_script")
		{
			string nextScriptName = dialogueDataList[line].GetCommandSubType();
			FindObjectOfType<SceneLoader>().LoadNextScript(nextScriptName);
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
			if (dialogueDataList[line].GetCommandSubType() == "left")
			{
				leftUnit = null;
				leftPortrait.sprite = Resources.Load("StandingImage/" + "transparent", typeof(Sprite)) as Sprite;
				isLeftUnitOld = false;
			}
			else if (dialogueDataList[line].GetCommandSubType() == "right")
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
		else
		{
			Debug.LogError("Undefined effectType : " + dialogueDataList[line].GetCommandType());
		}
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
			nameText.text = dialogueDataList[line].GetName();
		else
			nameText.text = null;
		dialogueText.text = dialogueDataList[line].GetDialogue();
	}

	void OnMouseDown()
	{
		if ((isWaitingMouseInput) && (!sceneLoader.IsScreenActive()) && (!skipQuestionUI.activeInHierarchy))
		{
			isWaitingMouseInput = false;
		}
	}
	
	// Use this for initialization
	void Start () {
		Initialize();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
