using UnityEngine;

namespace BattleUI
{
	public class SkillPanel : MonoBehaviour
	{
		private BattleManager gameManager;

		public void Start()
		{
			gameManager = FindObjectOfType<BattleManager>();
		}

		public void CallbackSkillIndex(int index)
		{
			gameManager.CallbackSkillIndex(index);
		}

		public void CallbackPointerEnterSkillIndex(int index)
		{
			gameManager.CallbackPointerEnterSkillIndex(index);
		}

		public void CallbackPointerExitSkillIndex(int index)
		{
			gameManager.CallbackPointerExitSkillIndex(index);
		}

		public void CallbackSkillUICancel()
		{
			gameManager.CallbackSkillUICancel();
		}
	}
}
