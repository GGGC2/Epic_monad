using UnityEngine;
using System.Collections;
using Enums;

public class TileInfo {
	Vector2 tilePosition;
	TileForm tileForm;
	Element tileElement;
	bool isEmptyTile;
	
	public Vector2 GetTilePosition() { return tilePosition; }
	public TileForm GetTileForm() { return tileForm; }
	public Element GetTileElement() { return tileElement; }
	public bool IsEmptyTile() { return isEmptyTile; }    
	
	public TileInfo(Vector2 tilePosition, string tileInfoString)
	{    
		if (tileInfoString[0] == '-')
		{
			this.isEmptyTile = true;
			return;    
		}
		
		this.isEmptyTile = false;
		
		char tileFormChar = tileInfoString[0];    
		this.tilePosition = tilePosition;
		
		if (tileFormChar == 'F')
			this.tileForm = TileForm.Flatland;
		else if (tileFormChar == 'H')
			this.tileForm = TileForm.Hill;
		else
		{
			Debug.LogError("Undefined tileForm: <" + tileFormChar + ">" + " at " + tilePosition);
		}
		
		if (tileInfoString.Length < 2)
		{
			this.tileElement = Element.None;
			return;
		}
		
		char tileElementChar = tileInfoString[1];
		
		if (tileElementChar == 'F')
			this.tileElement = Element.Fire;
		else if (tileElementChar == 'W')
			this.tileElement = Element.Water;
		else if (tileElementChar == 'P')
			this.tileElement = Element.Plant;
		else if (tileElementChar == 'M')
			this.tileElement = Element.Metal;
		else
			Debug.LogError("Undefined tileType: <" + tileElement + ">" + " at " + tilePosition);
	} 
}
