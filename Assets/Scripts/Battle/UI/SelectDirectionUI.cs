using UnityEngine;

namespace BattleUI
{
	public class SelectDirectionUI : MonoBehaviour{
		private BattleManager battleManager;

		public ArrowButton[] ArrowButtons;

		public void Start(){
			battleManager = FindObjectOfType<BattleManager>();
			ArrowButtons = FindObjectsOfType<ArrowButton>();
		}

		public void HighlightArrowButton(){
			foreach(ArrowButton button in ArrowButtons)
				button.CheckAndHighlightImage();
		}

		public void CallbackDirection(string directionString){
			if(!battleManager.onTutorial)
				battleManager.CallbackDirection(directionString);
			else{
				TutorialScenario scenario = FindObjectOfType<TutorialScenario>();
				if(scenario.mission == TutorialScenario.TutorialMission.SelectDirection && scenario.missionDirection.ToString() == directionString){
					battleManager.CallbackDirection(directionString);
					scenario.NextStep();
				}
			}
		}
	}
}
