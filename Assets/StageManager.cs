using UnityEngine;
using System.Collections;

public class StageManager : MonoBehaviour {

	public TextAsset mapData;
	public TextAsset unitData;

	// Use this for initialization
	void Awake () {
		Debug.Log(SceneData.nextDialogueName);
		if (SceneData.nextStageName != null)
		{
			string nextStageName = SceneData.nextStageName;
			Debug.Log("Data/" + SceneData.nextStageName + "_map, " + "Data/" + SceneData.nextStageName + "_unit");
			TextAsset nextMapFile = Resources.Load("Data/" + SceneData.nextStageName + "_map", typeof(TextAsset)) as TextAsset;
			mapData = nextMapFile;
			TextAsset nextUnitFile = Resources.Load("Data/" + SceneData.nextStageName + "_unit", typeof(TextAsset)) as TextAsset;
			unitData = nextUnitFile;
		}
	}
}
