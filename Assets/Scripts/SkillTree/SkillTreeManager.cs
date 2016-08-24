using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Save;

namespace SkillTree
{

[System.Serializable]
class Description
{
	public GameObject name;
	public GameObject level;
	public GameObject skillPoint;
}

[System.Serializable]
public class SkillColumnInfo
{
	public string unitName;
	public string column1Name;
	public string column2Name;
	public string column3Name;

	public SkillColumnInfo(string line)
	{
		CommaStringParser commaParser = new CommaStringParser(line);
		this.unitName = commaParser.Consume();
		this.column1Name = commaParser.Consume();
		this.column2Name = commaParser.Consume();
		this.column3Name = commaParser.Consume();
	}
}

public class SkillTreeManager : MonoBehaviour
{
	const int maxEnhanceLevel = 5;
	int selectedIndex = 0;
	public string SelectedUnitName
	{
		get
		{
			return partyUnitNames[selectedIndex];
		}
	}

	List<GameObject> nameButtons;
	GameObject column1Text;
	GameObject column2Text;
	GameObject column3Text;
	List<GameObject> column1Skills;
	List<GameObject> column2Skills;
	List<GameObject> column3Skills;
	Description description;
	GameObject standing;
	GameObject root;

	List<string> partyUnitNames = new List<string>();
	List<UnitInfo> unitInfos = new List<UnitInfo>();
	List<SkillInfo> skillInfos = new List<SkillInfo>();
	Dictionary<string, SkillInfo> skillInfoByName = new Dictionary<string, SkillInfo>();
	List<SkillColumnInfo> skillColumnInfos = new List<SkillColumnInfo>();

	void Start()
	{
		partyUnitNames = PartyDB.GetPartyUnits();

		unitInfos = Parser.GetParsedUnitInfo();
		skillInfos = Parser.GetParsedSkillInfo();
		foreach (SkillInfo skillInfo in skillInfos)
		{
			string skillName = skillInfo.skill.GetName();
			skillInfoByName[skillName] = skillInfo;
		}
		skillColumnInfos = Parser.GetSkillColumInfo();

		InitializeUIs();

		selectedIndex = 0;

		UpdateNameButtons();
		UpdateSkills(selectedIndex);
		UpdateDetail(selectedIndex);
		UpdateUnitImage(selectedIndex);
	}

	private void InitializeUIs()
	{
		root = GameObject.Find("Canvas");

		nameButtons = new List<GameObject>();
		Transform characterNames = root.transform.Find("CharacterNames");
		for (int i=1; i<=7; i++)
		{
			nameButtons.Add(characterNames.Find("name" + i).gameObject);
		}

		column1Skills = new List<GameObject>();
		Transform column1 = root.transform.Find("columns/left");
		Transform column2 = root.transform.Find("columns/middle");
		Transform column3 = root.transform.Find("columns/right");

		column1Text = column1.Find("Text").gameObject;
		column2Text = column2.Find("Text").gameObject;
		column3Text = column3.Find("Text").gameObject;

		column1Skills = new List<GameObject>();
		column2Skills = new List<GameObject>();
		column3Skills = new List<GameObject>();
		for (int i=1; i<=11; i++)
		{
			column1Skills.Add(column1.Find("skills/skill" + i).gameObject);
			column2Skills.Add(column2.Find("skills/skill" + i).gameObject);
			column3Skills.Add(column3.Find("skills/skill" + i).gameObject);
		}

		Transform descriptionTransform = root.transform.Find("description");
		this.description = new Description();
		this.description.name = descriptionTransform.Find("name").gameObject;
		this.description.level = descriptionTransform.Find("levelValue").gameObject;
		this.description.skillPoint = descriptionTransform.Find("skillPointValue").gameObject;

		this.standing = root.transform.Find("Standing").gameObject;
	}

