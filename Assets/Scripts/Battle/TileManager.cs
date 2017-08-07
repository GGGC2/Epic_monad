using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Enums;

public class TileManager : MonoBehaviour {

	public GameObject tilePrefab;

	Dictionary<Vector2, Tile> tiles = new Dictionary<Vector2, Tile>();

	float tileImageHeight = 0.5f*100/100;
	float tileImageWidth = 0.5f*200/100;

	public Dictionary<Vector2, Tile> GetAllTiles()
	{
		return tiles;
	}

	public Tile GetTile(int x, int y)
	{
		Vector2 key = new Vector2 (x, y);
		if (tiles.ContainsKey(key))
			return tiles[key];
		else
			return null;
	}

	public Tile GetTile(Vector2 position)
	{
		Vector2 key = position;
		if (tiles.ContainsKey(key))
			return tiles[key];
		else
			return null;
	}

	public Vector3 GetTilePos(Vector2 position)
	{
		Tile tile = GetTile(position);
		return tile.transform.position;
	}

	public static List<Tile> GetRouteTiles(List<Tile> tiles) {
		List<Tile> routeTiles = new List<Tile>();
		foreach (var tile in tiles) {
			// 타일 단차에 의한 부분(미구현)
			// 즉시 탐색을 종료한다.
			// break;

			// 첫 유닛을 만난 경우
			// 이번 타일을 마지막으로 종료한다.
			routeTiles.Add(tile);
			if (tile.IsUnitOnTile())
				break;
		}
		return routeTiles;
	}
	public static Tile GetRouteEndForAI(List<Tile> tiles) {
		foreach (var tile in tiles) {
			// 타일 단차에 의한 부분(미구현)
			// 즉시 null을 return한다.

			// 첫 유닛을 만난 경우
			// 이번 타일을 return하고 종료한다.
			if (tile.IsUnitOnTile())
				return tile;
		}
		return null;
	}
	public static Tile GetRouteEndForPC(List<Tile> tiles) {
		foreach (var tile in tiles) {
			// 타일 단차에 의한 부분(미구현)
			// 즉시 null을 return한다.

			// 첫 유닛을 만난 경우
			// 이번 타일을 return하고 종료한다.
			if (tile.IsUnitOnTile())
				return tile;
		}
		return tiles.Last ();
	}
	public List<Tile> GetTilesInRange(RangeForm form, Vector2 mid, int minReach, int maxReach, int width, Direction dir)
	{
		if (form == RangeForm.Diamond)
		{
			return GetTilesInDiamondRange(mid, minReach, maxReach);
		}
		else if (form == RangeForm.Square)
		{
			return GetTilesInSquareRange(mid, minReach, maxReach);
		}
		else if (form == RangeForm.Straight)
		{
			return GetTilesInStraightRange(mid, minReach, maxReach, dir);
		}
		else if (form == RangeForm.Cross)
		{
			return GetTilesInCrossRange(mid, minReach, maxReach);
		}
		else if (form == RangeForm.Diagonal)
		{
			return GetTilesInDiagonalCrossRange(mid, minReach, maxReach);
		}
		else if (form == RangeForm.AllDirection)
		{
			return GetTilesInAllDirectionRange(mid, minReach, maxReach);
		}
		else if (form == RangeForm.Front)
		{
			return GetTilesInFrontRange(mid, minReach, maxReach, width, dir);
		}
		else if (form == RangeForm.Sector)
		{
			return GetTilesInSectorRange(mid, minReach, maxReach, dir);
		}
		else if (form == RangeForm.Global)
		{
			return GetTilesInGlobalRange();
		}
		else
			return GetTilesInDiamondRange(mid, minReach, maxReach); // temp return value.
	}

	List<Tile> GetTilesInDiamondRange(Vector2 mid, int minReach, int maxReach)
	{
		List<Tile> tilesInRange = new List<Tile>();
		tilesInRange.Add(GetTile(mid));
		for (int i = 1; i <= maxReach; i++)
		{
			tilesInRange = AddNearbyTiles(tilesInRange);
		}

		List<Tile> exceptTiles = new List<Tile>();
		if (minReach > 0)
			exceptTiles.Add(GetTile(mid));
		for (int i = 1; i < minReach; i++)
		{
			exceptTiles = AddNearbyTiles(exceptTiles);
		}

		List<Tile> resultTiles = tilesInRange.Except(exceptTiles).ToList();

		return resultTiles;
	}

