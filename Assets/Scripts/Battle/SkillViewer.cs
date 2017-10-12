using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;
using Enums;

public class SkillUI : MonoBehaviour, IPointerEnterHandler{
	public Image iconSlot;
	public Skill mySkill;
	public SkillViewer viewer;

	void IPointerEnterHandler.OnPointerEnter(PointerEventData pointerData){
		SetViewer();
	}

	public void SetViewer(){
		if(mySkill != null && viewer != null){
			viewer.UpdateSkillViewer(mySkill);
			EventSystem.current.SetSelectedGameObject(gameObject);
		}
	}
}

public class SkillViewer : SkillUI{
	public Text viewerNameText;
	public Text costText;
	public Text cooldownText;
	public Image rangeType;
	public Text rangeText;
	public Text explainText;

	public GameObject CellPrefab;
	public List<Cell> cells = new List<Cell> ();

	public enum CallerType{DetailInfo, ActionButton, EtherPanel}
	public CallerType caller;

	void OnDisable(){
		//Debug.Log("Why Disabled?");
	}
	public void Initialize(){
		viewerNameText.text = "";
		costText.text = "";
		cooldownText.text = "";
		rangeType.sprite = Resources.Load<Sprite>("transparent");
		rangeText.text = "";
		explainText.text = "";
		HideSecondRange ();
	}

	public void UpdateSkillViewer(Skill skill){
		mySkill = skill;
		if(mySkill is ActiveSkill){
			ActiveSkill activeSkill = (ActiveSkill)mySkill;

			costText.text = "<color=cyan>행동력 " + activeSkill.GetRequireAP() + "</color>";
			int cooldown = activeSkill.GetCooldown();
			if(cooldown > 0){
				cooldownText.text = "재사용 대기 " + cooldown.ToString() + "페이즈";
			}else{
				cooldownText.text = "";
			}

			rangeText.text = "";

			if(activeSkill.GetSkillType() == Enums.SkillType.Point){
				rangeType.sprite = Resources.Load<Sprite>("Icon/Skill/SkillType/Target");
				rangeText.text += GetFirstRangeText(activeSkill);
			}else if(activeSkill.GetSkillType() == Enums.SkillType.Route){
				rangeType.sprite = Resources.Load<Sprite>("Icon/Skill/SkillType/Line");
				rangeText.text += GetFirstRangeText(activeSkill);
			}else{
				rangeType.sprite = Resources.Load<Sprite>("Icon/Skill/SkillType/Auto");	
			}

			DisplaySecondRange ((ActiveSkill)mySkill);
		}else{
			costText.text = "";
			cooldownText.text = "특성(자동 적용)";
			rangeType.sprite = Resources.Load<Sprite>("Icon/Empty");
			rangeText.text = "";
			HideSecondRange ();
			explainText.text = "";
		}

		SetNameText();
		explainText.text = Utility.ExplainTextReplace(
			mySkill.skillDataText.Replace("VALUE1", GetSkillValueText(mySkill.firstTextValueType, mySkill.firstTextValueCoef, mySkill.firstTextValueBase)).
								  Replace("VALUE2", GetSkillValueText(mySkill.secondTextValueType, mySkill.secondTextValueCoef, mySkill.secondTextValueBase)));

		if(SceneManager.GetActiveScene().name == "BattleReady"){
			explainText.text += "\n<color=#ff9999ff>에테르 " + mySkill.ether + "</color>";
		}
	}

	public void SetNameText(){
		Debug.Assert(mySkill != null, name + "'s mySkill is null!");
		viewerNameText.text = mySkill.korName;
	}

	string GetFirstRangeText(ActiveSkill skill){
		string result = "";
		if(skill.GetFirstMinReach() > 1)
			result = skill.GetFirstMinReach()+"~";
		return result + skill.GetFirstMaxReach();
	}

	string GetSkillValueText(Stat statType, float coef, float baseValue){
        if(statType == Stat.Level) {
            return ((int)(GameData.PartyData.level * coef + baseValue)).ToString();
        }
        else if(SceneManager.GetActiveScene().name == "Battle"){
            Unit unit = MonoBehaviour.FindObjectOfType<UnitManager>().GetAllUnits().Find(u => u.GetNameEng() == mySkill.owner);
            return ((int)(unit.GetStat(statType) * coef + baseValue)).ToString();
        }
        else return ((int)((float)UnitInfo.GetStat(mySkill.owner, statType)*coef + baseValue)).ToString();
	}

	void DisplaySecondRange(ActiveSkill skill){
		//Debug.Log("Show SecondRange.");
		for (int i = cells.Count - 1; i >= 0; i--) {
			Cell cell = cells [i];
			cells.Remove (cell);
			Destroy (cell.gameObject);
		}
		int rowNum = 11;
		Dictionary<Vector2, Color> rangeColors = skill.RangeColorsForSecondRangeDisplay (rowNum);
		for (int x = 0; x < rowNum; x++) {
			for (int y = 0; y < rowNum; y++) {
				Cell cell = Instantiate(CellPrefab, rangeText.transform).GetComponent<Cell>();
				cells.Add (cell);
				cell.SetSize (new Vector2 (9, 9));
				Vector2 pos = new Vector2 (x, y);

				if(caller == CallerType.EtherPanel){
					cell.SetPosition (pos, new Vector2(-30, -120));
				}else if(caller == CallerType.ActionButton){
					cell.SetPosition (pos, new Vector2(-85, -145));
				}else{
					cell.SetPosition (pos, new Vector2(-35, -110));
				}
				
				cell.SetColor (rangeColors [pos]);
				if (x == (rowNum - 1) / 2 && x == y)
					cell.SetAsDotCell ();
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