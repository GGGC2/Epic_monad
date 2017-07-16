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
		GetStageDataFiles();
	}

	void GetStageDataFiles(){
		if (SceneData.stageNumber > 0){
			TextAsset nextMapFile = Resources.Load<TextAsset>("Data/Stage" + SceneData.stageNumber + "_map");
			mapData = nextMapFile;
			TextAsset nextUnitFile = Resources.Load<TextAsset>("Data/Stage" + SceneData.stageNumber + "_unit");
			unitData = nextUnitFile;
			TextAsset nextBattleEndConditionFile = Resources.Load<TextAsset>("Data/Stage" + SceneData.stageNumber + "_battleEndCondition");
			battleEndConditionData = nextBattleEndConditionFile;
			TextAsset nextBgmFile = Resources.Load<TextAsset>("Data/Stage" + SceneData.stageNumber + "_bgm");
			bgmData = nextBgmFile;
		}
	}

	void Awake ()
	{
		GetStageDataFiles();
	}
}