using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BattleUI{
	public class CommandPanel : MonoBehaviour{
		private BattleManager battleManager;

		Dictionary<ActionCommand, Button> buttons;
		bool lockOn=false;

		void Start(){
			battleManager = FindObjectOfType<BattleManager>();
		}

		void Update(){
			if(battleManager.battleData.currentState == CurrentState.FocusToUnit){
				if(buttons[ActionCommand.Move].interactable && Input.GetKeyDown(KeyCode.Q))
					CallbackMoveCommand();
				else if(buttons[ActionCommand.Skill].interactable && Input.GetKeyDown(KeyCode.W))
					CallbackSkillCommand();
				else if(buttons[ActionCommand.Standby].interactable && Input.GetKeyDown(KeyCode.E))
					CallbackStandbyCommand();
				else if(buttons[ActionCommand.Rest].interactable && Input.GetKeyDown(KeyCode.R))
					CallbackRestCommand();
			}
		}

		public void Initialize(){
			lockOn = false;
			buttons = new Dictionary<ActionCommand, Button> ();
			buttons[ActionCommand.Move]=GameObject.Find ("MoveButton").GetComponent<Button> ();
			buttons[ActionCommand.Skill]=GameObject.Find("SkillButton").GetComponent<Button>();
			buttons[ActionCommand.Standby]=GameObject.Find("StandbyButton").GetComponent<Button>();
			buttons[ActionCommand.Rest]=GameObject.Find("RestButton").GetComponent<Button>();
		}

		public void OnOffButton(ActionCommand command, bool turnOn){
			if (lockOn)
				Debug.Log ("Command button onoff is locked");
			else {
				Debug.Log (turnOn + " command " + command);
				buttons [command].interactable = turnOn;
			}
		}
		public void TurnOnOnlyThisButton(ActionCommand command){
			foreach (var pair in buttons) {
				if (pair.Key == command) {
					OnOffButton (pair.Key, true);
				} else {
					OnOffButton (pair.Key, false);
				}
			}
		}
		public void LockOnOffState(){
			lockOn = true;
		}
		public void UnlockOnOffState(){
			lockOn = false;
		}
		public void AddListenerToButton(ActionCommand command, UnityEngine.Events.UnityAction action){
			Debug.Log ("Add listener to command "+command);
			buttons[command].onClick.AddListener (action);
		}
		public void RemoveListenerToButton(ActionCommand command, UnityEngine.Events.UnityAction action){
			Debug.Log ("Remove listener to command "+command);
			buttons[command].onClick.RemoveListener (action);
		}

		public void CallbackMoveCommand(){
			battleManager.CallbackMoveCommand();
		}
		void CallbackSkillCommand(){
			battleManager.CallbackSkillCommand();
		}
		void CallbackStandbyCommand(){
			battleManager.CallbackStandbyCommand();
		}
		void CallbackRestCommand(){
			battleManager.CallbackRestCommand();
		}

		void CallbackOnPointerEnterRestCommand(){
			battleManager.CallbackOnPointerEnterRestCommand();
		}
		void CallbackOnPointerExitRestCommand(){
			battleManager.CallbackOnPointerExitRestCommand();
		}
	}
}