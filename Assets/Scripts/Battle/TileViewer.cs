using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Enums;

public class TileViewer : MonoBehaviour {

	Image tileImage;
	Text nameText;
	Text apText;
	Image elementImage;

	public void UpdateTileViewer(GameObject tile)
	{
		Tile tileInfo = tile.GetComponent<Tile>();
		if (tileImage == null) {
			
			Debug.LogError("TileImage is not exist : " + gameObject.GetInstanceID());
		}
		tileImage.sprite = tile.GetComponent<SpriteRenderer>().sprite;
		nameText.text = tileInfo.GetTileName();
	    int requiredAP = tileInfo.GetRequireAPAtTile();
	    int unclimbableAP = 200;
	    if (requiredAP < unclimbableAP)
	    {
	        apText.text = "AP " + requiredAP;
	    }
	    else
	    {
	        apText.text = "-";
	    }

        SetElementImage(tileInfo.GetTileElement());
	}

	void SetElementImage(Element element)
	{
		if (element == Element.Fire)
			elementImage.sprite = Resources.Load("Icon/fire", typeof(Sprite)) as Sprite;
		else if (element == Element.Water)
			elementImage.sprite = Resources.Load("Icon/water", typeof(Sprite)) as Sprite;
		else if (element == Element.Plant)
			elementImage.sprite = Resources.Load("Icon/plant", typeof(Sprite)) as Sprite;
		else if (element == Element.Metal)
			elementImage.sprite = Resources.Load("Icon/metal", typeof(Sprite)) as Sprite;
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
