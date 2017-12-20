using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GameData;

public class SkillSelectButton : SkillUI, IPointerDownHandler{
    public RightScreen_BattleReady RightPanel;
    public int row = 0;
    public int level = 0;
    bool selected = false;

    void Awake(){
        iconSlot = GetComponent<Image>();
    }

    void Update(){
        if(selected){
            iconSlot.color = Color.white;
        }else{
            iconSlot.color = Color.gray;
        }
    }

    public void Initialize(){
        var RM = FindObjectOfType<ReadyManager>();
        mySkill = Skill.Find(RightPanel.allSkillList, RM.RecentUnitButton.nameString, level, row);
        if(mySkill == null){
            gameObject.SetActive(false);
        }else if(mySkill.requireLevel > PartyData.level){
            iconSlot.sprite = Resources.Load<Sprite>("Icon/Standby");
        }else{
            iconSlot.sprite = mySkill.icon;

            SelectedUnit owner = RM.selectedUnits.Find(unit => unit.name == mySkill.owner);
            if(owner != null){
                //Debug.Log(owner.selectedSkills[0].korName);
                selected = owner.selectedSkills.Any(skill => skill.korName == mySkill.korName);
            }
        }
		name = "SkillSelectButton(" + level + "," + row + ")";
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData){
        var RM = FindObjectOfType<ReadyManager>();
        SelectedUnit owner = RM.selectedUnits.Find(unit => unit.name == mySkill.owner);
        if(mySkill.requireLevel > PartyData.level)  {return;}
        else if(mySkill.requireLevel == 0){ //고유 특성을 누르면 초기화
            FindObjectOfType<BattleReadyPanel>().skillButtonList.ForEach(button => button.selected = false);
            owner.selectedSkills.Clear();
            Select(owner);
        }else if(owner.selectedSkills.Exists(skill => skill == mySkill)){
            owner.selectedSkills.Remove(mySkill);
            selected = false;
        }else if(owner.CurrentEther + mySkill.ether <= PartyData.MaxEther){
            Select(owner);
        }
    }

    void Select(SelectedUnit owner){
        owner.selectedSkills.Add(mySkill);
        selected = true;
    }
}