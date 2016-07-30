using UnityEngine;

namespace WorldMap
{
public class ToNextDialogueButton : MonoBehaviour
{
	public void OnButtonClick()
	{
		WorldMapManager.ToNextDialogue();
	}
}
}