	List<Tile> GetTilesInSquareRange(Vector2 mid, int minReach, int maxReach)
	{
		List<Tile> tilesInRange = new List<Tile>();
		tilesInRange.Add(GetTile(mid));
		for (int i = 1; i <= maxReach; i++)
		{
			tilesInRange = AddNearbySquareTiles(tilesInRange);
		}

		List<Tile> exceptTiles = new List<Tile>();
		if (minReach > 0)
			exceptTiles.Add(GetTile(mid));
		for (int i = 1; i < minReach; i++)
		{
			exceptTiles = AddNearbySquareTiles(exceptTiles);
		}

		List<Tile> resultTiles = tilesInRange.Except(exceptTiles).ToList();

		return resultTiles;
	}

	List<Tile> GetTilesInStraightRange(Vector2 mid, int minReach, int maxReach, Direction dir)
	{
		List<Tile> tilesInRange = new List<Tile>();

		for(int i = minReach; i < maxReach+1; i++)
		{
			Vector2 position = mid + ToVector2(dir)*i;
			if (GetTile(position) != null && !tilesInRange.Contains(GetTile(position)))
			{
				tilesInRange.Add(GetTile(position));
			}
		}

		tilesInRange = tilesInRange.FindAll(t => t != null);

		return tilesInRange;
	}

	List<Tile> GetTilesInCrossRange(Vector2 mid, int minReach, int maxReach)
	{
		List<Tile> tilesInRange = new List<Tile>();

		if (minReach == 0) tilesInRange.Add(GetTile(mid));
		minReach = Math.Max(1, minReach);
		tilesInRange = tilesInRange.Concat(GetTilesInStraightRange(mid, minReach, maxReach, Direction.LeftUp)).ToList();
		tilesInRange = tilesInRange.Concat(GetTilesInStraightRange(mid, minReach, maxReach, Direction.LeftDown)).ToList();
		tilesInRange = tilesInRange.Concat(GetTilesInStraightRange(mid, minReach, maxReach, Direction.RightUp)).ToList();
		tilesInRange = tilesInRange.Concat(GetTilesInStraightRange(mid, minReach, maxReach, Direction.RightDown)).ToList();

		tilesInRange = tilesInRange.FindAll(t => t != null);

		// Debug.Log("No. of selected tiles : " + tilesInRange.Count);
		return tilesInRange;
	}

	List<Tile> GetTilesInDiagonalCrossRange(Vector2 mid, int minReach, int maxReach)
	{
		List<Tile> tilesInRange = new List<Tile>();

		if (minReach == 0) tilesInRange.Add(GetTile(mid));
		minReach = Math.Max(1, minReach);
		tilesInRange = tilesInRange.Concat(GetTilesInStraightRange(mid, minReach, maxReach, Direction.Left)).ToList();
		tilesInRange = tilesInRange.Concat(GetTilesInStraightRange(mid, minReach, maxReach, Direction.Right)).ToList();
		tilesInRange = tilesInRange.Concat(GetTilesInStraightRange(mid, minReach, maxReach, Direction.Up)).ToList();
		tilesInRange = tilesInRange.Concat(GetTilesInStraightRange(mid, minReach, maxReach, Direction.Down)).ToList();

		tilesInRange = tilesInRange.FindAll(t => t != null);

		return tilesInRange;
	}

	List<Tile> GetTilesInAllDirectionRange(Vector2 mid, int minReach, int maxReach)
	{
		List<Tile> tilesInRange = new List<Tile>();

		if (minReach == 0) tilesInRange.Add(GetTile(mid));
		minReach = Math.Max(1, minReach);
		tilesInRange = tilesInRange.Concat(GetTilesInCrossRange(mid, minReach,maxReach)).ToList();
		tilesInRange = tilesInRange.Concat(GetTilesInDiagonalCrossRange(mid, minReach, maxReach)).ToList();

		tilesInRange = tilesInRange.FindAll(t => t != null);

		return tilesInRange;
	}

	List<Tile> GetTilesInFrontRange(Vector2 mid, int minReach, int maxReach, int width, Direction dir)
	{
		List<Tile> tilesInRange = new List<Tile>();
		Vector2 perpendicular = new Vector2(ToVector2(dir).y, ToVector2(dir).x); // 바라보는 방향과 수직인 벡터

		for (int i = minReach; i <= maxReach; i++)
		{
			Vector2 centerPos = mid + ToVector2(dir) * i;
			tilesInRange.Add(GetTile(centerPos));
			int subwidth = 0;
			for (int j = width; j > 1; j -= 2)
			{
				subwidth += 1;
				if (mid.x == centerPos.x)
				{
					// x좌표로 펼친다
					Vector2 leftSide = centerPos + new Vector2(subwidth, 0);
					tilesInRange.Add(GetTile(leftSide));
					Vector2 rightSide = centerPos + new Vector2(-subwidth, 0);
					tilesInRange.Add(GetTile(rightSide));
				}
				else
				{
					// y좌표로 펼친다
					Vector2 leftSide = centerPos + new Vector2(0, subwidth);
					tilesInRange.Add(GetTile(leftSide));
					Vector2 rightSide = centerPos + new Vector2(0, -subwidth);
					tilesInRange.Add(GetTile(rightSide));
				} 
			}
		}

		tilesInRange = tilesInRange.FindAll(t => t != null);

		return tilesInRange;
	}

