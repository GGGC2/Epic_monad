using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialScenario : MonoBehaviour{
	public TutorialManager Manager;
	public List<Sprite> Stage1;
	public int index;

	public enum TutorialMission{None, MoveCommand, SkillCommand, StandbyCommand, SelectTile, SelectDirection, SelectSkill, ApplySkill}
	public TutorialMission mission;
	public Vector2 missionTile;
	public Enums.Direction missionDirection;
	public int missionSkillIndex;

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
			}else if(index == 6){
				mission = TutorialMission.SelectTile;
				missionTile = new Vector2(5,4);
			}else if(index == 7){
				mission = TutorialMission.SelectDirection;
				missionDirection = Enums.Direction.RightDown;
			}else if(index == 8){
				mission = TutorialMission.SkillCommand;
			}else if(index == 9){
				mission = TutorialMission.SelectSkill;
				missionSkillIndex = 1;
			}else if(index == 10){
				mission = TutorialMission.SelectDirection;
				missionDirection = Enums.Direction.RightDown;
			}else if(index == 11){
				mission = TutorialMission.ApplySkill;
			}else if(index == 12){
				mission = TutorialMission.MoveCommand;
			}else if(index == 13){
				mission = TutorialMission.SelectTile;
				missionTile = new Vector2(6,5);
			}else if(index == 14){
				mission = TutorialMission.SelectDirection;
				missionDirection = Enums.Direction.LeftDown;
			}else if(index == 15){
				mission = TutorialMission.SkillCommand;
			}else if(index == 16){
				mission = TutorialMission.SelectSkill;
				missionSkillIndex = 1;
			}else if(index == 17){
				mission = TutorialMission.SelectDirection;
				missionDirection = Enums.Direction.LeftDown;
			}else if(index == 18){
				mission = TutorialMission.ApplySkill;
			}else if(index == 19){
				mission = TutorialMission.StandbyCommand;
			}else if(index == 20){
				mission = TutorialMission.MoveCommand;
			}else if(index == 21){
				mission = TutorialMission.SelectTile;
				missionTile = new Vector2(4,4);
			}else if(index == 22){
				mission = TutorialMission.SelectDirection;
				missionDirection = Enums.Direction.LeftDown;
			}
			else
				SetNewSprite();
		}
	}
}