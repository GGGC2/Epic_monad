using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Enums;

namespace Battle.Turn
{
	public class MoveStates
	{
		private static IEnumerator UpdatePreviewAP(BattleData battleData, Dictionary<Vector2, TileWithPath> movableTilesWithPath)
		{
			while (true)
			{
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

		public static IEnumerator SelectMovingPointState(BattleData battleData)
		{
			while (battleData.currentState == CurrentState.SelectMovingPoint)
			{
				Dictionary<Vector2, TileWithPath> movableTilesWithPath = PathFinder.CalculatePath(battleData.selectedUnitObject);
				List<GameObject> movableTiles = new List<GameObject>();
				foreach (KeyValuePair<Vector2, TileWithPath> movableTileWithPath in movableTilesWithPath)
				{
					movableTiles.Add(movableTileWithPath.Value.tile);
				}
				battleData.tileManager.PaintTiles(movableTiles, TileColor.Blue);

				battleData.uiManager.EnableCancelButtonUI();
				battleData.isWaitingUserInput = true;

				var update = UpdatePreviewAP(battleData, movableTilesWithPath);
				battleData.battleManager.StartCoroutine(update);
				yield return battleData.battleManager.StartCoroutine(EventTrigger.WaitOr(
					battleData.triggers.rightClicked,
					battleData.triggers.cancelClicked,
					battleData.triggers.selectedTileByUser
				));
				battleData.battleManager.StopCoroutine(update);

				battleData.isWaitingUserInput = false;

				//yield break 넣으면 코루틴 강제종료
				if (battleData.triggers.rightClicked.Triggered || battleData.triggers.cancelClicked.Triggered)
				{
					battleData.uiManager.DisableCancelButtonUI();
					battleData.tileManager.DepaintTiles(movableTiles, TileColor.Blue);

					battleData.currentState = CurrentState.FocusToUnit;
					battleData.isWaitingUserInput = false;
					yield break;
				}

				// FIXME : 어딘가로 옮겨야 할 텐데...
				GameObject destTile = battleData.tileManager.GetTile(battleData.move.selectedTilePosition);
				List<GameObject> destPath = movableTilesWithPath[battleData.move.selectedTilePosition].path;
				Vector2 currentTilePos = battleData.selectedUnitObject.GetComponent<Unit>().GetPosition();
				Vector2 distanceVector = battleData.move.selectedTilePosition - currentTilePos;
				int distance = (int)Mathf.Abs(distanceVector.x) + (int)Mathf.Abs(distanceVector.y);
				int totalUseActivityPoint = movableTilesWithPath[battleData.move.selectedTilePosition].requireActivityPoint;

				battleData.move.moveCount += distance;

				battleData.tileManager.DepaintTiles(movableTiles, TileColor.Blue);
				battleData.currentState = CurrentState.CheckDestination;
				battleData.uiManager.DisableCancelButtonUI();
				BattleManager battleManager = battleData.battleManager;
				yield return battleManager.StartCoroutine(CheckDestination(battleData, destTile, destPath, totalUseActivityPoint, distance));
			}
			yield return null;
		}

		private static IEnumerator CheckDestination(BattleData battleData, GameObject destTile, List<GameObject> destPath, int totalUseActivityPoint, int distance)
		{
			while (battleData.currentState == CurrentState.CheckDestination)
			{
				// 목표지점만 푸른색으로 표시
				// List<GameObject> destTileList = new List<GameObject>();
				// destTileList.Add(destTile);
				List<GameObject> destTileList = destPath;
				destTileList.Add(destTile);
				battleData.tileManager.PaintTiles(destTileList, TileColor.Blue);
				// UI를 띄우고
				battleData.uiManager.EnableSelectDirectionUI();
				battleData.uiManager.SetDestCheckUIAP(battleData.selectedUnitObject, totalUseActivityPoint);

				// 카메라를 옮기고
				Camera.main.transform.position = new Vector3(destTile.transform.position.x, destTile.transform.position.y, -10);
				battleData.uiManager.SetMovedUICanvasOnCenter((Vector2)destTile.transform.position);
				// 클릭 대기

				battleData.uiManager.EnableCancelButtonUI();
				battleData.isWaitingUserInput = true;
				yield return battleData.battleManager.StartCoroutine(EventTrigger.WaitOr(
					battleData.triggers.selectedDirectionByUser,
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
					Camera.main.transform.position = new Vector3(battleData.selectedUnitObject.transform.position.x, battleData.selectedUnitObject.transform.position.y, -10);
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

		public static IEnumerator MoveToTile(BattleData battleData, GameObject destTileGO, Direction directionAtDest, int totalUseActivityPoint)
		{
			CaptureMoveSnapshot(battleData);

			Tile beforeTile = battleData.SelectedUnitTile;
			Unit unit = battleData.SelectedUnit;
			Tile nextTile = destTileGO.GetComponent<Tile>();
			unit.ApplyMove(beforeTile, nextTile, directionAtDest, totalUseActivityPoint);

			battleData.previewAPAction = null;
			battleData.currentState = CurrentState.FocusToUnit;
			battleData.alreadyMoved = true;
			BattleManager battleManager = battleData.battleManager;

			// 연계 정보 업데이트
			battleData.chainList = ChainList.RefreshChainInfo(battleData.chainList);

			yield return null;
		}

		private static void CaptureMoveSnapshot(BattleData battleData)
		{
			Debug.Log("Capture move snapshot");
			BattleData.MoveSnapshopt snapshot = new BattleData.MoveSnapshopt();
			snapshot.tile = battleData.SelectedUnitTile;
			snapshot.ap = battleData.SelectedUnit.activityPoint;
			snapshot.direction = battleData.SelectedUnit.direction;
			battleData.moveSnapshot = snapshot;
		}

		public static void RestoreMoveSnapshot(BattleData battleData)
		{
			Debug.Log("Restore move snapshot");
			var snapshot = battleData.moveSnapshot;
			Unit unit = battleData.SelectedUnit;
			Tile beforeTile = battleData.SelectedUnitTile;
			Tile nextTile = snapshot.tile;
			unit.ApplySnapshot(beforeTile, nextTile, snapshot.direction, snapshot.ap);
			// 연계 정보 업데이트.
			battleData.chainList = ChainList.RefreshChainInfo(battleData.chainList);
		}
	}
}
