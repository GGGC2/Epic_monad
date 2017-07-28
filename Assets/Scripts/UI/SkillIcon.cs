using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class SkillIcon : MonoBehaviour, IPointerEnterHandler{
	public SkillUIManager InfoPanel;
	public Text InfoText;
	public Skill skill;
	public int column;
	public int level;
	void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData){
		if(GetComponent<Image>().sprite != Resources.Load<Sprite>("Icon/Empty")){
			InfoText.text = skill.GetSkillDataText().Replace("VALUE", Calculator.GetSkillBasePower(GameObject.Find("PowerText").GetComponent<Text>().text, skill));
			InfoPanel.UpdateSkillInfoPanel(skill, FindObjectOfType<ReadyManager>().currentUnitName);
			/*string currentUnitName = FindObjectOfType<ReadyManager>().currentUnitName;
			TextAsset skillData = Resources.Load<TextAsset>("Data/testSkillData");
			List<SkillInfo> skillInfoList = Parser.GetParsedSkillInfo();
			Skill thisSkill = skillInfoList.Find(skill => skill.owner == currentUnitName && skill.requireLevel == level && skill.column == column).GetSkill();
			InfoText.text = thisSkill.GetName() + "\n\n" + thisSkill.GetSkillDataText().Replace("VALUE", Calculator.GetSkillBasePower(UnitInfo.GetStat(currentUnitName, UnitInfo.StatType.Power), thisSkill));*/
		}
	}
}