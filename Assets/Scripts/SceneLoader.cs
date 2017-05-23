using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using WorldMap;
using DG.Tweening;

public class SceneLoader : MonoBehaviour {
	// public string nextSceneName;
	public GameObject fadeoutScreenObject;

	public void LoadNextBattleScene(string nextSceneName)
	{
		if (FindObjectOfType<DialogueManager>() != null)
			FindObjectOfType<DialogueManager>().InactiveAdventureUI();
		StartCoroutine(FadeoutAndLoadBattleScene(nextSceneName));
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
		Debug.Log("Load new scene");
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

	IEnumerator FadeoutAndLoadBattleScene(string nextSceneName)
	{
		Time.timeScale = 0;

		fadeoutScreenObject.SetActive(true);
		var img = fadeoutScreenObject.GetComponent<Image>();
		var tween = img.DOColor(Color.black,1f).SetUpdate(true);
		while(tween.IsPlaying())
		{
			yield return null;
		}

		SceneData.nextStageName = nextSceneName;
		Debug.Log("input next battle - " + SceneData.nextStageName);

		SceneManager.LoadScene("Battle");
	}

	IEnumerator FadeoutAndLoadDialogueScene(string nextScriptFileName)
	{
		Time.timeScale = 0;

		fadeoutScreenObject.SetActive(true);
		var img = fadeoutScreenObject.GetComponent<Image>();
		var tween = img.DOColor(Color.black,1f).SetUpdate(true);
		while(tween.IsPlaying())
		{
			yield return null;
		}

		SceneData.nextDialogueName = nextScriptFileName;
		Debug.Log("input next dialogue - " + SceneData.nextDialogueName);

		SceneManager.LoadScene("dialogue");
	}

	IEnumerator FadeoutAndLoadWorldmapScene(string nextStoryName)
	{
		Time.timeScale = 0;

		fadeoutScreenObject.SetActive(true);
		var img = fadeoutScreenObject.GetComponent<Image>();
		var tween = img.DOColor(Color.black,1f).SetUpdate(true);
		while(tween.IsPlaying())
		{
			yield return null;
		}

		// need use save data
		WorldMapManager.currentStory = nextStoryName;
		Debug.Log("input next story - " + WorldMapManager.currentStory);

		SceneManager.LoadScene("worldMap");
	}
}
