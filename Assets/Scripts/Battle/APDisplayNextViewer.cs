using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine.UI;

public class APDisplayNextViewer : MonoBehaviour {

	public GameObject portraitPrefab;

	List<GameObject> portraits;

	public void UpdateAPDisplay(List<GameObject> units)
	{
		ClearViewer();
		
		List<GameObject> sortedUnits = SortByNextPhaseAP(units);
		
		int count = 1;
		foreach (var unit in sortedUnits)
		{   
			GameObject portrait = Instantiate(portraitPrefab) as GameObject;
			string imagePath = "UnitImage/portrait_" + unit.GetComponent<Unit>().GetNameInCode().ToString();
			portrait.GetComponent<Image>().sprite = Resources.Load(imagePath, typeof(Sprite)) as Sprite;
			
			ApplyNextAPText(unit, portrait);
			
			portraits.Add(portrait);
			
			portrait.transform.SetParent(GameObject.Find("APDisplayNextPanel").transform);
			portrait.GetComponent<RectTransform>().anchoredPosition = new Vector3 (120 + 40 * count, 5, 0);
			portrait.transform.localScale = new Vector3 (1, 1, 1);
			
			bool isFirst = (count == 0);
			ActiveBorder(unit, portrait, isFirst);
			
			count ++;
		}
	}
	
	void ApplyNextAPText(GameObject unit, GameObject portrait)
	{
		int nextPhaseAP = unit.GetComponent<Unit>().GetCurrentActivityPoint() +
						  unit.GetComponent<Unit>().GetActualStat(Stat.Dexturity);  
		
		portrait.transform.Find("APText").GetComponent<Text>().text = nextPhaseAP.ToString();
	}
	
	void ActiveBorder(GameObject unit, GameObject portrait, bool isFirst)
	{
		int standardActionPoint = FindObjectOfType<UnitManager>().GetStandardActionPoint();
		int currentPhaseAP = unit.GetComponent<Unit>().GetCurrentActivityPoint();
		int nextPhaseAP = currentPhaseAP + unit.GetComponent<Unit>().GetActualStat(Stat.Dexturity);
		
		if (isFirst)
		{
			portrait.transform.Find("GreenBorder").gameObject.SetActive(false);
		}
		else if (nextPhaseAP >= standardActionPoint)
		{
			portrait.transform.Find("RedBorder").gameObject.SetActive(false);
		}
		else
		{
			portrait.transform.Find("GreenBorder").gameObject.SetActive(false);
			portrait.transform.Find("RedBorder").gameObject.SetActive(false);
		}
	}

	List<GameObject> SortByNextPhaseAP(List<GameObject> units)
	{
		List<GameObject> sortedUnits = new List<GameObject>();
		
		for (int i = 1; i < units.Count; i++)
		{
			sortedUnits.Add(units[i]);
		}
		
		// 소팅. 레디 상태일 경우 가중치 1000. (무조건 앞에 온다)
		sortedUnits.Sort(delegate(GameObject x, GameObject y)
		{
			if (x.GetComponent<Unit>() == null && y.GetComponent<Unit>() == null) return 0;
			else if (y.GetComponent<Unit>() == null) return -1;
			else if (x.GetComponent<Unit>() == null) return 1;
			else return CompareByActionPoint(x, y);
		});
		
		return sortedUnits;
	}
	
	int GetNextPhaseAP(GameObject unit)
	{
		Unit unitInfo = unit.GetComponent<Unit>();
		int standardActionPoint = FindObjectOfType<UnitManager>().GetStandardActionPoint();
		int nextPhaseAP;
		
		if (unitInfo.GetCurrentActivityPoint() >= standardActionPoint)
			nextPhaseAP = 1000 + unitInfo.GetCurrentActivityPoint();
		else
			nextPhaseAP = unitInfo.GetCurrentActivityPoint() + unitInfo.GetActualStat(Stat.Dexturity);

		return nextPhaseAP;   
	}
	
	int CompareByActionPoint(GameObject unit, GameObject anotherUnit)
	{
		int compareResultByCurrentActionPoint = anotherUnit.GetComponent<Unit>().GetCurrentActivityPoint().CompareTo(unit.GetComponent<Unit>().GetCurrentActivityPoint());
		if (compareResultByCurrentActionPoint == 0)
		{
			int compareResultByTrueDexturity = anotherUnit.GetComponent<Unit>().GetStat(Stat.Dexturity).CompareTo(unit.GetComponent<Unit>().GetStat(Stat.Dexturity));
			if (compareResultByTrueDexturity == 0)
				return anotherUnit.GetInstanceID().CompareTo(unit.GetInstanceID());
			else
				return compareResultByTrueDexturity;
		}
		else
			return compareResultByCurrentActionPoint;
	}
	
	void ClearViewer()
	{
		int length = portraits.Count;
		for (int i = 0; i < length; i++)
		{
			Destroy(portraits[i]);
		}   
	}

	// Use this for initialization
	void Awake () {
	   portraits = new List<GameObject>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
