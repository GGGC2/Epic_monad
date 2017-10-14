using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Enums;
using Battle.Skills;

public class Tile : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler {
	public Element element;
	public int APAtStandardHeight;
	public int height;
	string displayName;
	public Vector2 position;
	public Vector3 realPosition {
		get { return transform.position; }
	}
	Unit unitOnTile = null;
	public SpriteRenderer sprite;
	public GameObject highlightWall;
	public bool IsReachPoint{
		get{
			return highlightWall.activeSelf && BattleData.tutorialManager == null;
		}
	}
	public bool isMouseOver;
	public List<Color> colors;
    List<TileStatusEffect> statusEffectList = new List<TileStatusEffect>();
	bool isPreSeleted = false;
	public TextMesh CostAP;
	public SpriteRenderer tileColorSprite;
	public SpriteRenderer trapImage;

	public void SetPreSelected(bool input) {isPreSeleted = input;}

	public void SetTilePos(int x, int y) {position = new Vector2(x, y);}

	public Vector2 GetTilePos() {return position;}

	public void SetTileInfo(Element element, int typeIndex, int APAtStandardHeight, int height, string displayName){
		string typeIndexString = typeIndex.ToString();
		if (typeIndex < 10)
			typeIndexString = "0" + typeIndexString;

		string imageName = element.ToString() + "_" + typeIndexString;
		string imagePath = "TileImage/" + imageName;
		GetComponent<SpriteRenderer>().sprite = Resources.Load(imagePath, typeof(Sprite)) as Sprite;

		SetTileAPAtStandardHeight(APAtStandardHeight);
		SetTileHeight(height);
		SetTileElement(element);
		SetDisplayName (displayName);
	}

    public List<TileStatusEffect> StatusEffectList { get { return statusEffectList; } }
    public List<TileStatusEffect> GetStatusEffectList() { return statusEffectList; }
    public void SetStatusEffectList(List<TileStatusEffect> newStatusEffectList) { statusEffectList = newStatusEffectList; }
    public void SetTileAPAtStandardHeight(int APAtStandardHeight)	{	this.APAtStandardHeight = APAtStandardHeight;	}

	public void SetTileHeight(int height)	{	this.height = height;	}

	public void SetTileElement(Element element)	{	this.element = element;	}

	public void SetDisplayName(string displayName)	{	this.displayName = displayName;	}

	public int GetTileAPAtStandardHeight()	{	return APAtStandardHeight;	}

	public int GetHeight()	{	return height;	}

	public Element GetTileElement()	{	return element;	}

	public int GetBaseMoveCost()	{	return APAtStandardHeight;	}

	public bool IsUnitOnTile ()	{	return !(unitOnTile == null);	}

	public void SetUnitOnTile(Unit unit)	{	unitOnTile = unit;	}
    
    public void RemoveStatusEffect(TileStatusEffect statusEffect) {
        bool toBeRemoved = true;
        Skill originSkill = statusEffect.GetOriginSkill();
        if (originSkill is ActiveSkill)
            toBeRemoved = ((ActiveSkill)originSkill).SkillLogic.TriggerTileStatusEffectRemoved(this, statusEffect);
        if (toBeRemoved) {
            LogManager.Instance.Record(new StatusEffectLog(statusEffect, StatusEffectChangeType.Remove, 0, 0, 0));
            // statusEffectList = statusEffectList.FindAll(se => se != statusEffect);
        }
    }
    public void UpdateRemainPhaseAtPhaseEnd() {
        foreach (var statusEffect in statusEffectList) {
            if (!statusEffect.GetIsInfinite())  statusEffect.flexibleElem.display.remainPhase--;
            if (statusEffect.GetRemainPhase() <= 0) {
                RemoveStatusEffect(statusEffect);
            }
        }
    }
    public Unit GetUnitOnTile (){
		return unitOnTile;
	}

	public string GetTileName(){
		return displayName;
	}

	void IPointerEnterHandler.OnPointerEnter(PointerEventData pointerData){
		OnMouseOver ();

		if (isPreSeleted)
			TileManager.Instance.preSelectedMouseOverTile = this;

		BattleManager BM = BattleData.battleManager;
		UIManager UM = BattleData.uiManager;

		if (IsUnitOnTile()){
			ChainList.ShowChainOfThisUnit(unitOnTile);
			ChainList.ShowUnitsTargetingThisTile (this);

			if (!BM.EnemyUnitSelected()){
                UM.UpdateUnitViewer(unitOnTile);
				UM.apBarUI.FindAndHighlightPortrait(unitOnTile);
			}
		}
        if(!BM.TileSelected())
		    UM.SetTileViewer(this);

		if (isPreSeleted){
			BM.OnMouseEnterHandlerFromTile(position);
		}
	}
    public void UpdateRealPosition() {
        Vector3 positionBefore = transform.position;
        transform.position = TileManager.CalculateRealTilePosition((int)position.x, (int)position.y, height);
    }
    void IPointerExitHandler.OnPointerExit(PointerEventData pointerData){
		UpdateRealPosition();
		clickStarted = false;
		CostAP.text = "";

		OnMouseExit ();

		if (IsUnitOnTile()){
			ChainList.HideChainYellowDisplay ();
			ChainList.HideUnitsTargetingThisTile (this);
		}

		BattleManager BM = BattleData.battleManager;
		UIManager UM = BattleData.uiManager;

        if (!BM.TileSelected())
            UM.DisableTileViewerUI();
        if (!BM.EnemyUnitSelected()){
			UM.DisableUnitViewer();
			UM.apBarUI.ResetAllPortraitColor();
		}

		if (isPreSeleted){
			BM.OnMouseExitHandlerFromTile(position);
		}
	}
		
