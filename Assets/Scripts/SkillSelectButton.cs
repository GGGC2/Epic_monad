using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GameData;

public class SkillSelectButton : SkillUI, IPointerDownHandler{
    public DetailInfoPanelInBattleReady RightPanel;
    public int row = 0;
    public int level = 0;
    bool selected = false;

    void Awake(){
        iconSlot = GetComponent<Image>();
    }

    void Start(){
        Initialize();
    }

    void Update(){
        if(selected){
            iconSlot.color = Color.white;
        }else{
            iconSlot.color = Color.gray;
        }
    }

    public void Initialize(){
        mySkill = Skill.Find(RightPanel.allSkillList, RightPanel.RecentButton.nameString, level, row);
        if(mySkill == null || mySkill.requireLevel > PartyData.level){
            gameObject.SetActive(false);
        }else{
            iconSlot.sprite = mySkill.icon;

            var RM = FindObjectOfType<ReadyManager>();
            SelectedUnit owner = RM.selectedUnits.Find(unit => unit.name == mySkill.owner);
            if(owner.selectedSkills.Exists(skill => skill == mySkill)){
                selected = true;
            }else if(owner.CurrentEther + mySkill.ether <= PartyData.MaxEther){
                selected = false;
            }
        }
		name = "SkillSelectButton(" + level + "," + row + ")";
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData){
        var RM = FindObjectOfType<ReadyManager>();
        SelectedUnit owner = RM.selectedUnits.Find(unit => unit.name == mySkill.owner);
        if(owner.selectedSkills.Exists(skill => skill == mySkill)){
            owner.selectedSkills.Remove(mySkill);
            selected = false;
        }else if(owner.CurrentEther + mySkill.ether <= PartyData.MaxEther){
            owner.selectedSkills.Add(mySkill);
            selected = true;
        }
    }
}