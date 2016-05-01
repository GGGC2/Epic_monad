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

	public List<GameObject> GetTilesInRange(RangeForm form, Vector2 mid, int minReach, int maxReach, Direction dir, bool includeMyself)
	{
		if (form == RangeForm.Square)
		{
			return GetTilesInSquareRange(mid, minReach, maxReach, includeMyself);
		}
        else if (form == RangeForm.Straight)
        {
            return GetTilesInStraightRange(mid, minReach, maxReach, dir, includeMyself);
        }
        else if (form == RangeForm.Cross)
        {
            return GetTilesInCrossRange(mid, minReach, maxReach, includeMyself);
        }
        else if (form == RangeForm.DiagonalCross)
        {
            return GetTilesInDiagonalCrossRange(mid, minReach, maxReach, includeMyself);
        }
		else
			return GetTilesInSquareRange(mid, minReach, maxReach, includeMyself); // temp return value.
	}
	
	List<GameObject> GetTilesInSquareRange(Vector2 mid, int minReach, int maxReach, bool includeMyself)
	{
		List<GameObject> tilesInRange = new List<GameObject>();
		tilesInRange.Add(GetTile(mid));
		for (int i = 0; i < maxReach; i++)
		{
			tilesInRange = AddNearbyTiles(tilesInRange);
		}
		
		if (!includeMyself)
		{
			tilesInRange.Remove(tilesInRange[0]);
		}
		
		return tilesInRange;
	}
	
	List<GameObject> GetTilesInStraightRange(Vector2 mid, int minReach, int maxReach, Direction dir, bool includeMyself)
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
		
		if (!includeMyself)
		{
			tilesInRange.Remove(tilesInRange[0]);
		}
		
		return tilesInRange;
	}
	
	List<GameObject> GetTilesInCrossRange(Vector2 mid, int minReach, int maxReach, bool includeMyself)
	{
		List<GameObject> tilesInRange = new List<GameObject>();
		tilesInRange.Add(GetTile(mid));

        tilesInRange = tilesInRange.Concat(GetTilesInStraightRange(mid, minReach, maxReach, Direction.LeftUp, false)).ToList();
        tilesInRange = tilesInRange.Concat(GetTilesInStraightRange(mid, minReach, maxReach, Direction.LeftDown, false)).ToList();
        tilesInRange = tilesInRange.Concat(GetTilesInStraightRange(mid, minReach, maxReach, Direction.RightUp, false)).ToList();
        tilesInRange = tilesInRange.Concat(GetTilesInStraightRange(mid, minReach, maxReach, Direction.RightDown, false)).ToList();
        
        if(!includeMyself)
        {
            tilesInRange.Remove(tilesInRange[0]);
        }
        
        return tilesInRange;
    }
    
    List<GameObject> GetTilesInDiagonalCrossRange(Vector2 mid, int minReach, int maxReach, bool includeMyself)
    {
        List<GameObject> tilesInRange = new List<GameObject>();
        tilesInRange.Add(GetTile(mid));

        tilesInRange = tilesInRange.Concat(GetTilesInStraightRange(mid, minReach, maxReach, Direction.Left, false)).ToList();
        tilesInRange = tilesInRange.Concat(GetTilesInStraightRange(mid, minReach, maxReach, Direction.Right, false)).ToList();
        tilesInRange = tilesInRange.Concat(GetTilesInStraightRange(mid, minReach, maxReach, Direction.Up, false)).ToList();
        tilesInRange = tilesInRange.Concat(GetTilesInStraightRange(mid, minReach, maxReach, Direction.Down, false)).ToList();
        
        if(!includeMyself)
        {
            tilesInRange.Remove(tilesInRange[0]);
        }
        
        return tilesInRange;
    }
    
	public void ChangeTilesToSeletedColor(List<GameObject> tiles, TileColor color)
	{
		foreach(var tile in tiles)
		{
			if (color == TileColor.Red)
				tile.GetComponent<Tile>().SetTileColor(new Color(1, 0.5f, 0.5f, 1));
			else if (color == TileColor.Blue)
				tile.GetComponent<Tile>().SetTileColor(new Color(0.6f, 0.6f, 1, 1));
			tile.GetComponent<Tile>().SetPreSelected(true);
		}
	}
	
	public void ChangeTilesFromSeletedColorToDefaultColor(List<GameObject> tiles)
	{
		foreach(var tile in tiles)
		{
			tile.GetComponent<Tile>().SetTileColor(Color.white);
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
		// tile.GetComponent<Tile>().SetTileForm(tileForm);
		// tile.GetComponent<Tile>().SetTileElement(tileElement);
		
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
