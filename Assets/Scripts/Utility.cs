using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Enums;

public class Utility : MonoBehaviour {

	public static Direction GetMouseDirectionByUnit(GameObject unit)
	{
		Direction mouseDirectionByUnit;
		Vector2 unitPosition = unit.transform.position;
		
		string directionString = "";
		Vector2 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3 (Input.mousePosition.x,Input.mousePosition.y,0));
		
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

	public static float GetDegreeToTarget(GameObject unit, Vector2 targetPosition)
	{
		Vector2 unitPosition = unit.GetComponent<Unit>().GetPosition();
		float deltaDegree = Mathf.Atan2(targetPosition.y - unitPosition.y, targetPosition.x - unitPosition.x) * Mathf.Rad2Deg;
		
		return deltaDegree;
	}
	
	public static Direction GetDirectionToTarget(GameObject unit, List<GameObject> selectedTiles)
	{
		Vector2 averagePos = new Vector2(0, 0);
		foreach (var tile in selectedTiles)
		{
			averagePos += tile.GetComponent<Tile>().GetTilePos();
		}
		averagePos = averagePos / (float)selectedTiles.Count;
		
		return GetDirectionToTarget(unit, averagePos);
	}
	
	public static Direction GetDirectionToTarget(GameObject unit, Vector2 targetPosition)
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
	
	public static float GetDegreeAtAttack(GameObject unitObject, GameObject targetObject)
	{
		if (unitObject == targetObject) return 180;
		
		float deltaDegreeAtLook = GetDegreeToTarget(unitObject, targetObject.GetComponent<Unit>().GetPosition());
		
		float targetDegree;
		Unit target = targetObject.GetComponent<Unit>();
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
	
	public static float GetDirectionBonus(GameObject unitObject, GameObject targetObject)
	{
		if (targetObject.GetComponent<Unit>() == null) return 1;
		
		float deltaDegreeAtAttack = GetDegreeAtAttack(unitObject, targetObject);
		if ((deltaDegreeAtAttack < 45) || (deltaDegreeAtAttack > 315)) return 1.25f;
		else if ((deltaDegreeAtAttack < 135) || (deltaDegreeAtAttack > 225)) return 1.1f;
		else return 1;
	}
	
	public static float GetCelestialBonus(GameObject attacker, GameObject defender)
	{
		Celestial attackerCelestial = attacker.GetComponent<Unit>().GetCelestial();
		Celestial defenderCelestial = defender.GetComponent<Unit>().GetCelestial();
		
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
}
