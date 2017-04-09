using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Enums;

namespace Battle.Turn
{
	public class UdongNoodle
	{
		public static Vector2 FindNearestEnemy(List<GameObject> movableTiles, List<Unit> units, Unit mainUnit)
		{
			Side otherSide;

			if (mainUnit.GetSide() == Side.Ally)
			{
				otherSide = Side.Enemy;
			}
			else
			{
				otherSide = Side.Ally;
			}

			var positions = from tileGO in movableTiles
					from unit in units
					let tile = tileGO.GetComponent<Tile>()
					where unit.GetSide() == otherSide
					let distance = Vector2.Distance(tile.GetTilePos(), unit.GetPosition())
					orderby distance
					select tile.GetTilePos();

			List<Vector2> availablePositions = positions.ToList();
			if (availablePositions.Count > 0)
			{
				return availablePositions[0];
			}
			return mainUnit.GetPosition();
		}

		public static Tile FindEnemyTile(List<GameObject> activeTileRange, Unit mainUnit)
		{
			Side otherSide;

			if (mainUnit.GetSide() == Side.Ally)
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
								 let unit = tile.GetUnitOnTile()
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

        public static Vector2 CalculateDestination(List<GameObject> movableTileObjects, List<Unit> units, Unit mainUnit) {
            // Destination calculation algorithm
            //
            // If it's far from (>3) manastone, go to the nearest manastone
            // Else if it's close to player (<=4), go to the nearest player
            // Else pick a random tile from moveable tiles that are close (<=2) to manastone
            
            float distanceFromNearestManastone = float.PositiveInfinity;
            foreach (Unit unit in units)
            {
                if (unit.GetNameInCode() != "manastone")
                {
                    continue;
                }
                float distance = Vector2.Distance(mainUnit.GetPosition(), unit.GetPosition());
                if (distanceFromNearestManastone > distance)
                {
                    distanceFromNearestManastone = distance;
                }
            }


            if (distanceFromNearestManastone > 3)
            {
                return FindNearestManastone(movableTileObjects, units, mainUnit);
            }


            float distanceFromNearestPlayer = float.PositiveInfinity;
            foreach (Unit unit in units) {
                if (unit.GetSide() != Side.Ally) {
                    continue;
                }
                float distance = Vector2.Distance(mainUnit.GetPosition(), unit.GetPosition());
                if (distanceFromNearestPlayer > distance) {
                    distanceFromNearestPlayer = distance;
                }
            }


            if (distanceFromNearestPlayer <= 4)
            {
                return FindNearestPlayer(movableTileObjects, units, mainUnit);
            }

            List<Tile> nearManastoneTiles = new List<Tile>();
            foreach (GameObject movableTileObject in movableTileObjects) {
                Tile movableTile = movableTileObject.GetComponent<Tile>();

                foreach (Unit unit in units) {
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

        public static Vector2 FindNearestManastone(List<GameObject> movableTileObjects, List<Unit> units, Unit mainUnit) {


            var positions = from tileGO in movableTileObjects
                            from unit in units
                            let tile = tileGO.GetComponent<Tile>()
                            where unit.GetNameInCode() == "manastone"
                            let distance = Vector2.Distance(tile.GetTilePos(), unit.GetPosition())
                            orderby distance
                            select tile.GetTilePos();

            List<Vector2> availablePositions = positions.ToList();
            if (availablePositions.Count > 0) {
                return availablePositions[0];
            }
            return mainUnit.GetPosition();
        }

        public static Vector2 FindNearestPlayer(List<GameObject> movableTileObjects, List<Unit> units, Unit mainUnit) {


            var positions = from tileGO in movableTileObjects
                            from unit in units
                            let tile = tileGO.GetComponent<Tile>()
                            where unit.GetSide() == Side.Ally
                            let distance = Vector2.Distance(tile.GetTilePos(), unit.GetPosition())
                            orderby distance
                            select tile.GetTilePos();

            List<Vector2> availablePositions = positions.ToList();
            if (availablePositions.Count > 0) {
                return availablePositions[0];
            }
            return mainUnit.GetPosition();
        }
    }

	public class OrchidBrain
	{
		public static bool Skill1Available(BattleData battleData)
		{
			Direction beforeDirection = battleData.selectedUnit.GetDirection();
			Unit selectedUnit = battleData.selectedUnit;

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

			return enemies.Count() >= 2;
		}

		public static bool Skill2Available(BattleData battleData)
		{
			Direction beforeDirection = battleData.selectedUnit.GetDirection();
			Unit selectedUnit = battleData.selectedUnit;

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
					let nearTileGOs = tileManager.GetTilesInRange(RangeForm.Diamond, tile.GetTilePos(), 0, 1, Direction.Down)
					let nearTileUnits = nearTileGOs.Select(tileGO2 => tileGO2.GetComponent<Tile>().GetUnitOnTile())
					let nearUnits = nearTileUnits.Where(unitGO => unitGO != null && unitGO.GetComponent<Unit>().GetSide() == Side.Ally)
					where nearUnits.Count() >= 2
					select tileGO;

			return enemiesNearEachOther.Count() > 0;
		}

		public static bool Skill3Available(BattleData battleData)
		{
			Direction beforeDirection = battleData.selectedUnit.GetDirection();
			Unit selectedUnit = battleData.selectedUnit;

			battleData.indexOfSeletedSkillByUser = 3;
			Skill selectedSkill = battleData.SelectedSkill;

			List<GameObject> selectedTiles = new List<GameObject>();
			selectedTiles = battleData.tileManager.GetTilesInRange(selectedSkill.GetFirstRangeForm(),
														selectedUnit.GetPosition(),
														selectedSkill.GetFirstMinReach(),
														selectedSkill.GetFirstMaxReach(),
														selectedUnit.GetDirection());

			var enemies = from tileGO in selectedTiles
					let tile = tileGO.GetComponent<Tile>()
					let unitGO = tile.GetUnitOnTile()
					where unitGO != null
					where unitGO.GetComponent<Unit>().GetSide() == Side.Ally
					select unitGO;

			return enemies.Count() > 0;
		}

		public static Direction? Skill4AvailableDirection(BattleData battleData)
		{
			Direction beforeDirection = battleData.selectedUnit.GetDirection();
			Unit selectedUnit = battleData.selectedUnit;

			if (selectedUnit.activityPoint < 70)
			{
				return null;
			}

			battleData.indexOfSeletedSkillByUser = 4;
			Skill selectedSkill = battleData.SelectedSkill;

			List<GameObject> selectedTiles = new List<GameObject>();

			foreach (Direction direction in new List<Direction> { Direction.LeftUp, Direction.LeftDown, Direction.RightUp, Direction.RightDown})
			{
				selectedTiles = battleData.tileManager.GetTilesInRange(selectedSkill.GetSecondRangeForm(),
														selectedUnit.GetPosition(),
														selectedSkill.GetSecondMinReach(),
														selectedSkill.GetSecondMaxReach(),
														direction);

				var enemies = from tileGO in selectedTiles
						let tile = tileGO.GetComponent<Tile>()
						let unitGO = tile.GetUnitOnTile()
						where unitGO != null
						where unitGO.GetComponent<Unit>().GetSide() == Side.Ally
						select unitGO;

				if (enemies.Count() > 0)
				{
					return direction;
				}
			}
			return null;
		}

		public static IEnumerator Skill4Knockback(BattleData battleData)
		{
			Unit selectedUnit = battleData.selectedUnit;

			battleData.indexOfSeletedSkillByUser = 4;
			Skill selectedSkill = battleData.SelectedSkill;

			var selectedTiles = battleData.tileManager.GetTilesInRange(selectedSkill.GetSecondRangeForm(),
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

			foreach (var enemy in enemies)
			{
				Vector2 dirVec = battleData.tileManager.ToVector2(selectedUnit.GetDirection());
				BattleManager battleManager = battleData.battleManager;

				GameObject targetTile = battleData.tileManager.GetTile(enemy.GetComponent<Unit>().GetPosition() + dirVec * 3);
				enemy.GetComponent<Unit>().GetKnockedBack(battleData, targetTile);

				yield return new WaitForSeconds(0.5f);
			}
		}

		public static IEnumerator AIStart(BattleData battleData)
		{
			BattleManager battleManager = battleData.battleManager;
			bool nothingTodo = true;

			if (Skill1Available(battleData))
			{
				nothingTodo = false;
				yield return battleManager.StartCoroutine(AIStates.AIAttack(battleData, 1));
			}

			if (Skill2Available(battleData))
			{
				nothingTodo = false;
				yield return battleManager.StartCoroutine(AIStates.AIAttack(battleData, 2));
			}

			if (Skill3Available(battleData))
			{
				nothingTodo = false;
				yield return battleManager.StartCoroutine(AIStates.AIAttack(battleData, 3));
			}

			Direction? skill4AvailableDirection = Skill4AvailableDirection(battleData);
			if (skill4AvailableDirection.HasValue)
			{
				nothingTodo = false;
				battleData.uiManager.SetSkillNamePanelUI(battleData.SelectedSkill.GetName());
				Debug.LogError("Skill 4 available " + skill4AvailableDirection.Value);
				battleData.selectedUnit.SetDirection(skill4AvailableDirection.Value);
				yield return battleManager.StartCoroutine(AIStates.AIAttack(battleData, 4));
				yield return battleManager.StartCoroutine(Skill4Knockback(battleData));
				battleData.uiManager.ResetSkillNamePanelUI();
			}

			if (nothingTodo)
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
			Unit currentUnit = battleData.selectedUnit;
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

			Dictionary<Vector2, TileWithPath> movableTilesWithPath = PathFinder.CalculatePath(battleData.selectedUnit);
			List<GameObject> movableTiles = new List<GameObject>();
			foreach (KeyValuePair<Vector2, TileWithPath> movableTileWithPath in movableTilesWithPath)
			{
				movableTiles.Add(movableTileWithPath.Value.tile);
			}

			battleData.uiManager.UpdateApBarUI(battleData, battleData.unitManager.GetAllUnits());

		    Unit currentUnit = battleData.selectedUnit;
		    Vector2 destPosition;

            if (currentUnit.GetNameInCode() == "monster")
		    {
                // stage 3 달팽이 몬스터 temp AI
		        destPosition = SpaghettiConLumache.CalculateDestination(movableTiles, battleData.unitManager.GetAllUnits(),
		            battleData.selectedUnit);
		    }
			if (currentUnit.GetNameInCode() == "orchid")
			{
				destPosition = battleData.selectedUnit.GetPosition();
			}
		    else
		    {
                // var randomPosition = movableTiles[Random.Range(0, movableTiles.Count)].GetComponent<Tile>().GetTilePos();
                destPosition = UdongNoodle.FindNearestEnemy(movableTiles, battleData.unitManager.GetAllUnits(), battleData.selectedUnit);
            }

            // FIXME : 어딘가로 옮겨야 할 텐데...
            GameObject destTile = battleData.tileManager.GetTile(destPosition);
			List<GameObject> destPath = movableTilesWithPath[destPosition].path;
			Vector2 currentTilePos = currentUnit.GetPosition();
			Vector2 distanceVector = destPosition - currentTilePos;
			int distance = (int)Mathf.Abs(distanceVector.x) + (int)Mathf.Abs(distanceVector.y);
			int totalUseActivityPoint = movableTilesWithPath[destPosition].requireActivityPoint;

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

			if (battleData.retreatUnits.Contains(battleData.selectedUnit))
			{
				yield return battleManager.StartCoroutine(BattleManager.FadeOutEffect(battleData.selectedUnit, 1));
				battleData.unitManager.DeleteRetreatUnit(battleData.selectedUnit);
				Debug.Log("SelectedUnit retreats");
				GameObject.Destroy(battleData.selectedUnit.gameObject);
				yield break;
			}

			if (battleData.deadUnits.Contains(battleData.selectedUnit))
			{
				battleData.selectedUnit.gameObject.GetComponent<SpriteRenderer>().color = Color.red;
				yield return battleManager.StartCoroutine(BattleManager.FadeOutEffect(battleData.selectedUnit, 1));
				battleData.unitManager.DeleteDeadUnit(battleData.selectedUnit);
				Debug.Log("SelectedUnit is dead");
				GameObject.Destroy(battleData.selectedUnit.gameObject);
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
				yield return battleManager.StartCoroutine(SelectSkillApplyDirection(battleData, battleData.selectedUnit.GetDirection()));
			}
			else
			{
				battleData.currentState = CurrentState.SelectSkillApplyPoint;
				yield return battleManager.StartCoroutine(SelectSkillApplyPoint(battleData, battleData.selectedUnit.GetDirection()));
			}

			battleData.previewAPAction = null;
			battleData.uiManager.UpdateApBarUI(battleData, battleData.unitManager.GetAllUnits());
		}

		public static IEnumerator SelectSkillApplyDirection(BattleData battleData, Direction originalDirection)
		{
			Direction beforeDirection = originalDirection;
			List<GameObject> selectedTiles = new List<GameObject>();
			Unit selectedUnit = battleData.selectedUnit;
			Skill selectedSkill = battleData.SelectedSkill;

			selectedTiles = battleData.tileManager.GetTilesInRange(selectedSkill.GetSecondRangeForm(),
														selectedUnit.GetPosition(),
														selectedSkill.GetSecondMinReach(),
														selectedSkill.GetSecondMaxReach(),
														selectedUnit.GetDirection());

			Tile selectedTile = UdongNoodle.FindEnemyTile(selectedTiles, battleData.selectedUnit);

			if (selectedTile == null)
			{
				Debug.LogError("Cannot find unit for attack. " );
				// 아무것도 할 게 없을 경우 휴식
				battleData.currentState = CurrentState.RestAndRecover;
				yield return battleData.battleManager.StartCoroutine(RestAndRecover.Run(battleData));
				yield break;
			}

			BattleManager battleManager = battleData.battleManager;
			battleData.currentState = CurrentState.CheckApplyOrChain;

			List<GameObject> tilesInSkillRange = GetTilesInSkillRange(battleData, selectedTile, selectedUnit);

			yield return SkillAndChainStates.ApplyChain(battleData, selectedTile, tilesInSkillRange, GetTilesInFirstRange(battleData));
			FocusUnit(battleData.selectedUnit);
			battleData.currentState = CurrentState.FocusToUnit;

			battleData.uiManager.ResetSkillNamePanelUI();
		}

		public static IEnumerator SelectSkillApplyPoint(BattleData battleData, Direction originalDirection)
		{
			Direction beforeDirection = originalDirection;
			Unit selectedUnit = battleData.selectedUnit;

			while (battleData.currentState == CurrentState.SelectSkillApplyPoint)
			{
				Vector2 selectedUnitPos = battleData.selectedUnit.GetPosition();

				List<GameObject> activeRange = new List<GameObject>();
				Skill selectedSkill = battleData.SelectedSkill;
				activeRange = 
					battleData.tileManager.GetTilesInRange(selectedSkill.GetFirstRangeForm(),
														selectedUnitPos,
														selectedSkill.GetFirstMinReach(),
														selectedSkill.GetFirstMaxReach(),
														battleData.selectedUnit.GetDirection());

				Tile selectedTile = UdongNoodle.FindEnemyTile(activeRange, battleData.selectedUnit);

				if (selectedTile == null)
				{
					Debug.LogError("Cannot find unit for attack. " );
                    // 아무것도 할 게 없을 경우 휴식
                    battleData.currentState = CurrentState.RestAndRecover;
                    yield return battleData.battleManager.StartCoroutine(RestAndRecover.Run(battleData));
                    yield break;
				}

				battleData.uiManager.SetSkillNamePanelUI(selectedSkill.GetName());
				battleData.move.selectedTilePosition = selectedTile.GetTilePos();

				// 타겟팅 스킬을 타겟이 없는 장소에 지정했을 경우 적용되지 않도록 예외처리 필요 - 대부분의 스킬은 논타겟팅. 추후 보강.

				BattleManager battleManager = battleData.battleManager;
				battleData.currentState = CurrentState.CheckApplyOrChain;

				List<GameObject> tilesInSkillRange = GetTilesInSkillRange(battleData, selectedTile, selectedUnit);

				yield return SkillAndChainStates.ApplyChain(battleData, selectedTile, tilesInSkillRange, GetTilesInFirstRange(battleData));
				FocusUnit(battleData.selectedUnit);
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
																			selectedUnit.GetDirection());
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

		private static List<GameObject> GetTilesInFirstRange(BattleData battleData)
		{
			var firstRange = battleData.tileManager.GetTilesInRange(battleData.SelectedSkill.GetFirstRangeForm(),
																battleData.selectedUnit.GetPosition(),
																battleData.SelectedSkill.GetFirstMinReach(),
																battleData.SelectedSkill.GetFirstMaxReach(),
																battleData.selectedUnit.GetDirection());

			return firstRange;
		}

	}
}
