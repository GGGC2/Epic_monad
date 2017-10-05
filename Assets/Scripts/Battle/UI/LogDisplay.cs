using UnityEngine;
using UnityEngine.UI;

public class LogDisplay : MonoBehaviour {
    public Text text;
    public Log log;

    public void SetText() {
        text.text = log.GetText();
    }
}
