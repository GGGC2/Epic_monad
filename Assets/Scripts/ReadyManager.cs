using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
public class ReadyManager : MonoBehaviour{
	TextAsset csvFile;
	public List<UnitPanel> Selected = new List<UnitPanel>();

	void Start(){
		csvFile = Resources.Load<TextAsset>("Data/StageAvailablePC");
		string[] StageData = Parser.FindRowDataOf(csvFile.text, SceneData.nextStageName);

		for(int i = 2; i < 20; i++){
			GameObject availableUnitPanel = GameObject.Find("AvailableUnit" + (i-1));
			if(i < StageData.Length)				
				availableUnitPanel.GetComponent<UnitPanel>().SetNameAndSprite(StageData[i]);
			else
				availableUnitPanel.gameObject.SetActive(false);
		}

		for(int i = 1; i <= 8; i++){
			UnitPanel Panel = GameObject.Find("SelectedUnit"+i).GetComponent<UnitPanel>();
			if(i <= Int32.Parse(StageData[1]))
				Selected.Add(Panel);
			else
				Panel.gameObject.SetActive(false);
		}
	}

	void Update(){
		if(Input.GetKeyDown(KeyCode.A)){
			GameObject.Find("SceneLoader").GetComponent<SceneLoader>().LoadNextBattleScene(SceneData.nextStageName, false);
		}
	}
}
