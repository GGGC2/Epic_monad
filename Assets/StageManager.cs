using UnityEngine;
using System.Collections;
using GameData;
public class StageManager : MonoBehaviour {
	public TextAsset mapData;
	public TextAsset GetMapData()
	{
		if (loaded == false)
		{
			Load();
		}
		return mapData;
	}

	public TextAsset unitData;
	public TextAsset GetUnitData()
	{
		if (loaded == false)
		{
			Load();
		}
		return unitData;
	}
	public TextAsset battleEndConditionData;
	public TextAsset GetBattleEndConditionData()
	{
		if (loaded == false)
		{
			Load();
		}
		return battleEndConditionData;
	}
	public TextAsset bgmData;
	public TextAsset GetBgmData()
	{
		if (loaded == false)
		{
			Load();
		}
		return bgmData;
	}

	private bool loaded = false;

	public void Load()
	{
		loaded = true;
		Debug.Log(SceneData.dialogueName);
		if (SceneData.stageName != null)
		{
			string nextStageName = SceneData.stageName;
			// Debug.Log("Data/" + SceneData.nextStageName + "_map, " + "Data/" + SceneData.nextStageName + "_unit");
			TextAsset nextMapFile = Resources.Load("Data/" + SceneData.stageName + "_map", typeof(TextAsset)) as TextAsset;
			mapData = nextMapFile;
			TextAsset nextUnitFile = Resources.Load("Data/" + SceneData.stageName + "_unit", typeof(TextAsset)) as TextAsset;
			unitData = nextUnitFile;
			TextAsset nextBattleEndConditionFile = Resources.Load("Data/" + SceneData.stageName + "_battleEndCondition", typeof(TextAsset)) as TextAsset;
			battleEndConditionData = nextBattleEndConditionFile;
			TextAsset nextBgmFile = Resources.Load("Data/" + SceneData.stageName + "_bgm", typeof(TextAsset)) as TextAsset;
			bgmData = nextBgmFile;
		}
	}

	void Awake () {
		//Debug.Log(SceneData.nextDialogueName);
		if (SceneData.stageName != null)
		{
			string nextStageName = SceneData.stageName;
			// Debug.Log("Data/" + SceneData.nextStageName + "_map, " + "Data/" + SceneData.nextStageName + "_unit");
			TextAsset nextMapFile = Resources.Load("Data/" + SceneData.stageName + "_map", typeof(TextAsset)) as TextAsset;
			mapData = nextMapFile;
			TextAsset nextUnitFile = Resources.Load("Data/" + SceneData.stageName + "_unit", typeof(TextAsset)) as TextAsset;
			unitData = nextUnitFile;
			TextAsset nextBattleEndConditionFile = Resources.Load("Data/" + SceneData.stageName + "_battleEndCondition", typeof(TextAsset)) as TextAsset;
			battleEndConditionData = nextBattleEndConditionFile;
			TextAsset nextBgmFile = Resources.Load("Data/" + SceneData.stageName + "_bgm", typeof(TextAsset)) as TextAsset;
			bgmData = nextBgmFile;
		}
	}
}