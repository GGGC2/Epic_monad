using UnityEngine;
using System.Collections;
using Enums;
using System;

public class TileInfo {
	Vector2 tilePosition;
	int tileAPAtStandardHeight;
	int tileHeight; // 추후 높이 시스템 구현되면 사용.
	Element tileElement;
	int tileTypeIndex;
	bool isEmptyTile;

	public Vector2 GetTilePosition() { return tilePosition; }
	public Element GetTileElement() { return tileElement; }
	public int GetTileAPAtStandardHeight() { return tileAPAtStandardHeight; }
	public int GetTileHeight() { return tileHeight; }
	public int GetTileIndex() { return tileTypeIndex; }
	public bool IsEmptyTile() { return isEmptyTile; }

	public TileInfo(Vector2 tilePosition, string tileInfoString)
	{
		if (tileInfoString[0] == '-')
		{
			this.isEmptyTile = true;
			return;
		}

		this.isEmptyTile = false;

		this.tilePosition = tilePosition;

		char tileElementChar = tileInfoString[0];

		if (tileElementChar == 'F')
			this.tileElement = Element.Fire;
		else if (tileElementChar == 'W')
			this.tileElement = Element.Water;
		else if (tileElementChar == 'P')
			this.tileElement = Element.Plant;
		else if (tileElementChar == 'M')
			this.tileElement = Element.Metal;
		else if (tileElementChar == 'N')
			this.tileElement = Element.None;
		else
			Debug.LogError("Undefined tileType: <" + tileElement + ">" + " at " + tilePosition);

		string tileTypeIndexSubstring = tileInfoString.Substring(1,2);
		int number;
		if (Int32.TryParse (tileTypeIndexSubstring, out number))
			this.tileTypeIndex = Convert.ToInt32(tileTypeIndexSubstring);
		else
			Debug.LogError ("Undefined tileTypeIndex: <" + tileTypeIndexSubstring + ">" + "at" + tilePosition);

		string tileHeightSubstring = tileInfoString.Substring(3,2);
		if (Int32.TryParse(tileHeightSubstring, out number))
			this.tileHeight = Convert.ToInt32(tileHeightSubstring);
		else
			Debug.LogError ("Undefined tileHeight: <" + tileHeightSubstring + ">" + "at" + tilePosition);

		// FIXME : 타일 AP 세팅 부분. 임시 구현.
		if (tileElement == Element.Water)
			this.tileAPAtStandardHeight = 9999;
		else
			this.tileAPAtStandardHeight = 3;

	}
}
