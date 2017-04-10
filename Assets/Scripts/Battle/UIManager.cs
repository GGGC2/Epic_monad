using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

using BattleUI;

public class UIManager : MonoBehaviour
{
	const int skillButtonCount = 5;

	GameObject apBarUI;
	GameObject commandUI;
	GameObject skillUI;
	GameObject skillCheckUI;
	GameObject destCheckUI;
	GameObject unitViewerUI;
	GameObject selectedUnitViewerUI;
	GameObject tileViewerUI;
	GameObject selectDirectionUI;
	GameObject cancelButtonUI;
	GameObject skillNamePanelUI;
	GameObject movedUICanvas;

	GameObject notImplementedDebugPanel;

	private void Awake()
	{
		apBarUI = GameObject.Find("APBarPanel");
		commandUI = GameObject.Find("CommandPanel");
		skillUI = GameObject.Find("SkillPanel");
		skillCheckUI = GameObject.Find("SkillCheckPanel");
		destCheckUI = GameObject.Find("DestCheckPanel");
		unitViewerUI = GameObject.Find("UnitViewerPanel");
		selectedUnitViewerUI = GameObject.Find("SelectedUnitViewerPanel");
		tileViewerUI = GameObject.Find("TileViewerPanel");
		selectDirectionUI = GameObject.Find("SelectDirectionUI");
		cancelButtonUI = GameObject.Find("CancelButtonPanel");
		skillNamePanelUI = GameObject.Find("SkillNamePanel");
		movedUICanvas = GameObject.Find("MovedUICanvas");

		notImplementedDebugPanel = GameObject.Find("NotImplementedDebugPanel");
	}

	private void Start()
	{
		commandUI.SetActive(false);
		skillUI.SetActive(false);
		skillCheckUI.SetActive(false);
		destCheckUI.SetActive(false);
		unitViewerUI.SetActive(false);
		selectedUnitViewerUI.SetActive(false);
		tileViewerUI.SetActive(false);
		selectDirectionUI.SetActive(false);
		cancelButtonUI.SetActive(false);
		skillNamePanelUI.GetComponent<SkillNamePanel>().Hide();
	}

	public void UpdateApBarUI(BattleData battleData, List<Unit> allUnits) {
		apBarUI.SetActive(true);
		apBarUI.GetComponent<APBarPannel>().UpdateAPDisplay(battleData, allUnits);
	}

	public void SetCommandUIName(Unit selectedUnit)
	{
		commandUI.SetActive(true);
		commandUI.transform.Find("NameText").GetComponent<Text>().text = selectedUnit.GetName();
	}

	public void DisableCommandUI()
	{
		commandUI.SetActive(false);
	}

	private void EnableSkillUI()
	{
		skillUI.SetActive(true);
		foreach (Transform transform in skillUI.transform)
		{
			transform.gameObject.SetActive(true);
		}
	}

	public void UpdateSkillInfo(Unit selectedUnit)
	{
		EnableSkillUI();
		List<Skill> skillList = selectedUnit.GetLearnedSkillList();

		if (skillList.Count > skillButtonCount) {
			Debug.LogError("Too many skill count " + skillList.Count);
		}

		for (int i = 0; i < skillButtonCount; i++)
		{
			GameObject skillButton = GameObject.Find((i + 1).ToString() + "SkillButton"); //?? skillUI.transform.Find(i + "SkillButton")
			if (i >= skillList.Count)
			{
				skillButton.SetActive(false);
				continue;
			}

			skillButton.transform.Find("NameText").GetComponent<Text>().text = skillList[i].GetName();
			
			Skill skill = skillList[i];
			Unit caster = selectedUnit;
			int originAP = skill.GetRequireAP();
			int actualAP = Battle.Skills.SkillLogicFactory.Get(skill).CalculateAP(originAP, caster);			
			Text apText = skillButton.transform.Find("APText").GetComponent<Text>();
			apText.text = actualAP.ToString() + " AP";
			if (originAP < actualAP)
				apText.color = Color.red;
			else if (originAP > actualAP)
				apText.color = Color.green;
			else
				apText.color = Color.white;

			var skillCooldownDict = selectedUnit.GetUsedSkillDict();
			if (skillCooldownDict.ContainsKey(skillList[i].GetName()))
			{
				int remainCooldown = skillCooldownDict[skillList[i].GetName()];
				skillButton.transform.Find("CooldownText").GetComponent<Text>().text = "재사용까지 " + remainCooldown + "페이즈";
			}
			else
				skillButton.transform.Find("CooldownText").GetComponent<Text>().text = "";
		}
	}