	private void UpdateNameButtons()
	{
		foreach (GameObject nameGO in nameButtons)
		{
			Text text = nameGO.transform.Find("text").GetComponent<Text>();
			text.text = "";
			nameGO.GetComponent<Image>().enabled = false;
		}

		for (int i=0; i<partyUnitNames.Count; i++)
		{
			Text text = nameButtons[i].transform.Find("text").GetComponent<Text>();
			text.text = GetUnitInfo(i).name;
			nameButtons[i].GetComponent<Image>().enabled = true;
		}
	}

	private void UpdateSkills(int selectedIndex)
	{
		InitializeSkillUI(column1Skills);
		InitializeSkillUI(column2Skills);
		InitializeSkillUI(column3Skills);

		SkillColumnInfo skillColumnInfo = GetSkillColumnInfo(selectedIndex);
		column1Text.GetComponent<Text>().text = skillColumnInfo.column1Name;
		column2Text.GetComponent<Text>().text = skillColumnInfo.column2Name;
		column3Text.GetComponent<Text>().text = skillColumnInfo.column3Name;

		UnitInfo unitInfo = GetUnitInfo(selectedIndex);
		string unitNameInCode = unitInfo.nameInCode;

		List<SkillInfo> unitSkills = new List<SkillInfo>();
		foreach (SkillInfo skillInfo in skillInfos)
		{
			if (skillInfo.owner == unitNameInCode)
			{
				unitSkills.Add(skillInfo);
			}
		}

		foreach (SkillInfo unitSkillInfo in unitSkills)
		{
			GameObject skillGameObject = GetSkillGameObject(unitSkillInfo.column, unitSkillInfo.requireLevel);
			skillGameObject.GetComponent<SkillButton>().SetSkillInfo(unitSkillInfo);

			Skill skill = unitSkillInfo.skill;
			bool isLearned = SkillDB.IsLearned(unitNameInCode, skill.GetName());
			bool haveSkillPoint = GetAvailableSkillPoint(unitNameInCode) > 0;
			bool isEnhanceable = SkillDB.GetEnhanceLevel(unitNameInCode, skill.GetName()) < maxEnhanceLevel && haveSkillPoint;

			int totalSkillPointUntilLevel = unitSkillInfo.requireLevel - 1;
			int usedSkillPointUntilLevel = GetUsedSkillPointUntilLevel(unitNameInCode, unitSkillInfo.requireLevel - 1);
			// bool isLearnable = usedSkillPointUntilLevel >= totalSkillPointUntilLevel && haveSkillPoint;
			bool isLearnable = PartyDB.GetPartyLevel() >= unitSkillInfo.GetRequireLevel() && haveSkillPoint;

			if (!isLearned && !isLearnable)
			{
				skillGameObject.GetComponent<SkillButton>().ChangeState(this, SkillButtonState.NotLearnable);
			}
			else if (!isLearned && isLearnable)
			{
				skillGameObject.GetComponent<SkillButton>().ChangeState(this, SkillButtonState.Learnable);
			}
			else if (isLearned && isEnhanceable)
			{
				skillGameObject.GetComponent<SkillButton>().ChangeState(this, SkillButtonState.LearnedEnhanceable);
			}
			else if (isLearned && !isEnhanceable)
			{
				skillGameObject.GetComponent<SkillButton>().ChangeState(this, SkillButtonState.LearnedMaxEnhanced);
			}
			else
			{
				Debug.LogError("Invalid case");
			}
		}
		//  required level validation is needed
	}

	private int GetAvailableSkillPoint(string unitName)
	{
		return PartyDB.GetPartyLevel() - GetAllUsedSkillPoint(unitName);
	}

	private int GetUsedSkillPointUntilLevel(string unitName, int level)
	{
		List<string> skillNames = SkillDB.GetLearnedSkillNames(unitName);
		int sum = 0;
		foreach (string skillName in skillNames)
		{
			if (skillInfoByName[skillName].requireLevel <= level)
			{
				sum += SkillDB.GetEnhanceLevel(unitName, skillName);
			}
		}
		return sum;
	}

