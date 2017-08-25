using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

namespace Battle.Turn{
	public class MoveStates{
		private static IEnumerator UpdatePreviewPathAndAP(Dictionary<Vector2, TileWithPath> movableTilesWithPath){
			BattleData.preSelectedTilePosition = null;
			while (true){
				BattleUI.UnitViewer viewer = GameObject.Find("SelectedUnitViewerPanel").GetComponent<BattleUI.UnitViewer>();
				MonoBehaviour.FindObjectOfType<TileManager>().DepaintAllTiles(TileColor.Red);
				viewer.OffPreviewAp();
				if (BattleData.preSelectedTilePosition.HasValue == false){
					BattleData.previewAPAction = null;
				}
				else{
					var preSelectedTile = BattleData.preSelectedTilePosition.Value;
					int requiredAP = movableTilesWithPath[preSelectedTile].requireActivityPoint;
					BattleData.previewAPAction = new APAction(APAction.Action.Move, requiredAP);
					Tile tileUnderMouse = TileManager.Instance.preSelectedMouseOverTile;
					tileUnderMouse.CostAP.text = requiredAP.ToString();
					viewer.PreviewAp(BattleData.selectedUnit, requiredAP);
					foreach(Tile tile in movableTilesWithPath[tileUnderMouse.GetTilePos()].path){
						tile.PaintTile(TileColor.Red);
					}
				}
				BattleData.uiManager.UpdateApBarUI();
				yield return null;
			}
		}

		public static IEnumerator CheckDestination(Tile destTile, List<Tile> destPath, int totalUseActivityPoint){
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

			yield return BattleData.battleManager.AtActionEnd();
		}
	}
}
