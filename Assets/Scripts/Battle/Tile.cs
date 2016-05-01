using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using Enums;

public class Tile : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler {

	public TileForm form;
	public Element element;
	Vector2 position;
	GameObject unitOnTile = null;

	class TileColor
	{
		public SpriteRenderer sprite;
		public Color originalColor;
		public bool isHighlight;

		public TileColor(GameObject tile)
		{
			sprite = tile.GetComponent<SpriteRenderer>();
			originalColor = Color.white;
			isHighlight = false;
		}
	}

	TileColor tileColor;

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

	public void SetTileColor(Color color)
	{
		tileColor.originalColor = color;
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
		else
		{
			Debug.Log("Invaild tiletype : " + type.ToString());
			return 1;
		}
	}

	void IPointerEnterHandler.OnPointerEnter(PointerEventData pointerData)
	{
		tileColor.isHighlight = true;

		if (IsUnitOnTile())
		{
			if (FindObjectOfType<BattleManager>().IsLeftClicked()) return;

			FindObjectOfType<UIManager>().UpdateUnitViewer(unitOnTile);
		}

		FindObjectOfType<UIManager>().SetTileViewer(gameObject);
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData pointerData)
	{
		tileColor.isHighlight = false;

		FindObjectOfType<UIManager>().DisableTileViewerUI();

		if (FindObjectOfType<BattleManager>().IsLeftClicked()) return;
		FindObjectOfType<UIManager>().DisableUnitViewer();
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
		tileColor = new TileColor(gameObject);
	}

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {
		if (tileColor.isHighlight)
			tileColor.sprite.color = tileColor.originalColor - new Color(0.3f, 0.3f, 0.3f, 0);
		else
			tileColor.sprite.color = tileColor.originalColor;
	}
}
