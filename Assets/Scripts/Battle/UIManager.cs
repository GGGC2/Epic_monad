using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using BattleUI;
using Enums;

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
	GameObject placedUnitCheckUI;
    GameObject statusEffectDisplayPanel;
    Vector3 originalStatusEffectDisplayPanelPosition;
    GameObject logDisplayPanel;
    GameObject conditionPanel;
    GameObject configurationPanel;
    GameObject menuPanel;

	public GameObject chainBonusObj;
	public GameObject celestialBonusObj;
	public GameObject directionBonusObj;
	public GameObject heightBonusObj;

	GameObject notImplementedDebugPanel;
	List<ActionButton> actionButtons = new List<ActionButton>();

	void Awake(){
		instance = this;
		apBarUI = FindObjectOfType<APBarPanel>();
		for(int i = 0; i < 8; i++){
			actionButtons.Add(GameObject.Find("ActionButton"+i).GetComponent<ActionButton>());
		}
		unitViewerUI                            = GameObject.Find("UnitViewerPanel");
        statusEffectDisplayPanel                = GameObject.Find("StatusEffectDisplayPanel");
        conditionPanel                          = GameObject.Find("ConditionPanel");
        logDisplayPanel                         = GameObject.Find("LogDisplayPanel");
        configurationPanel                      = GameObject.Find("ConfigurationPanel");
        selectedUnitViewerUI                    = GameObject.Find("SelectedUnitViewerPanel");
		tileViewerUI                            = GameObject.Find("TileViewerPanel");
		selectDirectionUI                       = FindObjectOfType<SelectDirectionUI>();
		skillNamePanelUI                        = GameObject.Find("SkillNamePanel");
		movedUICanvas                           = GameObject.Find("MovingUICanvas");
		phaseUI                                 = GameObject.Find("PhasePanel");
		detailInfoUI                            = FindObjectOfType<DetailInfoPanel>();
		placedUnitCheckUI                       = GameObject.Find("PlacedUnitCheckPanel");
		notImplementedDebugPanel                = GameObject.Find("NotImplementedDebugPanel");
        menuPanel                               = GameObject.Find("MenuPanel");
        selectDirectionUI.Initialize();
        TutorialScenario.selectDirectionUI = selectDirectionUI;
	}

	void Start(){
		skillViewer.gameObject.SetActive(false);
		unitViewerUI.SetActive(false);
        statusEffectDisplayPanel.SetActive(false);
        configurationPanel.SetActive(false);
        logDisplayPanel.SetActive(false);
        selectedUnitViewerUI.SetActive(false);
		tileViewerUI.SetActive(false);
		selectDirectionUI.gameObject.SetActive(false);
		detailInfoUI.gameObject.SetActive(false);
		placedUnitCheckUI.SetActive(false);
		apBarUI.MoveOverScreen(false);
        menuPanel.SetActive(false);

        originalStatusEffectDisplayPanelPosition = statusEffectDisplayPanel.transform.position;
		startFinished = true;
		DeactivateAllBonusText();
	}

	void Update(){
		if(Setting.shortcutEnable){
			if(Input.GetKeyDown(KeyCode.Alpha1)){
			StartCoroutine(actionButtons[0].OnClick());
			}if(Input.GetKeyDown(KeyCode.Alpha2)){
				StartCoroutine(actionButtons[1].OnClick());
			}if(Input.GetKeyDown(KeyCode.Alpha3)){
				StartCoroutine(actionButtons[2].OnClick());
			}if(Input.GetKeyDown(KeyCode.Alpha4)){
				StartCoroutine(actionButtons[3].OnClick());
			}if(Input.GetKeyDown(KeyCode.Alpha5)){
				StartCoroutine(actionButtons[4].OnClick());
			}if(Input.GetKeyDown(KeyCode.Alpha6)){
				StartCoroutine(actionButtons[5].OnClick());
			}if(Input.GetKeyDown(KeyCode.Alpha7)){
				StartCoroutine(actionButtons[6].OnClick());
			}if(Input.GetKeyDown(KeyCode.Alpha8)){
				StartCoroutine(actionButtons[7].OnClick());
			}if(Input.GetKeyDown(KeyCode.Q)){
				StartCoroutine(actionButtons[BattleData.turnUnit.activeSkillList.Count].OnClick());
			}
		}
	}

	public void UpdateApBarUI() {
		if (BattleData.readiedUnits.Count != 0) {
			apBarUI.UpdateAPDisplay(BattleData.unitManager.GetAllUnits());	
			apBarUI.MoveOverScreen(true);
		}
	}

	//전술 보너스 텍스트 표시
	public void PrintDirectionBonus(DirectionCategory directionCategory, float bonus){
		directionBonusObj.SetActive (true);
		if (directionCategory == DirectionCategory.Side)
            directionBonusObj.GetComponentInChildren<Text>().text = "측면 공격 (x" + bonus + ")";
		else if (directionCategory == DirectionCategory.Back)
            directionBonusObj.GetComponentInChildren<Text>().text = "후면 공격 (x" + bonus + ")";
    }
	public void PrintCelestialBonus(float bonus){
        celestialBonusObj.SetActive(true);
        string text = "천체속성 (x" + bonus + ")";
        celestialBonusObj.GetComponentInChildren<Text>().text = text;
		// Invoke("ActiveFalseAtDelay", 0.5f);
	}
	public void PrintHeightBonus(float bonus){
		heightBonusObj.SetActive(true);
        string text = "고저차 (x" + bonus + ")";
        heightBonusObj.GetComponentInChildren<Text>().text = text;
    }
	public void PrintChainBonus(int chainCount){
		float chainBonus;

		if (chainCount < 2)	chainBonus = 1.0f;
		else if (chainCount == 2) chainBonus = 1.2f;
		else if (chainCount == 3) chainBonus = 1.5f;
		else if (chainCount == 4) chainBonus = 2.0f;
		else chainBonus = 3.0f;

		if (chainCount < 2)	return;

		chainBonusObj.SetActive(true);
		string text = "연계" + chainCount + "단 (x" + chainBonus + ")";
        chainBonusObj.GetComponentInChildren<Text>().text = text;
    }
	public void DeactivateAllBonusText(){
		celestialBonusObj.SetActive(false);
		chainBonusObj.SetActive(false);
		directionBonusObj.SetActive(false);
		heightBonusObj.SetActive(false);
    }

	public void TurnOnOnlyOneAction(int skillIndex){
		for (int i = 0; i < 8; i++){
			actionButtons [i].Activate (i == skillIndex);
		}
	}
	public void TurnOffAllActions(){
		for (int i = 0; i < 8; i++) {
			actionButtons [i].Activate (false);
		}
	}

	public bool ActionButtonOnOffLock = false;

	public void ControlListenerOfActionButton(int i, bool onOff, UnityAction action){
		if(onOff){
			actionButtons[i].clicked.AddListener(action);
		}else{
			actionButtons[i].clicked.RemoveListener(action);
		}
	}

	private void EnableSkillUI(){
		skillViewer.gameObject.SetActive(true);
		foreach (Transform transform in skillViewer.transform){
			transform.gameObject.SetActive(true);
		}
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
    public Unit GetUnitInUnitViewer() {
        return unitViewerUI.GetComponent<UnitViewer>().GetUnit();
    }
    public bool IsTileViewerShowing() {
        return tileViewerUI.activeInHierarchy;
    }
    public Tile GetTileInTileViewer() {
        return tileViewerUI.GetComponent<TileViewer>().GetTile();
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

    public void ActivateConditionPanel() {
        conditionPanel.GetComponent<ConditionPanel>().Initialize(BattleTriggerManager.Instance.triggers);
        conditionPanel.SetActive(true);
    }

    public void ActivateLogDisplayPanel() {
        logDisplayPanel.SetActive(true);
        logDisplayPanel.GetComponent<LogDisplayPanel>().Initialize();
    }

    public void InActiveLogDisplayPanel() {
        logDisplayPanel.SetActive(false);
    }

    public void ToggleMenuPanelActive() {
        menuPanel.SetActive(!menuPanel.activeSelf);
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

	public void EnablePlacedUnitCheckUI() {
		placedUnitCheckUI.SetActive(true);
		ReadyManager RM = FindObjectOfType<ReadyManager>();
		placedUnitCheckUI.GetComponent<PlacedUnitCheckPanel>().SetUnitPortraits(RM.selectedUnits);
	}

	public void DisablePlacedUnitCheckUI() {
		placedUnitCheckUI.SetActive(false);
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

    public Vector2 GetActionButtonPosition(int i) {
        return actionButtons[i].transform.position;
    }

	public void SetActionButtons(){
		skillViewer.gameObject.SetActive(false);
		Unit unit = BattleData.turnUnit;
		for (int i = 0; i < 8; i++) {
			// actionButtons [i].icon.sprite = Resources.Load<Sprite> ("transparent");
			actionButtons [i].Absent();
			// actionButtons [i].skill = null;
			if (i < unit.activeSkillList.Count) {
				actionButtons [i].InitializeWithSkill (unit.activeSkillList [i]);
				actionButtons [i].Activate (unit.IsThisSkillUsable (unit.activeSkillList [i]));
			} else if (i == unit.activeSkillList.Count) {
				if (unit.IsStandbyPossible ()) {
					actionButtons [i].icon.sprite = Resources.Load<Sprite> ("Icon/Standby");
                    actionButtons [i].type = ActionButtonType.Standby;
				} else {
					actionButtons [i].icon.sprite = Resources.Load<Sprite> ("Icon/Rest");
                    actionButtons[i].type = ActionButtonType.Rest;
                }
				actionButtons [i].Activate (true);
			}
		}
	}
    public void AddCollectableActionButton() {
        Unit unit = BattleData.turnUnit;
        ActionButton button = actionButtons[unit.activeSkillList.Count + 1];
        button.type = ActionButtonType.Collect;
        button.Activate(true);
    }

	public void HideActionButtons(){
		actionButtons.ForEach(button => button.Absent());
	}
}