using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour {
	private static SoundManager instance = null;
	public static SoundManager Instance { get { return instance; } }

	BattleManager battleManager;
	AudioSource audioSource;

	public void PlayBgm(string name){
		Debug.Log("Play bgm : " + "Sound/" + name);
		AudioClip bgm = Resources.Load<AudioClip>("Sound/" + name);
		if(audioSource.clip == null || audioSource.clip != bgm)
			audioSource.clip = bgm;
		if(!audioSource.isPlaying)
			audioSource.Play();
	}
	public void PlaySE(string name){
		AudioClip SE = Resources.Load<AudioClip>("Sound/SE/" + name);
		audioSource.PlayOneShot (SE);
	}

	void Awake () {
		if (instance != null && instance != this){
			Destroy(this.gameObject);
			return;
		}else
			instance = this;

		DontDestroyOnLoad(this.gameObject);

		battleManager = FindObjectOfType<BattleManager>();
		audioSource = gameObject.GetComponent<AudioSource>();
	}
}