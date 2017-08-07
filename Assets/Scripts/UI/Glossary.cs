using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Glossary : MonoBehaviour{
	public GlossaryData.GlossaryType currentType;
	public int page;
	public Text InformationText;
	List<GlossaryData> GlossaryDataList = new List<GlossaryData>();
	/*
	List<GlossaryData> GroupDataList = new List<GlossaryData>();
	List<GlossaryData> PersonDataList = new List<GlossaryData>();
	List<GlossaryData> EtcDataList = new List<GlossaryData>();*/
	public List<Text> ButtonTextList;
	void OnEnable(){
		if(GlossaryDataList.Count == 0){
			GlossaryDataList = Parser.GetParsedData<GlossaryData>(Resources.Load<TextAsset>("Data/Glossary"), Parser.ParsingDataType.Glossary);
			/*foreach(GlossaryData data in GlossaryDataList){
				if(data.Type == GlossaryData.GlossaryType.Group)
					GroupDataList.Add(data);
				else if(data.Type == GlossaryData.GlossaryType.Person)
					PersonDataList.Add(data);
				else
					EtcDataList.Add(data);
			}*/
		}

		UpdateButtonName();
	}
	public void SetText(int number){
		if(number == 0){
			InformationText.text = "";	
		}else{
			GlossaryData searchResult = GlossaryDataList.Find(data => data.Type == currentType && data.number == number);
			if(searchResult != null)
				InformationText.text = searchResult.text;
		}
	}

	public void SetCurrentType(int number){
		currentType = (GlossaryData.GlossaryType)number;
	}

	public void UpdateButtonName(){
		for(int i = 0; i < 9; i++){
			GlossaryData searchResult = GlossaryDataList.Find(data => data.Type == currentType && data.number == i+1);
			if(searchResult == null)
				ButtonTextList[i].text = "";
			else
				ButtonTextList[i].text = searchResult.name;
		}
	}
}

public class GlossaryData{
	public enum GlossaryType {Group = 1, Person = 2, Etc = 3}
	public GlossaryType Type;
	public int number;
	public string name;
	public string text;
	public GlossaryData(string textRowData){
		StringParser parser = new StringParser(textRowData, '\t');
		Type = parser.ConsumeEnum<GlossaryType>();
		number = parser.ConsumeInt();
		name = parser.Consume();
		text = parser.Consume();
	}
}