using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Glossary : MonoBehaviour{
	public List<GlossaryData> GlossaryDataList = new List<GlossaryData>();
	public List<Text> ButtonTextList;
	void OnEnable(){
		if(GlossaryDataList.Count == 0)
			GlossaryDataList = Parser.GetParsedData<GlossaryData>(Resources.Load<TextAsset>("Data/Glossary"), Parser.ParsingDataType.Glossary);

		for(int i = 0; i < 9; i++){
			ButtonTextList[i].text = GlossaryDataList[i].name;
		}
	}
}

public class GlossaryData{
	public enum GlossaryType {Group, Person}
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