	private int GetAllUsedSkillPoint(string unitName)
	{
		List<string> skillNames = SkillDB.GetLearnedSkillNames(unitName);
		int sum = 0;
		foreach (string skillName in skillNames)
		{
			sum += SkillDB.GetEnhanceLevel(unitName, skillName);
		}
		return sum;
	}

	private void InitializeSkillUI(List<GameObject> skills)
	{
		foreach (GameObject skill in skills)
		{
			skill.GetComponent<SkillButton>().ChangeState(this, SkillButtonState.NotExist);
		}
	}

	public void OnSkillButtonClick(string skillName)
	{
		Debug.Log("Skill clicked " + skillName);

		UnitInfo unitInfo = GetUnitInfo(selectedIndex);
		string unitNameInCode = unitInfo.nameInCode;
		bool haveSkillPoint = GetAvailableSkillPoint(unitNameInCode) > 0;
		bool isLearned = SkillDB.IsLearned(unitNameInCode, skillName);
		bool isEnhanceable = SkillDB.GetEnhanceLevel(unitNameInCode, skillName) < maxEnhanceLevel && haveSkillPoint;

		if (!isLearned)
		{
			SkillDB.Learn(unitNameInCode, skillName);
		}
		else if (isEnhanceable)
		{
			SkillDB.Enhance(unitNameInCode, skillName);
		}
		else
		{
			Debug.LogError("Cannot learn or enhance");
		}

		UpdateSkills(selectedIndex);
		UpdateDetail(selectedIndex);
	}

	private void UpdateDetail(int selectedIndex)
	{
		UnitInfo unitInfo = GetUnitInfo(selectedIndex);
		this.description.name.GetComponent<Text>().text = unitInfo.name;
		this.description.level.GetComponent<Text>().text = PartyDB.GetPartyLevel().ToString();
		this.description.skillPoint.GetComponent<Text>().text = GetAvailableSkillPoint(unitInfo.nameInCode).ToString();
	}

	private void UpdateUnitImage(int selectedIndex)
	{
		UnitInfo unitInfo = GetUnitInfo(selectedIndex);
		string resourceName = "StandingImage/" + unitInfo.nameInCode + "_standing";
		Sprite standingSprite = Resources.Load(resourceName, typeof(Sprite)) as Sprite;

		if (standingSprite == null)
		{
			Debug.LogError("Cannot find standing sprite " + resourceName);
		}

		standing.GetComponent<Image>().sprite = standingSprite;
	}

	public void OnNameButtonClicked(int index)
	{
		selectedIndex = index;
		UpdateNameButtons();
		UpdateSkills(selectedIndex);
		UpdateDetail(selectedIndex);
		UpdateUnitImage(selectedIndex);
	}

	private UnitInfo GetUnitInfo(int index)
	{
		foreach (UnitInfo unitInfo in unitInfos)
		{
			if (unitInfo.nameInCode == partyUnitNames[index])
			{
				return unitInfo;
			}
		}

		return null;
	}

	private SkillColumnInfo GetSkillColumnInfo(int index)
	{
		foreach (SkillColumnInfo skillColumnInfo in skillColumnInfos)
		{
			if (skillColumnInfo.unitName == partyUnitNames[index])
			{
				return skillColumnInfo;
			}
		}

		return null;
	}

	private GameObject GetSkillGameObject(int column, int requireLevel)
	{
		List<GameObject> skillsInColumn = null;
		switch (column)
		{
			case 1:
				skillsInColumn = column1Skills;
				break;
			case 2:
				skillsInColumn = column2Skills;
				break;
			case 3:
				skillsInColumn = column3Skills;
				break;
			default:
				Debug.LogError("Invalid column" + column);
				break;
		}

		List<int> requireLevels = new List<int> { 1, 6, 12, 18, 24, 30, 36, 42, 48, 54, 60 };
		int skillIndex = requireLevels.IndexOf(requireLevel);

		return skillsInColumn[skillIndex];
	}

	public void ReturnToWorldMap()
	{
		SceneManager.LoadScene("worldMap");
	}
}
}
