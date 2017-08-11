using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArrowButton : MonoBehaviour
{
	public enum DirectionTypeIndex{UpLeft, UpRight, DownLeft, DownRight};
	public DirectionTypeIndex DirectionType;
	public Image realImage;
	public Button button {
		get {
			return GetComponent<Button> ();
		}
	}
	public void CheckAndHighlightImage()
	{
		Vector3 mousePositionScreen = Input.mousePosition;
		Vector3 mousePositionWorld = Camera.main.ScreenToWorldPoint(mousePositionScreen);
		Vector3 unitPosition = BattleData.selectedUnit.realPosition;

		if(DirectionType == DirectionTypeIndex.UpLeft && mousePositionWorld.x < unitPosition.x && mousePositionWorld.y > unitPosition.y)
		{
			GetComponent<Button>().Select();
		}
		else if(DirectionType == DirectionTypeIndex.UpRight && mousePositionWorld.x > unitPosition.x && mousePositionWorld.y > unitPosition.y)
		{
			GetComponent<Button>().Select();
		}
		else if(DirectionType == DirectionTypeIndex.DownLeft && mousePositionWorld.x < unitPosition.x && mousePositionWorld.y < unitPosition.y)
		{
			GetComponent<Button>().Select();
		}
		else if(DirectionType == DirectionTypeIndex.DownRight && mousePositionWorld.x > unitPosition.x && mousePositionWorld.y < unitPosition.y)
		{
			GetComponent<Button>().Select();
		}
	}
}