using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

using BattleUI;

public class UIManager : MonoBehaviour
{
	public bool startFinished = false;
	const int skillButtonCount = 5;

	APBarPanel apBarUI;
	GameObject commandUI;
	GameObject skillUI;
	SkillCheckPanel skillCheckUI;
    GameObject ApplyButton;
	GameObject WaitButton;
	GameObject destCheckUI;
	GameObject unitViewerUI;
	GameObject selectedUnitViewerUI;
	GameObject tileViewerUI;
	SelectDirectionUI selectDirectionUI;
	GameObject cancelButtonUI;
	GameObject skillNamePanelUI;
	GameObject movedUICanvas;
	GameObject phaseUI;

	GameObject notImplementedDebugPanel;

	void Awake()
	{
		apBarUI = FindObjectOfType<APBarPanel>();
		commandUI = GameObject.Find("CommandPanel");
		skillUI = GameObject.Find("SkillPanel");
		skillCheckUI = FindObjectOfType<SkillCheckPanel>();
        ApplyButton = GameObject.Find("ApplyButton");
		WaitButton = GameObject.Find("WaitButton");
		destCheckUI = GameObject.Find("DestCheckPanel");
		unitViewerUI = GameObject.Find("UnitViewerPanel");
		selectedUnitViewerUI = GameObject.Find("SelectedUnitViewerPanel");
		tileViewerUI = GameObject.Find("TileViewerPanel");
		selectDirectionUI = FindObjectOfType<SelectDirectionUI>();
		cancelButtonUI = GameObject.Find("CancelButtonPanel");
		skillNamePanelUI = GameObject.Find("SkillNamePanel");
		movedUICanvas = GameObject.Find("MovingUICanvas");
		phaseUI = GameObject.Find("PhasePanel");
		notImplementedDebugPanel = GameObject.Find("NotImplementedDebugPanel");
	}

	void Start(){
		commandUI.SetActive(false);
		skillUI.SetActive(false);
		skillCheckUI.gameObject.SetActive(false);
		destCheckUI.SetActive(false);
		unitViewerUI.SetActive(false);
		selectedUnitViewerUI.SetActive(false);
		tileViewerUI.SetActive(false);
		selectDirectionUI.gameObject.SetActive(false);
		cancelButtonUI.SetActive(false);
		skillNamePanelUI.GetComponent<SkillNamePanel>().Hide();

		startFinished = true;
		StartCoroutine(FindObjectOfType<BattleManager>().InstantiateTurnManager());
	}

