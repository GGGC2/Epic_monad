using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

using Enums;
using Battle.Skills;

public class Tile : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler {
	public Element element;
	public int APAtStandardHeight;
	public int height;
	string displayName;
	public Vector2 position;
	Unit unitOnTile = null;
	public SpriteRenderer sprite;
	public bool isHighlight;
	public List<Color> colors;
    List<TileStatusEffect> statusEffectList = new List<TileStatusEffect>();

	bool isPreSeleted = false;

	public void SetPreSelected(bool input)	{	isPreSeleted = input;	}

	public void SetTilePos(int x, int y)	{	position = new Vector2(x, y);	}

	public Vector2 GetTilePos()	{	return position;	}

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

    public List<TileStatusEffect> GetStatusEffectList() { return statusEffectList; }
    public void SetStatusEffectList(List<TileStatusEffect> newStatusEffectList) { statusEffectList = newStatusEffectList; }
    public void SetTileAPAtStandardHeight(int APAtStandardHeight)	{	this.APAtStandardHeight = APAtStandardHeight;	}

	public void SetTileHeight(int height)	{	this.height = height;	}

	public void SetTileElement(Element element)	{	this.element = element;	}

	public void SetDisplayName(string displayName)	{	this.displayName = displayName;	}

	public int GetTileAPAtStandardHeight()	{	return APAtStandardHeight;	}

	public int GetTileHeight()	{	return height;	}

	public Element GetTileElement()	{	return element;	}

	public int GetRequireAPAtTile()	{	return APAtStandardHeight;	}

	public bool IsUnitOnTile ()	{	return !(unitOnTile == null);	}

	public void SetUnitOnTile(Unit unit)	{	unitOnTile = unit;	}
    
    public void RemoveStatusEffect(TileStatusEffect statusEffect) {
        bool toBeRemoved = true;
        ActiveSkill originSkill = statusEffect.GetOriginSkill();
        if (originSkill != null)
            toBeRemoved = SkillLogicFactory.Get(originSkill).TriggerTileStatusEffectRemoved(this, statusEffect);
        if (toBeRemoved) {
            Debug.Log(statusEffect.GetDisplayName() + " is removed from tile ( " + position.x + ", " + position.y + ")");
            statusEffectList = statusEffectList.FindAll(se => se != statusEffect);
        }
    }
    public void UpdateRemainPhaseAtPhaseEnd() {
        foreach (var statusEffect in statusEffectList) {
            if (!statusEffect.GetIsInfinite())
                statusEffect.DecreaseRemainPhase();
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

	int CalculatingRequireAPOfTile(int tileAPAtStandardHeight, int tileHeight){
		if (tileAPAtStandardHeight >= 0 && tileHeight >= 0)
			return tileAPAtStandardHeight * (1 + tileHeight); // 임시 공식.
		else{
			Debug.Log("Invaild input. tileAP : " + tileAPAtStandardHeight + " height : " + tileHeight);
			return 1;
		}
	}

	void IPointerEnterHandler.OnPointerEnter(PointerEventData pointerData){
		HighlightTile ();

		BattleManager battleManager = FindObjectOfType<BattleManager>();
		BattleData battleData = battleManager.battleData;
		if (IsUnitOnTile()){
			ChainList.ShowChainOfThisUnit(unitOnTile);
			ChainList.ShowUnitsTargetingThisTile (this);

			if (battleManager.EnemyUnitSelected()) return;

			FindObjectOfType<UIManager>().UpdateUnitViewer(unitOnTile);
		}

		FindObjectOfType<UIManager>().SetTileViewer(this);

		if (isPreSeleted)
			battleManager.OnMouseEnterHandlerFromTile(position);
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData pointerData){
		DehighlightTile ();

		if(FindObjectOfType<UIManager>() != null)
			FindObjectOfType<UIManager>().DisableTileViewerUI();

		if (IsUnitOnTile())
		{
			ChainList.HideChainYellowDisplay ();
			ChainList.HideUnitsTargetingThisTile (this);
		}

		BattleManager battleManager = FindObjectOfType<BattleManager>();
		if (battleManager.EnemyUnitSelected()) return;
		FindObjectOfType<UIManager>().DisableUnitViewer();

		if (isPreSeleted){
			battleManager.OnMouseExitHandlerFromTile(position);
		}
	}


	void IPointerDownHandler.OnPointerDown(PointerEventData pointerData){
		if (pointerData.button != PointerEventData.InputButton.Left)
			return;

		BattleManager battleManager = FindObjectOfType<BattleManager>();
		if(!battleManager.onTutorial){
			if ((isPreSeleted))
				battleManager.OnMouseDownHandlerFromTile(position);
		}else{
			TutorialScenario tutorial = battleManager.tutorialManager.currentScenario;
			if(tutorial.mission == TutorialScenario.Mission.SelectTile && tutorial.missionTilePos == position){
				battleManager.OnMouseDownHandlerFromTile(position);
				battleManager.tutorialManager.NextStep();
			}
		}
	}

	void Awake (){
		sprite = gameObject.GetComponent<SpriteRenderer>();
		colors = new List<Color>();
		DehighlightTile ();
	}


	/* Tile painting related */
	void HighlightTile(){
		isHighlight = true;
		RenewColor ();
	}
	void DehighlightTile(){
		isHighlight = false;
		RenewColor ();
	}

	public void PaintTile(TileColor tileColor)
	{
		Color color = TileColorToColor(tileColor);
		PaintTile(color);
	}
	public void PaintTile(TileColor tileColor, Direction projectileDirection)
	{
		Color color = TileColorToColor(tileColor);
		PaintTile(color);
	}
	void PaintTile(Color color) {
		colors.Add(color);
		RenewColor ();
	}

	public void DepaintTile(TileColor tileColor)
	{
		Color color = TileColorToColor(tileColor);
		DepaintTile(color);
	}
	void DepaintTile(Color color)
	{
		colors.Remove(color);
		RenewColor ();
	}

	void RenewColor(){
		sprite.color = mixColors (colors);
	}
	Color mixColors(List<Color> colors)
	{
		Color color;
		if (colors.Count == 0)
			color = Color.white;
		else color = new Color(colors.Average(_color => _color.r),
						 colors.Average(_color => _color.g),
						 colors.Average(_color => _color.b),
						 colors.Average(_color => _color.a));
		if (isHighlight)
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
	public override bool Equals (object obj)
	{		
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}
		
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
