using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Turn
{
	public class ObjectUnitBehaviour{
		
		public static IEnumerator AllObjectUnitsBehave(BattleData battleData)
		{
			List<Unit> objectUnits = battleData.GetObjectUnitsList();
			foreach (var unit in objectUnits) {
				unit.SetNotAlreadyBehavedObject ();
			}
			while (true) {
				//오브젝트 때문에 오브젝트가 죽을 수도 있으니 하나 행동 끝날 때마다 매번 오브젝트유닛 목록을 다시 받아온다
				objectUnits = battleData.GetObjectUnitsList();
				Unit selectedObjectUnit = GetNotAlreadyBehavedObjectUnit (objectUnits);
				if (selectedObjectUnit == null)
					break;
				yield return AnObjectUnitBehave (battleData, selectedObjectUnit);
				selectedObjectUnit.SetAlreadyBehavedObject ();
			}
			yield return null;
		}

		private static Unit GetNotAlreadyBehavedObjectUnit(List<Unit> objectUnits){
			Unit notAlreadyBehavedObjectUnit = null;
			foreach (var unit in objectUnits) {
				if (!unit.IsAlreadyBehavedObject ()) {
					notAlreadyBehavedObjectUnit = unit;
					break;
				}
			}
			return notAlreadyBehavedObjectUnit;
		}

		private static IEnumerator AnObjectUnitBehave(BattleData battleData, Unit objectUnit){
			Debug.Log ("An object behave");
			battleData.selectedUnit = objectUnit;
			yield return AIStates_old.AIAttack (battleData);
			yield return null;
		}		
	}
}