using UnityEngine;
using UnityEngine.UI;
using Save;

namespace SkillTree
{

enum SkillButtonState{
	NotExist,
	LearnedMaxEnhanced,
	LearnedEnhanceable,
	Learnable,
	NotLearnable
}

class SkillButton : MonoBehaviour{
	SkillButtonState state = SkillButtonState.NotExist;
	GameObject text;
	GameObject icon;
	GameObject border;
	Skill skill;
	GameObject level;

	public void Awake(){
		text = transform.Find("Text").gameObject;
		icon = transform.Find("Icon").gameObject;
		level = transform.Find("Level").gameObject;
		border = gameObject;

		UpdateState(null);
	}

	public void SetSkillInfo(Skill skill)
	{
		this.skill = skill;
	}

	public void ChangeState(SkillTreeManager skillTreeManager, SkillButtonState state)
	{
		this.state = state;
		UpdateState(skillTreeManager);
	}

	private void UpdateState(SkillTreeManager skillTreeManager)
	{
		Button button = GetComponent<Button>();
		button.animationTriggers.normalTrigger = "Normal";
		button.animationTriggers.highlightedTrigger = "Highlighted";


		border.GetComponent<Image>().enabled = false;
		level.GetComponent<Text>().enabled = false;
		icon.GetComponent<Image>().enabled = false;
		text.GetComponent<Text>().enabled = false;
		GetComponent<Button>().onClick.RemoveAllListeners();
		GetComponent<Button>().interactable = false;

		switch (state){
			case SkillButtonState.NotExist:
				button.animator.SetTrigger(button.animationTriggers.disabledTrigger);
				break;
			case SkillButtonState.LearnedEnhanceable:
				border.GetComponent<Image>().enabled = true;
				LoadIcon(skillTreeManager);
				icon.GetComponent<Image>().enabled = true;
				text.GetComponent<Text>().enabled = true;
				text.GetComponent<Text>().text = skill.korName;
				level.GetComponent<Text>().enabled = true;
				level.GetComponent<Text>().text = SkillDB.GetEnhanceLevel(skillTreeManager.SelectedUnitName, skill.korName).ToString();
				GetComponent<Button>().interactable = true;
				button.onClick.AddListener(() => skillTreeManager.OnSkillButtonClick(skill.korName));
				button.animator.SetTrigger(button.animationTriggers.normalTrigger);
				break;
			case SkillButtonState.LearnedMaxEnhanced:
				border.GetComponent<Image>().enabled = true;
				LoadIcon(skillTreeManager);
				icon.GetComponent<Image>().enabled = true;
				text.GetComponent<Text>().enabled = true;
				text.GetComponent<Text>().text = skill.korName;
				level.GetComponent<Text>().enabled = true;
				level.GetComponent<Text>().text = SkillDB.GetEnhanceLevel(skillTreeManager.SelectedUnitName, skill.korName).ToString();
				GetComponent<Button>().interactable = true;
				button.animationTriggers.highlightedTrigger = "Normal";
				button.animator.SetTrigger(button.animationTriggers.normalTrigger);
				break;
			case SkillButtonState.Learnable:
				border.GetComponent<Image>().enabled = true;
				LoadIcon(skillTreeManager);
				icon.GetComponent<Image>().enabled = true;
				text.GetComponent<Text>().enabled = true;
				text.GetComponent<Text>().text = skill.korName;
				level.GetComponent<Text>().enabled = true;
				level.GetComponent<Text>().text = SkillDB.GetEnhanceLevel(skillTreeManager.SelectedUnitName, skill.korName).ToString();
				GetComponent<Button>().interactable = true;
				button.animationTriggers.normalTrigger = "Disabled";
				button.onClick.AddListener(() => skillTreeManager.OnSkillButtonClick(skill.korName));
				button.animator.SetTrigger(button.animationTriggers.normalTrigger);
				break;
			case SkillButtonState.NotLearnable:
				border.GetComponent<Image>().enabled = true;
				LoadIcon(skillTreeManager);
				icon.GetComponent<Image>().enabled = true;
				text.GetComponent<Text>().enabled = true;
				text.GetComponent<Text>().text = skill.korName;
				level.GetComponent<Text>().enabled = true;
				level.GetComponent<Text>().text = SkillDB.GetEnhanceLevel(skillTreeManager.SelectedUnitName, skill.korName).ToString();
				GetComponent<Button>().interactable = false;
				button.animator.SetTrigger(button.animationTriggers.disabledTrigger);
				break;
			default:
				Debug.LogError("Invalid state : " + state);
				break;
		}
	}

	private void LoadIcon(SkillTreeManager skillTreeManager)
	{
		string path = "Icon/" + skillTreeManager.SelectedUnitName + "_" + skill.row + "_" + skill.requireLevel;
		Debug.Log("Path is " + path);
		Sprite sprite = Resources.Load(path, typeof(Sprite)) as Sprite;
		icon.GetComponent<Image>().sprite = sprite;
	}
}
}
