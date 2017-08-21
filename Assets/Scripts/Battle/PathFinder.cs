using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Enums;

public static class PathFinder {
	
	class TileTuple{
		public Vector2 tilePosition;
		public TileWithPath tileWithPath; 
		
		public TileTuple (Vector2 tilePosition, TileWithPath tileWithPath)
		{
			this.tilePosition = tilePosition;
			this.tileWithPath = tileWithPath;
		}
	}

	public static Dictionary<Vector2, TileWithPath> CalculateMovablePathsForAI(Unit unit, ActiveSkill skill){
		return CalculatePathsFromThisTileForAI (unit, unit.GetTileUnderUnit (), unit.GetCurrentActivityPoint (), skill);
	}
	public static Dictionary<Vector2, TileWithPath> CalculateMovablePaths(Unit unit){
		return CalculatePathsFromThisTile (unit, unit.GetTileUnderUnit (), unit.GetCurrentActivityPoint ());
	}

	// AI용 경로탐색이다. 걸림돌이 있으면 부수고 가는 경우까지 고려한다.
	public static Dictionary<Vector2, TileWithPath> CalculatePathsFromThisTileForAI(Unit unit, Tile tile, int maxAPUse, ActiveSkill skill){
		Dictionary<Vector2, Tile> tiles = TileManager.Instance.GetAllTiles();
		Vector2 startPos = tile.GetTilePos ();

		Queue<TileTuple> tileQueue = new Queue<TileTuple>();

		Dictionary<Vector2, TileWithPath> tilesWithPath = new Dictionary<Vector2, TileWithPath>();
		TileWithPath startPoint = new TileWithPath(tiles[startPos]);
		tilesWithPath.Add(startPos, startPoint);

		tileQueue.Enqueue(new TileTuple(startPos, startPoint));

		while (tileQueue.Count > 0){
			TileTuple newTileTuple = tileQueue.Dequeue();
			Vector2 newPosition = newTileTuple.tilePosition;
			TileWithPath newTileWithPath = newTileTuple.tileWithPath;
			foreach (Direction direction in EnumUtil.directions) {
				SearchNearbyTileForAI (tiles, tilesWithPath, tileQueue, unit, newPosition, newPosition + Utility.ToVector2(direction), maxAPUse, skill);
			}
		}

		Debug.Log ("AI pathfinding ended");

		return tilesWithPath;
	}

	// AI용이다. 장애물이 있는 타일은 갈 수 없다고 치는 게 아니라 그냥 장애물을 부수는 데 필요한 AP를 추가한다.
	static void SearchNearbyTileForAI(Dictionary<Vector2, Tile> tiles, Dictionary<Vector2, TileWithPath> tilesWithPath,
		Queue<TileTuple> tileQueue, Unit unit, Vector2 currentTilePosition, Vector2 nearbyTilePosition, int maxAPUse, ActiveSkill skill){

		if (!tiles.ContainsKey(nearbyTilePosition)) return;

		Tile currentTile = tiles[currentTilePosition];
		Tile nearbyTile = tiles[nearbyTilePosition];

		int deltaHeight = Mathf.Abs(currentTile.GetHeight() - nearbyTile.GetHeight());
		if (deltaHeight >= 2) return;

		TileWithPath prevTileWithPath = tilesWithPath[currentTilePosition];
		TileWithPath nearbyTileWithPath = new TileWithPath(nearbyTile, prevTileWithPath, unit);
		int requireAP = nearbyTileWithPath.requireActivityPoint;

		if (nearbyTile.IsUnitOnTile ()) {
			Unit obstacle = nearbyTile.GetUnitOnTile ();

			// 중립 유닛도 포함해야 하는데...
			// 한칸 바로 앞을 공격할 수 없는 AI가 나오면 수정해야 함
			if (obstacle.IsSeenAsEnemyToThisAIUnit (unit)) {
				SkillLocation location;
				if (skill.GetSkillType () != SkillType.Point) {
					location = new SkillLocation (currentTile, currentTile, Utility.VectorToDirection (nearbyTilePosition - currentTilePosition));
				} else {
					location = new SkillLocation (currentTile, nearbyTile, Utility.GetDirectionToTarget (currentTilePosition, nearbyTilePosition));
				}
				Casting casting = new Casting (unit, skill, location);
				int destroyNeedCount = obstacle.CalculateIntKillNeedCount (casting);
				if (destroyNeedCount > 100) {
					Debug.Log ("Destroy need count is over 100");
					return;
				}
				int destroyCost = destroyNeedCount * unit.GetActualRequireSkillAP (skill);
				requireAP += destroyCost;
				nearbyTileWithPath.AddDestroyUnitCost (destroyCost);
			} else {
				return;
			}
		}

		if (requireAP > maxAPUse) {
			return;
		}

		if (!tilesWithPath.ContainsKey(nearbyTilePosition)){
			tilesWithPath.Add(nearbyTilePosition, nearbyTileWithPath);
			tileQueue.Enqueue(new TileTuple(nearbyTilePosition, nearbyTileWithPath));
			return;
		}

		TileWithPath existingNearbyTileWithPath = tilesWithPath[nearbyTilePosition];
		if (existingNearbyTileWithPath.requireActivityPoint > nearbyTileWithPath.requireActivityPoint){
			tilesWithPath.Remove(nearbyTilePosition);
			tilesWithPath.Add(nearbyTilePosition, nearbyTileWithPath);
			tileQueue.Enqueue(new TileTuple(nearbyTilePosition, nearbyTileWithPath));
			return;
		}
	}


