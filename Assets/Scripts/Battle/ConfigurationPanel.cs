using UnityEngine;
using UnityEngine.UI;

public class ConfigurationPanel : MonoBehaviour {
    Slider textObjectDurationSlider;
    Slider soundEffectVolumeSlider;
    Slider BGMVolumeSlider;
    Slider NPCBehaviourDurationSlider;

    public void Awake() {
        textObjectDurationSlider = GameObject.Find("TextObjectDurationSlider").GetComponent<Slider>();
        soundEffectVolumeSlider = GameObject.Find("SoundEffectVolumeSlider").GetComponent<Slider>();
        BGMVolumeSlider = GameObject.Find("BGMVolumeSlider").GetComponent<Slider>();
        NPCBehaviourDurationSlider = GameObject.Find("NPCBehaviourDurationSlider").GetComponent<Slider>();
    }
    public void onTextObjectDisplaySliderMoved() {
        Configuration.textObjectDuration = textObjectDurationSlider.value;
    }
    public void onSoundEffectVolumeSliderMoved() {
        Configuration.soundEffectVolume = soundEffectVolumeSlider.value;
    }
    public void onBGMVolumeSliderMoved() {
        Configuration.BGMVolume = BGMVolumeSlider.value;
        SoundManager.Instance.ChangeBGMVolume(Configuration.BGMVolume);
    }
    public void onNPCBehaviourDurationSliderMoved() {
        Configuration.NPCBehaviourDuration = NPCBehaviourDurationSlider.value / 2;
    }
}