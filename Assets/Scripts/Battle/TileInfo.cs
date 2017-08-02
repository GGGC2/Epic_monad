using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Enums;

public class TileInfo {
	Vector2 tilePosition;
	int tileAPAtStandardHeight;
	int tileHeight; // 추후 높이 시스템 구현되면 사용.
	Element tileElement;
	int tileTypeIndex;
	bool isEmptyTile;
	string displayName;

	public Vector2 GetTilePosition() { return tilePosition; }
	public Element GetTileElement() { return tileElement; }
	public int GetTileAPAtStandardHeight() { return tileAPAtStandardHeight; }
	public int GetTileHeight() { return tileHeight; }
	public int GetTileIndex() { return tileTypeIndex; }
	public bool IsEmptyTile() { return isEmptyTile; }
	public String GetDisplayName() { return displayName; }

	public TileInfo(Vector2 tilePosition, string tileInfoString){
		if (tileInfoString[0] == '-'){
			this.isEmptyTile = true;
			return;
		}

		this.isEmptyTile = false;

		this.tilePosition = tilePosition;

		char tileElementChar = tileInfoString[0];

		this.tileElement = ReadTileElementChar (tileElementChar);

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

		if (TileLibrary == null)
			LoadTileLibrary ();

		string tileIdentifier = tileInfoString.Substring (0, 3);
		this.tileAPAtStandardHeight = TileLibrary [tileIdentifier].baseAPCost;
		this.displayName = TileLibrary [tileIdentifier].displayName;
	}
	public static Element ReadTileElementChar(char tileElementChar){
		if (tileElementChar == 'F')
			return Element.Fire;
		else if (tileElementChar == 'W')
			return Element.Water;
		else if (tileElementChar == 'P')
			return Element.Plant;
		else if (tileElementChar == 'M')
			return Element.Metal;
		else if (tileElementChar == 'N')
			return Element.None;
		else {
			Debug.LogError ("Undefined tileType: <" + tileElementChar + ">");
			return Element.None;
		}
	}

	public static Dictionary<string, TileTypeData> TileLibrary = null;
	public class TileTypeData{
		public string identifier;
		public string displayName;
		public Element element;
		public int baseAPCost;
		public TileTypeData(string dataLine){
			CommaStringParser commaParser = new CommaStringParser(dataLine);
			displayName = commaParser.Consume();
			identifier=commaParser.Consume();
			baseAPCost = commaParser.ConsumeInt();
		}
	}
	private static void LoadTileLibrary (){
		TileLibrary = new Dictionary<string, TileTypeData> ();

		TextAsset csvFile;
		csvFile = Resources.Load("Data/TileLibrary") as TextAsset;
		string csvText = csvFile.text;
		string[] unparsedTileInfoStrings = csvText.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 1; i < unparsedTileInfoStrings.Length; i++) {
			TileTypeData tileType = new TileTypeData (unparsedTileInfoStrings [i]);
			TileLibrary [tileType.identifier] = tileType;
		}
	}
}
