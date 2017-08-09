using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArrowButton : MonoBehaviour
{
	public enum DirectionTypeIndex{UpLeft, UpRight, DownLeft, DownRight};
	public DirectionTypeIndex DirectionType;
	public Image realImage;
	public void CheckAndHighlightImage()
	{
		Vector3 mousePositionScreen = Input.mousePosition;
		Vector3 mousePositionWorld = Camera.main.ScreenToWorldPoint(mousePositionScreen);
		Vector3 unitPosition = FindObjectOfType<BattleManager>().battleData.selectedUnit.realPosition;
		
		//Debug.Log("mousePosition : " + mousePositionWorld);
		//Debug.Log("UnitPosition : " + unitPosition);

		if(DirectionType == DirectionTypeIndex.UpLeft && mousePositionWorld.x < unitPosition.x && mousePositionWorld.y > unitPosition.y)
		{
			GetComponent<Button>().Select();
			//realImage.sprite = gameObject.GetComponent<Button>().spriteState.highlightedSprite;
		}
		else if(DirectionType == DirectionTypeIndex.UpRight && mousePositionWorld.x > unitPosition.x && mousePositionWorld.y > unitPosition.y)
		{
			GetComponent<Button>().Select();
			//realImage.sprite = gameObject.GetComponent<Button>().spriteState.highlightedSprite;
		}
		else if(DirectionType == DirectionTypeIndex.DownLeft && mousePositionWorld.x < unitPosition.x && mousePositionWorld.y < unitPosition.y)
		{
			GetComponent<Button>().Select();
			//realImage.sprite = gameObject.GetComponent<Button>().spriteState.highlightedSprite;
		}
		else if(DirectionType == DirectionTypeIndex.DownRight && mousePositionWorld.x > unitPosition.x && mousePositionWorld.y < unitPosition.y)
		{
			GetComponent<Button>().Select();
			//realImage.sprite = gameObject.GetComponent<Button>().spriteState.highlightedSprite;
		}
	}
}