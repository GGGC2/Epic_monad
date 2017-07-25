using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Enums;

public class TileViewer : MonoBehaviour {

	Image tileImage;
	Text nameText;
	Text apText;
	Image elementImage;
    Text statusEffectText;

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
        UpdateEffect(tile);
	}

    void UpdateEffect(Tile tile) {
        List<TileStatusEffect> effectList = tile.GetStatusEffectList();
        // Debug.Log(unit.GetName() + " has " + effectList.Count + " se");
        int numberOfEffects = effectList.Count;
        string concattedText = "";
        for (int i = 0; i < numberOfEffects; i++) {
            concattedText += effectList[i].GetDisplayName();
            if (effectList[i].GetIsStackable())
                concattedText += "[" + effectList[i].GetRemainStack() + "]";
            if (effectList[i].GetRemainPhase() < 500)
                concattedText += "(" + effectList[i].GetRemainPhase() + ")";
            else
                concattedText += "(--)";
            for (int j = 0; j < effectList[i].fixedElem.actuals.Count; j++) {
                concattedText += (int)effectList[i].GetAmount(j);
                if (j < effectList[i].fixedElem.actuals.Count - 1) concattedText += ",";
            }
            if (i < numberOfEffects - 1)
                concattedText += " ";
        }
        statusEffectText.text = concattedText;
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
        statusEffectText = transform.Find("statusEffects").GetComponent<Text>();
    }

	// Use this for initialization
	void Start () {
	   
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
