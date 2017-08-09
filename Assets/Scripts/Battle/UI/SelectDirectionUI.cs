using UnityEngine;
using System.Collections.Generic;
using Enums;

namespace BattleUI
{
	public class SelectDirectionUI : MonoBehaviour{
		private BattleManager battleManager;

		public ArrowButton[] ArrowButtons;
		Dictionary<Direction, ArrowButton> arrowButtons;

		public void Start(){
			battleManager = FindObjectOfType<BattleManager>();
			arrowButtons = new Dictionary<Direction, ArrowButton> ();
			arrowButtons [Direction.RightUp] = ArrowButtons [0];
			arrowButtons [Direction.LeftUp] = ArrowButtons [1];
			arrowButtons [Direction.RightDown] = ArrowButtons [2];
			arrowButtons [Direction.LeftDown] = ArrowButtons [3];
		}

		public void HighlightArrowButton(){
			foreach(ArrowButton button in ArrowButtons)
				button.CheckAndHighlightImage();
		}

		public void AddListenerToDirection(Direction direction, UnityEngine.Events.UnityAction action){
			arrowButtons [direction].button.onClick.AddListener (action);
		}
		public void RemoveListenerToDirection(Direction direction, UnityEngine.Events.UnityAction action){
			arrowButtons [direction].button.onClick.RemoveListener (action);
		}

		public void CallbackDirection(string directionString){
			Debug.Log("Direction CallBack");
			if(!battleManager.onTutorial)
				battleManager.CallbackDirection(directionString);
			else{
				TutorialScenario scenario = battleManager.tutorialManager.currentScenario;
				Debug.Log(scenario.mission);
				Debug.Log(scenario.missionDirection.ToString());
				if(scenario.mission == TutorialScenario.Mission.SelectDirection && scenario.missionDirection.ToString() == directionString){
					battleManager.CallbackDirection(directionString);
					battleManager.tutorialManager.NextStep();
				}
			}
		}
	}
}
