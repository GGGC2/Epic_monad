using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using WorldMap;
using DG.Tweening;
using GameData;

public class SceneLoader : MonoBehaviour{
	// public string nextSceneName;
	public GameObject fadeoutScreenObject;

	public void GoToTitle(){
		if (FindObjectOfType<DialogueManager>() != null)
			FindObjectOfType<DialogueManager>().SetActiveAdventureUI(false);
		StartCoroutine(FadeoutAndLoadDialogueScene("Title"));
	}

	public void LoadNextBattleScene(){
		if (FindObjectOfType<DialogueManager>() != null)
			FindObjectOfType<DialogueManager>().SetActiveAdventureUI(false);
		StartCoroutine(FadeoutAndLoadBattleScene());
	}

	public void LoadNextDialogueScene(string nextSceneName)
	{
		if (FindObjectOfType<DialogueManager>() != null)
			FindObjectOfType<DialogueManager>().SetActiveAdventureUI(false);
		StartCoroutine(FadeoutAndLoadDialogueScene(nextSceneName));
	}

	public void LoadNextWorldMapScene(string storyName)
	{
		if (FindObjectOfType<DialogueManager>() != null)
			FindObjectOfType<DialogueManager>().SetActiveAdventureUI(false);
		StartCoroutine(FadeoutAndLoadWorldmapScene(storyName));
	}

	public bool IsScreenActive(){
		return fadeoutScreenObject.activeInHierarchy;
	}
	
	void Awake(){
		Application.backgroundLoadingPriority = ThreadPriority.Low;
	}

	IEnumerator Start(){
		yield return Fade(false);
		fadeoutScreenObject.SetActive(false);
		Time.timeScale = 1.0f;
	}

	public IEnumerator Fade(bool isBlack){
		Time.timeScale = 0;
		fadeoutScreenObject.SetActive(true);
		var img = fadeoutScreenObject.GetComponent<Image>();
		Tweener tween;

		if(isBlack)
			tween = img.DOColor(Color.black, 1f).SetUpdate(true);
		else{
			img.color = Color.black;
			tween = img.DOColor(new Color(0,0,0,0),1f).SetUpdate(true);
		}

		while(tween.IsPlaying())
			yield return null;

		if(!isBlack)
			fadeoutScreenObject.SetActive(false);
	}

	IEnumerator FadeoutAndLoadBattleScene(){
		yield return Fade(true);

        SceneData.isDialogue = false;
        if (!SceneData.isTestMode && !SceneData.isStageMode) {
            GameDataManager.Save();
        }
        if (SceneData.isTestMode || SceneData.stageNumber == 1 || SceneManager.GetActiveScene().name == "BattleReady")
            SceneManager.LoadScene("Battle");
        else {
            SceneManager.LoadScene("BattleReady");
        }
    }

	IEnumerator FadeoutAndLoadDialogueScene(string nextScriptFileName){
		yield return Fade(true);

		if (nextScriptFileName == "Title"){
			Time.timeScale = 1.0f;
			SceneManager.LoadScene("Title");
		}
		else{
			SceneData.dialogueName = nextScriptFileName;
            SceneData.isDialogue = true;
            if (!SceneData.isTestMode && !SceneData.isStageMode) {
                GameDataManager.Save();
            }

			SceneManager.LoadScene("Dialogue");
		}
	}

	IEnumerator FadeoutAndLoadWorldmapScene(string nextStoryName){
		yield return Fade(true);

		// need use save data
		WorldMapManager.currentStory = nextStoryName;
		Debug.Log("input next story - " + WorldMapManager.currentStory);

		SceneManager.LoadScene("worldMap");
	}
}
