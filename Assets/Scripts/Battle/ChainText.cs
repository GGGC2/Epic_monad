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
		/*
		string newText = "";

		string titleText = "[Chain List]\n";
		newText += titleText;
		foreach (var chain in battleManager.GetChainList())
		{
			// 현재 턴인 유닛에게 강조표시.
			if (!chain.GetSecondRange().Any(k => k.IsUnitOnTile()))
				newText += chain.Caster.GetName() + " >> \n";
			else
			{
				string unitName = chain.Caster.GetName();
				foreach (var tileObject in chain.GetSecondRange())
				{
					if (tileObject.GetComponent<Tile>().IsUnitOnTile())
					{
						newText += unitName + " >> " + tileObject.GetUnitOnTile().GetName() + "\n";
					}
				}
			}
		}
		text.text = newText;
		*/
	}
}
