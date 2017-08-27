using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameData;

public class ResultPanel : MonoBehaviour{
	public Text LevelText;
	public Text TriggerIndex;
	public Text ScoreText;
	public Text TotalExpText;
	public Text ReqExpText;
	public Text LevelUpText;
	public Image ExpBar;
	private bool alreadyClicked;
	public BattleTriggerManager Checker;
	public int runningFrame;

	public void Clicked(){
		if (!alreadyClicked) {
			alreadyClicked = true;
			StartCoroutine (IClicked ());
		}
	}

	public void Active()
	{
		Initialize();
		UpdatePanel(0);
		PrintResult();
	}

	void Initialize()
	{
		LevelUpText.enabled = false;
		LevelText.text = "" + PartyData.GetLevel();
		ReqExpText.text = "Next " + PartyData.reqExp;

		TriggerIndex.text = ""; // initialized
		ScoreText.text = "";
	}

	void PrintResult()
	{
		StartCoroutine (IClicked ());
	}

	public IEnumerator IClicked(){
		foreach(BattleTrigger trigger in Checker.triggers){
			//Debug.Log("TriggerName : " + trigger.korName + ", acquired : " + trigger.acquired);
			if(trigger.acquired){
				TriggerIndex.text += trigger.korName + " " + MultiplierText(trigger);
				TriggerIndex.text += "\n";
				yield return new WaitForSeconds(0.1f);
				if (!trigger.repeatable || trigger.count == 1)
					ScoreText.text += trigger.reward;
				else
					ScoreText.text += trigger.reward * trigger.count;
				ScoreText.text += "\n";
				yield return new WaitForSeconds(0.4f);
			}
		}

		TotalExpText.text = "획득 경험치 : " + BattleData.rewardPoint;
		
		//다 출력된 후 클릭을 해야 넘어감
		yield return new WaitUntil (() => Input.GetMouseButtonDown(0));

		int expTick = BattleData.rewardPoint/runningFrame;
		int levelInPrevFrame = PartyData.GetLevel();
		while(BattleData.rewardPoint > 0){
			if(expTick == 0)
				UpdateExp(1);
			else if(BattleData.rewardPoint >= expTick)
				UpdateExp(expTick);
			else
				UpdateExp(BattleData.rewardPoint);
			yield return null;

			if (levelInPrevFrame != PartyData.GetLevel())
				yield return StartCoroutine(ShowLevelUpText());
			levelInPrevFrame = PartyData.GetLevel();
		}

		//다 출력된 후 클릭을 해야 넘어감
		yield return new WaitUntil (() => Input.GetMouseButtonDown(0));

		Checker.sceneLoader.LoadNextDialogueScene(Checker.nextScriptName);
	}

	IEnumerator ShowLevelUpText()
	{
		LevelUpText.enabled = true;
		yield return new WaitForSeconds(0.5f);
	}

	string MultiplierText(BattleTrigger trigger){
		if(!trigger.repeatable || trigger.count == 1)
			return "";
		else
			return " x"+trigger.count;
	}

	void UpdateExp(int point){
		PartyData.AddExp(point);
		BattleData.rewardPoint -= point;
		UpdatePanel(BattleData.rewardPoint);
	}

	public void UpdatePanel(int remainScore){
		Debug.Log("exp : " +PartyData.exp + ", reqExp : " + PartyData.reqExp + ", Level : " + PartyData.GetLevel());
		TotalExpText.text = "획득 경험치 : " + remainScore;
		LevelText.text = "" + PartyData.GetLevel();
		ReqExpText.text = "Next " + (PartyData.reqExp - PartyData.exp);
		ExpBar.fillAmount = (float)PartyData.exp / (float)PartyData.reqExp;
	}
}