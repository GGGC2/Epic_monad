using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Enums;

public class TileWithPath {	
	public Tile tile; // 도착지점 
	public List<Tile> path; // '이전'까지의 경로
	public int requireActivityPoint; // '도착지점'까지 소모되는 ap
	
	public TileWithPath(Tile startTile){
		this.tile = startTile;
		this.path = new List<Tile>();
		this.requireActivityPoint = 0;
	}
	
	public TileWithPath(Tile destTile, TileWithPath prevTileWithPath, Unit movingUnit){
		this.tile = destTile;
		this.path = new List<Tile>();
		List<Tile> prevPath = prevTileWithPath.path;
		Tile lastPrevTile = prevTileWithPath.tile;
		foreach (var prevTile in prevPath) {
			this.path.Add (prevTile);
		}
		this.path.Add(lastPrevTile);

		this.requireActivityPoint = prevTileWithPath.requireActivityPoint + NewTileMoveCost(tile, lastPrevTile, prevPath.Count, movingUnit);
	}

	public static int NewTileMoveCost(Tile dest, Tile prev, int prevCount, Unit movingUnit){
		int climbMultiplier = 1;
		if(dest.GetHeight() > prev.GetHeight()) {
			climbMultiplier = 3;
		}
		int requireAP = dest.GetBaseMoveCost () * climbMultiplier + (prevCount + BattleData.selectedUnit.GetMovedTileCount ()) * Setting.moveCostAcc;

		float speed = movingUnit.GetSpeed ();
		requireAP = (int)(requireAP * (100f / speed));
		// 이동 필요 행동력 증감 효과 적용
		if (movingUnit.HasStatusEffect (StatusEffectType.RequireMoveAPChange)) {
			requireAP = (int)(movingUnit.CalculateActualAmount (requireAP, StatusEffectType.RequireMoveAPChange));
		}
		return requireAP;
	}

	public void AddDestroyUnitCost(int destroyCost){
		requireActivityPoint += destroyCost;
	}
}
