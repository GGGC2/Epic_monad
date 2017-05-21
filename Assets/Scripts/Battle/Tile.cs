using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

using Enums;
using Battle.Feature;

public class Tile : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler {

	public Element element;
	public int APAtStandardHeight;
	public int height;
	Vector2 position;
	Unit unitOnTile = null;
	public SpriteRenderer sprite;
	public bool isHighlight;
	public List<Color> colors;

	public SpriteRenderer arrowRenderer;
	public List<Sprite> arrows;

	bool isPreSeleted = false;

	public void SetPreSelected(bool input)	{	isPreSeleted = input;	}

	public void SetTilePos(int x, int y)	{	position = new Vector2(x, y);	}

	public Vector2 GetTilePos()	{	return position;	}

	public void SetTileInfo(Element element, int typeIndex, int APAtStandardHeight, int height)
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
	}

	public void SetTileAPAtStandardHeight(int APAtStandardHeight)	{	this.APAtStandardHeight = APAtStandardHeight;	}

	public void SetTileHeight(int height)	{	this.height = height;	}

	public void SetTileElement(Element element)	{	this.element = element;	}

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


	public Unit GetUnitOnTile ()
	{
		return unitOnTile;
	}

	public string GetTileName()
	{
		if (element == Element.None)
			return "평지";
		else if (element == Element.Water)
			return "물";
		else if (element == Element.Plant)
			return "풀밭";
		else
			return "--";
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
	}

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
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