	public void CheckUsableSkill(Unit selectedUnit)
	{
		List<Skill> skillList = selectedUnit.GetLearnedSkillList();

        Color enabledColor = new Color(1, 1, 1);
        Color disabledColor = new Color(1, 0, 0);

		int iterationCount = Math.Min(skillButtonCount, skillList.Count);
		for (int i = 0; i < iterationCount; i++)
		{
            Button skillButton = GameObject.Find((i + 1).ToString() + "SkillButton").GetComponent<Button>();
            skillButton.interactable = true;
		    skillButton.GetComponentInChildren<Text>().color = enabledColor;

            if (selectedUnit.GetCurrentActivityPoint() < selectedUnit.GetActualRequireSkillAP(skillList[i])
			|| selectedUnit.GetUsedSkillDict().ContainsKey(skillList[i].GetName()))
			{
                skillButton.interactable = false;
			    skillButton.GetComponentInChildren<Text>().color = disabledColor;
			}
		}
	}

	public void DisableSkillUI()
	{
		skillUI.SetActive(false);
	}

	public void SetSkillCheckAP(Unit selectedUnit, Skill selectedSkill)
	{
		skillCheckUI.SetActive(true);
		int requireAP = selectedUnit.GetActualRequireSkillAP(selectedSkill);
		string newAPText = "소모 AP : " + requireAP + "\n" +
			"잔여 AP : " + (selectedUnit.GetCurrentActivityPoint() - requireAP);
		skillCheckUI.transform.Find("APText").GetComponent<Text>().text = newAPText;
	}

	public void EnableSkillCheckChainButton(bool isPossible)
	{
		skillCheckUI.SetActive(true);
		GameObject.Find("ChainButton").GetComponent<Button>().interactable = isPossible;
	}

	public void DisableSkillCheckUI()
	{
		skillCheckUI.SetActive(false);
	}

	public void SetDestCheckUIAP(Unit selectedUnit, int totalUseActivityPoint)
	{
		destCheckUI.SetActive(true);
		string newAPText = "소모 AP : " + totalUseActivityPoint + "\n" +
			"잔여 AP : " + (selectedUnit.GetCurrentActivityPoint() - totalUseActivityPoint);
		destCheckUI.transform.Find("APText").GetComponent<Text>().text = newAPText;
	}

	public void DisableDestCheckUI()
	{
		destCheckUI.SetActive(false);
	}

	public void UpdateUnitViewer(Unit unitOnTile)
	{
		unitViewerUI.SetActive(true);
		unitViewerUI.GetComponent<UnitViewer>().UpdateUnitViewer(unitOnTile);
	}

	public bool IsUnitViewerShowing()
	{
		return unitViewerUI.activeInHierarchy;
	}

	public void DisableUnitViewer()
	{
		unitViewerUI.SetActive(false);
	}

	public void SetSelectedUnitViewerUI(Unit selectedUnit)
	{
		selectedUnitViewerUI.SetActive(true);
		selectedUnitViewerUI.GetComponent<UnitViewer>().UpdateUnitViewer(selectedUnit);
	}

	public void DisableSelectedUnitViewerUI()
	{
		selectedUnitViewerUI.SetActive(false);
	}

	public void SetTileViewer(Tile tile)
	{
		tileViewerUI.SetActive(true);
		FindObjectOfType<TileViewer>().UpdateTileViewer(tile);
	}

	public void DisableTileViewerUI()
	{
		tileViewerUI.SetActive(false);
	}

	public void EnableSelectDirectionUI()
	{
		selectDirectionUI.SetActive(true);
	}

	public void DisableSelectDirectionUI()
	{
		selectDirectionUI.SetActive(false);
	}

	public void EnableCancelButtonUI()
	{
		cancelButtonUI.SetActive(true);
	}

	public void DisableCancelButtonUI()
	{
		cancelButtonUI.SetActive(false);
	}

	public void ResetSkillNamePanelUI()
	{
		skillNamePanelUI.GetComponent<SkillNamePanel>().Hide();
	}

	public void SetSkillNamePanelUI(string skillName)
	{
		skillNamePanelUI.GetComponent<SkillNamePanel>().Set(skillName);
	}

	public void SetMovedUICanvasOnCenter(Vector2 position)
	{
		Vector3 newPosition = (new Vector3(position.x, position.y, -8));
		FindObjectOfType<CameraMover>().SetFixedPosition(newPosition);
		movedUICanvas.transform.position = newPosition;
	}

	public void AppendNotImplementedLog(String text)
	{
		if (notImplementedDebugPanel == null)
		{
			Debug.LogError("Cannot find not implemented debug panel\n" + text);
			return;
		}

		var debugPanel = notImplementedDebugPanel.GetComponent<NotImplementedDebugPanel>();
		debugPanel.Append(text);
	}
}
