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
				TutorialScenario tutorial = FindObjectOfType<TutorialScenario>();
				if(tutorial.mission == TutorialScenario.TutorialMission.MoveCommand){
					battleManager.CallbackMoveCommand();
					tutorial.NextStep();
				}
			}				
		}

		public void CallbackSkillCommand(){
			if(!battleManager.onTutorial)
				battleManager.CallbackSkillCommand();
			else{
				TutorialScenario tutorial = FindObjectOfType<TutorialScenario>();
				if(tutorial.mission == TutorialScenario.TutorialMission.SkillCommand){
					battleManager.CallbackSkillCommand();
					tutorial.NextStep();
				}
			}
		}

		public void CallbackStandbyCommand(){
			if(!battleManager.onTutorial)
				battleManager.CallbackStandbyCommand();
				else{
				TutorialScenario tutorial = FindObjectOfType<TutorialScenario>();
				if(tutorial.mission == TutorialScenario.TutorialMission.StandbyCommand){
					battleManager.CallbackStandbyCommand();
					tutorial.NextStep();
				}
			}
		}

		public void CallbackRestCommand(){
			if(!battleManager.onTutorial)
				battleManager.CallbackRestCommand();
		}

		public void CallbackOnPointerEnterRestCommand(){
			battleManager.CallbackOnPointerEnterRestCommand();
		}

		public void CallbackOnPointerExitRestCommand(){
			battleManager.CallbackOnPointerExitRestCommand();
		}
	}
}