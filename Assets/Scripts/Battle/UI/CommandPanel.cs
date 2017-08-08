using UnityEngine;
using UnityEngine.UI;

namespace BattleUI{
	public class CommandPanel : MonoBehaviour{
		private BattleManager battleManager;

		void Start(){
			battleManager = FindObjectOfType<BattleManager>();
		}

		void Update(){
			if(battleManager.battleData.currentState == CurrentState.FocusToUnit){
				if(GameObject.Find("MoveButton").GetComponent<Button>().interactable && Input.GetKeyDown(KeyCode.Q))
					CallbackMoveCommand();
				else if(GameObject.Find("SkillButton").GetComponent<Button>().interactable && Input.GetKeyDown(KeyCode.W))
					CallbackSkillCommand();
				else if(GameObject.Find("StandbyButton").GetComponent<Button>().interactable && Input.GetKeyDown(KeyCode.E))
					CallbackStandbyCommand();
				else if(GameObject.Find("RestButton").GetComponent<Button>().interactable && Input.GetKeyDown(KeyCode.R))
					CallbackRestCommand();
			}
		}

		public void CallbackMoveCommand(){
			if(!battleManager.onTutorial)
				battleManager.CallbackMoveCommand();
			else{
				TutorialScenario tutorial = battleManager.tutorialManager.currentScenario;
				if(tutorial.mission == TutorialScenario.Mission.MoveCommand){
					battleManager.CallbackMoveCommand();
					battleManager.tutorialManager.NextStep();
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
					battleManager.tutorialManager.NextStep();
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
					battleManager.tutorialManager.NextStep();
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
					battleManager.tutorialManager.NextStep();
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