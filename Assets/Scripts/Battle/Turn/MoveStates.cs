using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

namespace Battle.Turn{
	public class MoveStates{
		private static IEnumerator UpdatePreviewPathAndAP(Dictionary<Vector2, TileWithPath> movableTilesWithPath){
			while (true){
				MonoBehaviour.FindObjectOfType<TileManager>().DepaintAllTiles(TileColor.Red);
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
				yield return battleManager.StartCoroutine(CheckDestination(destTile, destPath, totalUseActivityPoint, distance));
			}
			yield return null;
		}

		private static IEnumerator CheckDestination(Tile destTile, List<Tile> destPath, int totalUseActivityPoint, int distance){
			/*
			while (BattleData.currentState == CurrentState.CheckDestination){
				// 목표지점만 푸른색으로 표시
				// List<GameObject> destTileList = new List<GameObject>();
				// destTileList.Add(destTile);
				List<Tile> destTileList = destPath;
				destTileList.Add(destTile);
				BattleData.tileManager.PaintTiles(destTileList, TileColor.Blue);
				// UI를 띄우고
				BattleData.uiManager.EnableSelectDirectionUI();
				BattleData.uiManager.SetDestCheckUIAP(BattleData.selectedUnit, totalUseActivityPoint);

				// 카메라를 옮기고
				BattleManager.MoveCameraToTile(destTile);
				BattleData.uiManager.SetMovedUICanvasOnTileAsCenter(destTile);
				
				// 클릭 대기
				BattleData.uiManager.EnableCancelButtonUI();
				BattleData.isWaitingUserInput = true;

				BattleManager battleManager = BattleData.battleManager;
				yield return battleManager.StartCoroutine(EventTrigger.WaitOr(
					BattleData.triggers.rightClicked,
					BattleData.triggers.cancelClicked,
					BattleData.triggers.directionSelectedByUser)
				);

				BattleData.isWaitingUserInput = false;
				BattleData.uiManager.DisableCancelButtonUI();

				// 클릭 중 취소하면 돌아감
				// moveCount 되돌리기
				// 카메라 유닛 위치로 원상복구
				// 이동가능 위치 다시 표시해주고
				// UI 숨기고
				if (BattleData.triggers.rightClicked.Triggered || BattleData.triggers.cancelClicked.Triggered){
					BattleData.move.moveCount -= distance;
					BattleManager.MoveCameraToUnit(BattleData.selectedUnit);
					BattleData.tileManager.DepaintTiles(destTileList, TileColor.Blue);
					BattleData.uiManager.DisableSelectDirectionUI();
					BattleData.uiManager.DisableDestCheckUI();
					BattleData.currentState = CurrentState.SelectMovingPoint;
					BattleData.isWaitingUserInput = false;
					yield break;
				}

				// 방향을 클릭하면 그 자리로 이동. MoveToTile 호출
				BattleData.tileManager.DepaintTiles(destTileList, TileColor.Blue);
				BattleData.currentState = CurrentState.MoveToTile;
				BattleData.uiManager.DisableDestCheckUI();
				yield return BattleManager.Instance.StartCoroutine(MoveToTile(destTile, BattleData.move.selectedDirection, totalUseActivityPoint));
			}
			yield return null;
			*/
			// 이동했을때 볼 방향 설정
			Direction finalDirection;
			Vector2 destPos = destTile.GetTilePos ();
			Debug.Log ("destPath count : " + destPath.Count);
			if (destPath.Count > 0) {
				Vector2 prevLastPos = destPath [destPath.Count - 1].GetTilePos ();
				Vector2 delta = destPos - prevLastPos;
				if (delta == new Vector2 (1, 0))
					finalDirection = Direction.RightDown;
				else if (delta == new Vector2 (-1, 0))
					finalDirection = Direction.LeftUp;
				else if (delta == new Vector2 (0, 1))
					finalDirection = Direction.RightUp;
				else // delta == new Vector2 (0, -1)
				finalDirection = Direction.LeftDown;
			}
			else { // 제자리 이동(나중엔 막아놔야 함)이면 방향은 그대로
				finalDirection = BattleData.selectedUnit.GetDirection ();
			}

			BattleData.currentState = CurrentState.MoveToTile;
			yield return BattleManager.Instance.StartCoroutine(MoveToTile(destTile, finalDirection, totalUseActivityPoint));
		}

		public static IEnumerator MoveToTile(Tile destTile, Direction finalDirection, int totalAPCost){
			CaptureMoveSnapshot();
            
			Unit unit = BattleData.selectedUnit;
			unit.ApplyMove(destTile, finalDirection, totalAPCost);

			BattleData.previewAPAction = null;
			BattleData.currentState = CurrentState.FocusToUnit;
			BattleData.alreadyMoved = true;

			yield return null;
		}

		private static void CaptureMoveSnapshot()
		{
			BattleData.MoveSnapshot snapshot = new BattleData.MoveSnapshot();
			snapshot.tile = BattleData.SelectedUnitTile;
			snapshot.ap = BattleData.selectedUnit.GetCurrentActivityPoint();
			snapshot.direction = BattleData.selectedUnit.GetDirection();
			BattleData.moveSnapshot = snapshot;
		}

		public static void RestoreMoveSnapshot()
		{
			Debug.Log("Restore move snapshot");
			var snapshot = BattleData.moveSnapshot;
			Unit unit = BattleData.selectedUnit;
			Tile beforeTile = BattleData.SelectedUnitTile;
			Tile nextTile = snapshot.tile;
			unit.ApplySnapshot(beforeTile, nextTile, snapshot.direction, snapshot.ap);
		}
	}
}
