using UnityEngine;
using UnityEngine.UI;

namespace BattleUI{
	public class CommandPanel : MonoBehaviour
	{
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
			battleManager.CallbackMoveCommand();
		}

		public void CallbackSkillCommand(){
			battleManager.CallbackSkillCommand();
		}

		public void CallbackRestCommand(){
			battleManager.CallbackRestCommand();
		}

		public void CallbackOnPointerEnterRestCommand(){
			battleManager.CallbackOnPointerEnterRestCommand();
		}

		public void CallbackOnPointerExitRestCommand(){
			battleManager.CallbackOnPointerExitRestCommand();
		}

		public void CallbackStandbyCommand(){
			battleManager.CallbackStandbyCommand();
		}
	}
}