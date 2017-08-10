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

	public static List<Vector2> GetRange(RangeForm form, Vector2 mid, int minReach, int maxReach, int width, Direction dir){
		if (form == RangeForm.Diamond)
			return GetDiamondRange(mid, minReach, maxReach);
		else if (form == RangeForm.Square)
			return GetSquareRange(mid, minReach, maxReach);
		else if (form == RangeForm.Straight)
			return GetStraightRange(mid, minReach, maxReach, dir);
		else if (form == RangeForm.Cross)
			return GetCrossRange(mid, minReach, maxReach);
		else if (form == RangeForm.Diagonal)
			return GetDiagonalCrossRange(mid, minReach, maxReach);
		else if (form == RangeForm.AllDirection)
			return GetAllDirectionRange(mid, minReach, maxReach);
		else if (form == RangeForm.Front)
			return GetFrontRange(mid, minReach, maxReach, width, dir);
		else if (form == RangeForm.Sector)
			return GetSectorRange(mid, minReach, maxReach, dir);
		else
			return GetDiamondRange(mid, minReach, maxReach); // default return value
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
				if ((x <= midX - minReach || x >= midX + minReach)
				    && (y <= midY - minReach || y >= midY + minReach))
					range.Add (pos);
			}
		}
		return range;
	}
	public static List<Vector2> GetStraightRange(Vector2 mid, int minReach, int maxReach, Direction dir)
	{
		List<Vector2> range = GetFrontRange(mid, minReach, maxReach, 1, dir);
		return range;
	}
	public static List<Vector2> GetCrossRange(Vector2 mid, int minReach, int maxReach)
	{
		List<Vector2> range = new List<Vector2>();
		if (minReach == 0)
			range.Add(mid);
		int newMinReach = Math.Max(1, minReach);
		range.AddRange (GetStraightRange (mid, newMinReach, maxReach, Direction.LeftUp));
		range.AddRange (GetStraightRange (mid, newMinReach, maxReach, Direction.LeftDown));
		range.AddRange (GetStraightRange (mid, newMinReach, maxReach, Direction.RightUp));
		range.AddRange (GetStraightRange (mid, newMinReach, maxReach, Direction.RightDown));
		return range;
	}
	public static List<Vector2> GetDiagonalCrossRange(Vector2 mid, int minReach, int maxReach)
	{
		List<Vector2> range = new List<Vector2>();
		if (minReach == 0)
			range.Add(mid);
		int newMinReach = Math.Max(1, minReach);
		range.AddRange (GetStraightRange (mid, newMinReach, maxReach, Direction.Left));
		range.AddRange (GetStraightRange (mid, newMinReach, maxReach, Direction.Right));
		range.AddRange (GetStraightRange (mid, newMinReach, maxReach, Direction.Up));
		range.AddRange (GetStraightRange (mid, newMinReach, maxReach, Direction.Down));
		return range;
	}
	public static List<Vector2> GetAllDirectionRange(Vector2 mid, int minReach, int maxReach)
	{
		List<Vector2> range = new List<Vector2>();
		range.AddRange (GetCrossRange (mid, minReach, maxReach));
		range.AddRange (GetDiagonalCrossRange (mid, minReach, maxReach));
		return range;
	}
	public static List<Vector2> GetFrontRange(Vector2 mid, int minReach, int maxReach, int width, Direction dir)
	{
		List<Vector2> range = new List<Vector2>();
		Vector2 frontVector = ToVector2 (dir);
		Vector2 perpendicularVector = new Vector2(frontVector.y, frontVector.x);
		int sideOffset = (width - 1) / 2;
		for (int side = - sideOffset; side <=  sideOffset; side++) {
			for (int front = minReach; front <= maxReach; front++) {
				Vector2 pos = mid + side * perpendicularVector + front * frontVector;
				range.Add (pos);
			}
		}
		return range;
	}
	public static List<Vector2> GetSectorRange(Vector2 mid, int minReach, int maxReach, Direction dir)
	{
		List<Vector2> range = new List<Vector2>();
		Vector2 frontVector = ToVector2 (dir);
		Vector2 perpendicularVector = new Vector2(frontVector.y, frontVector.x);
		for (int front = minReach; front <= maxReach; front++) {
			int sideOffset = front - 1;
			if (minReach == 0)
				sideOffset++;
			for (int side = - sideOffset; side <=  sideOffset; side++) {
				Vector2 pos = mid + side * perpendicularVector + front * frontVector;
				range.Add (pos);
			}
		}
		return range;
	}

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
