using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Enums;

namespace Battle.Turn
{
	public class UdongNoodle
	{
		public static Vector2 FindNearestEnemy(List<GameObject> movableTiles, List<GameObject> units, GameObject mainUnit)
		{
			var positions = from tileGO in movableTiles
					from unitGO in units
					let tile = tileGO.GetComponent<Tile>()
					let unit = unitGO.GetComponent<Unit>()
					where unit.GetSide() == Side.Ally
					let distance = Vector2.Distance(tile.GetTilePos(), unit.GetPosition())
					orderby distance
					select tile.GetTilePos();

			return positions.First();
		}
	}

	public class AIStates
	{
		public static IEnumerator AIMove(BattleData battleData)
		{
			BattleManager battleManager = battleData.battleManager;

			battleManager.StartCoroutine(AIDIe(battleData));

			Dictionary<Vector2, TileWithPath> movableTilesWithPath = PathFinder.CalculatePath(battleData.selectedUnitObject);
			List<GameObject> movableTiles = new List<GameObject>();
			foreach (KeyValuePair<Vector2, TileWithPath> movableTileWithPath in movableTilesWithPath)
			{
				movableTiles.Add(movableTileWithPath.Value.tile);
			}

			battleData.uiManager.UpdateApBarUI(battleData, battleData.unitManager.GetAllUnits());

			// var randomPosition = movableTiles[Random.Range(0, movableTiles.Count)].GetComponent<Tile>().GetTilePos();
			var randomPosition = UdongNoodle.FindNearestEnemy(movableTiles, battleData.unitManager.GetAllUnits(), battleData.selectedUnitObject);

			// FIXME : 어딘가로 옮겨야 할 텐데...
			GameObject destTile = battleData.tileManager.GetTile(randomPosition);
			List<GameObject> destPath = movableTilesWithPath[randomPosition].path;
			Vector2 currentTilePos = battleData.selectedUnitObject.GetComponent<Unit>().GetPosition();
			Vector2 distanceVector = randomPosition - currentTilePos;
			int distance = (int)Mathf.Abs(distanceVector.x) + (int)Mathf.Abs(distanceVector.y);
			int totalUseActivityPoint = movableTilesWithPath[randomPosition].requireActivityPoint;

			// battleData.moveCount += distance;

			// battleData.tileManager.DepaintTiles(movableTiles, TileColor.Blue);
			battleData.currentState = CurrentState.CheckDestination;

			List<GameObject> destTileList = destPath;
			destTileList.Add(destTile);
			battleData.tileManager.PaintTiles(destTileList, TileColor.Blue);

			// 카메라를 옮기고
			Camera.main.transform.position = new Vector3(destTile.transform.position.x, destTile.transform.position.y, -10);

			battleData.currentState = CurrentState.MoveToTile;
			yield return battleManager.StartCoroutine(MoveStates.MoveToTile(battleData, destTile, Direction.Right, totalUseActivityPoint));

			yield return AIAttack(battleData);
		}

		public static IEnumerator AIDIe(BattleData battleData)
		{
			BattleManager battleManager = battleData.battleManager;
			battleData.retreatUnits = battleData.unitManager.GetRetreatUnits();
			battleData.deadUnits = battleData.unitManager.GetDeadUnits();

			yield return battleManager.StartCoroutine(BattleManager.DestroyRetreatUnits(battleData));
			yield return battleManager.StartCoroutine(BattleManager.DestroyDeadUnits(battleData));

			if (battleData.retreatUnits.Contains(battleData.selectedUnitObject))
			{
				yield return battleManager.StartCoroutine(BattleManager.FadeOutEffect(battleData.selectedUnitObject, 1));
				battleData.unitManager.DeleteRetreatUnit(battleData.selectedUnitObject);
				Debug.Log("SelectedUnit retreats");
				GameObject.Destroy(battleData.selectedUnitObject);
				yield break;
			}

			if (battleData.deadUnits.Contains(battleData.selectedUnitObject))
			{
				battleData.selectedUnitObject.GetComponent<SpriteRenderer>().color = Color.red;
				yield return battleManager.StartCoroutine(BattleManager.FadeOutEffect(battleData.selectedUnitObject, 1));
				battleData.unitManager.DeleteDeadUnit(battleData.selectedUnitObject);
				Debug.Log("SelectedUnit is dead");
				GameObject.Destroy(battleData.selectedUnitObject);
				yield break;
			}
		}

