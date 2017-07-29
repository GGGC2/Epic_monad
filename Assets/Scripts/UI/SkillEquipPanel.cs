using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillEquipPanel : MonoBehaviour{
	public SkillIcon FixedIcon;
	SkillIcon[,] SkillIcons = new SkillIcon[3,8];

	void Start(){
		for(int i = 0; i < 3; i++){
			for(int j = 0; j < 8; j++){
				SkillIcons[i,j] = GameObject.Find(IntToColumnString(i)+(7*j+1)).GetComponent<SkillIcon>();
			}
		}
	}

	public void UpdateIcons(string unitName){
		ChangeSpriteOrEmpty(FixedIcon.gameObject.GetComponent<Image>(), "Icon/Skill/"+unitName+"/Fixed");
		SearchAndSetSkill(FixedIcon, unitName);
		foreach(SkillIcon IconSlot in SkillIcons){
			ChangeSpriteOrEmpty(IconSlot.gameObject.GetComponent<Image>(), "Icon/Skill/"+unitName+"/"+IconSlot.gameObject.name);
			SearchAndSetSkill(IconSlot, unitName);
		}
	}

	void SearchAndSetSkill(SkillIcon IconSlot, string unitName){
		List<Skill> Skills = Parser.GetSkills();
		Skill SearchResult = Skills.Find(skill => skill.owner == unitName && skill.requireLevel == IconSlot.level && skill.column == IconSlot.column);
		if(SearchResult != null)
			IconSlot.skill = SearchResult;
	}

	void ChangeSpriteOrEmpty(Image image, string spriteAddress){
		//Debug.Log("Put "+spriteAddress+" in "+image.gameObject.name);
		if(Resources.Load<Sprite>(spriteAddress) != null)
			image.sprite = Resources.Load<Sprite>(spriteAddress);
		else
			image.sprite = Resources.Load<Sprite>("Icon/Empty");
	}

	string IntToColumnString(int i){
		if(i == 0)
			return "Left";
		else if(i == 1)
			return "Mid";
		else if(i == 2)
			return "Right";
		else
			Debug.Log("Invalid Input");
			return "";
	}
}