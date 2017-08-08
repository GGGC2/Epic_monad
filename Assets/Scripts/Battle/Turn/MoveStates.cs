using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Enums;

namespace Battle.Turn{
	public class MoveStates{
		private static IEnumerator UpdatePreviewAP(BattleData battleData, Dictionary<Vector2, TileWithPath> movableTilesWithPath){
			while (true){
				if (battleData.preSelectedTilePosition.HasValue == false ||
					movableTilesWithPath.ContainsKey(battleData.preSelectedTilePosition.Value) == false)
				{
					battleData.previewAPAction = null;
				}
				else
				{
					var preSelectedTile = battleData.preSelectedTilePosition.Value;
					int requiredAP = movableTilesWithPath[preSelectedTile].requireActivityPoint;
					battleData.previewAPAction = new APAction(APAction.Action.Move, requiredAP);
				}
				battleData.uiManager.UpdateApBarUI(battleData, battleData.unitManager.GetAllUnits());
				yield return null;
			}
		}

		public static IEnumerator SelectMovingPointState(BattleData battleData){
			while (battleData.currentState == CurrentState.SelectMovingPoint){
				Dictionary<Vector2, TileWithPath> movableTilesWithPath = PathFinder.CalculatePath(battleData.selectedUnit);
				List<Tile> movableTiles = new List<Tile>();
				foreach (KeyValuePair<Vector2, TileWithPath> movableTileWithPath in movableTilesWithPath)
				{
					movableTiles.Add(movableTileWithPath.Value.tile);
				}
				battleData.tileManager.PaintTiles(movableTiles, TileColor.Blue);
				battleData.tileManager.PreselectTiles (movableTiles);

				battleData.uiManager.EnableCancelButtonUI();
				battleData.isWaitingUserInput = true;

				BattleManager battleManager = battleData.battleManager;
				var update = UpdatePreviewAP(battleData, movableTilesWithPath);
				battleManager.StartCoroutine(update);

				//튜토리얼 중일 경우 이동취소 입력을 무시한다
				if(battleManager.onTutorial){
					yield return battleManager.StartCoroutine(EventTrigger.WaitOr(battleData.triggers.tileSelectedByUser));
				}else{
					yield return battleManager.StartCoroutine(EventTrigger.WaitOr(
					battleData.triggers.rightClicked,
					battleData.triggers.cancelClicked,
					battleData.triggers.tileSelectedByUser));
				}
				
				battleManager.StopCoroutine(update);

				battleData.isWaitingUserInput = false;

				if (battleData.triggers.rightClicked.Triggered || battleData.triggers.cancelClicked.Triggered){
					battleData.unitManager.UpdateUnitOrder();
					battleData.uiManager.DisableCancelButtonUI();
					battleData.tileManager.DepaintTiles(movableTiles, TileColor.Blue);
					battleData.tileManager.DepreselectAllTiles ();

					battleData.currentState = CurrentState.FocusToUnit;
					battleData.isWaitingUserInput = false;
					yield break;
				}

				// FIXME : 어딘가로 옮겨야 할 텐데...
				Tile destTile = battleData.tileManager.GetTile(battleData.move.selectedTilePosition);
				List<Tile> destPath = movableTilesWithPath[battleData.move.selectedTilePosition].path;
				Vector2 currentTilePos = battleData.selectedUnit.GetPosition();
				Vector2 distanceVector = battleData.move.selectedTilePosition - currentTilePos;
				int distance = (int)Mathf.Abs(distanceVector.x) + (int)Mathf.Abs(distanceVector.y);
				int totalUseActivityPoint = movableTilesWithPath[battleData.move.selectedTilePosition].requireActivityPoint;

				battleData.move.moveCount += distance;

				battleData.tileManager.DepaintTiles(movableTiles, TileColor.Blue);
				battleData.tileManager.DepreselectAllTiles ();
				battleData.currentState = CurrentState.CheckDestination;
				battleData.uiManager.DisableCancelButtonUI();
				yield return battleManager.StartCoroutine(CheckDestination(battleData, destTile, destPath, totalUseActivityPoint, distance));
			}
			yield return null;
		}

