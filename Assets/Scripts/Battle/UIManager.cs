using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

using BattleUI;

public class UIManager : MonoBehaviour
{
	private static UIManager instance;
	public static UIManager Instance{
		get { return instance; }
	}

	public bool startFinished = false;

	APBarPanel apBarUI;
	GameObject commandUI;
	public CommandPanel commandPanel;
	GameObject skillUI;
	public SkillPanel skillPanel;
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
    GameObject statusEffectDisplayPanel;
    Vector3 originalStatusEffectDisplayPanelPosition;

	GameObject notImplementedDebugPanel;

	void Awake(){
		instance = this;
		apBarUI = FindObjectOfType<APBarPanel>();
		commandUI = GameObject.Find("CommandPanel");
		commandPanel = commandUI.GetComponent<CommandPanel> ();
		commandPanel.Initialize ();
		skillUI = GameObject.Find("SkillPanel");
		skillPanel = skillUI.GetComponent<SkillPanel> ();
		skillCheckUI = FindObjectOfType<SkillCheckPanel>();
        ApplyButton = GameObject.Find("ApplyButton");
		WaitButton = GameObject.Find("WaitButton");
		destCheckUI = GameObject.Find("DestCheckPanel");
		unitViewerUI = GameObject.Find("UnitViewerPanel");
        statusEffectDisplayPanel = GameObject.Find("StatusEffectDisplayPanel");
        selectedUnitViewerUI = GameObject.Find("SelectedUnitViewerPanel");
		tileViewerUI = GameObject.Find("TileViewerPanel");
		selectDirectionUI = FindObjectOfType<SelectDirectionUI>();
		cancelButtonUI = GameObject.Find("CancelButtonPanel");
		skillNamePanelUI = GameObject.Find("SkillNamePanel");
		movedUICanvas = GameObject.Find("MovingUICanvas");
		phaseUI = GameObject.Find("PhasePanel");
		notImplementedDebugPanel = GameObject.Find("NotImplementedDebugPanel");

		TutorialScenario.commandPanel = commandPanel;
		TutorialScenario.skillPanel = skillPanel;
	}

	void Start(){
		commandUI.SetActive(false);
		skillUI.SetActive(false);
		skillCheckUI.gameObject.SetActive(false);
		destCheckUI.SetActive(false);
		unitViewerUI.SetActive(false);
        statusEffectDisplayPanel.SetActive(false);
        selectedUnitViewerUI.SetActive(false);
		tileViewerUI.SetActive(false);
		selectDirectionUI.gameObject.SetActive(false);
		cancelButtonUI.SetActive(false);
		skillNamePanelUI.GetComponent<SkillNamePanel>().Hide();

        originalStatusEffectDisplayPanelPosition = statusEffectDisplayPanel.transform.position;
		startFinished = true;
		StartCoroutine(FindObjectOfType<BattleManager>().InstantiateTurnManager());
	}

	public void UpdateApBarUI(BattleData battleData, List<Unit> allUnits) {
		apBarUI.gameObject.SetActive(true);
		apBarUI.UpdateAPDisplay(battleData, allUnits);
	}

