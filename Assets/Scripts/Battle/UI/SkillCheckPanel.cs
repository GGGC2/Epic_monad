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
				TutorialScenario tutorial = FindObjectOfType<TutorialScenario>();
				if(tutorial.mission == TutorialScenario.TutorialMission.ApplySkill){
					battleManager.CallbackApplyCommand();
					tutorial.NextStep();
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