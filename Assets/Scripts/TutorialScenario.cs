using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialScenario : MonoBehaviour{
	public TutorialManager Manager;
	public List<Sprite> Stage1;
	public int index;

	public enum TutorialMission{None, MoveCommand, SkillCommand}
	public TutorialMission mission;

	public void SetNewSprite(){
		Sprite searchResult = Resources.Load<Sprite>("Tutorial/"+SceneManager.GetActiveScene().name + GameData.SceneData.stageNumber.ToString() + "_" + index);
		if(searchResult != null)
			Manager.image.sprite = searchResult;
		else
			Debug.LogError("Sprite NOT found!");
	}

	public void NextStep(){
		index += 1;
		if(GameData.SceneData.stageNumber == 1){
			if(index == 5){
				mission = TutorialMission.MoveCommand;
				Manager.image.enabled = false;
				Manager.DarkBG.enabled = false;
			}
			else
				SetNewSprite();
		}
	}
}