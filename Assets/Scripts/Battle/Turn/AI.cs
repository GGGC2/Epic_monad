using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Enums;

namespace Battle.Turn
{
	public class AIStates
	{
		public static IEnumerator AIMove(BattleData battleData)
		{
			Dictionary<Vector2, TileWithPath> movableTilesWithPath = PathFinder.CalculatePath(battleData.selectedUnitObject);
			List<GameObject> movableTiles = new List<GameObject>();
			foreach (KeyValuePair<Vector2, TileWithPath> movableTileWithPath in movableTilesWithPath)
			{
				movableTiles.Add(movableTileWithPath.Value.tile);
			}

			battleData.uiManager.UpdateApBarUI(battleData, battleData.unitManager.GetAllUnits());

			var randomPosition = movableTiles[Random.Range(0, movableTiles.Count)].GetComponent<Tile>().GetTilePos();

			// FIXME : 어딘가로 옮겨야 할 텐데...
			GameObject destTile = battleData.tileManager.GetTile(randomPosition);
			List<GameObject> destPath = movableTilesWithPath[randomPosition].path;
			Vector2 currentTilePos = battleData.selectedUnitObject.GetComponent<Unit>().GetPosition();
			Vector2 distanceVector = randomPosition - currentTilePos;
			int distance = (int)Mathf.Abs(distanceVector.x) + (int)Mathf.Abs(distanceVector.y);
			int totalUseActivityPoint = movableTilesWithPath[randomPosition].requireActivityPoint;

			// battleData.moveCount += distance;

			// battleData.tileManager.DepaintTiles(movableTiles, TileColor.Blue);
			battleData.currentState = CurrentState.CheckDestination;

			List<GameObject> destTileList = destPath;
			destTileList.Add(destTile);
			battleData.tileManager.PaintTiles(destTileList, TileColor.Blue);

			// 카메라를 옮기고
			Camera.main.transform.position = new Vector3(destTile.transform.position.x, destTile.transform.position.y, -10);

			battleData.currentState = CurrentState.MoveToTile;
			BattleManager battleManager = battleData.battleManager;
			yield return battleManager.StartCoroutine(MoveStates.MoveToTile(battleData, destTile, Direction.Right, totalUseActivityPoint));
		}
	}
}
