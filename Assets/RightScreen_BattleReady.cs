using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Enums;
using GameData;
using System.Linq;
using UnityEngine.EventSystems;

public class RightScreen_BattleReady : MonoBehaviour {
	public Image unitImage;
	public Text unitName;
	public Text hpText;
	public Text apText;
	public Text powerText;
	public Text defenseText;
	public Text resistText;
	public Text speedText;

	public Image classImage;
    public Image elementImage;
	public Image celestialImage;

	public List<Skill> allSkillList;
	List<ActiveSkill> allActiveSkillList;
	List<PassiveSkill> allPassiveSkillList;
	List<UnitStatusEffectInfo> allStatusEffectInfoList;
	List<TileStatusEffectInfo> allTileStatusEffectInfoList;

	List<SkillInfoButton> skillButtons = new List<SkillInfoButton>();
	public AvailableUnitButton RecentButton;

	readonly int testLevel = 30;
		
	public void SetCommonUnitInfoUI(string unitEngName){
		unitName.text = UnitInfo.ConvertToKoreanFullName(unitEngName);
		unitImage.sprite = Utility.IllustOf(unitEngName);
		string hpStatText = UnitInfo.GetStat(unitEngName, Stat.MaxHealth).ToString();
		hpText.text = hpStatText + " / " + hpStatText;
		int Agility = UnitInfo.GetStat(unitEngName, Stat.Agility);
		int level = PartyData.level;
		if (level == 0) level = testLevel; // 테스트를 위해 추가.
		apText.text = level + 60 + (Agility / 2) + " (+" + Agility + ")";
		
		powerText.text = UnitInfo.GetStat(unitEngName, Stat.Power).ToString();
		defenseText.text = UnitInfo.GetStat(unitEngName, Stat.Defense).ToString();
		resistText.text = UnitInfo.GetStat(unitEngName, Stat.Resistance).ToString();
		speedText.text = "100"; // 사기 기본값은 무조건 100

		Utility.SetClassImage(classImage, UnitInfo.GetUnitClass(unitEngName));
        Utility.SetElementImage(elementImage, UnitInfo.GetElement(unitEngName));
        Utility.SetCelestialImage(celestialImage, UnitInfo.GetCelestial(unitEngName));

		//GetSelectedSkillList(unitEngName);
		SetSkillToDetailInfoPanel(unitEngName);
	}

	List<ActiveSkill> activeSkillList = new List<ActiveSkill>();
	List<PassiveSkill> passiveSkillList = new List<PassiveSkill>();

	public void SetSkillToDetailInfoPanel(string unitEngName) {
		skillButtons.ForEach(button => button.gameObject.SetActive(true));

		var RM = FindObjectOfType<ReadyManager>();
        SelectedUnit unit = RM.selectedUnits.Find(selectedUnit => selectedUnit.name == unitEngName);
		if(unit == null){
			skillButtons.ForEach(button => button.gameObject.SetActive(false));
			return;
		}

		// 고유 특성을 맨 앞으로
		Skill uniquePassive = Parser.GetSkillByUnit(unitEngName).Find(pSkill => pSkill.requireLevel == 0);
		skillButtons.First().Initialize(uniquePassive);
				
		// 나머지 스킬 표시
		Debug.Log("selectedSkills : " + unit.selectedSkills.Count);
		for (int i = 1; i <= 10; i++){
			if(i <= unit.selectedSkills.Count){
				skillButtons[i].Initialize(unit.selectedSkills[i-1]);
			}else{
				skillButtons[i].gameObject.SetActive(false);
			}

            /*if (i <= activeSkillList.Count){
                skillButtons[i].Initialize(activeSkillList[i-1]);
            }else if(i < activeSkillList.Count + passiveSkillList.Count){
                skillButtons[i].Initialize(passiveSkillList[i - activeSkillList.Count]);
            }else{
                skillButtons[i].gameObject.SetActive(false);
            }*/
        }

		// 스킬 상세설명 초기화
		SkillInfoButton skillButton = skillButtons.Find(button => button.isActiveAndEnabled);
		skillButton.GetComponent<SkillInfoButton>().SetViewer();
		EventSystem.current.SetSelectedGameObject(skillButton.gameObject);
	}

	public void GetSelectedSkillList(string unitEngName) {
        /*int level = GameData.PartyData.level;
		if (level == 0) level = testLevel;

		activeSkillList = new List<ActiveSkill>();
		passiveSkillList = new List<PassiveSkill>();

        foreach (var activeSkill in allActiveSkillList) {
            if (activeSkill.owner == nameString && activeSkill.requireLevel <= level){
                activeSkill.ApplyUnitStatusEffectList(allStatusEffectInfoList, level);
                activeSkill.ApplyTileStatusEffectList(allTileStatusEffectInfoList, level);
                activeSkillList.Add(activeSkill);
			}
        }

		if(SceneData.stageNumber >= Setting.passiveOpenStage || SceneData.stageNumber == 0){
			foreach (var passiveSkill in allPassiveSkillList) {
        	    if (passiveSkill.owner == nameString && passiveSkill.requireLevel <= level){
            	    passiveSkill.ApplyUnitStatusEffectList(allStatusEffectInfoList, level);
	                passiveSkillList.Add(passiveSkill);
    	        }
        	}
		}
		
        // 비어있으면 디폴트 스킬로 채우도록.
        if (activeSkillList.Count == 0 && passiveSkillList.Count == 0) {
            foreach (var activeSkill in allActiveSkillList) {
                if (activeSkill.owner == "default" && activeSkill.requireLevel <= level)
                    activeSkillList.Add(activeSkill);
            }
		}*/
		var RM = FindObjectOfType<ReadyManager>();
        SelectedUnit unit = RM.selectedUnits.Find(selectedUnit => selectedUnit.name == unitEngName);
		
    }

	void Awake () {
		allSkillList = Parser.GetSkills();
		allActiveSkillList = Parser.GetParsedData<ActiveSkill>();
		allPassiveSkillList = Parser.GetParsedData<PassiveSkill>();
		allStatusEffectInfoList = Parser.GetParsedData<UnitStatusEffectInfo>();
        allTileStatusEffectInfoList = Parser.GetParsedTileStatusEffectInfo();

		//0번이 고유 특성 자리
        for(int i = 0; i <= 10; i++){
            skillButtons.Add(GameObject.Find("SkillPrevButton"+i).GetComponent<SkillInfoButton>());
        }
	}
}