using UnityEngine;
using UnityEngine.UI;

namespace BattleUI
{
	public class SkillPanel : MonoBehaviour
	{
		private BattleManager battleManager;
		private Text skillDataText;

		public void Start()
		{
			battleManager = FindObjectOfType<BattleManager>();
			skillDataText = GameObject.Find("SkillDataText").GetComponent<Text>();
			skillDataText.text = "";
		}

		public void CallbackSkillIndex(int index)
		{
			battleManager.CallbackSkillIndex(index);
		}

		public void CallbackPointerEnterSkillIndex(int index)
		{
			battleManager.CallbackPointerEnterSkillIndex(index);			
			skillDataText.text = battleManager.battleData.PreSelectedSkill.GetSkillDataText();
		}

		public void CallbackPointerExitSkillIndex(int index)
		{
			battleManager.CallbackPointerExitSkillIndex(index);
			skillDataText.text = "";
		}

		public void CallbackSkillUICancel()
		{
			battleManager.CallbackSkillUICancel();
		}
	}
}
