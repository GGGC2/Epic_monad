using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Enums;

public class TileManager : MonoBehaviour {
	private static TileManager instance = null;
	public static TileManager Instance { get { return instance; } }
	public static void SetInstance() { instance = FindObjectOfType<TileManager>(); }

	public GameObject tilePrefab;

	Dictionary<Vector2, Tile> tiles = new Dictionary<Vector2, Tile>();
	bool preselectLockOn=false;

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
	public List<Tile> GetTilesInPositionRange(List<Vector2> positionRange){
		List<Tile> tiles = new List<Tile> ();
		foreach (Vector2 pos in positionRange) {
			Tile tile = GetTile (pos);
			if (tile != null)
				tiles.Add (tile);
		}
		return tiles;
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
		return GetTilesInPositionRange (Utility.GetDiamondRange (mid, minReach, maxReach));
	}

	List<Tile> GetTilesInSquareRange(Vector2 mid, int minReach, int maxReach)
	{
		return GetTilesInPositionRange (Utility.GetSquareRange (mid, minReach, maxReach));
	}

	List<Tile> GetTilesInStraightRange(Vector2 mid, int minReach, int maxReach, Direction dir)
	{
		return GetTilesInPositionRange (Utility.GetStraightRange (mid, minReach, maxReach, dir));
	}

	List<Tile> GetTilesInCrossRange(Vector2 mid, int minReach, int maxReach)
	{
		return GetTilesInPositionRange (Utility.GetCrossRange (mid, minReach, maxReach));
	}

	List<Tile> GetTilesInDiagonalCrossRange(Vector2 mid, int minReach, int maxReach)
	{
		return GetTilesInPositionRange (Utility.GetDiagonalCrossRange (mid, minReach, maxReach));
	}

	List<Tile> GetTilesInAllDirectionRange(Vector2 mid, int minReach, int maxReach)
	{
		return GetTilesInPositionRange (Utility.GetAllDirectionRange (mid, minReach, maxReach));
	}

	List<Tile> GetTilesInFrontRange(Vector2 mid, int minReach, int maxReach, int width, Direction dir)
	{
		return GetTilesInPositionRange (Utility.GetFrontRange (mid, minReach, maxReach, width, dir));
	}

	List<Tile> GetTilesInSectorRange(Vector2 mid, int minReach, int maxReach, Direction dir)
	{
		return GetTilesInPositionRange (Utility.GetSectorRange (mid, minReach, maxReach, dir));
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

	public void PreselectTiles(List<Tile> tiles)
	{
		if (preselectLockOn)
			return;
		else {
			foreach (Tile tile in tiles) {
				tile.SetPreSelected (true);
			}
		}
	}
	public void DepreselectAllTiles()
	{
		if (preselectLockOn)
			return;
		else {
			foreach (Tile tile in GetTilesInGlobalRange()) {
				tile.SetPreSelected (false);
			}
		}
	}
	public void LockPreselect()
	{
		preselectLockOn = true;
	}
	public void UnlockPreselect()
	{
		preselectLockOn = false;
	}

	public void PaintTiles(List<Tile> tiles, TileColor color)
	{
		foreach(var tile in tiles)
		{
			tile.PaintTile(color);
		}
	}
	public void DepaintTiles(List<Tile> tiles, TileColor color)
	{
		foreach(var tile in tiles)
		{
			tile.DepaintTile(color);
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