	public static Dictionary<Vector2, TileWithPath> CalculatePathsFromThisTile(Unit unit, Tile tile, int maxAPUse){
		Dictionary<Vector2, Tile> tiles = TileManager.Instance.GetAllTiles();
		Vector2 startPos = tile.GetTilePos ();

		Queue<TileTuple> tileQueue = new Queue<TileTuple>();

		Dictionary<Vector2, TileWithPath> tilesWithPath = new Dictionary<Vector2, TileWithPath>();
		TileWithPath startPoint = new TileWithPath(tiles[startPos]);
		tilesWithPath.Add(startPos, startPoint);
		// Queue에 넣음
		tileQueue.Enqueue(new TileTuple(startPos, startPoint));

		//// while loop
		while (tileQueue.Count > 0){
			// Queue에 있는 모든 원소에 대해
			TileTuple newTileTuple = tileQueue.Dequeue();
			Vector2 newPosition = newTileTuple.tilePosition;
			TileWithPath newTileWithPath = newTileTuple.tileWithPath;
			// 전후좌우에 있는 타일을 탐색.
			foreach (Direction direction in EnumUtil.directions) {
				SearchNearbyTile (tiles, tilesWithPath, tileQueue, unit, newPosition, newPosition + Utility.ToVector2(direction), maxAPUse);
			}
		}
		//// queue가 비었으면 loop를 탈출.
		return tilesWithPath;
	}
	
	static void SearchNearbyTile(Dictionary<Vector2, Tile> tiles, Dictionary<Vector2, TileWithPath> tilesWithPath,
		Queue<TileTuple> tileQueue, Unit unit, Vector2 tilePosition, Vector2 nearbyTilePosition, int maxAPUse){
		// if, 타일이 존재하지 않거나, 타일 위에 다른 유닛이 있거나, 다음타일과의 단차가 2 이상이거나,
		// 타일까지 드는 ap가 remain ap보다 큰 경우 고려하지 않음.
		if (!tiles.ContainsKey(nearbyTilePosition)) return;
		
		Tile nearbyTile = tiles[nearbyTilePosition];
		if (nearbyTile.IsUnitOnTile()) return;

		Tile currentTile = tiles[tilePosition];
		int deltaHeight = Mathf.Abs(currentTile.GetHeight() - nearbyTile.GetHeight());
		if (deltaHeight >= 2) return;

		TileWithPath prevTileWithPath = tilesWithPath[tilePosition];
		TileWithPath nearbyTileWithPath = new TileWithPath(nearbyTile, prevTileWithPath, unit);
		int requireAP = nearbyTileWithPath.requireActivityPoint;
		if (requireAP > maxAPUse) {
			return;
		}

		//	 if, 새로운 타일이거나, 기존보다 ap가 더 적게 드는 경로일 경우 업데이트하고 해당 타일을 queue에 넣음.
		if (!tilesWithPath.ContainsKey(nearbyTilePosition)){
			tilesWithPath.Add(nearbyTilePosition, nearbyTileWithPath);
			tileQueue.Enqueue(new TileTuple(nearbyTilePosition, nearbyTileWithPath));
			return;
		}
		
		TileWithPath existingNearbyTileWithPath = tilesWithPath[nearbyTilePosition];
		if (existingNearbyTileWithPath.requireActivityPoint > nearbyTileWithPath.requireActivityPoint){
			tilesWithPath.Remove(nearbyTilePosition);
			tilesWithPath.Add(nearbyTilePosition, nearbyTileWithPath);
			tileQueue.Enqueue(new TileTuple(nearbyTilePosition, nearbyTileWithPath));
			return;
		}
	}
}
