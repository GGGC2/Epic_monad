using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour {

	public TextAsset dialogueData;

	Sprite transparent;
	
	Image leftPortrait;
	Image rightPortrait; 
	Text nameText;
	Text dialogueText;
	
	string leftUnit;
	string rightUnit;
	
	List<DialogueData> dialogueDataList;
	int line;
	int endLine;
	bool isWaitingMouseInput;
	
	bool isLeftUnitOld;
	
	SceneLoader sceneLoader;
	GameObject skipQuestionUI;

	public void SkipDialogue()
	{
		line = endLine;
		FindObjectOfType<SceneLoader>().LoadNextScene();
	}

	public void ActiveSkipQuestionUI()
	{
		skipQuestionUI.SetActive(true);
	}

	public void InactiveSkipQuestionUI()
	{
		skipQuestionUI.SetActive(false);
	}

	void Initialize()
	{
		sceneLoader = FindObjectOfType<SceneLoader>();
		skipQuestionUI = GameObject.Find("SkipQuestion");
		InactiveSkipQuestionUI();
		
		transparent = Resources.Load("StandingImage/" + "transparent", typeof(Sprite)) as Sprite;
		
		leftPortrait = GameObject.Find("LeftPortrait").GetComponent<Image>();
		rightPortrait = GameObject.Find("RightPortrait").GetComponent<Image>();
		nameText = GameObject.Find("NameText").GetComponent<Text>();
		dialogueText = GameObject.Find("DialogueText").GetComponent<Text>();
		
		leftPortrait.sprite = transparent;
		rightPortrait.sprite = transparent;
		
		leftUnit = null;
		rightUnit = null;
		
		dialogueDataList = Parser.GetParsedDialogueData(dialogueData);
		
		line = 0;
		endLine = dialogueDataList.Count; 
		
		isLeftUnitOld = true;
		
		StartCoroutine(PrintAllLine());
	}
	
	IEnumerator PrintEachLine()
    {
        leftPortrait.color = Color.gray;
        rightPortrait.color = Color.gray;

        if (!dialogueDataList[line].IsEffect())
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


            isWaitingMouseInput = true;
            while (isWaitingMouseInput)
            {
                yield return null;
            }
        }
        else
        {
            if (dialogueDataList[line].GetEffectType() == "appear")
            {
                if (dialogueDataList[line].GetEffectSubType() == "left")
                {
                    Sprite loadedSprite = Resources.Load("StandingImage/" + dialogueDataList[line].GetNameInCode() + "_standing", typeof(Sprite)) as Sprite;
                    if (loadedSprite != null) 
                    {
                        leftUnit = dialogueDataList[line].GetNameInCode();               
                        leftPortrait.sprite = loadedSprite;
                        isLeftUnitOld = false;
                    }
                }
                else if (dialogueDataList[line].GetEffectSubType() == "right")
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
                    Debug.LogError("Undefined effectSubType : " + dialogueDataList[line].GetEffectSubType());
                }
            }
            else if (dialogueDataList[line].GetEffectType() == "disappear")
            {
                if (dialogueDataList[line].GetEffectSubType() == "left")
                {
                    leftUnit = null;
                    leftPortrait.sprite = Resources.Load("StandingImage/" + "transparent", typeof(Sprite)) as Sprite;
                    isLeftUnitOld = false;
                }
                else if (dialogueDataList[line].GetEffectSubType() == "right")
                {
                    rightUnit = null;
                    rightPortrait.sprite = Resources.Load("StandingImage/" + "transparent", typeof(Sprite)) as Sprite;
                    isLeftUnitOld = true;
                }
                // 양쪽을 동시에 제거할경우 다음 유닛은 무조건 왼쪽에서 등장. 오른쪽 등장 명령어 사용하는 경우는 예외.
                else if (dialogueDataList[line].GetEffectSubType() == "all")
                {
                    leftUnit = null;
                    leftPortrait.sprite = Resources.Load("StandingImage/" + "transparent", typeof(Sprite)) as Sprite;
                    rightUnit = null;
                    rightPortrait.sprite = Resources.Load("StandingImage/" + "transparent", typeof(Sprite)) as Sprite;
                    isLeftUnitOld = false;
                }
                else
                {
                    Debug.LogError("Undefined effectSubType : " + dialogueDataList[line].GetEffectSubType());
                }
            }
            else
            {
                Debug.LogError("Undefined effectType : " + dialogueDataList[line].GetEffectType());
            }
            line++;
        }
    }

    IEnumerator PrintAllLine()
	{
		while (line < endLine)
		{
			yield return StartCoroutine(PrintEachLine());
			yield return null;
		}
		
		FindObjectOfType<SceneLoader>().LoadNextScene();
		yield return null;
	}
	
	void OnMouseDown()
	{
		if ((isWaitingMouseInput) && (!sceneLoader.IsScreenActive()) && (!skipQuestionUI.activeInHierarchy))
		{
			isWaitingMouseInput = false;
			line++;
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
