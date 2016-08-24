using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

using Enums;
using Battle.Feature;

public class Tile : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler {

	public TileForm form;
	public Element element;
	Vector2 position;
	GameObject unitOnTile = null;
	public SpriteRenderer sprite;
	public bool isHighlight;
	public List<Color> colors;

	bool isPreSeleted = false;

	public void SetPreSelected(bool input)
	{
		isPreSeleted = input;
	}

	public void SetTilePos(int x, int y)
	{
		position = new Vector2(x, y);
	}

	public Vector2 GetTilePos()
	{
		return position;
	}

	public void SetTileInfo(TileForm form, Element element)
	{
		string imageName = form.ToString() + "_" + element.ToString();
		string imagePath = "TileImage/" + imageName;
		GetComponent<SpriteRenderer>().sprite = Resources.Load(imagePath, typeof(Sprite)) as Sprite;

		SetTileForm(form);
		SetTileElement(element);
	}

	public void SetTileForm(TileForm form)
	{
		this.form = form;
	}

	public void SetTileElement(Element element)
	{
		this.element = element;
	}

	public TileForm GetTileForm()
	{
		return form;
	}

	public Element GetTileElement()
	{
		return element;
	}

	public int GetRequireAPAtTile()
	{
		return GetRequireAPFromTileType(form);
	}

	public bool IsUnitOnTile ()
	{
		return !(unitOnTile == null);
	}

	public void SetUnitOnTile(GameObject unit)
	{
		unitOnTile = unit;
	}

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


	public GameObject GetUnitOnTile ()
	{
		return unitOnTile;
	}

	public string GetTileName()
	{
		if (form == TileForm.Flatland)
			return "평지";
		else if (form == TileForm.Hill)
			return "언덕";
		else
			return "";
	}

	int GetRequireAPFromTileType(TileForm type)
	{
		if (type == TileForm.Flatland)
		{
			// USING ONLY TEST
			return EditInfo.RequireApAtFlatland;
			// return 3;
		}
		else if (type == TileForm.Hill)
		{
			// USING ONLY TEST
			return EditInfo.RequireApAtHill;
			// return 5;
		}
		else if (type == TileForm.Water)
		{
			return 9999;
		}
		else if (type == TileForm.HigherHill)
		{
			return EditInfo.RequireApAtHigherHill;
		}
		else if (type == TileForm.Cliff)
		{
			return 9999;
		}
		else
		{
			Debug.Log("Invaild tiletype : " + type.ToString());
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
			ColorChainTilesByUnit.Show(unitOnTile.GetComponent<Unit>());

			List<Unit> unitsTargetThisTile = battleData.GetUnitsTargetThisTile(this);
			foreach (Unit unit in unitsTargetThisTile)
			{
				unit.ShowChainIcon();
			}

			if (battleManager.IsLeftClicked()) return;

			FindObjectOfType<UIManager>().UpdateUnitViewer(unitOnTile);
		}

		FindObjectOfType<UIManager>().SetTileViewer(gameObject);

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
			ColorChainTilesByUnit.Hide(unitOnTile.GetComponent<Unit>());
		}

		BattleManager battleManager = FindObjectOfType<BattleManager>();
		BattleData battleData = battleManager.battleData;
		List<Unit> unitsTargetThisTile = battleData.GetUnitsTargetThisTile(this);
		foreach (Unit unit in unitsTargetThisTile)
		{
			unit.HideChainIcon();
		}

		if (battleManager.IsLeftClicked()) return;
		FindObjectOfType<UIManager>().DisableUnitViewer();

		if (isPreSeleted)
		{
			battleManager.OnMouseExitHandlerFromTile(position);
		}
	}

	void IPointerDownHandler.OnPointerDown(PointerEventData pointerData)
	{
		BattleManager gameManager = FindObjectOfType<BattleManager>();
		if ((isPreSeleted) && (gameManager != null))
		{
			gameManager.OnMouseDownHandlerFromTile(position);
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
}
