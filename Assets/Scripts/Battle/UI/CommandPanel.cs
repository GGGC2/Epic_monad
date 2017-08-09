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
			buttons = new Dictionary<ActionCommand, Button> ();
			buttons[ActionCommand.Move]=GameObject.Find ("MoveButton").GetComponent<Button> ();
			buttons[ActionCommand.Skill]=GameObject.Find("SkillButton").GetComponent<Button>();
			buttons[ActionCommand.Standby]=GameObject.Find("StandbyButton").GetComponent<Button>();
			buttons[ActionCommand.Rest]=GameObject.Find("RestButton").GetComponent<Button>();
		}

		public void AddListenerToButton(ActionCommand command, UnityEngine.Events.UnityAction action){
			Debug.Log ("Add listener to commnad No."+command);
			buttons[command].onClick.AddListener (action);
		}
		public void RemoveListenerToButton(ActionCommand command, UnityEngine.Events.UnityAction action){
			Debug.Log ("Remove listener to command No."+command);
			buttons[command].onClick.RemoveListener (action);
		}
		public void CallbackMoveCommand(){
			if(!battleManager.onTutorial)
				battleManager.CallbackMoveCommand();
			else{
				TutorialScenario tutorial = battleManager.tutorialManager.currentScenario;
				if(tutorial.mission == TutorialScenario.Mission.MoveCommand){
					battleManager.CallbackMoveCommand();
				}
			}				
		}

		public void CallbackSkillCommand(){
			if(!battleManager.onTutorial)
				battleManager.CallbackSkillCommand();
			else{
				TutorialScenario tutorial = battleManager.tutorialManager.currentScenario;
				if(tutorial.mission == TutorialScenario.Mission.SkillCommand){
					battleManager.CallbackSkillCommand();
				}
			}
		}

		public void CallbackStandbyCommand(){
			if(!battleManager.onTutorial)
				battleManager.CallbackStandbyCommand();
			else{
				TutorialScenario tutorial = battleManager.tutorialManager.currentScenario;
				if(tutorial.mission == TutorialScenario.Mission.Standby){
					battleManager.CallbackStandbyCommand();
				}
			}
		}

		public void CallbackRestCommand(){
			if(!battleManager.onTutorial)
				battleManager.CallbackRestCommand();
			else{
				TutorialScenario tutorial = battleManager.tutorialManager.currentScenario;
				if(tutorial.mission == TutorialScenario.Mission.Rest){
					battleManager.CallbackRestCommand();
				}
			}
		}

		public void CallbackOnPointerEnterRestCommand(){
			battleManager.CallbackOnPointerEnterRestCommand();
		}

		public void CallbackOnPointerExitRestCommand(){
			battleManager.CallbackOnPointerExitRestCommand();
		}
	}
}