using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class APText : MonoBehaviour {

	Text text;
	UnitManager unitManager;
	BattleManager battleManager;

	// Use this for initialization
	void Start () {
		text = gameObject.GetComponent<Text>();
		unitManager = BattleData.unitManager;
		battleManager = BattleData.battleManager;
	}

	// Update is called once per frame
	void Update () {
		string newText = "";

		string phaseText = "[Phase " + BattleData.battleManager.GetCurrentPhase() + "]\n";
		newText += phaseText;
		string apText = "[Standard AP : " + unitManager.GetStandardActivityPoint() + "]\n";
		newText += apText;
		foreach (var unit in unitManager.GetAllUnits())
		{
			// 현재 턴인 유닛에게 강조표시.
			if (battleManager.GetSelectedUnit() == unit)
				newText += "> ";
			string unitText = unit.name + " : " + unit.GetCurrentActivityPoint() + "\n";
			newText += unitText;
		}
		text.text = newText;
	}
}
