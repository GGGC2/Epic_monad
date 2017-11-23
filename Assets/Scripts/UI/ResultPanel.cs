using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameData;
using Enums;

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

	public void Initialize(){
		LevelUpText.enabled = false;
		LevelText.text = "" + PartyData.level;
		ReqExpText.text = "Next " + PartyData.reqExp;

		TriggerIndex.text = ""; // initialized
		ScoreText.text = "";

		StartCoroutine(PrintResult());
	}

	public IEnumerator PrintResult(){
		List<BattleTrigger> scoreTriggers = BattleTriggerManager.Instance.triggers.FindAll(trig =>
			(trig.resultType == TrigResultType.Win || trig.resultType == TrigResultType.Bonus) && trig.acquired && trig.reward != 0);
		Debug.Log("count of scoreTriggers : " + scoreTriggers.Count);
		
		foreach(var trig in scoreTriggers){
			Debug.Log("Writing trigger info : " + trig.korName + "...");
			BattleData.rewardPoint += trig.reward;
			TriggerIndex.text += trig.korName + " " + MultiplierText(trig) + "\n";

			yield return new WaitForSeconds(0.1f);
			
			if (!trig.repeatable || trig.count == 1){
				ScoreText.text += trig.reward;
			}else{
				ScoreText.text += trig.reward * trig.count;
			}
				
			ScoreText.text += "\n";
			yield return new WaitForSeconds(0.4f);
		}

		TotalExpText.text = "획득 경험치 : " + BattleData.rewardPoint;
		
		//다 출력된 후 클릭을 해야 넘어감
		yield return new WaitUntil (() => Input.GetMouseButtonDown(0));

		PartyData.level += 1;

		Checker.sceneLoader.LoadNextDialogueScene(Checker.nextScriptName);
	}

	IEnumerator ShowLevelUpText(){
		LevelUpText.enabled = true;
		yield return new WaitForSeconds(0.5f);
	}

	string MultiplierText(BattleTrigger trigger){
		if(!trigger.repeatable || trigger.count == 1)
			return "";
		else
			return " x"+trigger.count;
	}

	/*void UpdateExp(int point){
		PartyData.AddExp(point);
		BattleData.rewardPoint -= point;
		UpdatePanel(BattleData.rewardPoint);
	}

	public void UpdatePanel(int remainScore){
		Debug.Log("exp : " +PartyData.exp + ", reqExp : " + PartyData.reqExp + ", Level : " + PartyData.level);
		TotalExpText.text = "획득 경험치 : " + remainScore;
		LevelText.text = "" + PartyData.level;
		ReqExpText.text = "Next " + (PartyData.reqExp - PartyData.exp);
		ExpBar.fillAmount = (float)PartyData.exp / (float)PartyData.reqExp;
	}*/
}