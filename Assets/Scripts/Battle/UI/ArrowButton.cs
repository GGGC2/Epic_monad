using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Enums;

public class ArrowButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
	public Direction direction;
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
			battleManager.CallbackDirection(direction);
		};
		UnityEngine.Events.UnityAction UserLongSelectDirection= () => {
			battleManager.CallbackDirectionLong(direction);
		};
		LeftClickEnd.AddListener (UserSelectDirection);
		LongLeftClickEnd.AddListener (UserLongSelectDirection);
	}

	public void CheckAndHighlightImage(){
		Vector3 mousePositionScreen = Input.mousePosition;
		Vector3 mousePositionWorld = Camera.main.ScreenToWorldPoint(mousePositionScreen);
		Unit unit = BattleData.selectedUnit;

		Direction selectedDirection = Utility.GetMouseDirectionByUnit (unit, unit.GetDirection ());

		if (direction == selectedDirection) {
			button.Select ();
		}
	}

	float durationThreshold = 1.0f;
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