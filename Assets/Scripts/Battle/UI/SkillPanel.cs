using UnityEngine;

namespace BattleUI
{
	public class SkillPanel : MonoBehaviour
	{
		private BattleManager battleManager;

		public void Start()
		{
			battleManager = FindObjectOfType<BattleManager>();
		}

		public void CallbackSkillIndex(int index)
		{
			battleManager.CallbackSkillIndex(index);
		}

		public void CallbackPointerEnterSkillIndex(int index)
		{
			battleManager.CallbackPointerEnterSkillIndex(index);
		}

		public void CallbackPointerExitSkillIndex(int index)
		{
			battleManager.CallbackPointerExitSkillIndex(index);
		}

		public void CallbackSkillUICancel()
		{
			battleManager.CallbackSkillUICancel();
		}
	}
}
