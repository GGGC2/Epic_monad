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
			if(BattleData.currentState == CurrentState.FocusToUnit && Setting.shortcutEnable){
				if (buttons [ActionCommand.Skill].interactable && Input.GetKeyDown (KeyCode.Q)){
					buttons [ActionCommand.Skill].onClick.Invoke ();
				}else if (buttons [ActionCommand.Standby].interactable && Input.GetKeyDown (KeyCode.W)){
					buttons [ActionCommand.Standby].onClick.Invoke ();
				}
			}
		}

		public void Initialize(){
			commandsOnOffLockOn = false;
			buttons = new Dictionary<ActionCommand, Button> ();
			buttons[ActionCommand.Skill]=GameObject.Find("SkillButton").GetComponent<Button>();
			buttons[ActionCommand.Standby]=GameObject.Find("StandbyButton").GetComponent<Button>();
		}

		public void OnOffButton(ActionCommand command, bool turnOn){
			if (commandsOnOffLockOn)
				return;

			buttons [command].interactable = turnOn;
			if(command == ActionCommand.Standby){
				buttons[command].image.sprite = Resources.Load<Sprite>("CommandUI/Rest");
			}
		}
		public void TurnOnOnlyThisButton(ActionCommand command){
			foreach(var pair in buttons){
				if(pair.Key == command){
					OnOffButton (pair.Key, true);
				}else{
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

		void CallbackOnPointerEnterRestCommand(){
			battleManager.CallbackOnPointerEnterRestCommand();
		}
		void CallbackOnPointerExitRestCommand(){
			battleManager.CallbackOnPointerExitRestCommand();
		}
	}
}