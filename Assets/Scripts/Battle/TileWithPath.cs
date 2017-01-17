using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Enums;

public class TileWithPath {
	int apGap = 2; // 이동 계차.
	
	public GameObject tile; // 도착지점 
	public List<GameObject> path; // '이전'까지의 경로
	public int requireActivityPoint; // '도착지점'까지 소모되는 ap
	
	public TileWithPath(GameObject startTile)
	{
		this.tile = startTile;
		this.path = new List<GameObject>();
		this.requireActivityPoint = 0;
	}
	
	public TileWithPath(GameObject destTile, TileWithPath prevTileWithPath)
	{
		this.tile = destTile; 
		this.path = new List<GameObject>();
		List<GameObject> prevPath = prevTileWithPath.path;
		GameObject lastPrevTile = prevTileWithPath.tile;
		foreach (var prevTile in prevPath)
			this.path.Add(prevTile);
		this.path.Add(lastPrevTile);

		// USING ONLY TEST.
		// int apGap = EditInfo.ApGap;

		// 상승이동일 경우 기본값 * 3. 아닐 경우 기본값 * 1
		int climbValue = 1;

		int heightOfDestTile = destTile.GetComponent<Tile>().GetTileHeight();
		int heightOfLastPrevTile = lastPrevTile.GetComponent<Tile>().GetTileHeight();
		if (heightOfDestTile > heightOfLastPrevTile)
			climbValue = 3;

		this.requireActivityPoint = prevTileWithPath.requireActivityPoint 
									+ (tile.GetComponent<Tile>().GetRequireAPAtTile() * climbValue + prevPath.Count * apGap);
	}
}
