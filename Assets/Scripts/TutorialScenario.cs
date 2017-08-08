using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialScenario : MonoBehaviour{
	public TutorialManager Manager;
	public List<Sprite> Stage1;
	public int index;

	public void Initialize(){
		if(GameData.SceneData.stageNumber == 1){
			//Debug.Log("Tutorial/" + SceneManager.GetActiveScene().name + GameData.SceneData.stageNumber.ToString() + "_1");
			Manager.image.sprite = Resources.Load<Sprite>("Tutorial/" + SceneManager.GetActiveScene().name + GameData.SceneData.stageNumber.ToString() + "_1");
		}
	}
}