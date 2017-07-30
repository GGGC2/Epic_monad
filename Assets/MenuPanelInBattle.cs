using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPanelInBattle : MonoBehaviour {

	public GameObject conditionPanel;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void PopupConditionPanel()
	{
		conditionPanel.SetActive(true);
		this.gameObject.SetActive(false);
	}

	public void CloseMenu()
	{
		this.gameObject.SetActive(false);
	}
}
