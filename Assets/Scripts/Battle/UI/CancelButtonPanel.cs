using UnityEngine;

namespace BattleUI
{
	public class CancelButtonPanel : MonoBehaviour
	{
		private BattleManager battleManager;

		public void Start()
		{
			battleManager = FindObjectOfType<BattleManager>();
		}

		public void CallbackCancel()
		{
			battleManager.CallbackCancel();
		}
	}
}
