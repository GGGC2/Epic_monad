using UnityEngine;
using UnityEngine.UI;

namespace BattleUI
{
	public class SkillPanel : MonoBehaviour
	{
		private BattleManager battleManager;
		Text skillApText;
		Text skillDataText;
		Text skillRange1Text;
		Text skillCooldownText;

		public void Start()
		{
			battleManager = FindObjectOfType<BattleManager>();
			skillApText = GameObject.Find("SkillApText").GetComponent<Text>();
			skillApText.text = "";
			skillRange1Text = GameObject.Find("SkillRange1Text").GetComponent<Text>();
			skillRange1Text.text = "";
			skillCooldownText = GameObject.Find("SkillCooldownText").GetComponent<Text>();
			skillCooldownText.text = "";
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
			
			Skill preSelectedSkill = battleManager.battleData.PreSelectedSkill;
			
			skillApText.text = preSelectedSkill.GetRequireAP().ToString();
			skillRange1Text.text = preSelectedSkill.GetFirstMinReach().ToString() + "-" + preSelectedSkill.GetFirstMaxReach().ToString();
			int cooldown = preSelectedSkill.GetCooldown();
			if (cooldown > 0)
				skillCooldownText.text = "재사용까지 " + cooldown.ToString() + " 페이즈";
			skillDataText.text = preSelectedSkill.GetSkillDataText();
		}

		public void CallbackPointerExitSkillIndex(int index)
		{
			battleManager.CallbackPointerExitSkillIndex(index);
			skillApText.text = "";
			skillRange1Text.text = "";
			skillCooldownText.text = "";
			skillDataText.text = "";
		}

		public void CallbackSkillUICancel()
		{
			battleManager.CallbackSkillUICancel();
		}
	}
}
