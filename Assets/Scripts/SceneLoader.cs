using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using WorldMap;
using DG.Tweening;

public class SceneLoader : MonoBehaviour{
	// public string nextSceneName;
	public GameObject fadeoutScreenObject;

	public void GoToTitle()
	{
		if (FindObjectOfType<DialogueManager>() != null)
			FindObjectOfType<DialogueManager>().InactiveAdventureUI();
		StartCoroutine(FadeoutAndLoadDialogueScene("Title"));
	}

	public void LoadNextBattleScene(string nextSceneName, bool ready)
	{
		if (FindObjectOfType<DialogueManager>() != null)
			FindObjectOfType<DialogueManager>().InactiveAdventureUI();
		StartCoroutine(FadeoutAndLoadBattleScene(nextSceneName, ready));
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

	IEnumerator FadeoutAndLoadBattleScene(string nextSceneName, bool ready)
	{
		yield return Fadeout();

		SceneData.nextStageName = nextSceneName;

		if(ready)
			SceneManager.LoadScene("BattleReady");
		else
			SceneManager.LoadScene("Battle");
	}

	IEnumerator FadeoutAndLoadDialogueScene(string nextScriptFileName)
	{
		yield return Fadeout();

		if (nextScriptFileName == "Title")
		{
			Time.timeScale = 1.0f;
			SceneManager.LoadScene("title");
		}
		else
		{
			SceneData.nextDialogueName = nextScriptFileName;
			Debug.Log("input next dialogue - " + SceneData.nextDialogueName);

			SceneManager.LoadScene("dialogue");
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
