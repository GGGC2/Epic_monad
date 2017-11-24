using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

public class PlaceInfo {
	public Vector2 position;
	public Direction direction;
	
	public PlaceInfo(Vector2 position, Direction direction) {
		this.position = position;
		this.direction = direction;
	}

	public PlaceInfo(string line){
		StringParser tabParser = new StringParser(line, '\t');
		int posX = tabParser.ConsumeInt();
		int posY = tabParser.ConsumeInt();
		this.position = new Vector2(posX, posY);
		this.direction = tabParser.ConsumeEnum<Direction>();
	}
}
