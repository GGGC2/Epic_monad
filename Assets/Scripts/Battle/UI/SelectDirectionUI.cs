using UnityEngine;

namespace BattleUI
{
	public class SelectDirectionUI : MonoBehaviour
	{
		private BattleManager battleManager;

		public void Start()
		{
			battleManager = FindObjectOfType<BattleManager>();
		}

		public void CallbackDirection(string directionString)
		{
			battleManager.CallbackDirection(directionString);
		}
	}
}
