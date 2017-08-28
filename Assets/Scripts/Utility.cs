using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Enums;
using GameData;

public class Utility : MonoBehaviour {
	public static Direction GetMouseDirectionByUnit(Unit unit, Direction originalDirection){
		Debug.Assert(unit != null);
		Direction mouseDirectionByUnit;
		Vector2 unitPosition = unit.realPosition;
		
		string directionString = "";
		Vector2 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3 (Input.mousePosition.x,Input.mousePosition.y,0));

		if(unit.GetTileUnderUnit().isMouseOver){ return originalDirection; }
		
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

	public static float GetDegreeToTarget(Vector2 startPosition, Vector2 targetPosition){
		float deltaDegree = Mathf.Atan2(targetPosition.y - startPosition.y, targetPosition.x - startPosition.x) * Mathf.Rad2Deg;
		return deltaDegree;
	}
	
	public static Direction GetDirectionToTarget(Unit unit, List<Tile> selectedTiles){
		Vector2 averagePos = new Vector2(0, 0);
		foreach (var tile in selectedTiles){
			averagePos += tile.GetTilePos();
		}
		averagePos = averagePos / (float)selectedTiles.Count;
		
		return GetDirectionToTarget(unit, averagePos);
	}
	
	public static Direction GetDirectionToTarget(Unit unit, Vector2 targetPosition){
		return GetDirectionToTarget (unit.GetPosition (), targetPosition);
	}
	public static Direction GetDirectionToTarget(Vector2 startPosition, Vector2 targetPosition){
		float deltaDegree = GetDegreeToTarget(startPosition, targetPosition);

		if ((-45 < deltaDegree) && (deltaDegree <= 45)) return Direction.RightDown;
		else if ((45 < deltaDegree) && (deltaDegree <= 135)) return Direction.RightUp;
		else if ((-135 < deltaDegree) && (deltaDegree <= -45)) return Direction.LeftDown;
		else if ((deltaDegree <= -135) || (135 < deltaDegree)) return Direction.LeftUp;

		else {
			Debug.LogWarning("Result degree : " + deltaDegree);
			return Direction.RightUp;	
		}
	}
	
	public static float GetDegreeAtAttack(Unit unit, Unit target){
		if (unit == target) return 180;
		
		float deltaDegreeAtLook = GetDegreeToTarget(unit.GetPosition(), target.GetPosition());
		
		float targetDegree;
		if (target.GetDirection() == Direction.RightDown) targetDegree = 0;
		else if (target.GetDirection() == Direction.RightUp) targetDegree = 90;
		else if (target.GetDirection() == Direction.LeftUp) targetDegree = -180;
		else targetDegree = -90;
		
		float deltaDegreeAtAttack = Mathf.Abs(targetDegree - deltaDegreeAtLook);
		
		return deltaDegreeAtAttack;
	}
	
	public static float GetDirectionBonus(Unit unit, Unit target){
		if (SceneData.stageNumber < Setting.directionOpenStage){
			return 1.0f;
		}
		
		//대상이 없으면 패널을 출력하지 말아야 함 / '지형지물'일 경우는 방향 보너스가 적용되지 않음
		if (target == null || target.IsObject) return 1.0f;
		
		float deltaDegreeAtAttack = GetDegreeAtAttack(unit, target);
		if ((deltaDegreeAtAttack < 45) || (deltaDegreeAtAttack > 315)) return 1.25f;
		else if ((deltaDegreeAtAttack < 135) || (deltaDegreeAtAttack > 225)) return 1.1f;
		else return 1.0f;
	}
	
	public static float GetCelestialBonus(Unit attacker, Unit defender){
		Celestial attackerCelestial = attacker.GetCelestial();
		Celestial defenderCelestial = defender.GetCelestial();
		if(SceneData.stageNumber < Setting.celestialOpenStage){
			return 1.0f;
		}
		// Earth > Sun > Moon > Earth
		if (attackerCelestial == Celestial.Sun) {
			if (defenderCelestial == Celestial.Moon)
				return 1.2f;
			else if (defenderCelestial == Celestial.Earth)
				return 0.8f;
			else
				return 1.0f; 
		} else if (attackerCelestial == Celestial.Moon) {
			if (defenderCelestial == Celestial.Earth)
				return 1.2f;
			else if (defenderCelestial == Celestial.Sun)
				return 0.8f;
			else
				return 1.0f; 
		} else if (attackerCelestial == Celestial.Earth) {
			if (defenderCelestial == Celestial.Sun)
				return 1.2f;
			else if (defenderCelestial == Celestial.Moon)
				return 0.8f;
			else
				return 1.0f; 
		}
		else return 1;
	}

