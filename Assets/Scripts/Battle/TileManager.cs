using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Enums;

public class TileManager : MonoBehaviour {

	public GameObject tilePrefab;
	
	Dictionary<Vector2, GameObject> tiles = new Dictionary<Vector2, GameObject>();

	float tileHeight = 0.5f*100/100;
	float tileWidth = 0.5f*200/100;

	public Dictionary<Vector2, GameObject> GetAllTiles()
	{
		return tiles;
	}

	public GameObject GetTile(int x, int y)
	{
		Vector2 key = new Vector2 (x, y);
		if (tiles.ContainsKey(key))
			return tiles[key];
		else
			return null;
	}
	
	public GameObject GetTile(Vector2 position)
	{
		Vector2 key = position;
		if (tiles.ContainsKey(key))
			return tiles[key];
		else
			return null;
	}
	
	public Vector3 GetTilePos(Vector2 position)
	{
		GameObject tile = GetTile(position);
		return tile.transform.position;
	}

	public List<GameObject> GetTilesInRange(RangeForm form, Vector2 mid, int minReach, int maxReach, Direction dir)
	{
		if (form == RangeForm.Square)
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
		else if (form == RangeForm.DiagonalCross)
		{
			return GetTilesInDiagonalCrossRange(mid, minReach, maxReach);
		}
		else if (form == RangeForm.Sector)
		{
			return GetTilesInSectorRange(mid, minReach, maxReach, dir);
		}
		else
			return GetTilesInSquareRange(mid, minReach, maxReach); // temp return value.
	}
	
	List<GameObject> GetTilesInSquareRange(Vector2 mid, int minReach, int maxReach)
	{
		List<GameObject> tilesInRange = new List<GameObject>();
		tilesInRange.Add(GetTile(mid));
		for (int i = 0; i < maxReach; i++)
		{
			tilesInRange = AddNearbyTiles(tilesInRange);
		}
		
		return tilesInRange;
	}
	
	List<GameObject> GetTilesInStraightRange(Vector2 mid, int minReach, int maxReach, Direction dir)
	{
		List<GameObject> tilesInRange = new List<GameObject>();
		tilesInRange.Add(GetTile(mid));
		
		for(int i = 0; i < maxReach; i++)
		{
			Vector2 position = mid + ToVector2(dir)*(i+1);
			if (GetTile(position) != null)
			{
				tilesInRange.Add(GetTile(position));
			}
		}
		
		return tilesInRange;
	}
	
	List<GameObject> GetTilesInCrossRange(Vector2 mid, int minReach, int maxReach)
	{
		List<GameObject> tilesInRange = new List<GameObject>();
		tilesInRange.Add(GetTile(mid));

		tilesInRange = tilesInRange.Concat(GetTilesInStraightRange(mid, minReach, maxReach, Direction.LeftUp)).ToList();
		tilesInRange = tilesInRange.Concat(GetTilesInStraightRange(mid, minReach, maxReach, Direction.LeftDown)).ToList();
		tilesInRange = tilesInRange.Concat(GetTilesInStraightRange(mid, minReach, maxReach, Direction.RightUp)).ToList();
		tilesInRange = tilesInRange.Concat(GetTilesInStraightRange(mid, minReach, maxReach, Direction.RightDown)).ToList();
		
		return tilesInRange;
	}
	
	List<GameObject> GetTilesInDiagonalCrossRange(Vector2 mid, int minReach, int maxReach)
	{
		List<GameObject> tilesInRange = new List<GameObject>();
		tilesInRange.Add(GetTile(mid));

		tilesInRange = tilesInRange.Concat(GetTilesInStraightRange(mid, minReach, maxReach, Direction.Left)).ToList();
		tilesInRange = tilesInRange.Concat(GetTilesInStraightRange(mid, minReach, maxReach, Direction.Right)).ToList();
		tilesInRange = tilesInRange.Concat(GetTilesInStraightRange(mid, minReach, maxReach, Direction.Up)).ToList();
		tilesInRange = tilesInRange.Concat(GetTilesInStraightRange(mid, minReach, maxReach, Direction.Down)).ToList();
		
		return tilesInRange;
	}