	public void ActivateCommandUIAndSetName(Unit selectedUnit)
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
    public void SetSkillUI(Unit selectedUnit) {
        List<ActiveSkill> skillList = selectedUnit.GetLearnedSkillList();
        skillPanel.SetMaxPage((skillList.Count - 1) / SkillPanel.onePageButtonsNum);
        skillPanel.triggerEnabled(selectedUnit);
        EnableSkillUI();
        
        UpdateSkillInfo(selectedUnit);
    }
    public void UpdateSkillInfo(Unit selectedUnit) {
        List<ActiveSkill> skillList = selectedUnit.GetLearnedSkillList();
        for (int i = 0; i < SkillPanel.onePageButtonsNum; i++)
		{
			int skillIndex = i + SkillPanel.onePageButtonsNum * skillPanel.GetPage();
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
        TurnOnOnlyUsableSkills(selectedUnit);
	}

	public void TurnOnOnlyUsableSkills(Unit selectedUnit){
		List<ActiveSkill> skillList = selectedUnit.GetLearnedSkillList();
		int page = skillPanel.GetPage();

		for (int skillIndex = SkillPanel.onePageButtonsNum * page + 1; skillIndex <= SkillPanel.onePageButtonsNum * (page + 1); skillIndex++) {
			if (skillIndex > skillList.Count
				|| selectedUnit.GetCurrentActivityPoint () < selectedUnit.GetActualRequireSkillAP (skillList [skillIndex - 1])
				|| selectedUnit.GetUsedSkillDict ().ContainsKey (skillList [skillIndex - 1].GetName ()))
				skillPanel.OnOffSkillButton (skillIndex, true);
			else
				skillPanel.OnOffSkillButton (skillIndex, false);
		}
	}

	public void DisableSkillUI()
	{
		skillUI.SetActive(false);
	}

	public void SetSkillCheckAP(Casting casting)
	{
		skillCheckUI.gameObject.SetActive(true);
		int requireAP = casting.RequireAP;
		string newAPText = "소모 AP : " + requireAP + "\n" +
			"잔여 AP : " + (casting.Caster.GetCurrentActivityPoint() - requireAP);
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

	public void UpdateUnitViewer(Unit unitOnTile){
		unitViewerUI.SetActive(true);
		unitViewerUI.GetComponent<UnitViewer>().UpdateUnitViewer(unitOnTile);
	}

	public bool IsUnitViewerShowing()
	{
		return unitViewerUI.activeInHierarchy;
	}

	public void DisableUnitViewer() {
        unitViewerUI.GetComponent<UnitViewer>().RefreshStatusEffectIconList(); ;
        unitViewerUI.SetActive(false);
	}

	public void SetSelectedUnitViewerUI(Unit selectedUnit)
	{
		if (selectedUnit == null)
			return;
		selectedUnitViewerUI.SetActive(true);
		selectedUnitViewerUI.GetComponent<UnitViewer>().UpdateUnitViewer(selectedUnit);
	}

	public void DisableSelectedUnitViewerUI() {
        unitViewerUI.GetComponent<UnitViewer>().RefreshStatusEffectIconList();
        selectedUnitViewerUI.SetActive(false);
	}

    public void ActivateStatusEffectDisplayPanelAndSetText(Vector3 displacement, StatusEffect statusEffect) {
        statusEffectDisplayPanel.SetActive(true);
        RectTransform panelRect = statusEffectDisplayPanel.GetComponent<RectTransform>();
        statusEffectDisplayPanel.transform.position = displacement + 
                    new Vector3(panelRect.sizeDelta.x * panelRect.lossyScale.x / 2 + StatusEffectIcon.WIDTH/2,
                                 panelRect.sizeDelta.y * panelRect.lossyScale.y / 2 +   StatusEffectIcon.HEIGHT/2, 0);
        statusEffectDisplayPanel.GetComponent<StatusEffectDisplayPanel>().SetText(statusEffect);
    }

    public void DisableStatusEffectDisplayPanel() {
        statusEffectDisplayPanel.transform.position = originalStatusEffectDisplayPanelPosition;
        statusEffectDisplayPanel.SetActive(false);
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

	public void HideSkillNamePanelUI()
	{
		skillNamePanelUI.GetComponent<SkillNamePanel>().Hide();
	}

	public void SetSkillNamePanelUI(string skillName)
	{
		skillNamePanelUI.GetComponent<SkillNamePanel>().Set(skillName);
	}

	public void SetMovedUICanvasOnUnitAsCenter(Unit unit)
	{
		SetMovedUICanvasOnObjectAsCenter (unit);
	}
	public void SetMovedUICanvasOnTileAsCenter(Tile tile)
	{
		SetMovedUICanvasOnObjectAsCenter (tile);
	}
	private void SetMovedUICanvasOnObjectAsCenter(MonoBehaviour obj)
	{
		if (obj == null)
			return;
		Vector2 position = (Vector2)obj.gameObject.transform.position;
		SetMovedUICanvasOnCenter (position);
	}
	private void SetMovedUICanvasOnCenter(Vector2 position)
	{
		Vector3 newPosition = (new Vector3(position.x, position.y, -8));
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
