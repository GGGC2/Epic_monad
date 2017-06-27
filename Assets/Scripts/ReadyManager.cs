using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class ReadyManager : MonoBehaviour{
	TextAsset csvFile;
	void Start()
	{
		csvFile = Resources.Load("Data/StageAvailablePC", typeof(TextAsset)) as TextAsset;
		string dataString = csvFile.text;
		string[] unparsedDataStrings = dataString.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
		string Data = "";

		foreach(string TempData in unparsedDataStrings)
		{
			string[] TempElements = TempData.Split('\t');
			if(TempElements[0] == SceneData.nextStageName)
			{
				Data = TempData;
				break;
			}
		}

		string[] elements = Data.Split('\t');
		for(int i = 1; i < elements.Length; i++)
		{

		}
	}
}
