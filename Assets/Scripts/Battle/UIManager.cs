using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

using BattleUI;

public class UIManager : MonoBehaviour{
	private static UIManager instance;
	public static UIManager Instance{
		get { return instance; }
	}

	public bool startFinished = false;

	public APBarPanel apBarUI;
	public SkillViewer skillViewer;
	GameObject unitViewerUI;
	public GameObject selectedUnitViewerUI;
	GameObject tileViewerUI;
	SelectDirectionUI selectDirectionUI;
	GameObject skillNamePanelUI;
	GameObject movedUICanvas;
	GameObject phaseUI;
	DetailInfoPanel detailInfoUI;
    GameObject statusEffectDisplayPanel;
    Vector3 originalStatusEffectDisplayPanelPosition;

	GameObject notImplementedDebugPanel;
	List<ActionButton> actionButtons = new List<ActionButton>();

	void Awake(){
		instance = this;
		apBarUI = FindObjectOfType<APBarPanel>();
		skillViewer = FindObjectOfType<SkillViewer>();
		for(int i = 0; i < 8; i++){
			actionButtons.Add(GameObject.Find("ActionButton"+i).GetComponent<ActionButton>());
		}
		unitViewerUI = GameObject.Find("UnitViewerPanel");
        statusEffectDisplayPanel = GameObject.Find("StatusEffectDisplayPanel");
        selectedUnitViewerUI = GameObject.Find("SelectedUnitViewerPanel");
		tileViewerUI = GameObject.Find("TileViewerPanel");
		selectDirectionUI = FindObjectOfType<SelectDirectionUI>();
		selectDirectionUI.Initialize ();
		skillNamePanelUI = GameObject.Find("SkillNamePanel");
		movedUICanvas = GameObject.Find("MovingUICanvas");
		phaseUI = GameObject.Find("PhasePanel");
		detailInfoUI = FindObjectOfType<DetailInfoPanel>();
		notImplementedDebugPanel = GameObject.Find("NotImplementedDebugPanel");
		TutorialScenario.selectDirectionUI = selectDirectionUI;
	}

	void Start(){
		skillViewer.gameObject.SetActive(false);
		unitViewerUI.SetActive(false);
        statusEffectDisplayPanel.SetActive(false);
        selectedUnitViewerUI.SetActive(false);
		tileViewerUI.SetActive(false);
		selectDirectionUI.gameObject.SetActive(false);
		detailInfoUI.gameObject.SetActive(false);

        originalStatusEffectDisplayPanelPosition = statusEffectDisplayPanel.transform.position;
		startFinished = true;
	}

	public void UpdateApBarUI() {
		if (BattleData.readiedUnits.Count != 0) {
			apBarUI.gameObject.SetActive(true);
			apBarUI.UpdateAPDisplay(FindObjectOfType<UnitManager>().GetAllUnits());	
		}
	}

	private void EnableSkillUI(){
		skillViewer.gameObject.SetActive(true);
		foreach (Transform transform in skillViewer.transform){
			transform.gameObject.SetActive(true);
		}
	}
    public void SetSkillUI(Unit selectedUnit) {
        List<ActiveSkill> skillList = selectedUnit.GetLearnedSkillList();
        //skillPanel.SetMaxPage((skillList.Count - 1) / SkillPanel.onePageButtonsNum);
        //skillPanel.triggerEnabled(selectedUnit);
        EnableSkillUI();
        
        UpdateSkillInfo(selectedUnit);
    }
    public void UpdateSkillInfo(Unit selectedUnit) {
        List<ActiveSkill> skillList = selectedUnit.GetLearnedSkillList();
        for (int i = 0; i < SkillPanel.onePageButtonsNum; i++){
			/*int skillIndex = i + SkillPanel.onePageButtonsNum * skillPanel.GetPage();
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
				skillButton.transform.Find("CooldownText").GetComponent<Text>().text = "";*/
		}
        TurnOnOnlyUsableSkills(selectedUnit);
	}

	public void TurnOnOnlyUsableSkills(Unit selectedUnit){
		List<ActiveSkill> skillList = selectedUnit.GetLearnedSkillList();
		/*int page = skillPanel.GetPage();

		for (int skillIndex = SkillPanel.onePageButtonsNum * page + 1; skillIndex <= SkillPanel.onePageButtonsNum * (page + 1); skillIndex++) {
			if (skillIndex > skillList.Count
				|| selectedUnit.GetCurrentActivityPoint () < selectedUnit.GetActualRequireSkillAP (skillList [skillIndex - 1])
				|| selectedUnit.GetUsedSkillDict ().ContainsKey (skillList [skillIndex - 1].GetName ()))
				skillPanel.OnOffSkillButton (skillIndex, false);
			else
				skillPanel.OnOffSkillButton (skillIndex, true);
		}*/
	}

	public void DisableSkillUI(){
		skillViewer.gameObject.SetActive(false);
	}

