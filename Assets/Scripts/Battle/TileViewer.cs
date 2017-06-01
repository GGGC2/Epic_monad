using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Enums;

public class TileViewer : MonoBehaviour {

	Image tileImage;
	Text nameText;
	Text apText;
	Image elementImage;

	public void UpdateTileViewer(Tile tile)
	{
		if (tileImage == null) {
			
			Debug.LogError("TileImage is not exist : " + gameObject.GetInstanceID());
		}
		tileImage.sprite = tile.gameObject.GetComponent<SpriteRenderer>().sprite;
		nameText.text = tile.GetTileName();
	    int requiredAP = tile.GetRequireAPAtTile();
	    int unclimbableAP = 200;
	    if (requiredAP < unclimbableAP)
	    {
	        apText.text = "AP " + requiredAP;
	    }
	    else
	    {
	        apText.text = "-";
	    }

        SetElementImage(tile.GetTileElement());
	}

	void SetElementImage(Element element)
	{
		if (element == Element.Fire)
			elementImage.sprite = Resources.Load("Icon/Element/fire", typeof(Sprite)) as Sprite;
		else if (element == Element.Water)
			elementImage.sprite = Resources.Load("Icon/Element/water", typeof(Sprite)) as Sprite;
		else if (element == Element.Plant)
			elementImage.sprite = Resources.Load("Icon/Element/plant", typeof(Sprite)) as Sprite;
		else if (element == Element.Metal)
			elementImage.sprite = Resources.Load("Icon/Element/metal", typeof(Sprite)) as Sprite;
		else
			elementImage.sprite = Resources.Load("Icon/transparent", typeof(Sprite)) as Sprite;
	}

	void Awake () {
		tileImage = transform.Find("TileImage").GetComponent<Image>();
		nameText = transform.Find("NameText").GetComponent<Text>();
		apText = transform.Find("APText").GetComponent<Text>();
		elementImage = transform.Find("ElementImage").GetComponent<Image>();	
	}

	// Use this for initialization
	void Start () {
	   
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
