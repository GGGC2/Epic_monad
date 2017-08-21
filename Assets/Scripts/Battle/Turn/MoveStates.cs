using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

namespace Battle.Turn{
	public class MoveStates{
		private static IEnumerator UpdatePreviewPathAndAP(Dictionary<Vector2, TileWithPath> movableTilesWithPath){
			while (true){
				BattleUI.UnitViewer viewer = GameObject.Find("SelectedUnitViewerPanel").GetComponent<BattleUI.UnitViewer>();
				MonoBehaviour.FindObjectOfType<TileManager>().DepaintAllTiles(TileColor.Red);
				viewer.OffPreviewAp();
				if (BattleData.preSelectedTilePosition.HasValue == false ||
					movableTilesWithPath.ContainsKey(BattleData.preSelectedTilePosition.Value) == false)
				{
					BattleData.previewAPAction = null;
				}else{
					var preSelectedTile = BattleData.preSelectedTilePosition.Value;
					int requiredAP = movableTilesWithPath[preSelectedTile].requireActivityPoint;
					BattleData.previewAPAction = new APAction(APAction.Action.Move, requiredAP);
					Tile tileUnderMouse = MonoBehaviour.FindObjectOfType<TileManager>().preSelectedMouseOverTile;
					tileUnderMouse.CostAP.text = requiredAP.ToString();
					viewer.PreviewAp(BattleData.selectedUnit, requiredAP);
					foreach(Tile tile in movableTilesWithPath[tileUnderMouse.GetTilePos()].path){
						tile.PaintTile(TileColor.Red);
					}
				}
				BattleData.uiManager.UpdateApBarUI(BattleData.unitManager.GetAllUnits());
				yield return null;
			}
		}

		public static IEnumerator SelectMovingPointState(){
			while (BattleData.currentState == CurrentState.SelectMovingPoint){
				Dictionary<Vector2, TileWithPath> movableTilesWithPath = PathFinder.CalculateMovablePaths(BattleData.selectedUnit);
				List<Tile> movableTiles = new List<Tile>();
				foreach (KeyValuePair<Vector2, TileWithPath> movableTileWithPath in movableTilesWithPath){
					movableTiles.Add(movableTileWithPath.Value.tile);
				}
				BattleData.tileManager.PaintTiles(movableTiles, TileColor.Blue);
				BattleData.tileManager.PreselectTiles (movableTiles);

				BattleData.uiManager.EnableCancelButtonUI();
				BattleData.isWaitingUserInput = true;

				BattleManager battleManager = BattleData.battleManager;
				var update = UpdatePreviewPathAndAP(movableTilesWithPath);
				battleManager.StartCoroutine(update);

				yield return battleManager.StartCoroutine(EventTrigger.WaitOr(
					BattleData.triggers.rightClicked,
					BattleData.triggers.cancelClicked,
					BattleData.triggers.tileSelectedByUser)
				);
				
				battleManager.StopCoroutine(update);
				TileManager.Instance.DepaintAllTiles(TileColor.Red);
				foreach(Tile tile in TileManager.Instance.GetTilesInGlobalRange()){
					tile.CostAP.text = "";
				}

				BattleData.isWaitingUserInput = false;

				if (BattleData.triggers.rightClicked.Triggered || BattleData.triggers.cancelClicked.Triggered){
					BattleData.unitManager.UpdateUnitOrder();
					BattleData.uiManager.DisableCancelButtonUI();
					BattleData.tileManager.DepaintTiles(movableTiles, TileColor.Blue);
					BattleData.tileManager.DepreselectAllTiles ();

					BattleData.currentState = CurrentState.FocusToUnit;
					BattleData.isWaitingUserInput = false;
					yield break;
				}

				// FIXME : 어딘가로 옮겨야 할 텐데...
				Tile destTile = BattleData.tileManager.GetTile(BattleData.move.selectedTilePosition);
				List<Tile> destPath = movableTilesWithPath[BattleData.move.selectedTilePosition].path;
				Vector2 currentTilePos = BattleData.selectedUnit.GetPosition();
				Vector2 distanceVector = BattleData.move.selectedTilePosition - currentTilePos;
				int distance = (int)Mathf.Abs(distanceVector.x) + (int)Mathf.Abs(distanceVector.y);
				int totalUseActivityPoint = movableTilesWithPath[BattleData.move.selectedTilePosition].requireActivityPoint;

				BattleData.move.moveCount += distance;

				BattleData.tileManager.DepaintTiles(movableTiles, TileColor.Blue);
				BattleData.tileManager.DepreselectAllTiles ();
				BattleData.currentState = CurrentState.CheckDestination;
				BattleData.uiManager.DisableCancelButtonUI();
				yield return battleManager.StartCoroutine(CheckDestination(destTile, destPath, totalUseActivityPoint));
			}
			yield return null;
		}

		private static IEnumerator CheckDestination(Tile destTile, List<Tile> destPath, int totalUseActivityPoint){
			// 이동했을때 볼 방향 설정
			Direction finalDirection = Utility.GetFinalDirectionOfPath (destTile, destPath, BattleData.selectedUnit.GetDirection ());

			BattleData.currentState = CurrentState.MoveToTile;
			yield return BattleManager.Instance.StartCoroutine(MoveToTile(destTile, finalDirection, totalUseActivityPoint, destPath.Count));
		}

		public static IEnumerator MoveToTile(Tile destTile, Direction finalDirection, int totalAPCost, int tileCount){
			Unit unit = BattleData.selectedUnit;
			BattleData.moveSnapshot = new BattleData.MoveSnapshot(unit);
			unit.ApplyMove(destTile, finalDirection, totalAPCost, tileCount);

			BattleData.previewAPAction = null;
			BattleData.currentState = CurrentState.FocusToUnit;
			BattleData.alreadyMoved = true;

			yield return null;
		}
	}
}