	List<Tile> GetTilesInSectorRange(Vector2 mid, int minReach, int maxReach, Direction dir)
	{
		List<Tile> tilesInRange = new List<Tile>();
		Vector2 perpendicular = new Vector2(ToVector2(dir).y, ToVector2(dir).x); // 부채꼴 방향과 수직인 벡터

		if (minReach == 0)
		{
			for(int i = 0; i <= maxReach; i++)
			{
				int j = i;
				Vector2 position = mid + ToVector2(dir) * i;
				tilesInRange.Add(GetTile(position));
				while(j > 0)
				{
					tilesInRange.Add(GetTile(position + perpendicular*j));
					tilesInRange.Add(GetTile(position - perpendicular*j));
					j--;
				}
			}
		}
		else
		{
			for(int i = 1; i <= maxReach; i++)
			{
				int j = i-1;
				Vector2 position = mid + ToVector2(dir) * i;
				tilesInRange.Add(GetTile(position));
				while(j > 0)
				{
					tilesInRange.Add(GetTile(position + perpendicular*j));
					tilesInRange.Add(GetTile(position - perpendicular*j));
					j--;
				}
			}
		}

		List<Tile> exceptTiles = new List<Tile>();

		List<Tile> resultTiles = tilesInRange.Except(exceptTiles).ToList();

		resultTiles = resultTiles.FindAll(t => t != null);

		return resultTiles;
	}

	List<Tile> GetTilesInGlobalRange()
	{
		List<Tile> tilesInRange = new List<Tile>();
		foreach (var key in tiles.Keys)
		{
			tilesInRange.Add(tiles[key]);
		}

		return tilesInRange;
	}

	public void PaintTiles(List<Tile> tiles, TileColor color)
	{
		foreach(var tile in tiles)
		{
			tile.PaintTile(color);
			tile.SetPreSelected(true);
		}
	}

	public void DepaintTiles(List<Tile> tiles, TileColor color)
	{
		foreach(var tile in tiles)
		{
			tile.DepaintTile(color);
			tile.SetPreSelected(false);
		}
	}

	public void DepaintAllTiles(TileColor color)
	{
		DepaintTiles(GetTilesInGlobalRange(), color);
	}

	public List<Tile> AddNearbyTiles(List<Tile> tileList)
	{
		List<Tile> newTileList = new List<Tile>();
		foreach (var tile in tileList)
		{
			Vector2 position = tile.GetTilePos();

			if (!newTileList.Contains(tile))
			{
				newTileList.Add(tile);
			}

			Tile nearbyUpTile = GetTile(position + Vector2.up);
			if (nearbyUpTile != null && !newTileList.Contains(nearbyUpTile))
			{
				newTileList.Add(nearbyUpTile);
			}
			Tile nearbyDownTile = GetTile(position + Vector2.down);
			if (nearbyDownTile != null && !newTileList.Contains(nearbyDownTile))
			{
				newTileList.Add(nearbyDownTile);
			}
			Tile nearbyLeftTile = GetTile(position + Vector2.left);
			if (nearbyLeftTile != null && !newTileList.Contains(nearbyLeftTile))
			{
				newTileList.Add(nearbyLeftTile);
			}
			Tile nearbyRightTile = GetTile(position + Vector2.right);
			if (nearbyRightTile != null && !newTileList.Contains(nearbyRightTile))
			{
				newTileList.Add(nearbyRightTile);
			}
		}

		return newTileList;
	}