    public static float GetHeightBonus(Unit attacker, Unit defender){
		if(SceneData.stageNumber < Setting.heightOpenStage){
			return 1;
		}

		// 상대가 낮으면 20% 추가, 상대가 높으면 20% 감소
        int attackerHeight = attacker.GetHeight();
		int defenderHeight = defender.GetHeight();

		if (attackerHeight > defenderHeight)
			return 1.2f;
		else if (attackerHeight < defenderHeight)
			return 0.8f;
		else return 1;
    }

	public static int GetDistance(Vector2 position1, Vector2 position2){
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
	public static List<Vector2> GetDiamondRange(Vector2 mid, int minReach, int maxReach){
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
	public static List<Vector2> GetSquareRange(Vector2 mid, int minReach, int maxReach){
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
	public static List<Vector2> GetStraightRange(Vector2 mid, int minReach, int maxReach, Direction dir){
		List<Vector2> range = GetFrontRange(mid, minReach, maxReach, 1, dir);
		return range;
	}
	public static List<Vector2> GetCrossRange(Vector2 mid, int minReach, int maxReach){
		List<Vector2> range = new List<Vector2>();
		if (minReach == 0)
			range.Add(mid);
		int newMinReach = Math.Max(1, minReach);
		foreach (Direction direction in EnumUtil.directions)
			range.AddRange (GetStraightRange (mid, newMinReach, maxReach, direction));
		return range;
	}
	public static List<Vector2> GetDiagonalCrossRange(Vector2 mid, int minReach, int maxReach){
		List<Vector2> range = new List<Vector2>();
		if (minReach == 0)
			range.Add(mid);
		int newMinReach = Math.Max(1, minReach);
		foreach(Direction direction in EnumUtil.nonTileDirections)
			range.AddRange (GetStraightRange (mid, newMinReach, maxReach, direction));
		return range;
	}
	public static List<Vector2> GetAllDirectionRange(Vector2 mid, int minReach, int maxReach){
		List<Vector2> range = new List<Vector2>();
		range.AddRange (GetCrossRange (mid, minReach, maxReach));
		range.AddRange (GetDiagonalCrossRange (mid, minReach, maxReach));
		return range;
	}
	public static List<Vector2> GetFrontRange(Vector2 mid, int minReach, int maxReach, int width, Direction dir){
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
	public static List<Vector2> GetSectorRange(Vector2 mid, int minReach, int maxReach, Direction dir){
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

	public static Direction GetFinalDirectionOfPath(Tile destTile, List<Tile> path, Direction originalDirection){
		Direction finalDirection;

		int steps = path.Count;

		if (steps > 0) {
			Tile prevLastTile = path [steps - 1];
			Vector2 prevLastTilePosition = prevLastTile.GetTilePos ();
			Vector2 destPos = destTile.GetTilePos ();

			Vector2 delta = destPos - prevLastTilePosition;
			finalDirection = VectorToDirection(delta);
		}
		else {
			finalDirection = originalDirection;
		}
		return finalDirection;
	}

	public static Direction VectorToDirection(Vector2 vector){
		if (vector == new Vector2 (1, 0))
			return Direction.RightDown;
		else if (vector == new Vector2 (-1, 0))
			return Direction.LeftUp;
		else if (vector == new Vector2 (0, 1))
			return Direction.RightUp;
		else // vector == new Vector2 (0, -1)
			return Direction.LeftDown;
	}

	public static Vector2 ToVector2(Direction dir){
		if(dir == Direction.LeftUp)
			return Vector2.left;
		else if(dir == Direction.LeftDown)
			return Vector2.down;
		else if(dir == Direction.RightUp)
			return Vector2.up;
		else if(dir == Direction.RightDown)
			return Vector2.right;

		else if(dir == Direction.Left)
			return Vector2.left+Vector2.down;
		else if(dir == Direction.Right)
			return Vector2.right+Vector2.up;
		else if(dir == Direction.Up)
			return Vector2.left+Vector2.up;
		else
			return Vector2.right+Vector2.down;
	}

	public static Sprite IllustOf(string name){
		return Resources.Load<Sprite>("StandingImage/" + name + "_standing");
	}

	public static Sprite SkillIconOf(string owner, int level, int column){
		string address = "";
		if(column == 0){
			address = "Passive";
		}else if(column == 1){
			address = "Left";
		}else if(column == 2){
			address = "Mid";
		}else if(column == 3){
			address = "Right";
		}

		if(level != 0){
			address += (level+6) / 7;
		}
		
		return Resources.Load<Sprite>("Icon/Skill/" + owner + "/" + address);
	}

	public static IEnumerator WaitForFewFrames(int frameCount){
		for(int i = 0; i < frameCount; i++){
			yield return null;
		}
	}
}