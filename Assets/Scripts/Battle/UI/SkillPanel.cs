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
		Image range1Image;
        int page = 0;
        int maxPage = 0;
        Unit selectedUnit;

		//Enums.RangeForm과 값이 정확히 맞아야 함
		public Sprite[] RangeFormIcons;
		public Sprite transparent;

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
			range1Image = GameObject.Find("SkillRange1Image").GetComponent<Image>();
		}

        public int GetPage() { return page; }
        public void SetMaxPage(int maxPage) { this.maxPage = maxPage; }

        public void triggerEnabled(Unit selectedUnit) {
            this.selectedUnit = selectedUnit;
            page = 0;
            Button prevButton = gameObject.transform.Find("PrevSkillPageButton").GetComponent<Button>();
            Button nextButton = gameObject.transform.Find("NextSkillPageButton").GetComponent<Button>();
            prevButton.interactable = false;
            if (maxPage == 0) nextButton.interactable = false;
            else nextButton.interactable = true;
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
			
			int cooldown = preSelectedSkill.GetCooldown();
			if (cooldown > 0)
				skillCooldownText.text = "재사용까지 " + cooldown.ToString() + " 페이즈";
			skillDataText.text = preSelectedSkill.GetSkillDataText();
			
			/*if(preSelectedSkill.GetSkillType() == Enums.SkillType.Auto)
			{
				range1Image.sprite = transparent;
			}
			else
			{
				skillRange1Text.text = preSelectedSkill.GetFirstMinReach().ToString() + "-" + preSelectedSkill.GetFirstMaxReach().ToString();
				range1Image.sprite = RangeFormIcons[(int)preSelectedSkill.GetFirstRangeForm()];
			}*/
		}

		public void CallbackPointerExitSkillIndex(int index)
		{
			battleManager.CallbackPointerExitSkillIndex(index);
			skillApText.text = "";
			skillRange1Text.text = "";
			skillCooldownText.text = "";
			skillDataText.text = "";
			range1Image.sprite = transparent;
		}

		public void CallbackSkillUICancel()
		{
			battleManager.CallbackSkillUICancel();
		}

        public void CallbackNextPage() {
            page += 1;
            if (page > 0) {
                Button prevButton = gameObject.transform.Find("PrevSkillPageButton").GetComponent<Button>();
                prevButton.interactable = true;
            }
            if (page >= maxPage) {
                Button nextButton = gameObject.transform.Find("NextSkillPageButton").GetComponent<Button>();
                nextButton.interactable = false;
            }
            MonoBehaviour.FindObjectOfType<UIManager>().UpdateSkillInfo(selectedUnit);
        }
        public void CallbackPrevPage() {
            page -= 1;
            if (page <= 0) {
                Button prevButton = gameObject.transform.Find("PrevSkillPageButton").GetComponent<Button>();
                prevButton.interactable = false;
            }
            if (page < maxPage) {
                Button nextButton = gameObject.transform.Find("NextSkillPageButton").GetComponent<Button>();
                nextButton.interactable = true;
            }
            MonoBehaviour.FindObjectOfType<UIManager>().UpdateSkillInfo(selectedUnit);
        }
	}
}
