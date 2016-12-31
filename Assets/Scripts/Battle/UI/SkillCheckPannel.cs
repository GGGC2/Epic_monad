using UnityEngine;

namespace BattleUI
{
	public class SkillCheckPannel : MonoBehaviour
	{
		private BattleManager battleManager;

		public void Start()
		{
			battleManager = FindObjectOfType<BattleManager>();
		}

		public void CallbackApplyCommand()
		{
			battleManager.CallbackApplyCommand();
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
