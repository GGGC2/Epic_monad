using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GameData;

public class SkillSelectButton : SkillUI, IPointerDownHandler{
    public RightScreen_BattleReady rightPanel;
    ReadyManager RM;
    public int row = 0;
    public int level = 0;
    public bool selected = false;
    public CustomUIText EtherText;

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
        RM = ReadyManager.Instance;
        mySkill = Skill.Find(rightPanel.allSkillList, RM.RecentUnitButton.nameString, level, row);
        if(mySkill == null){
            gameObject.SetActive(false);
        }else if(mySkill.requireLevel > PartyData.level){
            iconSlot.sprite = Resources.Load<Sprite>("Icon/Standby");
            EtherText.text = "";
        }else{
            EtherText.text = mySkill.ether + "";
            iconSlot.sprite = mySkill.icon;

            SelectedUnit owner = RM.selectedUnits.Find(unit => unit.name == mySkill.owner);
            if(owner != null){
                selected = owner.selectedSkills.Any(skill => skill.korName == mySkill.korName);
            }
        }
		name = "SkillSelectButton(" + level + "," + row + ")";
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData){
        SelectedUnit owner = RM.selectedUnits.Find(unit => unit.name == mySkill.owner);
        if(mySkill.requireLevel > PartyData.level || owner == null)  {return;}
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

        string newEtherText = UnitInfo.ConvertToKoreanName(RM.RecentUnitButton.nameString)
            + "\n" + owner.CurrentEther + " / " + PartyData.MaxEther;
        RM.RecentUnitButton.NameText.GetComponent<Text>().text = newEtherText;
    }

    void Select(SelectedUnit owner){
        owner.selectedSkills.Add(mySkill);
        selected = true;
    }
}