		private static IEnumerator CheckDestination(BattleData battleData, Tile destTile, List<Tile> destPath, int totalUseActivityPoint, int distance)
		{
			while (battleData.currentState == CurrentState.CheckDestination)
			{
				// 목표지점만 푸른색으로 표시
				// List<GameObject> destTileList = new List<GameObject>();
				// destTileList.Add(destTile);
				List<Tile> destTileList = destPath;
				destTileList.Add(destTile);
				battleData.tileManager.PaintTiles(destTileList, TileColor.Blue);
				// UI를 띄우고
				battleData.uiManager.EnableSelectDirectionUI();
				battleData.uiManager.SetDestCheckUIAP(battleData.selectedUnit, totalUseActivityPoint);

				// 카메라를 옮기고
				BattleManager.MoveCameraToTile(destTile);
				battleData.uiManager.SetMovedUICanvasOnTileAsCenter(destTile);
				// 클릭 대기

				battleData.uiManager.EnableCancelButtonUI();
				battleData.isWaitingUserInput = true;
				yield return battleData.battleManager.StartCoroutine(EventTrigger.WaitOr(
					battleData.triggers.directionSelectedByUser,
					battleData.triggers.rightClicked,
					battleData.triggers.cancelClicked
				));
				battleData.isWaitingUserInput = false;
				battleData.uiManager.DisableCancelButtonUI();

				// 클릭 중 취소하면 돌아감
				// moveCount 되돌리기
				// 카메라 유닛 위치로 원상복구
				// 이동가능 위치 다시 표시해주고
				// UI 숨기고
				if (battleData.triggers.rightClicked.Triggered || battleData.triggers.cancelClicked.Triggered)
				{
					battleData.move.moveCount -= distance;
					BattleManager.MoveCameraToUnit(battleData.selectedUnit);
					battleData.tileManager.DepaintTiles(destTileList, TileColor.Blue);
					battleData.uiManager.DisableSelectDirectionUI();
					battleData.uiManager.DisableDestCheckUI();
					battleData.currentState = CurrentState.SelectMovingPoint;
					battleData.isWaitingUserInput = false;
					yield break;
				}

				// 방향을 클릭하면 그 자리로 이동. MoveToTile 호출
				battleData.tileManager.DepaintTiles(destTileList, TileColor.Blue);
				battleData.currentState = CurrentState.MoveToTile;
				battleData.uiManager.DisableDestCheckUI();
				BattleManager battleManager = battleData.battleManager;
				yield return battleManager.StartCoroutine(MoveToTile(battleData, destTile, battleData.move.selectedDirection, totalUseActivityPoint));
			}
			yield return null;
		}

		public static IEnumerator MoveToTile(BattleData battleData, Tile destTile, Direction directionAtDest, int totalUseActivityPoint)
		{
			CaptureMoveSnapshot(battleData);
            
			Unit unit = battleData.selectedUnit;
			unit.ApplyMove(destTile, directionAtDest, totalUseActivityPoint);

			battleData.previewAPAction = null;
			battleData.currentState = CurrentState.FocusToUnit;
			battleData.alreadyMoved = true;
			BattleManager battleManager = battleData.battleManager;

			yield return null;
		}

		private static void CaptureMoveSnapshot(BattleData battleData)
		{
			BattleData.MoveSnapshopt snapshot = new BattleData.MoveSnapshopt();
			snapshot.tile = battleData.SelectedUnitTile;
			snapshot.ap = battleData.selectedUnit.GetCurrentActivityPoint();
			snapshot.direction = battleData.selectedUnit.GetDirection();
			battleData.moveSnapshot = snapshot;
		}

		public static void RestoreMoveSnapshot(BattleData battleData)
		{
			Debug.Log("Restore move snapshot");
			var snapshot = battleData.moveSnapshot;
			Unit unit = battleData.selectedUnit;
			Tile beforeTile = battleData.SelectedUnitTile;
			Tile nextTile = snapshot.tile;
			unit.ApplySnapshot(beforeTile, nextTile, snapshot.direction, snapshot.ap);
		}
	}
}