	float durationThreshold = 1.0f;
	bool clickStarted = false;
	float timeClickStarted;
	public UnityEvent LeftClickEnd;
	public UnityEvent LongLeftClickEnd;
	void IPointerDownHandler.OnPointerDown(PointerEventData pointerData){
		if (pointerData.button == PointerEventData.InputButton.Left) {
			clickStarted = true;
			timeClickStarted = Time.time;
		}
	}
	void IPointerUpHandler.OnPointerUp(PointerEventData pointerData){
		if (clickStarted && pointerData.button == PointerEventData.InputButton.Left){
			clickStarted = false;
			LeftClickEnd.Invoke ();
		}
	}
	void Update(){
		if (clickStarted && Time.time - timeClickStarted > durationThreshold) {
			clickStarted = false;
			LongLeftClickEnd.Invoke ();
		}

		if (trapImage.enabled == true && statusEffectList.Count == 0){
			trapImage.enabled = false;
		}
		else if (statusEffectList.Count == 0) return;
		else {
			if (statusEffectList.Any(se => se.GetDisplayName() == "잘근잘근 덫(활성화 대기)")){
				trapImage.enabled = true;
				trapImage.color = new Color(1,1,1,0.5f);
			}
			else if (statusEffectList.Any(se => se.GetDisplayName() == "잘근잘근 덫")){
				trapImage.enabled = true;
				trapImage.color = Color.white;
			}
		}
	}

	void Awake (){
		colors = new List<Color>();
		OnMouseExit ();
		InitializeEvents ();
	}

	void InitializeEvents(){
		BattleManager battleManager = BattleManager.Instance;
		UnityEngine.Events.UnityAction UserSelectTile= () => {
			Debug.Log("Tile(" + position.x + "," + position.y + ") PreSelected : " + isPreSeleted);
			if (isPreSeleted)
				battleManager.OnMouseDownHandlerFromTile(position);
		};
		UnityEngine.Events.UnityAction UserLongSelectTile= () => {
			if (isPreSeleted)
				battleManager.OnLongMouseDownHandlerFromTile (position);
		};
		LeftClickEnd.AddListener (UserSelectTile);
		LongLeftClickEnd.AddListener (UserLongSelectTile);
	}


	/* Tile painting related */
	void OnMouseOver(){
		isMouseOver = true;
		RenewColor ();
	}
	void OnMouseExit(){
		isMouseOver = false;
		RenewColor ();
	}
	public void SetHighlight(bool action){
		highlightWall.SetActive(action);
	}

	public void PaintTile(TileColor tileColor){
		Color color = TileColorToColor(tileColor);
		PaintTile(color);
	}
	void PaintTile(Color color) {
		colors.Add(color);
		RenewColor ();
	}
	public void DepaintTile(){
		colors = new List<Color> ();
		RenewColor ();
	}
	public void DepaintTile(TileColor tileColor){
		Color color = TileColorToColor(tileColor);
		DepaintTile(color);
	}
	void DepaintTile(Color color){
		colors.Remove(color);
		RenewColor ();
	}

	void RenewColor(){
		if (colors.Count == 0) tileColorSprite.enabled = false;
		else {
			tileColorSprite.enabled = true;
			tileColorSprite.color = mixColors (colors);
		} 
	}
	Color mixColors(List<Color> colors){
		Color color;
		if (colors.Count == 0)
			color = Color.white;
		else color = new Color(colors.Average(_color => _color.r),
						 colors.Average(_color => _color.g),
						 colors.Average(_color => _color.b),
						 colors.Average(_color => _color.a));
		if (isMouseOver)
			color -= new Color(0.3f, 0.3f, 0.3f, 0);
		return color;
	}
	Color TileColorToColor(TileColor color) {
		if (color == TileColor.Red)
			return new Color (1, 0.5f, 0.5f, 1);
		else if (color == TileColor.Blue)
			return new Color (0.6f, 0.6f, 1, 1);
		else if (color == TileColor.Yellow)
			return new Color (1, 0.9f, 0.016f, 1);
		else if (color == TileColor.Purple)
			return new Color (0.87f, 0.20f, 0.80f);
		else if (color == TileColor.Green)
			return new Color (0.65f, 0.177f, 0.147f);
		else if (color == TileColor.Black)
			return new Color (0, 0, 0);
		else
			throw new NotImplementedException(color.ToString() + " is not a supported color");
	}

	// override object.Equals
	public override bool Equals (object obj){		
		if (obj == null || GetType() != obj.GetType()) {return false;}
		
		Tile tileObj = (Tile)obj;
		if (this.GetTilePos() == tileObj.GetTilePos()) 
		return true;
		else return false;
	}
	
	// override object.GetHashCode
	public override int GetHashCode()
	{
		return (int)this.position.x * 1000 + (int)this.position.y;
	}
}
