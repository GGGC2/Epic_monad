using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

using Save;
using WorldMap;

public class TitleForNexon : MonoBehaviour
{
	public DOTweenPath titlePath;
	public DOTweenAnimation titleAnimation;

	public List<Button> titleButtons;
	public List<DOTweenAnimation> buttonAnimations;

	public List<Button> worldMapButtons;
	public List<DOTweenAnimation> worldMapButtonAnimations;

	public void Awake()
	{
		foreach (var button in worldMapButtons)
		{
			button.enabled = false;
		}
	}

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

		ShowWorldMap();

		// SceneLoader sceneLoader = FindObjectOfType<SceneLoader>();
		// sceneLoader.LoadNextDialogueScene(saveData.progress.dialogue);
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

		ShowWorldMap();

		// SceneLoader sceneLoader = FindObjectOfType<SceneLoader>();
		// sceneLoader.LoadNextDialogueScene(saveData.progress.dialogue);
	}

	private void ShowWorldMap()
	{
		titlePath.DOPlay();
		titleAnimation.DOPlay();
		foreach (var animation in buttonAnimations)
		{
			animation.DOPlay();
		}
		foreach (var button in titleButtons)
		{
			button.enabled = false;
		}

		foreach (var animation in worldMapButtonAnimations)
		{
			animation.DOPlay();
		}
		foreach (var button in worldMapButtons)
		{
			button.enabled = true;
		}

		GetComponent<WorldMapManager>().enabled = true;
	}
}
