using UnityEngine;

namespace BattleUI{
	public class SkillCheckPanel : MonoBehaviour{
		private BattleManager battleManager;

		public void Start(){
			battleManager = FindObjectOfType<BattleManager>();
		}

		public void CallbackApplyCommand(){
			if(!battleManager.onTutorial)
				battleManager.CallbackApplyCommand();
			else{
				TutorialScenario tutorial = battleManager.tutorialManager.currentScenario;
				if(tutorial.mission == TutorialScenario.Mission.Apply){
					battleManager.CallbackApplyCommand();
					battleManager.tutorialManager.NextStep();
				}
			}
		}

		public void CallbackChainCommand()
		{
			battleManager.CallbackChainCommand();
		}

		public void CallbackCancel()
		{
			battleManager.CallbackCancel();
		}
	}
}