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
	}

	public void OnHighLevelClicked()
	{
		SaveDataCenter.Reset();
		SaveData saveData = SaveDataCenter.GetSaveData();

		saveData.progress.worldMap = "Pintos5";
		saveData.progress.dialogue = "Pintos#7-1";

		saveData.party.partyLevel = 18;
		saveData.party.partyUnitNames = new List<string>
		{
			"reina",
			"sisterna"
		};

		ShowWorldMap();
	}

	public void OnBossLevelClicked()
	{
		SaveDataCenter.Reset();
		SaveData saveData = SaveDataCenter.GetSaveData();

		saveData.progress.worldMap = "Pintos9";
		saveData.progress.dialogue = "Pintos#15-1";

		saveData.party.partyLevel = 30;
		saveData.party.partyUnitNames = new List<string>
		{
			"reina",
			"sisterna",
			"yeong",
			"eren",
			"luvericha"
		};

		ShowWorldMap();
	}

	private void ShowWorldMap()
	{
		WorldMapManager.currentStory = SaveDataCenter.GetSaveData().progress.worldMap;
		
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
