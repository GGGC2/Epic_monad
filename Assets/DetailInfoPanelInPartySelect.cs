using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Enums;
using GameData;
using System.Linq;

public class DetailInfoPanelInPartySelect : MonoBehaviour {

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

	List<ActiveSkill> allActiveSkillList;
	List<PassiveSkill> allPassiveSkillList;
	List<UnitStatusEffectInfo> allStatusEffectInfoList;
	List<TileStatusEffectInfo> allTileStatusEffectInfoList;

	List<SkillInfoButton> skillButtons = new List<SkillInfoButton>();

	readonly int testLevel = 30;
		
	public void SetCommonUnitInfoUI(string nameString){
		unitImage.sprite = Utility.IllustOf(nameString);
		unitName.text = UnitInfo.ConvertToKoreanFullName(nameString);
		string hpStatText = UnitInfo.GetStat(nameString, Stat.MaxHealth).ToString();
		hpText.text = hpStatText + " / " + hpStatText;
		int Agility = UnitInfo.GetStat(nameString, Stat.Agility);
		int level = PartyData.level;
		if (level == 0) level = testLevel; // 테스트를 위해 추가.
		apText.text = level + 60 + (Agility / 2) + " (+" + Agility + ")";
		
		powerText.text = UnitInfo.GetStat(nameString, Stat.Power).ToString();
		defenseText.text = UnitInfo.GetStat(nameString, Stat.Defense).ToString();
		resistText.text = UnitInfo.GetStat(nameString, Stat.Resistance).ToString();
		speedText.text = "100"; // 속도 기본값은 무조건 100

		Utility.SetClassImage(classImage, UnitInfo.GetUnitClass(nameString));
        Utility.SetElementImage(elementImage, UnitInfo.GetElement(nameString));
        Utility.SetCelestialImage(celestialImage, UnitInfo.GetCelestial(nameString));

		SearchSkillList(nameString);
		SetSkillToDetailInfoPanel();
	}

	List<ActiveSkill> activeSkillList = new List<ActiveSkill>();
	List<PassiveSkill> passiveSkillList = new List<PassiveSkill>();

	public void SetSkillToDetailInfoPanel() {
		skillButtons.ForEach(button => button.gameObject.SetActive(true));

		// 고유특성 (렙제 0인 특성) 은 무조건 앞으로
		if (passiveSkillList.Any(pSkill => pSkill.requireLevel == 0)) {
			PassiveSkill uniquePassive = passiveSkillList.Find(pSkill => pSkill.requireLevel == 0);
			skillButtons.First().Initialize(uniquePassive);
		}
		else {
			skillButtons.First().gameObject.SetActive(false);
		}
		
		// 나머지 스킬을 액티브 -> 패시브 순서로 표시
		for (int i = 1; i <= 10; i++){
            if (i <= activeSkillList.Count){
                skillButtons[i].Initialize(activeSkillList[i-1]);
            }else if(i < activeSkillList.Count + passiveSkillList.Count){
                skillButtons[i].Initialize(passiveSkillList[i - activeSkillList.Count]);
            }else{
                skillButtons[i].gameObject.SetActive(false);
            }
        }
	}

	public void SearchSkillList(string nameString) {
        int level = GameData.PartyData.level;
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
		}
    }

	void Awake () {
		allActiveSkillList = Parser.GetParsedData<ActiveSkill>();
		allPassiveSkillList = Parser.GetParsedData<PassiveSkill>();
		allStatusEffectInfoList = Parser.GetParsedData<UnitStatusEffectInfo>();
        allTileStatusEffectInfoList = Parser.GetParsedTileStatusEffectInfo();

		//0번이 고유 특성 자리
        for(int i = 0; i <= 10; i++){
            skillButtons.Add(GameObject.Find("SkillPrevButton"+i).GetComponent<SkillInfoButton>());
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}