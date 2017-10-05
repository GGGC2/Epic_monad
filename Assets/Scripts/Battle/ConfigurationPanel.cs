using UnityEngine;
using UnityEngine.UI;

public class ConfigurationPanel : MonoBehaviour {
    Slider textObjectDurationSlider;
    Slider soundEffectVolumeSlider;
    Slider BGMVolumeSlider;

    public void Awake() {
        textObjectDurationSlider = GameObject.Find("TextObjectDurationSlider").GetComponent<Slider>();
        soundEffectVolumeSlider = GameObject.Find("SoundEffectVolumeSlider").GetComponent<Slider>();
        BGMVolumeSlider = GameObject.Find("BGMVolumeSlider").GetComponent<Slider>();
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
}