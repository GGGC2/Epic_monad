using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectedUnits : MonoBehaviour 
{
	public List<GameObject> SelectedUnitPanels;

	void Start()
	{
		SelectedUnitPanels = new List<GameObject>();

		for(int i = 1; i <= 8; i++)
		{
			if(GameObject.Find("SelectedUnit"+i) != null)
			{
				SelectedUnitPanels.Add(GameObject.Find("SelectedUnit"+i));
			}
		}
	}
}