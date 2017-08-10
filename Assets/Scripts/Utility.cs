using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Enums;

public class Utility : MonoBehaviour {

	public static Direction GetMouseDirectionByUnit(Unit unit, Direction originalDirection){
		Direction mouseDirectionByUnit;
		Vector2 unitPosition = unit.realPosition;
		
		string directionString = "";
		Vector2 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3 (Input.mousePosition.x,Input.mousePosition.y,0));

		if(unit.GetTileUnderUnit().isHighlight){ return originalDirection; }
		
		if (mousePosition.x < unitPosition.x)
			directionString += "Left";
		else
			directionString += "Right";
			
		if (mousePosition.y > unitPosition.y)
			directionString += "Up";
		else
			directionString += "Down";
		
		mouseDirectionByUnit = (Direction)Enum.Parse(typeof(Direction), directionString);
		
		return mouseDirectionByUnit;
	}

	public static float GetDegreeToTarget(Unit unit, Vector2 targetPosition)
	{
		Vector2 unitPosition = unit.GetPosition();
		float deltaDegree = Mathf.Atan2(targetPosition.y - unitPosition.y, targetPosition.x - unitPosition.x) * Mathf.Rad2Deg;
		
		return deltaDegree;
	}
	
	public static Direction GetDirectionToTarget(Unit unit, List<Tile> selectedTiles)
	{
		Vector2 averagePos = new Vector2(0, 0);
		foreach (var tile in selectedTiles)
		{
			averagePos += tile.GetTilePos();
		}
		averagePos = averagePos / (float)selectedTiles.Count;
		
		return GetDirectionToTarget(unit, averagePos);
	}
	
	public static Direction GetDirectionToTarget(Unit unit, Vector2 targetPosition)
	{
		float deltaDegree = GetDegreeToTarget(unit, targetPosition);
		
		if ((-45 < deltaDegree) && (deltaDegree <= 45)) return Direction.RightDown;
		else if ((45 < deltaDegree) && (deltaDegree <= 135)) return Direction.RightUp;
		else if ((-135 < deltaDegree) && (deltaDegree <= -45)) return Direction.LeftDown;
		else if ((deltaDegree <= -135) || (135 < deltaDegree)) return Direction.LeftUp;

		else 
		{
			Debug.LogWarning("Result degree : " + deltaDegree);
			return Direction.RightUp;	
		}
	}
	
	public static float GetDegreeAtAttack(Unit unit, Unit target)
	{
		if (unit == target) return 180;
		
		float deltaDegreeAtLook = GetDegreeToTarget(unit, target.GetPosition());
		
		float targetDegree;
		if (target.GetDirection() == Direction.RightDown) targetDegree = 0;
		else if (target.GetDirection() == Direction.RightUp) targetDegree = 90;
		else if (target.GetDirection() == Direction.LeftUp) targetDegree = -180;
		else targetDegree = -90;
		
		float deltaDegreeAtAttack = Mathf.Abs(targetDegree - deltaDegreeAtLook);
		
		// if ((deltaDegreeAtAttack < 45) || (deltaDegreeAtAttack > 315)) Debug.LogWarning("Back attack to " + target.GetName() + " Degree : " + deltaDegreeAtAttack);
		// else if ((deltaDegreeAtAttack < 135) || (deltaDegreeAtAttack > 225)) Debug.LogWarning("Side attack to " + target.GetName() + " Degree : " + deltaDegreeAtAttack);
		// else Debug.LogWarning("Front attack to " + target.GetName() + " Degree : " + deltaDegreeAtAttack);
		
		return deltaDegreeAtAttack;
	}
	
	public static float GetDirectionBonus(Unit unit, Unit target)
	{
		if (target == null) return 1.0f;

		if (target.IsObject()) return 1.0f; // '지형지물'일 경우는 방향 보너스가 적용되지 않음
		
		float deltaDegreeAtAttack = GetDegreeAtAttack(unit, target);
		if ((deltaDegreeAtAttack < 45) || (deltaDegreeAtAttack > 315)) return 1.25f;
		else if ((deltaDegreeAtAttack < 135) || (deltaDegreeAtAttack > 225)) return 1.1f;
		else return 1.0f;
	}
	
	public static float GetCelestialBonus(Unit attacker, Unit defender)
	{
		Celestial attackerCelestial = attacker.GetCelestial();
		Celestial defenderCelestial = defender.GetCelestial();
		
		// Earth > Sun > Moon > Earth
		if (attackerCelestial == Celestial.Sun)
		{
			if (defenderCelestial == Celestial.Moon) return 1.2f;
			else if (defenderCelestial == Celestial.Earth) return 0.8f;
			else return 1.0f; 
		}
		else if (attackerCelestial == Celestial.Moon)
		{
			if (defenderCelestial == Celestial.Earth) return 1.2f;
			else if (defenderCelestial == Celestial.Sun) return 0.8f;
			else return 1.0f; 
		}
		else if (attackerCelestial == Celestial.Earth)
		{
			if (defenderCelestial == Celestial.Sun) return 1.2f;
			else if (defenderCelestial == Celestial.Moon) return 0.8f;
			else return 1.0f; 
		}
		
		else return 1;
	}

    public static float GetHeightBonus(Unit attacker, Unit defender)
    {
		// 상대가 낮으면 20% 추가, 상대가 높으면 20% 감소
        int attackerHeight = attacker.GetHeight();
		int defenderHeight = defender.GetHeight();

		if (attackerHeight > defenderHeight)
			return 1.2f;
		else if (attackerHeight < defenderHeight)
			return 0.8f;
		else return 1;
    }

	public static int GetDistance(Vector2 position1, Vector2 position2)
	{
		return Math.Abs((int)position1.x - (int)position2.x) + Math.Abs((int)position1.y - (int)position2.y);
	}

	public static List<Vector2> GetDiamondRange(Vector2 mid, int minReach, int maxReach)
	{
		List<Vector2> range = new List<Vector2>();
		int midX = (int)mid.x;
		int midY = (int)mid.y;
		for (int x = midX - maxReach; x <= midX + maxReach; x++) {
			for (int y =midY - maxReach; y <= midY + maxReach; y++) {
				Vector2 pos = new Vector2 (x, y);
				if (GetDistance (mid, pos) <= maxReach && GetDistance (mid, pos) >= minReach)
					range.Add (pos);
			}
		}
		return range;
	}
	public static List<Vector2> GetSquareRange(Vector2 mid, int minReach, int maxReach)
	{
		List<Vector2> range = new List<Vector2>();
		int midX = (int)mid.x;
		int midY = (int)mid.y;
		for (int x = midX - maxReach; x <= midX + maxReach; x++) {
			for (int y = midY - maxReach; y <= midY + maxReach; y++) {
				Vector2 pos = new Vector2 (x, y);
				if (x <= midX - minReach && x >= midX + minReach
					&& y <= midY - minReach && y >= midY + minReach)
					range.Add (pos);
			}
		}
		return range;
	}
	/*
	List<Vector2> GetStraightRange(Vector2 mid, int minReach, int maxReach, Direction dir)
	{
		List<Vector2> range = new List<Vector2>();
		int midX = (int)mid.x;
		int midY = (int)mid.y;
		for (int i = minReach; i <= maxReach; i++) {
			Vector2 pos = mid;
		}
		for (int x = midX - maxReach; x <= midX + maxReach; x++) {
			for (int y = midY - maxReach; y <= midY + maxReach; y++) {
				Vector2 pos = new Vector2 (x, y);
				if (x <= midX - minReach && x >= midX + minReach
					&& y <= midY - minReach && y >= midY + minReach)
					range.Add (pos);
			}
		}
		return range;

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

	List<Vector2> GetCrossRange(Vector2 mid, int minReach, int maxReach)
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

	List<Vector2> GetDiagonalCrossRange(Vector2 mid, int minReach, int maxReach)
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

	List<Vector2> GetAllDirectionRange(Vector2 mid, int minReach, int maxReach)
	{
		List<Tile> tilesInRange = new List<Tile>();

		if (minReach == 0) tilesInRange.Add(GetTile(mid));
		minReach = Math.Max(1, minReach);
		tilesInRange = tilesInRange.Concat(GetTilesInCrossRange(mid, minReach,maxReach)).ToList();
		tilesInRange = tilesInRange.Concat(GetTilesInDiagonalCrossRange(mid, minReach, maxReach)).ToList();

		tilesInRange = tilesInRange.FindAll(t => t != null);

		return tilesInRange;
	}

	List<Vector2> GetFrontRange(Vector2 mid, int minReach, int maxReach, int width, Direction dir)
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

	List<Vector2> GeSectorRange(Vector2 mid, int minReach, int maxReach, Direction dir)
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

	List<Vector2> GetGlobalRange()
	{
		List<Tile> tilesInRange = new List<Tile>();
		foreach (var key in tiles.Keys)
		{
			tilesInRange.Add(tiles[key]);
		}

		return tilesInRange;
	}

	*/

	public  static Vector2 ToVector2(Direction dir)
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
}
