using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PhaseDisplay : MonoBehaviour {

	Text phaseText;
	Text standardAPText;
	UnitManager unitManager;
	BattleManager battleManager;

	void Start () {
		phaseText = transform.Find("PhaseText").GetComponent<Text>();
		standardAPText = transform.Find("StandardAPText").GetComponent<Text>();
		unitManager = FindObjectOfType<UnitManager>();
		battleManager = FindObjectOfType<BattleManager>();
	}

	void Update () {
		phaseText.text = "페이즈 " + battleManager.GetCurrentPhase();
		// standardAPText.text = "Standard AP : " + unitManager.GetStandardActivityPoint() + "";
	}
}
