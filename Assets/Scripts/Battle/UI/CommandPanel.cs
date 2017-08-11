using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BattleUI{
	public class CommandPanel : MonoBehaviour{
		private BattleManager battleManager;

		Dictionary<ActionCommand, Button> buttons;

		void Start(){
			battleManager = FindObjectOfType<BattleManager>();
		}

		void Update(){
			if(BattleData.currentState == CurrentState.FocusToUnit){
				if (buttons [ActionCommand.Move].interactable && Input.GetKeyDown (KeyCode.Q))
					buttons [ActionCommand.Move].onClick.Invoke ();
				else if (buttons [ActionCommand.Skill].interactable && Input.GetKeyDown (KeyCode.W))
					buttons [ActionCommand.Skill].onClick.Invoke ();
				else if (buttons [ActionCommand.Standby].interactable && Input.GetKeyDown (KeyCode.E))
					buttons [ActionCommand.Standby].onClick.Invoke ();
				else if (buttons [ActionCommand.Rest].interactable && Input.GetKeyDown (KeyCode.R))
					buttons [ActionCommand.Rest].onClick.Invoke ();
			}
		}

		public void Initialize(){
			commandsOnOffLockOn = false;
			buttons = new Dictionary<ActionCommand, Button> ();
			buttons[ActionCommand.Move]=GameObject.Find ("MoveButton").GetComponent<Button> ();
			buttons[ActionCommand.Skill]=GameObject.Find("SkillButton").GetComponent<Button>();
			buttons[ActionCommand.Standby]=GameObject.Find("StandbyButton").GetComponent<Button>();
			buttons[ActionCommand.Rest]=GameObject.Find("RestButton").GetComponent<Button>();
		}

		public void OnOffButton(ActionCommand command, bool turnOn){
			if (commandsOnOffLockOn)
				return;
			buttons [command].interactable = turnOn;
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

		bool commandsOnOffLockOn=false;
		public void LockCommandsOnOff(){
			commandsOnOffLockOn = true;
		}
		public void UnlockCommandsOnOff(){
			commandsOnOffLockOn = false;
		}

		public void AddListenerToButton(ActionCommand command, UnityEngine.Events.UnityAction action){
			buttons[command].onClick.AddListener (action);
		}
		public void RemoveListenerToButton(ActionCommand command, UnityEngine.Events.UnityAction action){
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