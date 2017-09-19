using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameData;

public class Glossary : MonoBehaviour{
	int itemPerPage = 10;
	public GlossaryData.GlossaryType currentType;
	public int page; // 1이 아닌 0부터 시작
	public Text InformationText;
	public GameObject LeftArrow;
	public GameObject RightArrow;
	
	public List<Text> ButtonTextList;

	void OnEnable() {
		InitializeAll();
	}

	void InitializeAll(){
		page = 0;
		InitializeExceptPage();
	}

	void InitializeExceptPage(){
		SetArrowOnOff();
		UpdateButtonName();
		SetText(0);
	}

	public void SetText(int number){
		if (number == 0) {
			InformationText.text = "";	
		}else{
			GlossaryData searchResult = GlobalData.GlossaryDataList.Find (data => data.Type == currentType && data.index == number);
			if (searchResult != null && searchResult.level > 0) {
				InformationText.text = searchResult.text [searchResult.level];
			}
		}
	}

	public void SetCurrentType(int number){
		currentType = (GlossaryData.GlossaryType)number;
		InitializeAll();
	}

	public void UpdateButtonName(){
		for(int i = 0; i < 9; i++){
			GlossaryData searchResult = GlobalData.GlossaryDataList.Find(data => data.Type == currentType && data.index == page*itemPerPage+i+1);

			if (searchResult != null && searchResult.level > 0) {
				ButtonTextList [i].text = searchResult.name;
			} else {
				ButtonTextList [i].text = "";
			}
		}
	}

	public void MovePage(bool next){
		if(next){
			page += 1;
		}else{
			page -= 1;
		}
		InitializeExceptPage();
	}

	void SetArrowOnOff(){
		LeftArrow.SetActive(true);
		RightArrow.SetActive(true);
		if(page == 0){
			LeftArrow.SetActive(false);
		}
		if(page == CalcMaxPage()){
			RightArrow.SetActive(false);
		}
	}

	int CalcMaxPage(){
		return (GlobalData.GlossaryDataList.FindAll(data => data.Type == currentType).Count-1)/itemPerPage;
	}
}

public class GlossaryData{
	public enum GlossaryType { Group = 1, Person = 2, Etc = 3 }
	public GlossaryType Type;
	public int index;
	public int level;
	public string name;
	public Dictionary<int, string> text = new Dictionary<int, string>();
	public GlossaryData(string textRowData){
		StringParser parser = new StringParser(textRowData, '\t');
		Type = parser.ConsumeEnum<GlossaryType>();
		index = parser.ConsumeInt();
		level = 0;
		name = parser.Consume();
		text.Add(1, parser.Consume());
		text.Add(2, parser.Consume());
	}
}