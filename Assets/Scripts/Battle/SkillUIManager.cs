using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using Enums;

public class SkillUIManager : MonoBehaviour {
	public Text ApText;
	public Text CooldownText;
	public Text RangeText;
	public Text ExplainText;
	public Image ActualRange;
	public Image RangeType;

	public void UpdateSkillInfoPanel(Skill skill, string unitName){
		ExplainText.text = skill.skillDataText.Replace("VALUE1", GetSkillValueText(skill.firstTextValueType, skill.firstTextValueCoef, unitName)).
											   Replace("VALUE2", GetSkillValueText(skill.secondTextValueType, skill.secondTextValueCoef, unitName));

		if(skill is ActiveSkill){
			ActiveSkill activeSkill = (ActiveSkill)skill;

			ApText.text = activeSkill.GetRequireAP().ToString();

			int cooldown = activeSkill.GetCooldown();
			if (cooldown > 0)
				CooldownText.text = "재사용까지 " + cooldown.ToString() + " 페이즈";
			else
				CooldownText.text = "";

			Sprite actualRangeImage = Resources.Load<Sprite>("SkillRange/"+unitName+activeSkill.GetColumn()+"_"+activeSkill.GetRequireLevel());
			if(actualRangeImage != null)
				ActualRange.sprite = actualRangeImage;

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
		}
		else{
			ApText.text = "";
			CooldownText.text = "특성(자동 적용)";
			ActualRange.sprite = Resources.Load<Sprite>("Icon/Empty");
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

	string GetSkillValueText(Stat statType, float coef, string unitName){
		if(statType == Stat.Power)
			return ((int)((float)UnitInfo.GetStat(unitName, UnitInfo.StatType.Power)*coef)).ToString();
		else{
			Debug.Log("Unknown StatType");
			return null;
		}
	}
}