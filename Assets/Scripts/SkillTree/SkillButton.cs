using UnityEngine;
using UnityEngine.UI;
using Save;

namespace SkillTree
{

enum SkillButtonState
{
	NotExist,
	LearnedMaxEnhanced,
	LearnedEnhanceable,
	Learnable,
	NotLearnable
}

class SkillButton : MonoBehaviour
{
	SkillButtonState state = SkillButtonState.NotExist;
	GameObject text;
	GameObject icon;
	GameObject border;
	SkillInfo skillInfo;
	GameObject level;

	public void Awake()
	{
		text = transform.Find("Text").gameObject;
		icon = transform.Find("Icon").gameObject;
		level = transform.Find("Level").gameObject;
		border = gameObject;

		UpdateState(null);
	}

	public void SetSkillInfo(SkillInfo skillInfo)
	{
		this.skillInfo = skillInfo;
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


		level.GetComponent<Text>().enabled = false;
		icon.GetComponent<Image>().enabled = false;
		text.GetComponent<Text>().enabled = false;
		GetComponent<Button>().onClick.RemoveAllListeners();
		GetComponent<Button>().interactable = false;

		switch (state)
		{
			case SkillButtonState.NotExist:
				break;
			case SkillButtonState.LearnedEnhanceable:
				icon.GetComponent<Image>().enabled = true;
				text.GetComponent<Text>().enabled = true;
				text.GetComponent<Text>().text = skillInfo.skill.GetName();
				level.GetComponent<Text>().enabled = true;
				level.GetComponent<Text>().text = SkillDB.GetEnhanceLevel(skillTreeManager.SelectedUnitName, skillInfo.skill.GetName()).ToString();
				GetComponent<Button>().interactable = true;
				button.onClick.AddListener(() => skillTreeManager.OnSkillButtonClick(skillInfo.skill.GetName()));
				break;
			case SkillButtonState.LearnedMaxEnhanced:
				icon.GetComponent<Image>().enabled = true;
				text.GetComponent<Text>().enabled = true;
				text.GetComponent<Text>().text = skillInfo.skill.GetName();
				level.GetComponent<Text>().enabled = true;
				level.GetComponent<Text>().text = SkillDB.GetEnhanceLevel(skillTreeManager.SelectedUnitName, skillInfo.skill.GetName()).ToString();
				GetComponent<Button>().interactable = true;
				button.animationTriggers.highlightedTrigger = "Normal";
				break;
			case SkillButtonState.Learnable:
				icon.GetComponent<Image>().enabled = true;
				text.GetComponent<Text>().enabled = true;
				text.GetComponent<Text>().text = skillInfo.skill.GetName();
				level.GetComponent<Text>().enabled = true;
				level.GetComponent<Text>().text = SkillDB.GetEnhanceLevel(skillTreeManager.SelectedUnitName, skillInfo.skill.GetName()).ToString();
				GetComponent<Button>().interactable = true;
				button.animationTriggers.normalTrigger = "Disabled";
				button.onClick.AddListener(() => skillTreeManager.OnSkillButtonClick(skillInfo.skill.GetName()));
				break;
			case SkillButtonState.NotLearnable:
				icon.GetComponent<Image>().enabled = true;
				text.GetComponent<Text>().enabled = true;
				text.GetComponent<Text>().text = skillInfo.skill.GetName();
				level.GetComponent<Text>().enabled = true;
				level.GetComponent<Text>().text = SkillDB.GetEnhanceLevel(skillTreeManager.SelectedUnitName, skillInfo.skill.GetName()).ToString();
				GetComponent<Button>().interactable = false;
				break;
			default:
				Debug.LogError("Invalid state : " + state);
				break;
		}
	}
}
}
