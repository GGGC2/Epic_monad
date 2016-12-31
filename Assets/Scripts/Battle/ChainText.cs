using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq;

public class ChainText : MonoBehaviour {
	Text text;
	UnitManager unitManager;
	BattleManager battleManager;

	// Use this for initialization
	void Start () {
		text = gameObject.GetComponent<Text>();
		unitManager = FindObjectOfType<UnitManager>();
		battleManager = FindObjectOfType<BattleManager>();
	}

	// Update is called once per frame
	void Update () {
		string newText = "";

		string titleText = "[Chain List]\n";
		newText += titleText;
		foreach (var chainInfo in battleManager.GetChainList())
		{
			// 현재 턴인 유닛에게 강조표시.
			if (!chainInfo.GetTargetArea().Any(k => k.GetComponent<Tile>().IsUnitOnTile()))
				newText += chainInfo.GetUnit().GetComponent<Unit>().GetName() + " >> \n";
			else
			{
				string unitName = chainInfo.GetUnit().GetComponent<Unit>().GetName();
				foreach (var tileObject in chainInfo.GetTargetArea())
				{
					if (tileObject.GetComponent<Tile>().IsUnitOnTile())
					{
						newText += unitName + " >> " + tileObject.GetComponent<Tile>().GetUnitOnTile().GetComponent<Unit>().GetName() + "\n";
					}
				}
			}
		}
		text.text = newText;
	}
}