	public List<Tile> AddNearbySquareTiles(List<Tile> tileList)
	{
		List<Tile> newTileList = new List<Tile>();
		foreach (var tile in tileList)
		{
			Vector2 position = tile.GetTilePos();

			if (!newTileList.Contains(tile))
			{
				newTileList.Add(tile);
			}

			Tile nearbyUpTile = GetTile(position + Vector2.up);
			if (nearbyUpTile != null && !newTileList.Contains(nearbyUpTile))
			{
				newTileList.Add(nearbyUpTile);
			}
			Tile nearbyDownTile = GetTile(position + Vector2.down);
			if (nearbyDownTile != null && !newTileList.Contains(nearbyDownTile))
			{
				newTileList.Add(nearbyDownTile);
			}
			Tile nearbyLeftTile = GetTile(position + Vector2.left);
			if (nearbyLeftTile != null && !newTileList.Contains(nearbyLeftTile))
			{
				newTileList.Add(nearbyLeftTile);
			}
			Tile nearbyRightTile = GetTile(position + Vector2.right);
			if (nearbyRightTile != null && !newTileList.Contains(nearbyRightTile))
			{
				newTileList.Add(nearbyRightTile);
			}

			Tile nearbyUpLeftTile = GetTile(position + Vector2.up + Vector2.left);
			if (nearbyUpLeftTile != null && !newTileList.Contains(nearbyUpLeftTile))
			{
				newTileList.Add(nearbyUpLeftTile);
			}
			Tile nearbyUpRightTile = GetTile(position + Vector2.up + Vector2.right);
			if (nearbyUpRightTile != null && !newTileList.Contains(nearbyUpRightTile))
			{
				newTileList.Add(nearbyUpRightTile);
			}
			Tile nearbyDownLeftTile = GetTile(position + Vector2.down + Vector2.left);
			if (nearbyDownLeftTile != null && !newTileList.Contains(nearbyDownLeftTile))
			{
				newTileList.Add(nearbyDownLeftTile);
			}
			Tile nearbyDownRightTile = GetTile(position + Vector2.down + Vector2.right);
			if (nearbyDownRightTile != null && !newTileList.Contains(nearbyDownRightTile))
			{
				newTileList.Add(nearbyDownRightTile);
			}
		}

		return newTileList;
	}

	public Vector2 ToVector2(Direction dir)
	{
		if(dir == Direction.LeftUp)
		{
			return Vector2.left;
		}

		else if(dir == Direction.LeftDown)
		{
			return Vector2.down;
		}

		else if(dir == Direction.RightUp)
		{
			return Vector2.up;
		}

		else if(dir == Direction.RightDown)
		{
			return Vector2.right;
		}

		else if(dir == Direction.Left)
		{
			return Vector2.left+Vector2.down;
		}

		else if(dir == Direction.Right)
		{
			return Vector2.right+Vector2.up;
		}

		else if(dir == Direction.Up)
		{
			return Vector2.left+Vector2.up;
		}

		else return Vector2.right+Vector2.down;
	}

    public void UpdateTileStatusEffectsAtActionEnd() {
        foreach (var tile in GetAllTiles().Values) {
            foreach (var statusEffect in tile.GetStatusEffectList()) {
                if (statusEffect.IsOfType(StatusEffectType.Trap)) {
                    Trap.Update(statusEffect, tile);
                }
            }
        }
        foreach (var tile in GetAllTiles().Values) {
            foreach (var statusEffect in tile.GetStatusEffectList()) {
                if (statusEffect.GetRemainStack() != 0) {
                    for (int i = 0; i < statusEffect.fixedElem.actuals.Count; i++) {
                        statusEffect.CalculateAmount(i, true);
                    }
                } else
                    tile.RemoveStatusEffect(statusEffect);
            }
        }
    }

    public void EndPhase(int phase) {
        // Decrease each buff & debuff phase
        foreach (var tile in GetAllTiles())
            tile.Value.UpdateRemainPhaseAtPhaseEnd();
    }
    public bool isTileGenerationFinished = false;
	public void GenerateTiles (List<TileInfo> tileInfoList)
	{
		foreach (var tileInfo in tileInfoList)
		{
			GenerateTile(tileInfo);
		}
		isTileGenerationFinished = true;
	}

	public static List<Unit> GetUnitsOnTiles(List<Tile> tiles) {
		List<Unit> units = new List<Unit>();
		foreach (var tile in tiles) {
			if (tile.IsUnitOnTile()) {
				units.Add(tile.GetUnitOnTile());
			}
		}
		return units;
	}

	void GenerateTile (TileInfo tileInfo)
	{
		if (tileInfo.IsEmptyTile()) return;

		Vector2 tilePosition = tileInfo.GetTilePosition();
		Element tileElement = tileInfo.GetTileElement();
		int tileAPAtStandardHeight = tileInfo.GetTileAPAtStandardHeight();
		int tileHeight = tileInfo.GetTileHeight();
		int tileIndex = tileInfo.GetTileIndex();
		string displayName = tileInfo.GetDisplayName();

		int j = (int)tilePosition.y;
		int i = (int)tilePosition.x;

		// FIXME : 높이 보정치 추가할 것.
		Tile tile = Instantiate(tilePrefab, new Vector3(tileImageWidth * (j+i) * 0.5f, tileImageHeight * (j-i+tileHeight) * 0.5f, (j-i) * 0.1f), Quaternion.identity).GetComponent<Tile>();
		tile.SetTilePos(i, j);
		tile.SetTileInfo(tileElement, tileIndex, tileAPAtStandardHeight, tileHeight, displayName);
		tiles.Add(new Vector2(i, j), tile);
	}

	void Awake () {
		GenerateTiles(Parser.GetParsedTileInfo());
	}
}
