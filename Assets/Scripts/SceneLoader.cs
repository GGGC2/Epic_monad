using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour {
	// public string nextSceneName;
	public GameObject fadeoutScreenObject;

	// public void LoadNextScene()
	// {
	// 	StartCoroutine(FadeoutAndLoadScene());
	// }

	public void LoadNextScene(string nextSceneName)
	{
		StartCoroutine(FadeoutAndLoadScene(nextSceneName));
	}

	public void LoadNextScript(string nextScriptFileName)
	{
		StartCoroutine(FadeoutAndLoadScript(nextScriptFileName));
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

	// IEnumerator FadeoutAndLoadScene()
	// {
	// 	fadeoutScreenObject.SetActive(true);
	// 	for (int i = 0; i < 20; i++)
	// 	{
	// 		fadeoutScreenObject.GetComponent<Image>().color += new Color(0,0,0,0.05f);
	// 		yield return new WaitForSeconds(0.05f);
	// 	}

	// 	Application.LoadLevel(nextSceneName);
	// }

	IEnumerator FadeoutAndLoadScene(string nextSceneName)
	{
		fadeoutScreenObject.SetActive(true);
		for (int i = 0; i < 20; i++)
		{
			fadeoutScreenObject.GetComponent<Image>().color += new Color(0,0,0,0.05f);
			yield return new WaitForSeconds(0.05f);
		}

		SceneManager.LoadScene(nextSceneName);
	}

	IEnumerator FadeoutAndLoadScript(string nextScriptFileName)
	{
		fadeoutScreenObject.SetActive(true);
		for (int i = 0; i < 20; i++)
		{
			fadeoutScreenObject.GetComponent<Image>().color += new Color(0,0,0,0.05f);
			yield return new WaitForSeconds(0.05f);
		}

		DialogueManager.nextDialogueName = nextScriptFileName;
		Debug.Log("input - " + DialogueManager.nextDialogueName);

		SceneManager.LoadScene("dialogue");
	}
}
