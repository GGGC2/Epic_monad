using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using GameData;
public class ReadyManager : MonoBehaviour{
	TextAsset csvFile;
	public List<UnitPanel> selected = new List<UnitPanel>();
	public string currentUnitName;

	void Start()
	{
		csvFile = Resources.Load<TextAsset>("Data/StageAvailablePC");
		string[] stageData = Parser.FindRowDataOf(csvFile.text, SceneData.stageNumber.ToString());

		for (int i = 2; i < 20; i++)
		{
			GameObject availableUnitPanel = GameObject.Find("AvailableUnit" + (i-1));
			if (i < stageData.Length)				
				availableUnitPanel.GetComponent<UnitPanel>().SetNameAndSprite(stageData[i]);
			else
				availableUnitPanel.gameObject.SetActive(false);
		}

		for (int i = 1; i <= 8; i++)
		{
			UnitPanel Panel = GameObject.Find("SelectedUnit"+i).GetComponent<UnitPanel>();
			if (i <= Int32.Parse(stageData[1]))
			{
				selected.Add(Panel);
				Panel.unitName = "unselected";
			}
			else
				Panel.gameObject.SetActive(false);
		}

		DontDestroyOnLoad(gameObject);
	}

	void Update()
	{
		if(Input.GetKeyDown(KeyCode.A))
		{
			GameObject.Find("SceneLoader").GetComponent<SceneLoader>().LoadNextBattleScene();
		}
	}
}
