using UnityEngine;
using UnityEngine.UI;


public class NotImplementedDebugPanel : MonoBehaviour
{
	public Text logText;

	public void Start()
	{
		ToggleVisible();
	}

	public void Update()
	{
		if (Input.GetKeyDown(KeyCode.F))
		{
			ToggleVisible();
		}
	}

	private void ToggleVisible()
	{
		var backgroundImage = GetComponent<Image>();
		backgroundImage.enabled = !backgroundImage.enabled;
		logText.enabled = backgroundImage.enabled;
	}

	public void Append(string text)
	{
		logText.text += "\n" + text;
	}
}
