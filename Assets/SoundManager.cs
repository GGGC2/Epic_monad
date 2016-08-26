using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour {

	StageManager stageManager;
	AudioSource audioSource;

	public void PlayBgm(string name)
	{
		AudioClip bgm = Resources.Load("Sound/" + name, typeof(AudioClip)) as AudioClip;
		audioSource.clip = bgm;
		Debug.Log("Play bgm : " + "Sound/" + name);
		audioSource.Play();
	}

	// Use this for initialization
	void Awake () {
		stageManager = FindObjectOfType<StageManager>();
		audioSource = gameObject.GetComponent<AudioSource>();

		if (SceneManager.GetActiveScene().name == "Battle")
			PlayBgm("Script_Tense");
	}

	// Update is called once per frame
	void Update () {

	}
}
