using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Enums;

public class PathFinder {
	
	class TileTuple
	{
		public Vector2 tilePosition;
		public TileWithPath tileWithPath; 
		
		public TileTuple (Vector2 tilePosition, TileWithPath tileWithPath)
		{
			this.tilePosition = tilePosition;
			this.tileWithPath = tileWithPath;
		}
	}
	
	public static Dictionary<Vector2, TileWithPath> CalculatePath(Unit unit)
	{
		Dictionary<Vector2, Tile> tiles = GameObject.FindObjectOfType<TileManager>().GetAllTiles();
		Vector2 unitPosition = unit.GetPosition();

		Queue<TileTuple> tileQueue = new Queue<TileTuple>();
		
		Dictionary<Vector2, TileWithPath> tilesWithPath = new Dictionary<Vector2, TileWithPath>();
		TileWithPath startPoint = new TileWithPath(tiles[unitPosition]);
		tilesWithPath.Add(unitPosition, startPoint);
		// Queue에 넣음
		tileQueue.Enqueue(new TileTuple(unitPosition, startPoint));
		
		//// while loop
		while (tileQueue.Count > 0)
		{
			// Queue에 있는 모든 원소에 대해
			TileTuple newTileTuple = tileQueue.Dequeue();
			Vector2 newPosition = newTileTuple.tilePosition;
			TileWithPath newTileWithPath = newTileTuple.tileWithPath;
			// 전후좌우에 있는 타일을 탐색.
			SearchNearbyTile(tiles, tilesWithPath, tileQueue, unit, newPosition, newPosition + Vector2.up);
			SearchNearbyTile(tiles, tilesWithPath, tileQueue, unit, newPosition, newPosition + Vector2.down);
			SearchNearbyTile(tiles, tilesWithPath, tileQueue, unit, newPosition, newPosition + Vector2.left);
			SearchNearbyTile(tiles, tilesWithPath, tileQueue, unit, newPosition, newPosition + Vector2.right);
		}
        //// queue가 비었으면 loop를 탈출.		
        if (unit.HasStatusEffect(StatusEffectType.RequireMoveAPChange) || unit.HasStatusEffect(StatusEffectType.SpeedChange)) {
            foreach (TileWithPath tileWithPath in tilesWithPath.Values) {
                tileWithPath.requireActivityPoint = (int)(unit.CalculateActualAmount(tileWithPath.requireActivityPoint, StatusEffectType.RequireMoveAPChange));
                float speed = unit.CalculateActualAmount(100, StatusEffectType.SpeedChange);
                tileWithPath.requireActivityPoint = (int)(tileWithPath.requireActivityPoint * (100f / speed));
            }
        }
        return tilesWithPath;
	}
	
	static void SearchNearbyTile(Dictionary<Vector2, Tile> tiles, Dictionary<Vector2, TileWithPath> tilesWithPath,
								 Queue<TileTuple> tileQueue, Unit unit, Vector2 tilePosition, Vector2 nearbyTilePosition)
	{
		// if, 타일이 존재하지 않거나, 타일 위에 다른 유닛이 있거나, 다음타일과의 단차가 2 이상이거나,
		// 타일까지 드는 ap가 remain ap보다 큰 경우 고려하지 않음.
		if (!tiles.ContainsKey(nearbyTilePosition)) return;
		
		Tile nearbyTile = tiles[nearbyTilePosition];
		if (nearbyTile.IsUnitOnTile()) return;

		Tile currentTile = tiles[tilePosition];
		int deltaHeight = Mathf.Abs(currentTile.GetTileHeight() - nearbyTile.GetTileHeight());
		if (deltaHeight >= 2) return;

		TileWithPath prevTileWithPath = tilesWithPath[tilePosition];
		TileWithPath nearbyTileWithPath = new TileWithPath(nearbyTile, prevTileWithPath);
		int remainAP = unit.GetCurrentActivityPoint();
		int requireAP = nearbyTileWithPath.requireActivityPoint;
		// 필요 행동력(이동) 증감 효과 적용
		if (unit.HasStatusEffect(StatusEffectType.RequireMoveAPChange) || unit.HasStatusEffect(StatusEffectType.SpeedChange))
		{
			requireAP = (int)(unit.CalculateActualAmount(requireAP, StatusEffectType.RequireMoveAPChange));
            float speed = unit.CalculateActualAmount(100, StatusEffectType.SpeedChange);
            requireAP = (int)(requireAP * (100f / speed));
        }
		if (requireAP > remainAP) return;
		
		// else, 
		//	 if, 새로운 타일이거나, 기존보다 ap가 더 적게 드는 경로일 경우 업데이트하고 해당 타일을 queue에 넣음.
		if (!tilesWithPath.ContainsKey(nearbyTilePosition))
		{
			tilesWithPath.Add(nearbyTilePosition, nearbyTileWithPath);
			tileQueue.Enqueue(new TileTuple(nearbyTilePosition, nearbyTileWithPath));
			return;
		}
		
		TileWithPath existingNearbyTileWithPath = tilesWithPath[nearbyTilePosition];
		if (existingNearbyTileWithPath.requireActivityPoint > nearbyTileWithPath.requireActivityPoint)
		{
			tilesWithPath.Remove(nearbyTilePosition);
			tilesWithPath.Add(nearbyTilePosition, nearbyTileWithPath);
			tileQueue.Enqueue(new TileTuple(nearbyTilePosition, nearbyTileWithPath));
			return;
		}
	}
}
