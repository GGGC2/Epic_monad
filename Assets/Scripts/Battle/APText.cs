using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class APText : MonoBehaviour {

	Text text;
	UnitManager unitManager;
	BattleManager gameManager;

	// Use this for initialization
	void Start () {
		text = gameObject.GetComponent<Text>();
		unitManager = FindObjectOfType<UnitManager>();
		gameManager = FindObjectOfType<BattleManager>();
	}

	// Update is called once per frame
	void Update () {
		string newText = "";

		string phaseText = "[Phase " + FindObjectOfType<BattleManager>().GetCurrentPhase() + "]\n";
		newText += phaseText;
		string apText = "[Standard AP : " + unitManager.GetStandardActivityPoint() + "]\n";
		newText += apText;
		foreach (var unit in unitManager.GetAllUnits())
		{
			// 현재 턴인 유닛에게 강조표시.
			if (gameManager.GetSelectedUnit() == unit)
				newText += "> ";
			string unitText = unit.GetComponent<Unit>().name + " : " + unit.GetComponent<Unit>().GetCurrentActivityPoint() + "\n";
			newText += unitText;
		}
		text.text = newText;
	}
}
