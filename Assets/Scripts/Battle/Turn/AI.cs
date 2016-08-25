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
			Side otherSide;

			if (mainUnit.GetComponent<Unit>().GetSide() == Side.Ally)
			{
				otherSide = Side.Enemy;
			}
			else
			{
				otherSide = Side.Ally;
			}

			var positions = from tileGO in movableTiles
					from unitGO in units
					let tile = tileGO.GetComponent<Tile>()
					let unit = unitGO.GetComponent<Unit>()
					where unit.GetSide() == otherSide
					let distance = Vector2.Distance(tile.GetTilePos(), unit.GetPosition())
					orderby distance
					select tile.GetTilePos();

			List<Vector2> availablePositions = positions.ToList();
			if (availablePositions.Count > 0)
			{
				return availablePositions[0];
			}
			return mainUnit.GetComponent<Unit>().GetPosition();
		}

		public static Tile FindEnemyTile(List<GameObject> activeTileRange, GameObject mainUnit)
		{
			Side otherSide;

			if (mainUnit.GetComponent<Unit>().GetSide() == Side.Ally)
			{
				otherSide = Side.Enemy;
			}
			else
			{
				otherSide = Side.Ally;
			}

			var tilesHaveEnemy = from tileGO in activeTileRange
								 let tile = tileGO.GetComponent<Tile>()
								 where tile.GetUnitOnTile() != null
								 let unit = tile.GetUnitOnTile().GetComponent<Unit>()
								 where unit.GetSide() == otherSide
								 select tile;

			return tilesHaveEnemy.FirstOrDefault();
		}
	}

    public class SpaghettiConLumache {
        //
        //  ♪ღ♪*•.¸¸¸.•*¨¨*•.¸¸¸.•*•♪ღ♪¸.•*¨¨*•.¸¸¸.•*•♪ღ♪
        //  ♪ღ♪         Spaghetti Con Lumache          ♪ღ♪
        //  ♪ღ♪   Selection of the finest algorithms   ♪ღ♪
        //  ♪ღ♪        for Las Lumache to move         ♪ღ♪
        //  ♪ღ♪                                        ♪ღ♪
        //  ♪ღ♪          ** Chef's Choice **           ♪ღ♪
        //  ♪ღ♪*•.¸¸¸.•*¨¨*•.¸¸¸.•*•♪ღ♪¸.•*¨¨*•.¸¸¸.•*•♪ღ♪
        //

        public static Vector2 CalculateDestination(List<GameObject> movableTileObjects, List<GameObject> unitObjects, GameObject mainUnitObject) {
            // Destination calculation algorithm
            //
            // If it's far from (>3) manastone, go to the nearest manastone
            // Else if it's close to player (<=4), go to the nearest player
            // Else pick a random tile from moveable tiles that are close (<=2) to manastone

            Unit mainunit = mainUnitObject.GetComponent<Unit>();
            float distanceFromNearestManastone = float.PositiveInfinity;
            foreach (GameObject unitObject in unitObjects)
            {
                Unit unit = unitObject.GetComponent<Unit>();
                if (unit.GetNameInCode() != "manastone")
                {
                    continue;
                }
                float distance = Vector2.Distance(mainunit.GetPosition(), unit.GetPosition());
                if (distanceFromNearestManastone > distance)
                {
                    distanceFromNearestManastone = distance;
                }
            }


            if (distanceFromNearestManastone > 3)
            {
                return FindNearestManastone(movableTileObjects, unitObjects, mainUnitObject);
            }


            float distanceFromNearestPlayer = float.PositiveInfinity;
            foreach (GameObject unitObject in unitObjects) {
                Unit unit = unitObject.GetComponent<Unit>();
                if (unit.GetSide() != Side.Ally) {
                    continue;
                }
                float distance = Vector2.Distance(mainunit.GetPosition(), unit.GetPosition());
                if (distanceFromNearestPlayer > distance) {
                    distanceFromNearestPlayer = distance;
                }
            }


            if (distanceFromNearestPlayer <= 4)
            {
                return FindNearestPlayer(movableTileObjects, unitObjects, mainUnitObject);
            }

            List<Tile> nearManastoneTiles = new List<Tile>();
            foreach (GameObject movableTileObject in movableTileObjects) {
                Tile movableTile = movableTileObject.GetComponent<Tile>();

                foreach (GameObject unitObject in unitObjects) {
                    Unit unit = unitObject.GetComponent<Unit>();
                    if (unit.GetNameInCode() != "manastone") {
                        continue;
                    }
                    float distance = Vector2.Distance(movableTile.GetTilePos(), unit.GetPosition());
                    if (distance <= 4)
                    {
                        nearManastoneTiles.Add(movableTile);
                        break;
                    }
                }
            }

            Tile chosenRandomTile = nearManastoneTiles[Random.Range(0, nearManastoneTiles.Count)];

            return chosenRandomTile.GetTilePos();

        }

        public static Vector2 FindNearestManastone(List<GameObject> movableTileObjects, List<GameObject> unitObjects, GameObject mainUnitObject) {


            var positions = from tileGO in movableTileObjects
                            from unitGO in unitObjects
                            let tile = tileGO.GetComponent<Tile>()
                            let unit = unitGO.GetComponent<Unit>()
                            where unit.GetNameInCode() == "manastone"
                            let distance = Vector2.Distance(tile.GetTilePos(), unit.GetPosition())
                            orderby distance
                            select tile.GetTilePos();

            List<Vector2> availablePositions = positions.ToList();
            if (availablePositions.Count > 0) {
                return availablePositions[0];
            }
            return mainUnitObject.GetComponent<Unit>().GetPosition();
        }

        public static Vector2 FindNearestPlayer(List<GameObject> movableTileObjects, List<GameObject> unitObjects, GameObject mainUnitObject) {


            var positions = from tileGO in movableTileObjects
                            from unitGO in unitObjects
                            let tile = tileGO.GetComponent<Tile>()
                            let unit = unitGO.GetComponent<Unit>()
                            where unit.GetSide() == Side.Ally
                            let distance = Vector2.Distance(tile.GetTilePos(), unit.GetPosition())
                            orderby distance
                            select tile.GetTilePos();

            List<Vector2> availablePositions = positions.ToList();
            if (availablePositions.Count > 0) {
                return availablePositions[0];
            }
            return mainUnitObject.GetComponent<Unit>().GetPosition();
        }
    }

	public class OrchidBrain
	{
		public static bool Skill1Available(BattleData battleData)
		{
			Direction beforeDirection = battleData.selectedUnitObject.GetComponent<Unit>().GetDirection();
			Unit selectedUnit = battleData.selectedUnitObject.GetComponent<Unit>();

			battleData.indexOfSeletedSkillByUser = 1;
			Skill selectedSkill = battleData.SelectedSkill;

			List<GameObject> selectedTiles = new List<GameObject>();
			selectedTiles = battleData.tileManager.GetTilesInRange(selectedSkill.GetSecondRangeForm(),
														selectedUnit.GetPosition(),
														selectedSkill.GetSecondMinReach(),
														selectedSkill.GetSecondMaxReach(),
														selectedUnit.GetDirection());

			var enemies = from tileGO in selectedTiles
					let tile = tileGO.GetComponent<Tile>()
					let unitGO = tile.GetUnitOnTile()
					where unitGO != null
					where unitGO.GetComponent<Unit>().GetSide() == Side.Ally
					select unitGO;

			foreach (var a in enemies.ToList())
			{
				Debug.Log("target name  is " + a.GetComponent<Unit>().GetName());
			}

			return enemies.Count() >= 2;
		}

		public static bool Skill2Available(BattleData battleData)
		{
			Direction beforeDirection = battleData.selectedUnitObject.GetComponent<Unit>().GetDirection();
			Unit selectedUnit = battleData.selectedUnitObject.GetComponent<Unit>();

			battleData.indexOfSeletedSkillByUser = 2;
			Skill selectedSkill = battleData.SelectedSkill;

			List<GameObject> selectedTiles = new List<GameObject>();
			selectedTiles = battleData.tileManager.GetTilesInRange(selectedSkill.GetFirstRangeForm(),
														selectedUnit.GetPosition(),
														selectedSkill.GetFirstMinReach(),
														selectedSkill.GetFirstMaxReach(),
														selectedUnit.GetDirection());

			TileManager tileManager = battleData.tileManager;
			var enemiesNearEachOther =
					from tileGO in selectedTiles
					let tile = tileGO.GetComponent<Tile>()
					let nearTileGOs = tileManager.GetTilesInRange(RangeForm.Square, tile.GetTilePos(), 0, 1, Direction.Down)
					let nearTileUnits = nearTileGOs.Select(tileGO2 => tileGO2.GetComponent<Tile>().GetUnitOnTile())
					let nearUnits = nearTileUnits.Where(unitGO => unitGO != null && unitGO.GetComponent<Unit>().GetSide() == Side.Ally)
					where nearUnits.Count() >= 2
					select tileGO;

			return enemiesNearEachOther.Count() > 0;
		}

		public static IEnumerator AIStart(BattleData battleData)
		{
			BattleManager battleManager = battleData.battleManager;
			if (Skill1Available(battleData))
			{
				yield return battleManager.StartCoroutine(AIStates.AIAttack(battleData, 1));
			}

			if (Skill2Available(battleData))
			{
				yield return battleManager.StartCoroutine(AIStates.AIAttack(battleData, 2));
			}

			else
			{
				battleData.currentState = CurrentState.RestAndRecover;
				yield return battleData.battleManager.StartCoroutine(RestAndRecover.Run(battleData));
			}
		}
	}
    public class AIStates
	{
		public static IEnumerator StartAI(BattleData battleData)
		{
			Unit currentUnit = battleData.selectedUnitObject.GetComponent<Unit>();
			BattleManager battleManager = battleData.battleManager;
			if (currentUnit.GetNameInCode() == "orchid")
			{
				yield return battleManager.StartCoroutine(OrchidBrain.AIStart(battleData));
			}
			else
			{
				yield return battleManager.StartCoroutine(AIMove(battleData));
			}
		}
		public static IEnumerator AIMove(BattleData battleData)
		{
			BattleManager battleManager = battleData.battleManager;

			yield return battleManager.StartCoroutine(AIDIe(battleData));

			Dictionary<Vector2, TileWithPath> movableTilesWithPath = PathFinder.CalculatePath(battleData.selectedUnitObject);
			List<GameObject> movableTiles = new List<GameObject>();
			foreach (KeyValuePair<Vector2, TileWithPath> movableTileWithPath in movableTilesWithPath)
			{
				movableTiles.Add(movableTileWithPath.Value.tile);
			}

			battleData.uiManager.UpdateApBarUI(battleData, battleData.unitManager.GetAllUnits());

		    Unit currentUnit = battleData.selectedUnitObject.GetComponent<Unit>();
		    Vector2 destPosition;

            if (currentUnit.GetNameInCode() == "monster")
		    {
                // stage 3 달팽이 몬스터 temp AI
		        destPosition = SpaghettiConLumache.CalculateDestination(movableTiles, battleData.unitManager.GetAllUnits(),
		            battleData.selectedUnitObject);
		    }
			if (currentUnit.GetNameInCode() == "orchid")
			{
				destPosition = battleData.selectedUnitObject.GetComponent<Unit>().GetPosition();
			}
		    else
		    {
                // var randomPosition = movableTiles[Random.Range(0, movableTiles.Count)].GetComponent<Tile>().GetTilePos();
                destPosition = UdongNoodle.FindNearestEnemy(movableTiles, battleData.unitManager.GetAllUnits(), battleData.selectedUnitObject);
            }

            // FIXME : 어딘가로 옮겨야 할 텐데...
            GameObject destTile = battleData.tileManager.GetTile(destPosition);
			List<GameObject> destPath = movableTilesWithPath[destPosition].path;
			Vector2 currentTilePos = currentUnit.GetPosition();
			Vector2 distanceVector = destPosition - currentTilePos;
			int distance = (int)Mathf.Abs(distanceVector.x) + (int)Mathf.Abs(distanceVector.y);
			int totalUseActivityPoint = movableTilesWithPath[destPosition].requireActivityPoint;

			// battleData.moveCount += distance;

			// battleData.tileManager.DepaintTiles(movableTiles, TileColor.Blue);
			battleData.currentState = CurrentState.CheckDestination;

			List<GameObject> destTileList = destPath;
			destTileList.Add(destTile);

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
			return AIAttack(battleData, 1);
		}

		public static IEnumerator AIAttack(BattleData battleData, int selectedSkillIndex)
		{
			BattleManager battleManager = battleData.battleManager;
			battleData.indexOfSeletedSkillByUser = selectedSkillIndex;
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

			Tile selectedTile = UdongNoodle.FindEnemyTile(selectedTiles, battleData.selectedUnitObject);

			if (selectedTile == null)
			{
				// 아무것도 할 게 없을 경우 휴식
				battleData.currentState = CurrentState.RestAndRecover;
				yield return battleData.battleManager.StartCoroutine(RestAndRecover.Run(battleData));
				yield break;
			}
			// activeRange[Random.Range(0, activeRange.Count)].GetComponent<Tile>();

			battleData.uiManager.SetSkillNamePanelUI(selectedSkill.GetName());


			BattleManager battleManager = battleData.battleManager;
			battleData.currentState = CurrentState.CheckApplyOrChain;

			List<GameObject> tilesInSkillRange = GetTilesInSkillRange(battleData, selectedTile, selectedUnit);

			yield return SkillAndChainStates.ApplyChain(battleData, tilesInSkillRange);
			FocusUnit(battleData.SelectedUnit);
			battleData.currentState = CurrentState.FocusToUnit;

			battleData.uiManager.ResetSkillNamePanelUI();
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

				Tile selectedTile = UdongNoodle.FindEnemyTile(activeRange, battleData.selectedUnitObject);

				if (selectedTile == null)
				{
                    // 아무것도 할 게 없을 경우 휴식
                    battleData.currentState = CurrentState.RestAndRecover;
                    yield return battleData.battleManager.StartCoroutine(RestAndRecover.Run(battleData));
                    yield break;
				}
				// activeRange[Random.Range(0, activeRange.Count)].GetComponent<Tile>();

				battleData.uiManager.SetSkillNamePanelUI(selectedSkill.GetName());
				battleData.selectedTilePosition = selectedTile.GetTilePos();

				// 타겟팅 스킬을 타겟이 없는 장소에 지정했을 경우 적용되지 않도록 예외처리 필요 - 대부분의 스킬은 논타겟팅. 추후 보강.

				BattleManager battleManager = battleData.battleManager;
				battleData.currentState = CurrentState.CheckApplyOrChain;

				List<GameObject> tilesInSkillRange = GetTilesInSkillRange(battleData, selectedTile, selectedUnit);

				yield return SkillAndChainStates.ApplyChain(battleData, tilesInSkillRange);
				FocusUnit(battleData.SelectedUnit);
				battleData.currentState = CurrentState.FocusToUnit;
				battleData.uiManager.ResetSkillNamePanelUI();
			}
		}

		private static void FocusUnit(Unit unit)
		{
			Camera.main.transform.position = new Vector3(unit.transform.position.x, unit.transform.position.y, -10);
		}

		private static List<GameObject> GetTilesInSkillRange(BattleData battleData, Tile targetTile, Unit selectedUnit = null)
		{
				Skill selectedSkill = battleData.SelectedSkill;
				List<GameObject> selectedTiles = battleData.tileManager.GetTilesInRange(selectedSkill.GetSecondRangeForm(),
																			targetTile.GetTilePos(),
																			selectedSkill.GetSecondMinReach(),
																			selectedSkill.GetSecondMaxReach(),
																			battleData.selectedUnitObject.GetComponent<Unit>().GetDirection());
				if (selectedSkill.GetSkillType() == SkillType.Auto)
				{
					if (selectedUnit != null)
					{
						selectedTiles.Remove(battleData.tileManager.GetTile(selectedUnit.GetPosition()));
					}
					else
					{
						selectedTiles.Remove(targetTile.gameObject);
					}
				}
				return selectedTiles;
		}
	}
}
