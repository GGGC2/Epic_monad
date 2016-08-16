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
		int apGap = EditInfo.ApGap;

		this.requireActivityPoint = prevTileWithPath.requireActivityPoint + (tile.GetComponent<Tile>().GetRequireAPAtTile() + prevPath.Count * apGap);

		// 효과에 의한 이동력 소모 증가/감소
		if(prevPath[0].GetComponent<Unit>().HasStatusEffectType(StatusEffectType.RequireMoveAPIncrease))
		{
			float increaseDegree = 1.0f;
			foreach (var statusEffect in prevPath[0].GetComponent<Unit>().GetStatusEffectList())
			{
				if (statusEffect.GetStatusEffectType().Equals(StatusEffectType.RequireMoveAPIncrease))
				{
					increaseDegree = (100.0f + statusEffect.GetDegree()) / 100.0f;
				}
			}
			this.requireActivityPoint = (int) ((float) this.requireActivityPoint * increaseDegree);
		}
		
		if(prevPath[0].GetComponent<Unit>().HasStatusEffectType(StatusEffectType.RequireMoveAPDecrease))
		{
			float decreaseDegree = 1.0f;
			foreach (var statusEffect in prevPath[0].GetComponent<Unit>().GetStatusEffectList())
			{
				if (statusEffect.GetStatusEffectType().Equals(StatusEffectType.RequireMoveAPDecrease))
				{
					decreaseDegree = (100.0f - statusEffect.GetDegree()) / 100.0f;
				}
			}
			this.requireActivityPoint = (int) ((float) this.requireActivityPoint * decreaseDegree);
		}
	}
}
