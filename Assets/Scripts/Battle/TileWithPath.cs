using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Enums;

public class TileWithPath {
	int apGap = 2; // 이동 계차.
	
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

		// USING ONLY TEST.
		// int apGap = EditInfo.ApGap;

		// 상승이동일 경우 기본값 * 3. 아닐 경우 기본값 * 1
		int climbValue = 1;

		int heightOfDestTile = destTile.GetTileHeight();
		int heightOfLastPrevTile = lastPrevTile.GetTileHeight();
		if (heightOfDestTile > heightOfLastPrevTile)
			climbValue = 3;

		this.requireActivityPoint = prevTileWithPath.requireActivityPoint 
									+ (tile.GetRequireAPAtTile() * climbValue + prevPath.Count * apGap);
	}
}
