using UnityEngine;
using System.Collections;

namespace WorldMap
{
public class ToSkillTreeButton : MonoBehaviour {

	// Use this for initialization
	public void OnButtonClick()
	{
		WorldMapManager.ToSkillTree();
	}
}
}