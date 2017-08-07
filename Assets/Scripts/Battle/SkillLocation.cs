using UnityEngine;
using Enums;
public class SkillLocation {
	public static TileManager tileManager;
	Tile casterTile;
	Tile targetTile;
	Direction direction;
	public SkillLocation(Tile casterTile, Tile targetTile, Direction direction){
		this.casterTile = casterTile;
		this.targetTile = targetTile;
		this.direction = direction;
	}
	public SkillLocation(Vector2 casterPos, Tile targetTile, Direction direction){
		this.casterTile = tileManager.GetTile (casterPos);
		this.targetTile = targetTile;
		this.direction = direction;
	}
	public SkillLocation(Unit unit, Tile targetTile, Direction direction){
		this.casterTile = unit.GetTileUnderUnit ();
		this.targetTile = targetTile;
		this.direction = direction;
	}
	public Tile CasterTile{
		get { return casterTile; }
	}
	public Vector2 CasterPos{
		get { return casterTile.GetTilePos (); }
	}
	public Tile TargetTile{
		get { return targetTile; }
	}
	public Vector2 TargetPos{
		get { return targetTile.GetTilePos (); }
	}
	public Direction Direction{
		get { return direction; }
	}
}