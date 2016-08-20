using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Enums;

namespace Battle.Turn
{
	public class MoveStates
	{
		public static IEnumerator SelectMovingPointState(BattleData battleData)
		{
			while (battleData.currentState == CurrentState.SelectMovingPoint)
			{
				// List<GameObject> movableTiles = CheckMovableTiles(selectedUnitObject);
				Dictionary<Vector2, TileWithPath> movableTilesWithPath = PathFinder.CalculatePath(battleData.selectedUnitObject);
				List<GameObject> movableTiles = new List<GameObject>();
				foreach (KeyValuePair<Vector2, TileWithPath> movableTileWithPath in movableTilesWithPath)
				{
					movableTiles.Add(movableTileWithPath.Value.tile);
				}

				battleData.tileManager.PaintTiles(movableTiles, TileColor.Blue);

				battleData.rightClicked = false;
				battleData.cancelClicked = false;
				battleData.uiManager.EnableCancelButtonUI();

				battleData.isWaitingUserInput = true;
				battleData.isSelectedTileByUser = false;
				battleData.isPreSeletedTileByUser = false;
				while (!battleData.isSelectedTileByUser)
				{
					//yield break 넣으면 코루틴 강제종료
					if (battleData.rightClicked || battleData.cancelClicked)
					{
						battleData.rightClicked = false;
						battleData.cancelClicked = false;
						battleData.uiManager.DisableCancelButtonUI();

						battleData.tileManager.DepaintTiles(movableTiles, TileColor.Blue);

						battleData.currentState = CurrentState.FocusToUnit;
						battleData.isWaitingUserInput = false;
						yield break;
					}

					if (battleData.isPreSeletedTileByUser)
					{
						int requiredAP = movableTilesWithPath[battleData.preSelectedTilePosition].requireActivityPoint;
						battleData.previewAPAction = new APAction(APAction.Action.Move, requiredAP);
					}
					else
					{
						battleData.previewAPAction = null;
					}
					battleData.uiManager.UpdateApBarUI(battleData, battleData.unitManager.GetAllUnits());

					yield return null;
				}
				battleData.isPreSeletedTileByUser = false;
				battleData.isSelectedTileByUser = false;
				battleData.isWaitingUserInput = false;


				// FIXME : 어딘가로 옮겨야 할 텐데...
				GameObject destTile = battleData.tileManager.GetTile(battleData.selectedTilePosition);
				List<GameObject> destPath = movableTilesWithPath[battleData.selectedTilePosition].path;
				Vector2 currentTilePos = battleData.selectedUnitObject.GetComponent<Unit>().GetPosition();
				Vector2 distanceVector = battleData.selectedTilePosition - currentTilePos;
				int distance = (int)Mathf.Abs(distanceVector.x) + (int)Mathf.Abs(distanceVector.y);
				int totalUseActivityPoint = movableTilesWithPath[battleData.selectedTilePosition].requireActivityPoint;

				battleData.moveCount += distance;

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
				// 클릭 대기
				battleData.rightClicked = false;
				battleData.cancelClicked = false;
				battleData.uiManager.EnableCancelButtonUI();

				battleData.isWaitingUserInput = true;
				battleData.isSelectedDirectionByUser = false;
				while (!battleData.isSelectedDirectionByUser)
				{
					// 클릭 중 취소하면 돌아감
					// moveCount 되돌리기
					// 카메라 유닛 위치로 원상복구
					// 이동가능 위치 다시 표시해주고
					// UI 숨기고
					if (battleData.rightClicked || battleData.cancelClicked)
					{
						battleData.rightClicked = false;
						battleData.cancelClicked = false;
						battleData.uiManager.DisableCancelButtonUI();

						battleData.moveCount -= distance;
						Camera.main.transform.position = new Vector3(battleData.selectedUnitObject.transform.position.x, battleData.selectedUnitObject.transform.position.y, -10);
						battleData.tileManager.DepaintTiles(destTileList, TileColor.Blue);
						battleData.uiManager.DisableSelectDirectionUI();
						battleData.uiManager.DisableDestCheckUI();
						battleData.currentState = CurrentState.SelectMovingPoint;
						battleData.isWaitingUserInput = false;
						yield break;
					}
					yield return null;
				}
				battleData.isSelectedDirectionByUser = false;
				battleData.isWaitingUserInput = false;
				battleData.uiManager.DisableCancelButtonUI();

				// 방향을 클릭하면 그 자리로 이동. MoveToTile 호출
				battleData.tileManager.DepaintTiles(destTileList, TileColor.Blue);
				battleData.currentState = CurrentState.MoveToTile;
				battleData.uiManager.DisableDestCheckUI();
				BattleManager battleManager = battleData.battleManager;
				yield return battleManager.StartCoroutine(MoveToTile(battleData, destTile, battleData.selectedDirection, totalUseActivityPoint));
			}
			yield return null;
		}

		private static IEnumerator MoveToTile(BattleData battleData, GameObject destTile, Direction directionAtDest, int totalUseActivityPoint)
		{
			GameObject currentTile = battleData.tileManager.GetTile(battleData.selectedUnitObject.GetComponent<Unit>().GetPosition());
			currentTile.GetComponent<Tile>().SetUnitOnTile(null);
			battleData.selectedUnitObject.transform.position = destTile.transform.position + new Vector3(0, 0, -5f);
			battleData.selectedUnitObject.GetComponent<Unit>().SetPosition(destTile.GetComponent<Tile>().GetTilePos());
			battleData.selectedUnitObject.GetComponent<Unit>().SetDirection(directionAtDest);
			destTile.GetComponent<Tile>().SetUnitOnTile(battleData.selectedUnitObject);

			battleData.selectedUnitObject.GetComponent<Unit>().UseActivityPoint(totalUseActivityPoint);
			battleData.previewAPAction = null;

			battleData.currentState = CurrentState.FocusToUnit;
			battleData.alreadyMoved = true;
			BattleManager battleManager = battleData.battleManager;
			// yield return battleManager.StartCoroutine(BattleManager.FocusToUnit(battleData));

			yield return null;
		}
	}
}
