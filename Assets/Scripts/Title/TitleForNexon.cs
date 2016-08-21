using UnityEngine;
using System.Collections.Generic;

using Save;

public class TitleForNexon : MonoBehaviour
{
	public void OnLowLevelClicked()
	{
		SaveDataCenter.Reset();
		SaveData saveData = SaveDataCenter.GetSaveData();

		saveData.party.partyLevel = 1;
		saveData.party.partyUnitNames = new List<string>
		{
			"reina",
			"lucius"
		};

		SceneLoader sceneLoader = FindObjectOfType<SceneLoader>();
		sceneLoader.LoadNextDialogueScene(saveData.progress.dialogue);
	}

	public void OnHighLevelClicked()
	{
		SaveDataCenter.Reset();
		SaveData saveData = SaveDataCenter.GetSaveData();

		saveData.progress.worldMap = "Pintos2";
		saveData.progress.dialogue = "Pintos#2-1";

		saveData.party.partyLevel = 1;
		saveData.party.partyUnitNames = new List<string>
		{
			"reina",
			"lucius"
		};

		SceneLoader sceneLoader = FindObjectOfType<SceneLoader>();
		sceneLoader.LoadNextDialogueScene(saveData.progress.dialogue);
	}
}
