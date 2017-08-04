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

	public void GoToTitle()
	{
		if (FindObjectOfType<DialogueManager>() != null)
			FindObjectOfType<DialogueManager>().InactiveAdventureUI();
		StartCoroutine(FadeoutAndLoadDialogueScene("Title"));
	}

	public void LoadNextBattleScene()
	{
		if (FindObjectOfType<DialogueManager>() != null)
			FindObjectOfType<DialogueManager>().InactiveAdventureUI();
		StartCoroutine(FadeoutAndLoadBattleScene());
	}

	public void LoadNextDialogueScene(string nextSceneName)
	{
		if (FindObjectOfType<DialogueManager>() != null)
			FindObjectOfType<DialogueManager>().InactiveAdventureUI();
		StartCoroutine(FadeoutAndLoadDialogueScene(nextSceneName));
	}

	public void LoadNextWorldMapScene(string storyName)
	{
		if (FindObjectOfType<DialogueManager>() != null)
			FindObjectOfType<DialogueManager>().InactiveAdventureUI();
		StartCoroutine(FadeoutAndLoadWorldmapScene(storyName));
	}

	public bool IsScreenActive()
	{
		return fadeoutScreenObject.activeInHierarchy;
	}
	
	IEnumerator Start()
	{
		Time.timeScale = 0;

		fadeoutScreenObject.SetActive(true);
		var img = fadeoutScreenObject.GetComponent<Image>();
		img.color = Color.black;
		var tween = img.DOColor(new Color(0,0,0,0),1f).SetUpdate(true);
		while(tween.IsPlaying())
		{
			yield return null;
		}

		fadeoutScreenObject.SetActive(false);

		Time.timeScale = 1.0f;
	}

	IEnumerator Fadeout()
	{
		Time.timeScale = 0;

		fadeoutScreenObject.SetActive(true);
		var img = fadeoutScreenObject.GetComponent<Image>();
		var tween = img.DOColor(Color.black,1f).SetUpdate(true);
		while(tween.IsPlaying())
		{
			yield return null;
		}
	}

	IEnumerator FadeoutAndLoadBattleScene()
	{
		yield return Fadeout();

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

	IEnumerator FadeoutAndLoadDialogueScene(string nextScriptFileName)
	{
		yield return Fadeout();

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
			Debug.Log("input next dialogue - " + SceneData.dialogueName);

			SceneManager.LoadScene("Dialogue");
		}
	}

	IEnumerator FadeoutAndLoadWorldmapScene(string nextStoryName)
	{
		yield return Fadeout();

		// need use save data
		WorldMapManager.currentStory = nextStoryName;
		Debug.Log("input next story - " + WorldMapManager.currentStory);

		SceneManager.LoadScene("worldMap");
	}
}
