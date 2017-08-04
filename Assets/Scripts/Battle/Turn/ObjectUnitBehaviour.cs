﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

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
			Debug.Log ("An object behaves");
			battleData.selectedUnit = objectUnit;
			if (objectUnit.GetNameInCode() == "controller")
				yield return ControllerAttack(objectUnit);
			else
				yield return AIStates_old.AIAct ();
			yield return null;
		}

		static IEnumerator ControllerAttack(Unit objectUnit)
		{
			SoundManager.Instance.PlaySE ("ControllerGrawl");

			BattleManager.MoveCameraToUnit(objectUnit);
			GameObject effect = MonoBehaviour.Instantiate(Resources.Load("Effect/ControllerActive")) as GameObject;
			effect.transform.position = objectUnit.transform.position - new Vector3(0, 0, 0.01f);
			yield return new WaitForSeconds(0.5f);

			UnitManager unitManager = MonoBehaviour.FindObjectOfType<UnitManager>();
			List<Unit> targets = unitManager.GetAllUnits().FindAll(unit => unit.GetSide() == Side.Ally);
			foreach (var target in targets)
			{
				BattleManager.MoveCameraToUnit(target);
				float damageAmount = target.GetMaxHealth() * 0.15f;
				yield return target.Damaged(damageAmount, objectUnit, -target.GetStat(Stat.Defense), -target.GetStat(Stat.Resistance), true, false);
			}

			MonoBehaviour.Destroy(effect);

			yield return null;
		}		
	}
}