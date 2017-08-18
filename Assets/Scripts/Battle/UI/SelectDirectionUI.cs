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
		}
		public void Initialize(){
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

		public void EnableOnlyThisDirection(Direction direction){
			foreach(var pair in arrowButtons){
				if (pair.Key == direction)
					pair.Value.button.interactable = true;
				else
					pair.Value.button.interactable = false;
			}
		}
		public void EnableAllDirection(){
			foreach (var pair in arrowButtons)
				pair.Value.button.interactable = true;
		}
		public void AddListenerToDirection(Direction direction, UnityEngine.Events.UnityAction action){
			arrowButtons [direction].button.onClick.AddListener (action);
		}
		public void RemoveListenerToDirection(Direction direction, UnityEngine.Events.UnityAction action){
			arrowButtons [direction].button.onClick.RemoveListener (action);
		}
	}
}
