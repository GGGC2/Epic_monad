using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using Enums;
using UnityEngine.SceneManagement;

public class SkillUIManager : MonoBehaviour {
	public Text ApText;
	public Text CooldownText;
	public Text RangeText;
	public Text NameText;
	public Text ExplainText;
	public GameObject CellPrefab;
	public List<Cell> cells = new List<Cell> ();
	public Image RangeType;

	public void UpdateSkillInfoUI(Skill skill, string unitName){
		if(SceneManager.GetActiveScene().name == "BattleReady")
			NameText.text = skill.korName;
			
		ExplainText.text = skill.skillDataText.Replace("VALUE1", GetSkillValueText(skill.firstTextValueType, skill.firstTextValueCoef, skill.firstTextValueBase, unitName)).
											   Replace("VALUE2", GetSkillValueText(skill.secondTextValueType, skill.secondTextValueCoef, skill.secondTextValueBase, unitName));

		if(skill is ActiveSkill){
			ActiveSkill activeSkill = (ActiveSkill)skill;

			ApText.text = activeSkill.GetRequireAP().ToString();

			int cooldown = activeSkill.GetCooldown();
			if (cooldown > 0)
				CooldownText.text = "재사용까지 " + cooldown.ToString() + " 페이즈";
			else
				CooldownText.text = "";

			RangeText.text = "";
			if(activeSkill.GetSkillType() == Enums.SkillType.Point){
				RangeType.sprite = Resources.Load<Sprite>("Icon/Skill/SkillType/Target");
				RangeText.text += GetFirstRangeText(activeSkill);
			}
			else if(activeSkill.GetSkillType() == Enums.SkillType.Route){
				RangeType.sprite = Resources.Load<Sprite>("Icon/Skill/SkillType/Line");
				RangeText.text += GetFirstRangeText(activeSkill);
			}
			else
				RangeType.sprite = Resources.Load<Sprite>("Icon/Skill/SkillType/Auto");

			DisplaySecondRange ((ActiveSkill)skill);
		}
		else{
			ApText.text = "";
			CooldownText.text = "특성(자동 적용)";
			HideSecondRange ();
			RangeType.sprite = Resources.Load<Sprite>("Icon/Empty");
			RangeText.text = "";
		}
	}

	string GetFirstRangeText(ActiveSkill skill){
		string result = "";
		if(skill.GetFirstMinReach() > 1)
			result = skill.GetFirstMinReach()+"~";
		return result + skill.GetFirstMaxReach();
	}

	string GetSkillValueText(Stat statType, float coef, float baseValue, string unitName){
        if(statType == Stat.Level) {
            return ((int)(GameData.PartyData.level * coef + baseValue)).ToString();
        }
        else if(SceneManager.GetActiveScene().name == "Battle") {
            Unit unit = MonoBehaviour.FindObjectOfType<UnitManager>().GetAllUnits().Find(u => u.GetNameInCode() == unitName);
            return (Math.Round(unit.GetStat(statType) * coef + baseValue)).ToString();
        }
        else return ((int)((float)UnitInfo.GetStat(unitName, statType)*coef + baseValue)).ToString();
	}

	void DisplaySecondRange(ActiveSkill skill){
		for (int i = cells.Count - 1; i >= 0; i--) {
			Cell cell = cells [i];
			cells.Remove (cell);
			Destroy (cell.gameObject);
		}
		int rowNum = 11;
		Dictionary<Vector2, Color> rangeColors = skill.RangeColorsForSecondRangeDisplay (rowNum);
		for (int x = 0; x < rowNum; x++) {
			for (int y = 0; y < rowNum; y++) {
				Cell cell = Instantiate (CellPrefab, RangeText.transform).GetComponent<Cell> ();
				cells.Add (cell);
				cell.SetSize (new Vector2 (9, 9));
				Vector2 pos = new Vector2 (x, y);
				cell.SetPosition (pos, new Vector2(-65, -145));
				cell.SetColor (rangeColors [pos]);
			}
		}
	}
	public void HideSecondRange(){
		for (int i = cells.Count - 1; i >= 0; i--) {
			Cell cell = cells [i];
			cells.Remove (cell);
			Destroy (cell.gameObject);
		}
	}
}