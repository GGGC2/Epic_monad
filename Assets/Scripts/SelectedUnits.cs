using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectedUnits : MonoBehaviour 
{
	List<Image> SelectedUnitPanels;

	void Start()
	{
		SelectedUnitPanels = new List<Image>();

		for(int i = 1; i <= 8; i++)
		{
			SelectedUnitPanels.Add(GameObject.Find("SelectedUnit"+i).GetComponent<Image>());
		}
	}
}