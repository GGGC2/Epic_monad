using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
public class ReadyManager : MonoBehaviour{
	TextAsset csvFile;
	string[] StageData;
	public List<UnitPanel> Selected = new List<UnitPanel>();

	void Start()
	{
		csvFile = Resources.Load("Data/StageAvailablePC", typeof(TextAsset)) as TextAsset;
		string dataString = csvFile.text;
		string[] unparsedDataStrings = dataString.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
		string[] StageData = {""};

		foreach(string TempData in unparsedDataStrings)
		{
			string[] TempElements = TempData.Split(',');
			//Debug.Log("TempData : " + TempData);
			//Debug.Log("TempElements[0] : "+TempElements[0]);
			if(TempElements[0] == SceneData.nextStageName)
			{
				StageData = TempData.Split(',');
				break;
			}
		}

		for(int i = 2; i < StageData.Length; i++)
		{
			GameObject availableUnitPanel = GameObject.Find("AvailableUnit" + (i-1));
			availableUnitPanel.GetComponent<UnitPanel>().SetNameAndSprite(StageData[i]);
		}
		for(int i = 1; i <= 8; i++)
		{
			UnitPanel Panel = GameObject.Find("SelectedUnit"+i).GetComponent<UnitPanel>();
			if(i <= Int32.Parse(StageData[1]))
				Selected.Add(Panel);
			else
				Panel.gameObject.SetActive(false);
		}
	}
}