		public static IEnumerator AIAttack(BattleData battleData)
		{
			battleData.uiManager.DisableSkillUI();

			BattleManager battleManager = battleData.battleManager;
			battleData.indexOfSeletedSkillByUser = 1;
			Skill selectedSkill = battleData.SelectedSkill;

			SkillType skillTypeOfSelectedSkill = selectedSkill.GetSkillType();
			if (skillTypeOfSelectedSkill == SkillType.Auto || skillTypeOfSelectedSkill == SkillType.Self)
			{
				battleData.currentState = CurrentState.SelectSkillApplyDirection;
				yield return battleManager.StartCoroutine(SelectSkillApplyDirection(battleData, battleData.selectedUnitObject.GetComponent<Unit>().GetDirection()));
			}
			else
			{
				battleData.currentState = CurrentState.SelectSkillApplyPoint;
				yield return battleManager.StartCoroutine(SelectSkillApplyPoint(battleData, battleData.selectedUnitObject.GetComponent<Unit>().GetDirection()));
			}

			battleData.previewAPAction = null;
			battleData.uiManager.UpdateApBarUI(battleData, battleData.unitManager.GetAllUnits());

		}

		public static IEnumerator SelectSkillApplyDirection(BattleData battleData, Direction originalDirection)
		{
			Direction beforeDirection = originalDirection;
			List<GameObject> selectedTiles = new List<GameObject>();
			Unit selectedUnit = battleData.selectedUnitObject.GetComponent<Unit>();
			Skill selectedSkill = battleData.SelectedSkill;

			selectedTiles = battleData.tileManager.GetTilesInRange(selectedSkill.GetSecondRangeForm(),
														selectedUnit.GetPosition(),
														selectedSkill.GetSecondMinReach(),
														selectedSkill.GetSecondMaxReach(),
														selectedUnit.GetDirection());

			battleData.selectedUnitObject = selectedTiles[Random.Range(0, selectedTiles.Count)].GetComponent<Tile>().GetUnitOnTile();

			if (battleData.isSelectedTileByUser)
			{
				BattleManager battleManager = battleData.battleManager;
				battleData.currentState = CurrentState.CheckApplyOrChain;
				yield return battleManager.StartCoroutine(SkillAndChainStates.CheckApplyOrChain(battleData, battleData.SelectedUnitTile, originalDirection));
			}
		}

		public static IEnumerator SelectSkillApplyPoint(BattleData battleData, Direction originalDirection)
		{
			Direction beforeDirection = originalDirection;
			Unit selectedUnit = battleData.selectedUnitObject.GetComponent<Unit>();



			while (battleData.currentState == CurrentState.SelectSkillApplyPoint)
			{
				Vector2 selectedUnitPos = battleData.selectedUnitObject.GetComponent<Unit>().GetPosition();

				List<GameObject> activeRange = new List<GameObject>();
				Skill selectedSkill = battleData.SelectedSkill;
				activeRange = battleData.tileManager.GetTilesInRange(selectedSkill.GetFirstRangeForm(),
														selectedUnitPos,
														selectedSkill.GetFirstMinReach(),
														selectedSkill.GetFirstMaxReach(),
														battleData.selectedUnitObject.GetComponent<Unit>().GetDirection());

				Tile selectedTile = activeRange[Random.Range(0, activeRange.Count)].GetComponent<Tile>();

				battleData.selectedTilePosition = selectedTile.GetTilePos();

				// 타겟팅 스킬을 타겟이 없는 장소에 지정했을 경우 적용되지 않도록 예외처리 필요 - 대부분의 스킬은 논타겟팅. 추후 보강.

				BattleManager battleManager = battleData.battleManager;
				battleData.currentState = CurrentState.CheckApplyOrChain;

				List<GameObject> tilesInSkillRange = GetTilesInSkillRange(battleData, selectedTile);

				yield return SkillAndChainStates.ApplyChain(battleData, tilesInSkillRange);
				FocusUnit(battleData.SelectedUnit);
				battleData.currentState = CurrentState.FocusToUnit;

				yield return battleManager.StartCoroutine(SkillAndChainStates.CheckApplyOrChain(battleData, battleData.SelectedTile, originalDirection));
			}
		}

		private static void FocusUnit(Unit unit)
		{
			Camera.main.transform.position = new Vector3(unit.transform.position.x, unit.transform.position.y, -10);
		}

		private static List<GameObject> GetTilesInSkillRange(BattleData battleData, Tile targetTile)
		{
				Skill selectedSkill = battleData.SelectedSkill;
				List<GameObject> selectedTiles = battleData.tileManager.GetTilesInRange(selectedSkill.GetSecondRangeForm(),
																			targetTile.GetTilePos(),
																			selectedSkill.GetSecondMinReach(),
																			selectedSkill.GetSecondMaxReach(),
																			battleData.selectedUnitObject.GetComponent<Unit>().GetDirection());
				if (selectedSkill.GetSkillType() == SkillType.Auto)
					selectedTiles.Remove(targetTile.gameObject);
				return selectedTiles;
		}
	}
}
