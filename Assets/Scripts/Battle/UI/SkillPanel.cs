using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace BattleUI{
	public class SkillPanel : MonoBehaviour{
		private BattleManager battleManager;
		public SkillViewer skillPanel;
		Text skillApText;
		Text skillDataText;
		public Text rangeText;
		Text skillCooldownText;
		public Image rangeType;
        int page = 0;
        int maxPage = 0;
		public const int onePageButtonsNum = 5;
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
        }

		/*public void OnOffSkillButton(int skillIndex, bool turnOn){//첫번째 스킬의 index가 1이다(0이 아니라)
			if (skillsOnOffLockOn)
				return;

			Color enabledColor = new Color(1, 1, 1);
			Color disabledColor = new Color(1, 0, 0);

			int indexInCurrentPage = IndexInCurrentPage(skillIndex);
			Button skillButton = skillButtons [indexInCurrentPage];
			if (skillButton == null)
				return;

			if (turnOn) {
				skillButton.interactable = true;
				skillButton.GetComponentInChildren<Text> ().color = enabledColor;
			}
			else {
				skillButton.interactable = false;
				skillButton.GetComponentInChildren<Text> ().color = disabledColor;
			}
		}*/

		bool skillsOnOffLockOn = false;
		public void LockSkillsOnOff(){
			skillsOnOffLockOn = true;
		}
		public void UnlockSkillsOnOff(){
			skillsOnOffLockOn = false;
		}

		public void AddListenerToSkillButton(int skillIndex, UnityEngine.Events.UnityAction action){
			//skillButtons [IndexInCurrentPage (skillIndex)].onClick.AddListener (action);
		}
		public void RemoveListenerToSkillButton(int skillIndex, UnityEngine.Events.UnityAction action){
			//skillButtons [IndexInCurrentPage (skillIndex)].onClick.RemoveListener (action);
		}

		int IndexInCurrentPage(int skillIndex){ // value : 0~4
			return (skillIndex - 1) % onePageButtonsNum;
		}

		public void Update(){
			/*if(BattleData.currentState == CurrentState.SelectSkill && Setting.shortcutEnable){
				if (Input.GetKeyDown (KeyCode.A) && skillButtons [0].interactable && skillButtons [0].gameObject.activeSelf)
					skillButtons [0].onClick.Invoke ();
				else if(Input.GetKeyDown(KeyCode.S) && skillButtons[1].interactable && skillButtons[1].gameObject.activeSelf)
					skillButtons [1].onClick.Invoke ();
				else if(Input.GetKeyDown(KeyCode.D) && skillButtons[2].interactable && skillButtons[2].gameObject.activeSelf)
					skillButtons [2].onClick.Invoke ();
				else if(Input.GetKeyDown(KeyCode.F) && skillButtons[3].interactable && skillButtons[3].gameObject.activeSelf)
					skillButtons [3].onClick.Invoke ();
				else if(Input.GetKeyDown(KeyCode.G) && skillButtons[4].interactable && skillButtons[4].gameObject.activeSelf)
					skillButtons [4].onClick.Invoke ();
			}*/
		}

		/*public void CallbackPointerEnterSkillIndex(int index){
            index += page * 5;
			battleManager.CallbackPointerEnterSkillIndex(index);
			
			ActiveSkill preSelectedSkill = BattleData.PreSelectedSkill;
			skillPanel.UpdateSkillInfoUI(preSelectedSkill, BattleData.selectedUnit.name);
			GameObject.Find("SelectedUnitViewerPanel").GetComponent<UnitViewer>().PreviewAp(BattleData.selectedUnit, preSelectedSkill.GetRequireAP());
		}

		public void CallbackPointerExitSkillIndex(int index){
            index += page * 5;
			battleManager.CallbackPointerExitSkillIndex(index);
			OnEnable();
			skillPanel.HideSecondRange();
			GameObject.Find("SelectedUnitViewerPanel").GetComponent<UnitViewer>().OffPreviewAp();
		}*/

		void OnEnable(){
			skillApText.text = "";
			skillCooldownText.text = "";	
			skillDataText.text = "";
			rangeType.sprite = transparent;
			rangeText.text = "";
		}

		void OnDisable(){
			skillPanel.HideSecondRange();
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

		public void CallbackSkillUICancel()
		{
			battleManager.CallbackSkillUICancel();
		}
	}
}