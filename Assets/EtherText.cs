using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameData;

public class EtherText : MonoBehaviour{
	public ReadyManager Manager;
	public Text etherText;

	public void Update(){
		SelectedUnit unit = Manager.selectedUnits.Find(item => item.name == Manager.ReadyPanel.RightPanel.RecentButton.nameString);
		etherText.text = "에테르 " + unit.CurrentEther + "/" + (PartyData.MaxEther);
	}
}
