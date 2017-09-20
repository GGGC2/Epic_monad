using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

namespace Battle.Turn{
	public class MoveStates{
		private static IEnumerator UpdatePreviewPathAndAP(Dictionary<Vector2, TileWithPath> movableTilesWithPath){
			BattleData.mouseOverTilePosition = null;
			while (true){
				BattleUI.UnitViewer viewer = GameObject.Find("SelectedUnitViewerPanel").GetComponent<BattleUI.UnitViewer>();
				MonoBehaviour.FindObjectOfType<TileManager>().DepaintAllTiles(TileColor.Red);
				viewer.OffPreviewAp();
				if (BattleData.mouseOverTilePosition.HasValue == false){
					BattleData.previewAPAction = null;
				}
				else{
					var preSelectedTile = BattleData.mouseOverTilePosition.Value;
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
		
		public static IEnumerator MoveToTile(Tile destTile, Direction finalDirection, int totalAPCost, int tileCount){
			Unit unit = BattleData.selectedUnit;
			unit.ApplyMove(destTile, finalDirection, totalAPCost, tileCount);

			BattleData.previewAPAction = null;
			BattleData.alreadyMoved = true;
			BattleData.currentState = CurrentState.FocusToUnit;
			if(destTile.IsEscapePoint){
				BattleData.currentState = CurrentState.Destroy;
			}

			yield return BattleData.battleManager.AtActionEnd();
		}
	}
}
