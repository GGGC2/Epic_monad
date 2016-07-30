using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BattleEndChecker : MonoBehaviour {

	UnitManager unitManager;
	BattleData battleData;
	SceneLoader sceneLoader;

	public int maxPhase;
	bool isBattleEnd;

	// Use this for initialization
	void Start () {
		battleData = FindObjectOfType<BattleManager>().battleData;
		unitManager = battleData.unitManager;
		sceneLoader = FindObjectOfType<SceneLoader>();

		isBattleEnd = false;
		maxPhase = 5; // Using test.
	}
	
	// Update is called once per frame
	void Update () {
		CheckPushedButton();
		CheckRemainPhase();
	}

	void CheckPushedButton()
	{
		if (isBattleEnd) return;

		if (Input.GetKeyDown(KeyCode.S))
		{
			sceneLoader.LoadNextDialogueScene("pintos#2-3");
		}
	}

	void CheckRemainPhase()
	{	
		if (isBattleEnd) return;

		int currentPhase = battleData.currentPhase;

		if (currentPhase > maxPhase)
		{
			sceneLoader.LoadNextDialogueScene("pintos#2-3");
		}
	}
}
