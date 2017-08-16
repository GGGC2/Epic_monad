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
		FindAndOffAdvUI ();
		StartCoroutine(FadeoutAndLoadDialogueScene("Title"));
	}

	public void LoadNextBattleScene(){
		FindAndOffAdvUI ();
		StartCoroutine(FadeoutAndLoadBattleScene());
	}

	public void LoadNextDialogueScene(string nextSceneName){
		FindAndOffAdvUI ();
		StartCoroutine(FadeoutAndLoadDialogueScene(nextSceneName));
	}

	public void LoadNextWorldMapScene(string storyName){
		FindAndOffAdvUI ();
		StartCoroutine(FadeoutAndLoadWorldmapScene(storyName));
	}

	void FindAndOffAdvUI(){
		if (FindObjectOfType<DialogueManager>() != null)
			FindObjectOfType<DialogueManager>().SetActiveAdventureUI(false);
	}

	public bool IsScreenActive(){
		return fadeoutScreenObject.activeInHierarchy;
	}
	
	void Awake(){
		Application.backgroundLoadingPriority = ThreadPriority.Low;
	}

	IEnumerator Start(){
		yield return StartCoroutine(Fade(false));
	}

	//true면 FO, false면 FI.
	public IEnumerator Fade(bool isBlack){
		fadeoutScreenObject.SetActive(true);

		if(!isBlack)
			yield return new WaitForSeconds (Setting.fadeInWaitingTime);
		
		Time.timeScale = 0;
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

		if (!isBlack) {
			fadeoutScreenObject.SetActive (false);
		}

		Time.timeScale = 1.0f;
	}

	IEnumerator FadeoutAndLoadBattleScene(){
		yield return StartCoroutine(Fade(true));

        SceneData.isDialogue = false;
        if (!SceneData.isTestMode && !SceneData.isStageMode) {
            GameDataManager.Save();
        }
		if (SceneData.isTestMode || SceneData.stageNumber < Setting.readySceneOpenStage || SceneManager.GetActiveScene().name == "BattleReady")
            SceneManager.LoadScene("Battle");
        else
            SceneManager.LoadScene("BattleReady");
    }

	IEnumerator FadeoutAndLoadDialogueScene(string nextScriptFileName){
		yield return StartCoroutine(Fade(true));

		if (nextScriptFileName == "Title"){
			Time.timeScale = 1.0f;
			SceneManager.LoadScene("Title");
		}else{
			SceneData.dialogueName = nextScriptFileName;
            SceneData.isDialogue = true;
            if (!SceneData.isTestMode && !SceneData.isStageMode)
                GameDataManager.Save();

			SceneManager.LoadScene("Dialogue");
		}
	}

	IEnumerator FadeoutAndLoadWorldmapScene(string nextStoryName){
		yield return StartCoroutine(Fade(true));

		// need use save data
		WorldMapManager.currentStory = nextStoryName;
		Debug.Log("input next story - " + WorldMapManager.currentStory);

		SceneManager.LoadScene("worldMap");
	}
}