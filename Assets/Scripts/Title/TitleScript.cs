using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;
using Save;
using WorldMap;
using GameData;
using UnityEngine.SceneManagement;

public class TitleScript : MonoBehaviour{
    public GameDataManager gameDataManager;
	public DOTweenAnimation titlePath;
	public DOTweenAnimation titleAnimation;

	public List<Button> titleButtons;
	public List<DOTweenAnimation> buttonAnimations;

	public List<DOTweenAnimation> worldMapButtonAnimations;
    public void Start() {
        GameDataManager.Load();
		StartCoroutine (SoundManager.Instance.PlayBGM ("Monad_Title"));
    }
	public void Awake(){
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

	public void SelectStage() {
		GameDataManager.Reset();
		SceneManager.LoadScene("StageSelect");
	}

	public void ToTestScene(){
		SceneManager.LoadScene("Test");
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

		GetComponent<WorldMapManager>().enabled = true;
	}
}