using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour {
	private static SoundManager instance = null;
	public static SoundManager Instance { get { return instance; } }

	AudioSource audioSource;

	static Dictionary<string, AudioClip> BGMs;
	static Dictionary<string, AudioClip> SEs;

	public void PlayBGM(string name){
		audioSource.clip = BGMs [name];
        audioSource.volume = Configuration.BGMVolume;
		if(!audioSource.isPlaying)
			audioSource.Play();
	}
	public void PlaySE(string name){
		audioSource.PlayOneShot (SEs[name], Configuration.soundEffectVolume);
	}

    public void ChangeBGMVolume(float BGMVolume) {
        audioSource.volume = Configuration.BGMVolume;
    }

	void Awake () {
		if (instance != null && instance != this) {
			Destroy (this.gameObject);
			return;
		}
		else {
			instance = this;
		}

		DontDestroyOnLoad(this.gameObject);
		LoadBGMsAndSEs ();
		audioSource = gameObject.GetComponent<AudioSource>();
	}

	static void LoadBGMsAndSEs(){
		AudioClip[] BGMLists = Resources.LoadAll<AudioClip> ("BGMs");
		BGMs = new Dictionary<string, AudioClip> ();
		foreach (AudioClip BGM in BGMLists) {
			BGMs [BGM.name] = BGM;
		}
		AudioClip[] SELists = Resources.LoadAll<AudioClip> ("SEs");
		SEs = new Dictionary<string, AudioClip> ();
		foreach (AudioClip SE in SELists) {
			SEs [SE.name] = SE;
		}
	}
}