	List<GameObject> GetTilesInSectorRange(Vector2 mid, int minReach, int maxReach, Direction dir)
	{
		List<GameObject> tilesInRange = new List<GameObject>();
		Vector2 perpendicular = new Vector2(ToVector2(dir).y, ToVector2(dir).x); // 부채꼴 방향과 수직인 벡터

		for(int i = 0; i < maxReach; i++)
		{
			int j = i;
			Vector2 position = mid + ToVector2(dir)*(i+1);
			tilesInRange.Add(GetTile(position));
			while(j > 0)
			{
				tilesInRange.Add(GetTile(position + perpendicular*j));
				tilesInRange.Add(GetTile(position - perpendicular*j));
				j--;
			}
		}

		return tilesInRange;
	}
	
	public void PaintTiles(List<GameObject> tiles, TileColor color)
	{
		foreach(var tile in tiles)
		{
			tile.GetComponent<Tile>().PaintTile(color);
			tile.GetComponent<Tile>().SetPreSelected(true);
		}
	}
	
	public void DepaintTiles(List<GameObject> tiles, TileColor color)
	{
		foreach(var tile in tiles)
		{
			tile.GetComponent<Tile>().DepaintTile(color);
			tile.GetComponent<Tile>().SetPreSelected(false);
		}
	}
	
	List<GameObject> AddNearbyTiles(List<GameObject> tileList)
	{
		List<GameObject> newTileList = new List<GameObject>();
		foreach (var tile in tileList)
		{
			Vector2 position = tile.GetComponent<Tile>().GetTilePos();
			
			if (!newTileList.Contains(tile))
			{
				newTileList.Add(tile);
			}

			GameObject nearbyUpTile = GetTile(position + Vector2.up);
			if (nearbyUpTile != null && !newTileList.Contains(nearbyUpTile))
			{
				newTileList.Add(nearbyUpTile);
			}
			GameObject nearbyDownTile = GetTile(position + Vector2.down);
			if (nearbyDownTile != null && !newTileList.Contains(nearbyDownTile))
			{
				newTileList.Add(nearbyDownTile);
			}
			GameObject nearbyLeftTile = GetTile(position + Vector2.left);
			if (nearbyLeftTile != null && !newTileList.Contains(nearbyLeftTile))
			{
				newTileList.Add(nearbyLeftTile);
			}
			GameObject nearbyRightTile = GetTile(position + Vector2.right);
			if (nearbyRightTile != null && !newTileList.Contains(nearbyRightTile))
			{
				newTileList.Add(nearbyRightTile);
			}
		}
		
		return newTileList;
	}
	
	Vector2 ToVector2(Direction dir)
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
	
	void GenerateTiles (List<TileInfo> tileInfoList)
	{
		foreach (var tileInfo in tileInfoList)
		{
			GenerateTile(tileInfo);
		}
	}
	
	void GenerateTile (TileInfo tileInfo)
	{
		if (tileInfo.IsEmptyTile()) return;
		
		Vector2 tilePosition = tileInfo.GetTilePosition();
		TileForm tileForm = tileInfo.GetTileForm();
		Element tileElement = tileInfo.GetTileElement();
	
		int j = (int)tilePosition.y;
		int i = (int)tilePosition.x;
	
		GameObject tile = Instantiate(tilePrefab, new Vector3(tileWidth * (j+i) * 0.5f, tileHeight * (j-i) * 0.5f, (j-i) * 0.1f), Quaternion.identity) as GameObject;
		tile.GetComponent<Tile>().SetTilePos(i, j);
		tile.GetComponent<Tile>().SetTileInfo(tileForm, tileElement);
		
		tiles.Add(new Vector2(i, j), tile);
	}

	void Awake () {
		GenerateTiles(Parser.GetParsedTileInfo());
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
