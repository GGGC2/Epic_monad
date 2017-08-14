using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameData;

public class Glossary : MonoBehaviour{
	public GlossaryData.GlossaryType currentType;
	public int page;
	public Text InformationText;
	
	public List<Text> ButtonTextList;
	void OnEnable() {UpdateButtonName();}
	public void SetText(int number){
		if(number == 0){
			InformationText.text = "";	
		}else{
			GlossaryData searchResult = GlobalData.GlossaryDataList.Find(data => data.Type == currentType && data.index == number);
			if(searchResult != null && searchResult.level > 0)
				InformationText.text = searchResult.text;
		}
	}

	public void SetCurrentType(int number){
		currentType = (GlossaryData.GlossaryType)number;
	}

	public void UpdateButtonName(){
		//GlobalData.ViewAllGlossaryData();
		Debug.Log(currentType.ToString());
		for(int i = 0; i < 9; i++){
			GlossaryData searchResult = GlobalData.GlossaryDataList.Find(data => data.Type == currentType && data.index == i+1);

			if(searchResult != null && searchResult.level > 0){
				//Debug.Log("FOUND");
				ButtonTextList[i].text = searchResult.name;
			}
			else
				ButtonTextList[i].text = "";				
		}
	}
}

public class GlossaryData{
	public enum GlossaryType {Group = 1, Person = 2, Etc = 3}
	public GlossaryType Type;
	public int index;
	public int level;
	public string name;
	public string text;
	public GlossaryData(string textRowData){
		StringParser parser = new StringParser(textRowData, '\t');
		Type = parser.ConsumeEnum<GlossaryType>();
		index = parser.ConsumeInt();
		level = 0;
		name = parser.Consume();
		text = parser.Consume();
	}
}