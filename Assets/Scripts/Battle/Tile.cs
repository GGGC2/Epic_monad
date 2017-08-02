using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

using Enums;
using Battle.Feature;
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

	public List<GameObject> projectileDirectionArrows;

	bool isPreSeleted = false;

	public void SetPreSelected(bool input)	{	isPreSeleted = input;	}

	public void SetTilePos(int x, int y)	{	position = new Vector2(x, y);	}

	public Vector2 GetTilePos()	{	return position;	}

	public void SetTileInfo(Element element, int typeIndex, int APAtStandardHeight, int height, string displayName)
	{
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

	/* Tile painting related */
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

	public void PaintTile(Color color) {
		colors.Add(color);
	}

	public void DepaintTile(TileColor tileColor)
	{
		Color color = TileColorToColor(tileColor);
		DepaintTile(color);
	}

	public void DepaintTile(Color color)
	{
		colors.Remove(color);
	}

	public Color TileColorToColor(TileColor color) {
		if (color == TileColor.Red)
			return new Color(1, 0.5f, 0.5f, 1);
		else if (color == TileColor.Blue)
			return new Color(0.6f, 0.6f, 1, 1);
		else if (color == TileColor.Yellow)
			return new Color(1, 0.9f, 0.016f, 1);
		else
			throw new NotImplementedException(color.ToString() + " is not a supported color");
	}

	public void PaintProjectileArrow(Color color, Direction projectileDirection)
	{
		if (!projectileDirectionArrows[(int)projectileDirection -1].activeInHierarchy)
			projectileDirectionArrows[(int)projectileDirection -1].SetActive(true);
		projectileDirectionArrows[(int)projectileDirection -1].GetComponent<SpriteRenderer>().color = color;	
	}

	public void DepaintProjectileArrow()
	{
		projectileDirectionArrows.ForEach(arrow => arrow.SetActive(false));
	}
    
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
    public Unit GetUnitOnTile ()
	{
		return unitOnTile;
	}

	public string GetTileName()
	{
		return displayName;
	}

	int CalculatingRequireAPOfTile(int tileAPAtStandardHeight, int tileHeight)
	{
		if (tileAPAtStandardHeight >= 0 && tileHeight >= 0)
			return tileAPAtStandardHeight * (1 + tileHeight); // 임시 공식.
		else
		{
			Debug.Log("Invaild input. tileAP : " + tileAPAtStandardHeight + " height : " + tileHeight);
			return 1;
		}
	}

	void IPointerEnterHandler.OnPointerEnter(PointerEventData pointerData)
	{
		isHighlight = true;

		BattleManager battleManager = FindObjectOfType<BattleManager>();
		BattleData battleData = battleManager.battleData;
		if (IsUnitOnTile())
		{
			ColorChainTilesByUnit.Show(unitOnTile);

			List<Unit> unitsTargetThisTile = battleData.GetUnitsTargetThisTile(this);
			foreach (Unit unit in unitsTargetThisTile)
			{
				unit.ShowChainIcon();
			}

			if (battleManager.EnemyUnitSelected()) return;

			FindObjectOfType<UIManager>().UpdateUnitViewer(unitOnTile);
		}

		FindObjectOfType<UIManager>().SetTileViewer(this);

		if (isPreSeleted)
		{
			battleManager.OnMouseEnterHandlerFromTile(position);
		}
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData pointerData)
	{
		isHighlight = false;

		if(FindObjectOfType<UIManager>() != null)
			FindObjectOfType<UIManager>().DisableTileViewerUI();

		if (IsUnitOnTile())
		{
			ColorChainTilesByUnit.Hide(unitOnTile);
		}

		BattleManager battleManager = FindObjectOfType<BattleManager>();
		BattleData battleData = battleManager.battleData;
		List<Unit> unitsTargetThisTile = battleData.GetUnitsTargetThisTile(this);
		foreach (Unit unit in unitsTargetThisTile)
		{
			unit.HideChainIcon();
		}

		if (battleManager.EnemyUnitSelected()) return;
		FindObjectOfType<UIManager>().DisableUnitViewer();

		if (isPreSeleted)
		{
			battleManager.OnMouseExitHandlerFromTile(position);
		}
	}


	void IPointerDownHandler.OnPointerDown(PointerEventData pointerData)
	{
		if (pointerData.button != PointerEventData.InputButton.Left) {
			return;
		}

		BattleManager battleManager = FindObjectOfType<BattleManager>();
		if ((isPreSeleted) && (battleManager != null))
		{
			battleManager.OnMouseDownHandlerFromTile(position);
		}
	}

	void Awake ()
	{
		sprite = gameObject.GetComponent<SpriteRenderer>();
		isHighlight = false;
		colors = new List<Color>();
		projectileDirectionArrows.ForEach(arrow => arrow.SetActive(false));
	}

	void Update ()
	{
		sprite.color = mixColors(colors);

		if (isHighlight)
			sprite.color -= new Color(0.3f, 0.3f, 0.3f, 0);
	}

	Color mixColors(List<Color> colors)
	{
		if (colors.Count == 0){
			return Color.white;
		}

		return new Color(colors.Average(color => color.r),
						 colors.Average(color => color.g),
						 colors.Average(color => color.b),
						 colors.Average(color => color.a));
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
