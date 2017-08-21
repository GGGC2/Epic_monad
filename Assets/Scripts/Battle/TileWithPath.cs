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
	
	public TileWithPath(Tile destTile, TileWithPath prevTileWithPath){
		this.tile = destTile;
		this.path = new List<Tile>();
		List<Tile> prevPath = prevTileWithPath.path;
		Tile lastPrevTile = prevTileWithPath.tile;
		foreach (var prevTile in prevPath)
			this.path.Add(prevTile);
		this.path.Add(lastPrevTile);

		this.requireActivityPoint = prevTileWithPath.requireActivityPoint + NewTileMoveCost(tile, lastPrevTile, prevPath.Count);
	}

	public static int NewTileMoveCost(Tile dest, Tile prev, int prevCount){
		int climbMultiplier = 1;
		if(dest.GetHeight() > prev.GetHeight()) {climbMultiplier = 3;}
		return dest.GetBaseMoveCost()*climbMultiplier + (prevCount+BattleData.selectedUnit.GetMovedTileCount())*Setting.moveCostAcc;
	}
}
