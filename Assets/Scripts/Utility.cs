using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Enums;

public class Utility : MonoBehaviour {

	public static Direction GetMouseDirectionByUnit(Unit unit, Direction originalDirection){
		Direction mouseDirectionByUnit;
		Vector2 unitPosition = unit.gameObject.transform.position;
		
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
		if (target == null) return 1;

		if (target.IsObject()) return 1; // '지형지물'일 경우는 방향 보너스가 적용되지 않음
		
		float deltaDegreeAtAttack = GetDegreeAtAttack(unit, target);
		if ((deltaDegreeAtAttack < 45) || (deltaDegreeAtAttack > 315)) return 1.25f;
		else if ((deltaDegreeAtAttack < 135) || (deltaDegreeAtAttack > 225)) return 1.1f;
		else return 1;
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
}