	public IEnumerator MovePhaseUI(int currentPhase){
		Image img1 = phaseUI.GetComponent<Image>();
		Image img2 = phaseUI.transform.Find("AdditionalPanel").gameObject.GetComponent<Image>();

		phaseUI.transform.localPosition = new Vector3(-1280,0,0);
		phaseUI.transform.Find("Text").GetComponent<Text>().text = currentPhase + " 페 이 즈";
		img1.DOFade(1, 0.5f);
		img2.DOFade(1, 0.5f);
		iTween.MoveTo(phaseUI, iTween.Hash("position", new Vector3(0,0,0), "islocal", true, "time", 1));
		yield return new WaitForSeconds(2f);
		img1.DOFade(0, 0.5f);
		img2.DOFade(0, 0.5f);
		iTween.MoveTo(phaseUI, iTween.Hash("position", new Vector3(1280,0,0), "islocal", true, "time", 1));
		yield return null;
	}

	public void UpdateUnitViewer(Unit unitOnTile){
		unitViewerUI.SetActive(true);
		unitViewerUI.GetComponent<UnitViewer>().UpdateUnitViewer(unitOnTile);
	}

	public bool IsUnitViewerShowing() {
		return unitViewerUI.activeInHierarchy;
	}
    public bool IsTileViewerShowing() {
        return tileViewerUI.activeInHierarchy;
    }
	public void DisableUnitViewer() {
        unitViewerUI.GetComponent<UnitViewer>().RefreshStatusEffectIconList(); ;
        unitViewerUI.SetActive(false);
	}

	public void SetSelectedUnitViewerUI(Unit selectedUnit){
		if (selectedUnit == null){
			return;
		}
		selectedUnitViewerUI.SetActive(true);
		selectedUnitViewerUI.GetComponent<UnitViewer>().UpdateUnitViewer(selectedUnit);
	}
	public void UpdateSelectedUnitViewerUI(Unit selectedUnit){
		selectedUnitViewerUI.GetComponent<UnitViewer>().UpdateUnitViewer(selectedUnit);
	}

	public void DisableSelectedUnitViewerUI() {
        unitViewerUI.GetComponent<UnitViewer>().RefreshStatusEffectIconList();
        selectedUnitViewerUI.SetActive(false);
	}

    public void ActivateStatusEffectDisplayPanelAndSetText(Vector3 pivot, StatusEffect statusEffect) {
        statusEffectDisplayPanel.SetActive(true);
        statusEffectDisplayPanel.transform.position = CalculateStatusEffectDisplayPanelPosition(pivot);
        statusEffectDisplayPanel.GetComponent<StatusEffectDisplayPanel>().SetText(statusEffect);
    }

    public Vector3 CalculateStatusEffectDisplayPanelPosition(Vector3 pivot) {
        RectTransform panelRect = statusEffectDisplayPanel.GetComponent<RectTransform>();
        Vector3 displacement = pivot + new Vector3(panelRect.sizeDelta.x * panelRect.lossyScale.x / 2 + StatusEffectIcon.WIDTH / 2,
                                 panelRect.sizeDelta.y * panelRect.lossyScale.y / 2 + StatusEffectIcon.HEIGHT / 2, 0);
        if(displacement.x + panelRect.sizeDelta.x * panelRect.lossyScale.x/2 >= Screen.width) {
            displacement.x = Screen.width - panelRect.sizeDelta.x * panelRect.lossyScale.x/2;
        }
        return displacement;
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

	public void DisableTileViewerUI() {
        tileViewerUI.GetComponent<TileViewer>().RefreshStatusEffectIconList();
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

	public void HideSkillNamePanelUI()
	{
		skillNamePanelUI.GetComponent<SkillNamePanel>().Hide();
	}

	public void SetSkillNamePanelUI(string skillName){
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

	public UnityEvent activateDetailInfoEvent = new UnityEvent ();
	public UnityEvent deactivateDetailInfoEvent = new UnityEvent ();
	public void ActivateDetailInfoUI(Unit unit){
		activateDetailInfoEvent.Invoke ();
		detailInfoUI.gameObject.SetActive(true);
		detailInfoUI.unit = unit;
		detailInfoUI.Initialize();
	}
	public void DeactivateDetailInfoUI(){
		deactivateDetailInfoEvent.Invoke ();
		detailInfoUI.gameObject.SetActive (false);
	}
	public bool isDetailInfoUIActive(){
		return detailInfoUI.gameObject.activeSelf;
	}

	public void AppendNotImplementedLog(String text){
		if (notImplementedDebugPanel == null){
			Debug.LogError("Cannot find not implemented debug panel\n" + text);
			return;
		}

		var debugPanel = notImplementedDebugPanel.GetComponent<NotImplementedDebugPanel>();
		debugPanel.Append(text);
	}

	public void SetActionButtons(){
		skillViewer.gameObject.SetActive(false);
		Unit unit = BattleData.selectedUnit;
		for(int i = 0; i < 8; i++){
			actionButtons[i].icon.sprite = Resources.Load<Sprite>("transparent");
			actionButtons[i].skill = null;
			if(i < unit.activeSkillList.Count){
				actionButtons[i].Initialize(unit.activeSkillList[i]);
			}else if(i == unit.activeSkillList.Count){
				if(unit.IsStandbyPossible()){
					actionButtons[i].icon.sprite = Resources.Load<Sprite>("Icon/Standby");
				}else{
					actionButtons[i].icon.sprite = Resources.Load<Sprite>("Icon/Rest");
				}
			}
		}
	}

	public void HideActionButtons(){
		actionButtons.ForEach(button => button.icon.sprite = Resources.Load<Sprite>("transparent"));
	}
}