using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GameData;

public class SkillSelectButton : SkillUI, IPointerDownHandler{
    public DetailInfoPanelInBattleReady RightPanel;
    public int row;
    public int level;

    void Awake(){
        iconSlot = GetComponent<Image>();
    }

    void Start(){
        Initialize();
    }

    public void Initialize(){
        mySkill = Skill.Find(RightPanel.allSkillList, RightPanel.RecentButton.nameString, level, row);
        if(mySkill == null || mySkill.requireLevel > PartyData.GetLevel()){
            gameObject.SetActive(false);
        }else{
            iconSlot.sprite = mySkill.icon;
        }
		name = "SkillSelectButton(" + level + "," + row + ")";
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData){
        var RM = FindObjectOfType<ReadyManager>();
        /*Debug.Log(RM.selectedUnits.Count);
        Debug.Log(RM.selectedUnits[0].name);
        Debug.Log(mySkill.owner);*/
        RM.selectedUnits.Find(unit => unit.name == mySkill.owner).selectedSkills.Add(mySkill);
    }
}