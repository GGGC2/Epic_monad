using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
	GameObject commandUI;
	GameObject skillUI;
	GameObject skillCheckUI;
	GameObject destCheckUI;
	GameObject unitViewerUI;
	GameObject selectedUnitViewerUI;
	GameObject tileViewerUI;
	GameObject selectDirectionUI;
	GameObject cancelButtonUI;

	private void Awake()
	{
		commandUI = GameObject.Find("CommandPanel");
		skillUI = GameObject.Find("SkillPanel");
		skillCheckUI = GameObject.Find("SkillCheckPanel");
		destCheckUI = GameObject.Find("DestCheckPanel");
		unitViewerUI = GameObject.Find("UnitViewerPanel");
		selectedUnitViewerUI = GameObject.Find("SelectedUnitViewerPanel");
		tileViewerUI = GameObject.Find("TileViewerPanel");
		selectDirectionUI = GameObject.Find("SelectDirectionUI");
		cancelButtonUI = GameObject.Find("CancelButtonPanel");
	}

	private void Start()
	{
		commandUI.SetActive(false);
		skillUI.SetActive(false);
		skillCheckUI.SetActive(false);
		destCheckUI.SetActive(false);
		unitViewerUI.SetActive(false);
		selectedUnitViewerUI.SetActive(false);
		tileViewerUI.SetActive(false);
		selectDirectionUI.SetActive(false);
		cancelButtonUI.SetActive(false);
	}

	public void SetCommandUIName(GameObject selectedUnitObject)
	{
		commandUI.SetActive(true);
		commandUI.transform.Find("NameText").GetComponent<Text>().text = selectedUnitObject.GetComponent<Unit>().GetName();
	}

	public void DisableCommandUI()
	{
		commandUI.SetActive(false);
	}

	public void UpdateSkillInfo(GameObject selectedUnitObject)
	{
		skillUI.SetActive(true);

		List<Skill> skillList = selectedUnitObject.GetComponent<Unit>().GetSkillList();
		for (int i = 0; i < skillList.Count; i++)
		{
			GameObject skillButton = GameObject.Find((i + 1).ToString() + "SkillButton"); //?? skillUI.transform.Find(i + "SkillButton")
			skillButton.transform.Find("NameText").GetComponent<Text>().text = skillList[i].GetName();
			skillButton.transform.Find("APText").GetComponent<Text>().text = skillList[i].GetRequireAP().ToString() + " AP";
			skillButton.transform.Find("CooldownText").GetComponent<Text>().text = "";
		}
	}

	public void CheckUsableSkill(GameObject selectedUnitObject)
	{
		List<Skill> skillList = selectedUnitObject.GetComponent<Unit>().GetSkillList();
		for (int i = 0; i < skillList.Count; i++)
		{
			GameObject.Find((i + 1).ToString() + "SkillButton").GetComponent<Button>().interactable = true;
			if (selectedUnitObject.GetComponent<Unit>().GetCurrentActivityPoint() < skillList[i].GetRequireAP())
			{
				GameObject.Find((i + 1).ToString() + "SkillButton").GetComponent<Button>().interactable = false;
			}
		}
	}

	public void DisableSkillUI()
	{
		skillUI.SetActive(false);
	}

	public void SetSkillCheckAP(GameObject selectedUnitObject, Skill selectedSkill)
	{
		skillCheckUI.SetActive(true);
		int requireAP = selectedSkill.GetRequireAP();
		string newAPText = "소모 AP : " + requireAP + "\n" +
			"잔여 AP : " + (selectedUnitObject.GetComponent<Unit>().GetCurrentActivityPoint() - requireAP);
		skillCheckUI.transform.Find("APText").GetComponent<Text>().text = newAPText;
	}

	public void EnableSkillCheckChainButton(bool isPossible)
	{
		skillCheckUI.SetActive(true);
		GameObject.Find("ChainButton").GetComponent<Button>().interactable = isPossible;
	}

	public void DisableSkillCheckUI()
	{
		skillCheckUI.SetActive(false);
	}

	public void SetDestCheckUIAP(GameObject selectedUnitObject, int totalUseActionPoint)
	{
		destCheckUI.SetActive(true);
		string newAPText = "소모 AP : " + totalUseActionPoint + "\n" +
			"잔여 AP : " + (selectedUnitObject.GetComponent<Unit>().GetCurrentActivityPoint() - totalUseActionPoint);
		destCheckUI.transform.Find("APText").GetComponent<Text>().text = newAPText;
	}

	public void DisableDestCheckUI()
	{
		destCheckUI.SetActive(false);
	}

	public void UpdateUnitViewer(GameObject unitOnTile)
	{
		unitViewerUI.SetActive(true);
		FindObjectOfType<UnitViewer>().UpdateUnitViewer(unitOnTile);
	}

	public bool IsUnitViewerShowing()
	{
		return unitViewerUI.activeInHierarchy;
	}

	public void DisableUnitViewer()
	{
		unitViewerUI.SetActive(false);
	}

	public void SetSelectedUnitViewerUI(GameObject selectedUnitObject)
	{
		selectedUnitViewerUI.SetActive(true);
		selectedUnitViewerUI.GetComponent<SelectedUnitViewer>().UpdateUnitViewer(selectedUnitObject);
	}

	public void DisableSelectedUnitViewerUI()
	{
		selectedUnitViewerUI.SetActive(false);
	}

	public void SetTileViewer(GameObject tile)
	{
		tileViewerUI.SetActive(true);
		FindObjectOfType<TileViewer>().UpdateTileViewer(tile);
	}

	public void DisableTileViewerUI()
	{
		tileViewerUI.SetActive(false);
	}

	public void EnableSelectDirectionUI()
	{
		selectDirectionUI.SetActive(true);
	}

	public void DisableSelectDirectionUI()
	{
		selectDirectionUI.SetActive(false);
	}

	public void EnableCancelButtonUI()
	{
		cancelButtonUI.SetActive(true);
	}

	public void DisableCancelButtonUI()
	{
		cancelButtonUI.SetActive(false);
	}
}
