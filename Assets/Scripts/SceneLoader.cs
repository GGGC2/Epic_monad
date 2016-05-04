using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour {

	public string nextSceneName;
	public GameObject fadeoutScreenObject;

	public void LoadNextScene()
	{
		StartCoroutine(FadeoutAndLoadScene());
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
			yield return new WaitForSeconds(0.1f);
		}
		fadeoutScreenObject.SetActive(false);
	}
	
	IEnumerator FadeoutAndLoadScene()
	{
		fadeoutScreenObject.SetActive(true);
		for (int i = 0; i < 20; i++)
		{
			fadeoutScreenObject.GetComponent<Image>().color += new Color(0,0,0,0.05f);
			yield return new WaitForSeconds(0.1f);
		}
		Application.LoadLevel(nextSceneName);
	}
}
