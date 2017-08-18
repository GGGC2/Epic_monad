using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ArrowButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
	public enum DirectionTypeIndex{UpLeft, UpRight, DownLeft, DownRight};
	public DirectionTypeIndex DirectionType;
	public Image realImage;
	public Button button {
		get {
			return GetComponent<Button> ();
		}
	}

	void Awake(){
		InitializeEvents ();
	}

	void InitializeEvents(){
		BattleManager battleManager = FindObjectOfType<BattleManager>();
		UnityEngine.Events.UnityAction UserSelectDirection= () => {
			battleManager.CallbackDirection(DirectionType.ToString());
		};
		UnityEngine.Events.UnityAction UserLongSelectDirection= () => {
			battleManager.CallbackDirectionLong(DirectionType.ToString());
		};
		LeftClickEnd.AddListener (UserSelectDirection);
		LongLeftClickEnd.AddListener (UserLongSelectDirection);
	}

	public void CheckAndHighlightImage(){
		Vector3 mousePositionScreen = Input.mousePosition;
		Vector3 mousePositionWorld = Camera.main.ScreenToWorldPoint(mousePositionScreen);
		Vector3 unitPosition = BattleData.selectedUnit.realPosition;

		if (DirectionType == DirectionTypeIndex.UpLeft && mousePositionWorld.x < unitPosition.x && mousePositionWorld.y > unitPosition.y) {
			button.Select ();
		} else if (DirectionType == DirectionTypeIndex.UpRight && mousePositionWorld.x > unitPosition.x && mousePositionWorld.y > unitPosition.y) {
			button.Select ();
		} else if (DirectionType == DirectionTypeIndex.DownLeft && mousePositionWorld.x < unitPosition.x && mousePositionWorld.y < unitPosition.y) {
			button.Select ();
		} else if (DirectionType == DirectionTypeIndex.DownRight && mousePositionWorld.x > unitPosition.x && mousePositionWorld.y < unitPosition.y) {
			button.Select ();
		}
	}

	public float durationThreshold = 1.0f;
	bool clickStarted = false;
	float timeClickStarted;
	public UnityEvent LeftClickEnd;
	public UnityEvent LongLeftClickEnd;

	void IPointerDownHandler.OnPointerDown(PointerEventData pointerData){
		if (pointerData.button == PointerEventData.InputButton.Left) {
			clickStarted = true;
			timeClickStarted = Time.time;
		}
	}
	void IPointerUpHandler.OnPointerUp(PointerEventData pointerData){
		if (clickStarted && pointerData.button == PointerEventData.InputButton.Left) {
			clickStarted = false;
			LeftClickEnd.Invoke ();
		}
	}
	void IPointerExitHandler.OnPointerExit(PointerEventData pointerData){
		clickStarted = false;
	}
	void Update(){
		if (clickStarted && Time.time - timeClickStarted > durationThreshold) {
			clickStarted = false;
			LongLeftClickEnd.Invoke ();
		}
	}
}