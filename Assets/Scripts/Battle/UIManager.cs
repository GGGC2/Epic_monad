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

	public void UpdateApBarUI(BattleData battleData, List<GameObject> allUnits) {
		apBarUI.SetActive(true);
		apBarUI.GetComponent<APBarPannel>().UpdateAPDisplay(battleData, allUnits);
	}

	public void SetCommandUIName(GameObject selectedUnitObject)
	{
		commandUI.SetActive(true);
		commandUI.transform.Find("NameText").GetComponent<Text>().text = selectedUnitObject.GetComponent<Unit>().GetName();
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

	public void UpdateSkillInfo(GameObject selectedUnitObject)
	{
		EnableSkillUI();
		List<Skill> skillList = selectedUnitObject.GetComponent<Unit>().GetLearnedSkillList();

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
			skillButton.transform.Find("APText").GetComponent<Text>().text = skillList[i].GetRequireAP(skillList[i].GetLevel()).ToString() + " AP";
			skillButton.transform.Find("CooldownText").GetComponent<Text>().text = "";
		}
	}

	public void CheckUsableSkill(GameObject selectedUnitObject)
	{
		List<Skill> skillList = selectedUnitObject.GetComponent<Unit>().GetLearnedSkillList();

        Color enabledColor = new Color(1, 1, 1);
        Color disabledColor = new Color(1, 0, 0);

		int iterationCount = Math.Min(skillButtonCount, skillList.Count);
		for (int i = 0; i < iterationCount; i++)
		{
            Button skillButton = GameObject.Find((i + 1).ToString() + "SkillButton").GetComponent<Button>();
            skillButton.interactable = true;
		    skillButton.GetComponentInChildren<Text>().color = enabledColor;

            if (selectedUnitObject.GetComponent<Unit>().GetCurrentActivityPoint() < selectedUnitObject.GetComponent<Unit>().GetActualRequireSkillAP(skillList[i])
			|| selectedUnitObject.GetComponent<Unit>().GetUsedSkillDict().ContainsKey(skillList[i].GetName()))
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

	public void SetSkillCheckAP(GameObject selectedUnitObject, Skill selectedSkill)
	{
		skillCheckUI.SetActive(true);
		int requireAP = selectedUnitObject.GetComponent<Unit>().GetActualRequireSkillAP(selectedSkill);
		string newAPText = "소모 AP : " + requireAP + "\n" +
			"잔여 AP : " + (selectedUnitObject.GetComponent<Unit>().GetCurrentActivityPoint() - requireAP);
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

	public void SetDestCheckUIAP(GameObject selectedUnitObject, int totalUseActivityPoint)
	{
		destCheckUI.SetActive(true);
		string newAPText = "소모 AP : " + totalUseActivityPoint + "\n" +
			"잔여 AP : " + (selectedUnitObject.GetComponent<Unit>().GetCurrentActivityPoint() - totalUseActivityPoint);
		destCheckUI.transform.Find("APText").GetComponent<Text>().text = newAPText;
	}

	public void DisableDestCheckUI()
	{
		destCheckUI.SetActive(false);
	}

	public void UpdateUnitViewer(GameObject unitOnTile)
	{
		unitViewerUI.SetActive(true);
		FindObjectOfType<UnitViewer>().UpdateUnitViewer(unitOnTile);
	}

	public bool IsUnitViewerShowing()
	{
		return unitViewerUI.activeInHierarchy;
	}

	public void DisableUnitViewer()
	{
		unitViewerUI.SetActive(false);
	}

	public void SetSelectedUnitViewerUI(GameObject selectedUnitObject)
	{
		selectedUnitViewerUI.SetActive(true);
		selectedUnitViewerUI.GetComponent<UnitViewer>().UpdateUnitViewer(selectedUnitObject);
	}

	public void DisableSelectedUnitViewerUI()
	{
		selectedUnitViewerUI.SetActive(false);
	}

	public void SetTileViewer(GameObject tile)
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
}
