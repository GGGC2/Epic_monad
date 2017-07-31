using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace BattleUI
{
	public class SkillPanel : MonoBehaviour
	{
		private BattleManager battleManager;
		public SkillUIManager skillPanel;
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
		public List<Button> skillButtons;

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
		public void CallbackSkillIndex(int index){
			battleManager.CallbackSkillIndex(index);
		}

		public void Update(){
			if(battleManager.battleData.currentState == CurrentState.SelectSkill){
				if(Input.GetKeyDown(KeyCode.A) && skillButtons[0].interactable && skillButtons[0].gameObject.activeSelf)
					battleManager.CallbackSkillIndex(1);
				else if(Input.GetKeyDown(KeyCode.S) && skillButtons[1].interactable && skillButtons[1].gameObject.activeSelf)
					battleManager.CallbackSkillIndex(2);
				else if(Input.GetKeyDown(KeyCode.D) && skillButtons[2].interactable && skillButtons[2].gameObject.activeSelf)
					battleManager.CallbackSkillIndex(3);
				else if(Input.GetKeyDown(KeyCode.F) && skillButtons[3].interactable && skillButtons[3].gameObject.activeSelf)
					battleManager.CallbackSkillIndex(4);
				else if(Input.GetKeyDown(KeyCode.G) && skillButtons[4].interactable && skillButtons[4].gameObject.activeSelf)
					battleManager.CallbackSkillIndex(5);
			}
		}

		public void CallbackPointerEnterSkillIndex(int index){
			battleManager.CallbackPointerEnterSkillIndex(index);
			
			ActiveSkill preSelectedSkill = battleManager.battleData.PreSelectedSkill;
			skillPanel.UpdateSkillInfoUI(preSelectedSkill, battleManager.battleData.selectedUnit.name);
		}

		void OnEnable(){
			skillApText.text = "";
			skillCooldownText.text = "";	
			skillDataText.text = "";
			actualRange.sprite = transparent;
			rangeType.sprite = transparent;
			rangeText.text = "";
		}

		string GetFirstRangeText(ActiveSkill skill){
			string result = "";
			if(skill.GetFirstMinReach() > 1)
				result = skill.GetFirstMinReach()+"~";
			return result + skill.GetFirstMaxReach();
		}

		public string GetSkillBasePower(Unit unit, ActiveSkill skill){
			return ((int)(skill.GetPowerFactor(Enums.Stat.Power)*(float)unit.GetStat(Enums.Stat.Power))).ToString();
		}

		public void CallbackPointerExitSkillIndex(int index){
			battleManager.CallbackPointerExitSkillIndex(index);
			OnEnable();
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
