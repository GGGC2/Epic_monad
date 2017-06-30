﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
public class ReadyManager : MonoBehaviour{
	TextAsset csvFile;
	void Start()
	{
		Debug.Log(SceneData.nextStageName);
		csvFile = Resources.Load("Data/StageAvailablePC", typeof(TextAsset)) as TextAsset;
		string dataString = csvFile.text;
		string[] unparsedDataStrings = dataString.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
		string Data = "";

		foreach(string TempData in unparsedDataStrings)
		{
			string[] TempElements = TempData.Split(',');
			//Debug.Log("TempData : " + TempData);
			//Debug.Log("TempElements[0] : "+TempElements[0]);
			if(TempElements[0] == SceneData.nextStageName)
			{
				Data = TempData;
				Debug.Log("StageData : " + Data);
				break;
			}
		}

		string[] elements = Data.Split(',');
		for(int i = 2; i < elements.Length; i++)
		{
			GameObject availableUnitPanel = GameObject.Find("AvailableUnit" + (i-1));
			availableUnitPanel.GetComponent<Image>().sprite = Resources.Load<Sprite>("UnitImage/portrait_" + elements[i]);
			availableUnitPanel.GetComponent<AvailableUnitPanel>().unitName = elements[i];
		}
	}
}
