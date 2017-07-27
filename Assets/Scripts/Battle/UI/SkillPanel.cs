using UnityEngine;
using UnityEngine.UI;
using System;

namespace BattleUI
{
	public class SkillPanel : MonoBehaviour
	{
		private BattleManager battleManager;
		Text skillApText;
		Text skillDataText;
		public Text rangeText;
		Text skillCooldownText;
		public Image rangeType;
		public Image actualRange;
        int page = 0;
        int maxPage = 0;
        Unit selectedUnit;
		public Sprite transparent;

		public void Awake(){
			battleManager = FindObjectOfType<BattleManager>();
			skillApText = GameObject.Find("SkillApText").GetComponent<Text>();
			skillApText.text = "";
			skillCooldownText = GameObject.Find("SkillCooldownText").GetComponent<Text>();
			skillCooldownText.text = "";
			skillDataText = GameObject.Find("SkillDataText").GetComponent<Text>();
			skillDataText.text = "";
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

		public void Update(){
			if(battleManager.battleData.currentState == CurrentState.SelectSkill){
				if(Input.GetKeyDown(KeyCode.A))
					battleManager.CallbackSkillIndex(1);
				else if(Input.GetKeyDown(KeyCode.S))
					battleManager.CallbackSkillIndex(2);
				else if(Input.GetKeyDown(KeyCode.D))
					battleManager.CallbackSkillIndex(3);
				else if(Input.GetKeyDown(KeyCode.F))
					battleManager.CallbackSkillIndex(4);
				else if(Input.GetKeyDown(KeyCode.G))
					battleManager.CallbackSkillIndex(5);
			}
		}

		public void CallbackPointerEnterSkillIndex(int index){
			battleManager.CallbackPointerEnterSkillIndex(index);
			
			Skill preSelectedSkill = battleManager.battleData.PreSelectedSkill;
			
			skillApText.text = preSelectedSkill.GetRequireAP().ToString();
			
			int cooldown = preSelectedSkill.GetCooldown();
			if (cooldown > 0)
				skillCooldownText.text = "재사용까지 " + cooldown.ToString() + " 페이즈";
			skillDataText.text = preSelectedSkill.GetSkillDataText().Replace("VALUE", GetSkillBasePower(battleManager.battleData.selectedUnit, preSelectedSkill));
			
			Sprite actualRangeImage = Resources.Load<Sprite>("SkillRange/"+battleManager.battleData.selectedUnit.name+preSelectedSkill.GetColumn()+"_"+preSelectedSkill.GetRequireLevel());
			if(actualRangeImage != null)
				actualRange.sprite = actualRangeImage;

			if(preSelectedSkill.GetSkillType() == Enums.SkillType.Point){
				rangeType.sprite = Resources.Load<Sprite>("Icon/Skill/SkillType/Target");
				rangeText.text += GetFirstRangeText(preSelectedSkill);
			}
			else if(preSelectedSkill.GetSkillType() == Enums.SkillType.Route){
				rangeType.sprite = Resources.Load<Sprite>("Icon/Skill/SkillType/Line");
				rangeText.text += GetFirstRangeText(preSelectedSkill);
			}
			else
				rangeType.sprite = Resources.Load<Sprite>("Icon/Skill/SkillType/Auto");
		}

		void OnEnable(){
			skillApText.text = "";
			skillCooldownText.text = "";	
			skillDataText.text = "";
			actualRange.sprite = Resources.Load<Sprite>("Icon/Empty");
			rangeType.sprite = Resources.Load<Sprite>("Icon/Empty");
			rangeText.text = "";
		}

		string GetFirstRangeText(Skill skill){
			string result = "";
			if(skill.GetFirstMinReach() > 1)
				result = skill.GetFirstMinReach()+"~";
			return result + skill.GetFirstMaxReach();
		}

		public string GetSkillBasePower(Unit unit, Skill skill){
			return ((int)(skill.GetPowerFactor(Enums.Stat.Power)*(float)unit.GetStat(Enums.Stat.Power))).ToString();
		}

		public void CallbackPointerExitSkillIndex(int index)
		{
			battleManager.CallbackPointerExitSkillIndex(index);
			skillApText.text = "";
			rangeText.text = "";
			skillCooldownText.text = "";
			skillDataText.text = "";
			rangeType.sprite = transparent;
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
