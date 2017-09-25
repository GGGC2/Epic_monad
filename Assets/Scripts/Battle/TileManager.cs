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
	public Tile preSelectedMouseOverTile;

	public Dictionary<Vector2, Tile> GetAllTiles(){
		return tiles;
	}

	public Tile GetTile(int x, int y){
		Vector2 key = new Vector2 (x, y);
		if (tiles.ContainsKey(key))
			return tiles[key];
		else
			return null;
	}

	public Tile GetTile(Vector2 position){
		Vector2 key = position;
		if (tiles.ContainsKey(key)){
			return tiles[key];
		}else{
			//Debug.Log("there is NO tile at position " + position.x + ", " + position.y);
			return null;
		}
	}

	public Vector3 GetTilePos(Vector2 position){
		Tile tile = GetTile(position);
		Debug.Assert(tile != null, "NO GetTile(" + position.x + "," + position.y + ")");
		return tile.transform.position;
	}

    public Vector3 CalculateAverageRealPositionOfTile() {
        Vector3 average = new Vector3(0, 0, 0);
        foreach(var tile in tiles) {
            average += tile.Value.realPosition;
        }
        average /= tiles.Count;
        return average;
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
			// 즉시 막혀서 더는 진행하지 못하는 마지막 타일을 return한다.

			// 첫 유닛을 만난 경우
			// 이번 타일을 return하고 종료한다.
			if (tile.IsUnitOnTile())
				return tile;
		}
		if (tiles.Count == 0) // 맵의 한쪽 끝에서 빈 방향으로 쏘거나 낮은 타일에서 바로 옆 높은 타일 쪽으로 쏘면 이렇게 된다
			return null;
		else
			return tiles.Last ();
	}

	public List<Tile> GetTilesInRange(RangeForm form, Vector2 mid, int minReach, int maxReach, int width, Direction dir){
		if (form == RangeForm.Global)
			return GetTilesInGlobalRange ();
		else
			return GetTilesInPositionRange (Utility.GetRange (form, mid, minReach, maxReach, width, dir));
	}
	public List<Tile> GetTilesInGlobalRange()
	{
		List<Tile> tilesInRange = new List<Tile>();
		foreach (var key in tiles.Keys)
		{
			tilesInRange.Add(tiles[key]);
		}
		return tilesInRange;
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

	public void PreselectTiles(List<Tile> tiles){
		if (preselectLockOn)
			return;
		else {
			foreach (Tile tile in tiles) {
				tile.SetPreSelected (true);
			}
		}
	}

	public void DepreselectTiles(List<Tile> tiles) {
		if (preselectLockOn) {return;}
		else {
			foreach (Tile tile in tiles) {
				tile.SetPreSelected (false);
			}
		}
	}

	public void DepreselectAllTiles(){
		if(preselectLockOn) {return;}
		else{
			foreach (Tile tile in GetTilesInGlobalRange()) {
				tile.SetPreSelected (false);
			}
		}
	}

	public void SetPreselectLock(bool OnOff) {preselectLockOn = OnOff;}

	public void PaintTiles(List<Tile> tiles, TileColor color){
		foreach(var tile in tiles)
			tile.PaintTile(color);
	}
	public void DepaintTiles(List<Tile> tiles, TileColor color){
		foreach(var tile in tiles){
			tile.DepaintTile(color);
		}
	}
	public void SetHighlightTiles(List<Tile> tiles, bool action){
		foreach(var tile in tiles){
			tile.SetHighlight(action);
		}
	}

	public void DepaintAllTiles(TileColor color){
		DepaintTiles(GetTilesInGlobalRange(), color);
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
	public void GenerateTiles (List<TileInfo> tileInfoList){
		foreach (var tileInfo in tileInfoList){
			GenerateTile(tileInfo);
		}
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
    public void UpdateRealTilePositions() {
        foreach(var tile in GetAllTiles()) {
            tile.Value.UpdateRealPosition();
        }
    }
	public static Vector3 CalculateRealTilePosition(int posX, int posY, int height){
        int realX = 0 , realZ = 0;
        switch(BattleData.aspect) {
        case (Aspect.North):
            realX = posX + posY;
            realZ = posY - posX;
            break;
        case (Aspect.East):
            realX = posX - posY;
            realZ = posY + posX;
            break;
        case (Aspect.South):
            realX = - posX - posY;
            realZ = - posY + posX;
            break;
        case (Aspect.West):
            realX = - posX + posY;
            realZ = - posY - posX;
            break;
        }
		return new Vector3(Setting.tileImageWidth * realX * 0.5f, Setting.tileImageHeight * (realZ+height) * 0.5f, realZ * 0.1f);
	}
	void GenerateTile (TileInfo tileInfo){
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
		Tile tile = Instantiate(tilePrefab, CalculateRealTilePosition(i, j, tileHeight), Quaternion.identity).GetComponent<Tile>();
		tile.name = "Tile(" + i + "," + j + ")";
		tile.SetTilePos(i, j);
		tile.SetTileInfo(tileElement, tileIndex, tileAPAtStandardHeight, tileHeight, displayName);
		tiles.Add(new Vector2(i, j), tile);
	}
}