	public void UpdateApBarUI(BattleData battleData, List<Unit> allUnits) 
	{
		apBarUI.gameObject.SetActive(true);
		apBarUI.UpdateAPDisplay(battleData, allUnits);
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
    public void SetSkillUI() {
        Unit selectedUnit = MonoBehaviour.FindObjectOfType<BattleManager>().battleData.selectedUnit;
        List<ActiveSkill> skillList = selectedUnit.GetLearnedSkillList();
        SkillPanel skillPanel = skillUI.GetComponent<SkillPanel>();
        skillPanel.SetMaxPage((skillList.Count - 1) / skillButtonCount);
        skillPanel.triggerEnabled(selectedUnit);
        EnableSkillUI();
        
        UpdateSkillInfo(selectedUnit);
    }
    public void UpdateSkillInfo(Unit selectedUnit) {
        List<ActiveSkill> skillList = selectedUnit.GetLearnedSkillList();
        for (int i = 0; i < skillButtonCount; i++)
		{
            int skillIndex = i + skillButtonCount * skillUI.GetComponent<SkillPanel>().GetPage();
			GameObject skillButton = skillUI.transform.Find((i+1) + "SkillButton").gameObject;
			if (skillIndex >= skillList.Count)
			{
				//Debug.Log(selectedUnit+"'s skillCount : "+skillList.Count);
				skillButton.SetActive(false);
				continue;
			}
            skillButton.SetActive(true);

			skillButton.transform.Find("NameText").GetComponent<Text>().text = skillList[skillIndex].GetName();
			
			ActiveSkill skill = skillList[skillIndex];
			Unit caster = selectedUnit;
            int originAP = skill.GetRequireAP();
			int APChangedByStatusEffects = caster.GetActualRequireSkillAP(skill);
			int actualAP = Battle.Skills.SkillLogicFactory.Get(skill).CalculateAP(APChangedByStatusEffects, caster);			
			Text apText = skillButton.transform.Find("APText").GetComponent<Text>();
			apText.text = actualAP.ToString() + " AP";
			if (originAP < actualAP)
				apText.color = Color.red;
			else if (originAP > actualAP)
				apText.color = Color.green;
			else
				apText.color = Color.white;

			var skillCooldownDict = selectedUnit.GetUsedSkillDict();
			if (skillCooldownDict.ContainsKey(skillList[skillIndex].GetName()))
			{
				int remainCooldown = skillCooldownDict[skillList[skillIndex].GetName()];
				skillButton.transform.Find("CooldownText").GetComponent<Text>().text = "재사용까지 " + remainCooldown + "페이즈";
			}
			else
				skillButton.transform.Find("CooldownText").GetComponent<Text>().text = "";
		}
	}

	public void CheckUsableSkill(Unit selectedUnit)
	{
		List<ActiveSkill> skillList = selectedUnit.GetLearnedSkillList();

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

	public void SetSkillCheckAP(Unit selectedUnit, ActiveSkill selectedSkill)
	{
		skillCheckUI.gameObject.SetActive(true);
		int requireAP = selectedUnit.GetActualRequireSkillAP(selectedSkill);
		string newAPText = "소모 AP : " + requireAP + "\n" +
			"잔여 AP : " + (selectedUnit.GetCurrentActivityPoint() - requireAP);
		skillCheckUI.transform.Find("APText").GetComponent<Text>().text = newAPText;
	}

	public void EnableSkillCheckWaitButton(bool isApplyPossible, bool isWaitPossible)
	{
		skillCheckUI.gameObject.SetActive(true);
        ApplyButton.GetComponent<Button>().interactable = isApplyPossible;
		WaitButton.SetActive(isWaitPossible && isApplyPossible);
		//GameObject.Find("WaitButton").GetComponent<Button>().interactable = isPossible;
	}

	public void DisableSkillCheckUI()
	{
		skillCheckUI.gameObject.SetActive(false);
	}

	public void SetDestCheckUIAP(Unit selectedUnit, int totalUseActivityPoint)
	{
		destCheckUI.SetActive(true);
		string newAPText = "소모 AP : " + totalUseActivityPoint + "\n" +
			"잔여 AP : " + (selectedUnit.GetCurrentActivityPoint() - totalUseActivityPoint);
		destCheckUI.transform.Find("APText").GetComponent<Text>().text = newAPText;
	}

	public IEnumerator MovePhaseUI(int currentPhase)
	{
		Image img1 = phaseUI.GetComponent<Image>();
		Image img2 = phaseUI.transform.Find("AdditionalPanel").gameObject.GetComponent<Image>();

		phaseUI.transform.localPosition = new Vector3(-1280,0,0);
		phaseUI.transform.Find("Text").GetComponent<Text>().text = "Phase " + currentPhase;
		img1.DOFade(1, 0.5f);
		img2.DOFade(1, 0.5f);
		iTween.MoveTo(phaseUI, iTween.Hash("position", new Vector3(0,0,0), "islocal", true, "time", 1));
		yield return new WaitForSeconds(2f);
		img1.DOFade(0, 0.5f);
		img2.DOFade(0, 0.5f);
		iTween.MoveTo(phaseUI, iTween.Hash("position", new Vector3(1280,0,0), "islocal", true, "time", 1));
		yield return null;
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
		selectDirectionUI.gameObject.SetActive(true);
		selectDirectionUI.HighlightArrowButton();
	}

	public void DisableSelectDirectionUI()
	{
		selectDirectionUI.gameObject.SetActive(false);
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
		//FindObjectOfType<CameraMover>().SetFixedPosition(newPosition);
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
