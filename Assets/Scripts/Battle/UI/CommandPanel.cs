using UnityEngine;

namespace BattleUI
{
	public class CommandPanel : MonoBehaviour
	{
		private BattleManager battleManager;

		public void Start()
		{
			battleManager = FindObjectOfType<BattleManager>();
		}

		public void CallbackMoveCommand()
		{
			battleManager.CallbackMoveCommand();
		}

		public void CallbackSkillCommand()
		{
			battleManager.CallbackSkillCommand();
		}

		public void CallbackRestCommand()
		{
			battleManager.CallbackRestCommand();
		}

		public void CallbackOnPointerEnterRestCommand()
		{
			battleManager.CallbackOnPointerEnterRestCommand();
		}

		public void CallbackOnPointerExitRestCommand()
		{
			battleManager.CallbackOnPointerExitRestCommand();
		}

		public void CallbackStandbyCommand()
		{
			battleManager.CallbackStandbyCommand();
		}
	}
}
