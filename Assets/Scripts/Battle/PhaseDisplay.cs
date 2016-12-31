using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PhaseDisplay : MonoBehaviour {

	Text phaseText;
	Text standardAPText;
	UnitManager unitManager;
	BattleManager battleManager;
	BattleEndChecker battleEndChecker;

	// Use this for initialization
	void Start () {
		phaseText = transform.Find("PhaseText").GetComponent<Text>();
		standardAPText = transform.Find("StandardAPText").GetComponent<Text>();
		unitManager = FindObjectOfType<UnitManager>();
		battleManager = FindObjectOfType<BattleManager>();
		battleEndChecker = FindObjectOfType<BattleEndChecker>();
	}

	// Update is called once per frame
	void Update () {
		phaseText.text = "Phase\n" + FindObjectOfType<BattleManager>().GetCurrentPhase() + " / " + battleEndChecker.MaxPhase;
		// standardAPText.text = "Standard AP : " + unitManager.GetStandardActivityPoint() + "";
	}
}
