using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using WorldMap;

public class SceneLoader : MonoBehaviour {
	// public string nextSceneName;
	public GameObject fadeoutScreenObject;

	public void LoadNextBattleScene(string nextSceneName)
	{
		StartCoroutine(FadeoutAndLoadBattleScene(nextSceneName));
	}

	public void LoadNextDialogueScene(string nextSceneName)
	{
		StartCoroutine(FadeoutAndLoadDialogueScene(nextSceneName));
	}

	public void LoadNextWorldMapScene(string storyName)
	{
		StartCoroutine(FadeoutAndLoadWorldmapScene(storyName));
	}

	public bool IsScreenActive()
	{
		return fadeoutScreenObject.activeInHierarchy;
	}
	
	IEnumerator Start()
	{
		fadeoutScreenObject.SetActive(true);
		for (int i = 0; i < 20; i++)
		{
			fadeoutScreenObject.GetComponent<Image>().color -= new Color(0,0,0,0.05f);
			yield return new WaitForSeconds(0.05f);
		}
		fadeoutScreenObject.SetActive(false);
	}

	IEnumerator FadeoutAndLoadBattleScene(string nextSceneName)
	{
		fadeoutScreenObject.SetActive(true);
		for (int i = 0; i < 20; i++)
		{
			fadeoutScreenObject.GetComponent<Image>().color += new Color(0,0,0,0.05f);
			yield return new WaitForSeconds(0.05f);
		}

		SceneData.nextStageName = nextSceneName;
		Debug.Log("input next battle - " + SceneData.nextStageName);

		SceneManager.LoadScene("Battle");
	}

	IEnumerator FadeoutAndLoadDialogueScene(string nextScriptFileName)
	{
		fadeoutScreenObject.SetActive(true);
		for (int i = 0; i < 20; i++)
		{
			fadeoutScreenObject.GetComponent<Image>().color += new Color(0,0,0,0.05f);
			yield return new WaitForSeconds(0.05f);
		}

		SceneData.nextDialogueName = nextScriptFileName;
		Debug.Log("input next dialogue - " + SceneData.nextDialogueName);

		SceneManager.LoadScene("dialogue");
	}

	IEnumerator FadeoutAndLoadWorldmapScene(string nextStoryName)
	{
		fadeoutScreenObject.SetActive(true);
		for (int i = 0; i < 20; i++)
		{
			fadeoutScreenObject.GetComponent<Image>().color += new Color(0,0,0,0.05f);
			yield return new WaitForSeconds(0.05f);
		}

		// need use save data
		WorldMapManager.currentStory = nextStoryName;
		Debug.Log("input next story - " + WorldMapManager.currentStory);

		SceneManager.LoadScene("worldMap");
	}
}
