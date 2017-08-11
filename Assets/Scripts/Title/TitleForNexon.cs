using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;
using Save;
using WorldMap;
using GameData;

public class TitleForNexon : MonoBehaviour
{
    public GameDataManager gameDataManager;
	public DOTweenAnimation titlePath;
	public DOTweenAnimation titleAnimation;

	public List<Button> titleButtons;
	public List<DOTweenAnimation> buttonAnimations;

	public List<Button> worldMapButtons;
	public List<DOTweenAnimation> worldMapButtonAnimations;
    public void Start() {
        GameDataManager.Load();
		SoundManager.Instance.PlayBGM ("Monad_Title");
    }
	public void Awake(){
		foreach (var button in worldMapButtons)
			button.enabled = false;
	}

	public void NewGame(){
        GameDataManager.Reset();

		FindObjectOfType<SceneLoader>().LoadNextDialogueScene(SceneData.dialogueName);
	}
    public void LoadGame() {
        SceneLoader sceneLoader = FindObjectOfType<SceneLoader>();
        GameDataManager.Load();
        if (SceneData.isDialogue) {
            sceneLoader.LoadNextDialogueScene(SceneData.dialogueName);
        } else
            sceneLoader.LoadNextBattleScene();
        //ShowWorldMap();
    }

    private void ShowWorldMap(){
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
