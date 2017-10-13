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
				BattleData.tileManager.DepaintAllTiles(TileColor.Red);
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
		
		public static void MoveToTile(Vector2 destPos, Dictionary<Vector2, TileWithPath> path) {
            List<Tile> destPath = path[destPos].path;
            Tile destTile = BattleData.tileManager.GetTile(destPos);

            int tileCount = 0;
            bool trapOperated = false;
            foreach(var tile in destPath) {
                tileCount++;
                List<TileStatusEffect> traps = TileManager.Instance.FindTrapsWhoseRangeContains(tile);
                foreach(var trap in traps) {
                    Trap.OperateTrap(trap);
                    destTile = tile;
                    trapOperated = true;
                }
                if(trapOperated)    break;
            }
            Direction finalDirection = Utility.GetFinalDirectionOfPath(destTile, destPath, BattleData.selectedUnit.GetDirection());
            int totalAPCost = path[destTile.GetTilePos()].requireActivityPoint;

            BattleData.selectedUnit.ApplyMove(destTile, finalDirection, totalAPCost, tileCount);
		}
	}
}
