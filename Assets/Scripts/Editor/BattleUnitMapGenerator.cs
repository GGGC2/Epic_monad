/*using UnityEngine;
using UnityEditor;

public class BattleUnitMapGenerator{
	[MenuItem("Tools/ShowMapAndUnit")]
	private static void GenerateTileAndUnitInBattleScene(){
		UnitManager unitManager = GameObject.FindObjectOfType<UnitManager>();
		TileManager tileManager = GameObject.FindObjectOfType<TileManager>();

		tileManager.GenerateTiles(Parser.GetParsedTileInfo());
		unitManager.GenerateUnits();
		Debug.Log("Hi");
	}
}*/