using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Enums;

public class SkillUIManager : MonoBehaviour {
	public Text ApText;
	public Text CooldownText;
	public Text RangeText;
	public Text ExplainText;
	public Image ActualRange;
	public Image RangeType;

	public void UpdateSkillInfoPanel(ActiveSkill skill, string unitName){
		ApText.text = skill.GetRequireAP().ToString();
		ExplainText.text = skill.GetSkillDataText().Replace("VALUE1", GetSkillValueText(skill.firstTextValueType, skill.firstTextValueCoef, unitName)).
													Replace("VALUE2", GetSkillValueText(skill.secondTextValueType, skill.secondTextValueCoef, unitName));

		int cooldown = skill.GetCooldown();
		if (cooldown > 0)
			CooldownText.text = "재사용까지 " + cooldown.ToString() + " 페이즈";

		Sprite actualRangeImage = Resources.Load<Sprite>("SkillRange/"+unitName+skill.GetColumn()+"_"+skill.GetRequireLevel());
		if(actualRangeImage != null)
			ActualRange.sprite = actualRangeImage;

		RangeText.text = "";
		if(skill.GetSkillType() == Enums.SkillType.Point){
			RangeType.sprite = Resources.Load<Sprite>("Icon/Skill/SkillType/Target");
			RangeText.text += GetFirstRangeText(skill);
		}
		else if(skill.GetSkillType() == Enums.SkillType.Route){
			RangeType.sprite = Resources.Load<Sprite>("Icon/Skill/SkillType/Line");
			RangeText.text += GetFirstRangeText(skill);
		}
		else
			RangeType.sprite = Resources.Load<Sprite>("Icon/Skill/SkillType/Auto");
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