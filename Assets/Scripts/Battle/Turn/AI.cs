using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Enums;

namespace Battle.Turn
{
	public class UdongNoodle
	{
		public static Vector2 FindNearestEnemy(List<Tile> movableTiles, List<Unit> units, Unit mainUnit)
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

			var positions = from tile in movableTiles
					from unit in units
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

		public static Tile FindEnemyTile(List<Tile> activeTileRange, Unit mainUnit)
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

			var tilesHaveEnemy = from tile in activeTileRange
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

        public static Vector2 CalculateDestination(List<Tile> movableTiles, List<Unit> units, Unit mainUnit) {
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
                return FindNearestManastone(movableTiles, units, mainUnit);
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
                return FindNearestPlayer(movableTiles, units, mainUnit);
            }

            List<Tile> nearManastoneTiles = new List<Tile>();
            foreach (Tile movableTile in movableTiles) {

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

        public static Vector2 FindNearestManastone(List<Tile> movableTiles, List<Unit> units, Unit mainUnit) {


            var positions = from tile in movableTiles
                            from unit in units
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

        public static Vector2 FindNearestPlayer(List<Tile> movableTiles, List<Unit> units, Unit mainUnit) {


            var positions = from tile in movableTiles
                            from unit in units
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
			//Direction beforeDirection = battleData.selectedUnit.GetDirection();
			Unit selectedUnit = battleData.selectedUnit;

			battleData.indexOfSelectedSkillByUser = 1;
			Skill selectedSkill = battleData.SelectedSkill;

			List<Tile> selectedTiles = new List<Tile>();
			selectedTiles = battleData.tileManager.GetTilesInRange(selectedSkill.GetSecondRangeForm(),
														selectedUnit.GetPosition(),
														selectedSkill.GetSecondMinReach(),
														selectedSkill.GetSecondMaxReach(),
														selectedSkill.GetSecondWidth(),
														selectedUnit.GetDirection());

			var enemies = from tile in selectedTiles
					let unit = tile.GetUnitOnTile()
					where unit != null
					where unit.GetSide() == Side.Ally
					select unit;

			return enemies.Count() >= 2;
		}

		public static bool Skill2Available(BattleData battleData)
		{
			//Direction beforeDirection = battleData.selectedUnit.GetDirection();
			Unit selectedUnit = battleData.selectedUnit;

			battleData.indexOfSelectedSkillByUser = 2;
			Skill selectedSkill = battleData.SelectedSkill;

			List<Tile> selectedTiles = new List<Tile>();
			selectedTiles = battleData.tileManager.GetTilesInRange(selectedSkill.GetFirstRangeForm(),
														selectedUnit.GetPosition(),
														selectedSkill.GetFirstMinReach(),
														selectedSkill.GetFirstMaxReach(),
														selectedSkill.GetFirstWidth(),
														selectedUnit.GetDirection());

			TileManager tileManager = battleData.tileManager;
			var enemiesNearEachOther =
					from tile in selectedTiles
					let nearTiles = tileManager.GetTilesInRange(RangeForm.Diamond, tile.GetTilePos(), 0, 1, 0, Direction.Down)
					let nearTileUnits = nearTiles.Select(tile2 => tile2.GetUnitOnTile())
					let nearUnits = nearTileUnits.Where(unit => unit != null && unit.GetSide() == Side.Ally)
					where nearUnits.Count() >= 2
					select tile;

			return enemiesNearEachOther.Count() > 0;
		}

		public static bool Skill3Available(BattleData battleData)
		{
			//Direction beforeDirection = battleData.selectedUnit.GetDirection();
			Unit selectedUnit = battleData.selectedUnit;

			battleData.indexOfSelectedSkillByUser = 3;
			Skill selectedSkill = battleData.SelectedSkill;

			List<Tile> selectedTiles = new List<Tile>();
			selectedTiles = battleData.tileManager.GetTilesInRange(selectedSkill.GetFirstRangeForm(),
														selectedUnit.GetPosition(),
														selectedSkill.GetFirstMinReach(),
														selectedSkill.GetFirstMaxReach(),
														selectedSkill.GetFirstWidth(),
														selectedUnit.GetDirection());

			var enemies = from tile in selectedTiles
					let unit = tile.GetUnitOnTile()
					where unit != null
					where unit.GetSide() == Side.Ally
					select unit;

			return enemies.Count() > 0;
		}

		public static Direction? Skill4AvailableDirection(BattleData battleData)
		{
			//Direction beforeDirection = battleData.selectedUnit.GetDirection();
			Unit selectedUnit = battleData.selectedUnit;

			if (selectedUnit.GetCurrentActivityPoint() < 70)
			{
				return null;
			}

			battleData.indexOfSelectedSkillByUser = 4;
			Skill selectedSkill = battleData.SelectedSkill;

			List<Tile> selectedTiles = new List<Tile>();

			foreach (Direction direction in new List<Direction> { Direction.LeftUp, Direction.LeftDown, Direction.RightUp, Direction.RightDown})
			{
				selectedTiles = battleData.tileManager.GetTilesInRange(selectedSkill.GetSecondRangeForm(),
														selectedUnit.GetPosition(),
														selectedSkill.GetSecondMinReach(),
														selectedSkill.GetSecondMaxReach(),
														selectedSkill.GetSecondWidth(),
														direction);

				var enemies = from tile in selectedTiles
						let unit = tile.GetUnitOnTile()
						where unit != null
						where unit.GetSide() == Side.Ally
						select unit;

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

			battleData.indexOfSelectedSkillByUser = 4;
			Skill selectedSkill = battleData.SelectedSkill;

			var selectedTiles = battleData.tileManager.GetTilesInRange(selectedSkill.GetSecondRangeForm(),
													selectedUnit.GetPosition(),
													selectedSkill.GetSecondMinReach(),
													selectedSkill.GetSecondMaxReach(),
													selectedSkill.GetSecondWidth(),
													selectedUnit.GetDirection());

			var enemies = from tile in selectedTiles
					let unit = tile.GetUnitOnTile()
					where unit != null
					where unit.GetSide() == Side.Ally
					select unit;

			foreach (var enemy in enemies)
			{
				Vector2 dirVec = battleData.tileManager.ToVector2(selectedUnit.GetDirection());
				//BattleManager battleManager = battleData.battleManager;

				Tile targetTile = battleData.tileManager.GetTile(enemy.GetPosition() + dirVec * 3);
				enemy.GetKnockedBack(battleData, targetTile);

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
				yield return battleManager.StartCoroutine(AIStates_old.AIAttack(battleData, 1));
			}

			if (Skill2Available(battleData))
			{
				nothingTodo = false;
				yield return battleManager.StartCoroutine(AIStates_old.AIAttack(battleData, 2));
			}

			if (Skill3Available(battleData))
			{
				nothingTodo = false;
				yield return battleManager.StartCoroutine(AIStates_old.AIAttack(battleData, 3));
			}

			Direction? skill4AvailableDirection = Skill4AvailableDirection(battleData);
			if (skill4AvailableDirection.HasValue)
			{
				nothingTodo = false;
				battleData.uiManager.SetSkillNamePanelUI(battleData.SelectedSkill.GetName());
				Debug.LogError("Skill 4 available " + skill4AvailableDirection.Value);
				battleData.selectedUnit.SetDirection(skill4AvailableDirection.Value);
				yield return battleManager.StartCoroutine(AIStates_old.AIAttack(battleData, 4));
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
		public static Unit currentUnit;
		public static AIData currentUnitAIData;

		public static IEnumerator AIStart(BattleData battleData)
		{
			currentUnit = battleData.selectedUnit;
			currentUnitAIData = currentUnit.GetComponent<AIData>();
			BattleManager battleManager = battleData.battleManager;
			// if (currentUnit.isBoss)
			// 보스 전용 AI
			// else
			// 기절 & 활성화되었는지 체크
			if (!currentUnitAIData.IsActive())
			{
				CheckActiveTrigger(battleData);
			}
			
			if (currentUnit.HasStatusEffect(StatusEffectType.Faint) || !currentUnitAIData.IsActive())
			{
				yield return battleData.battleManager.StartCoroutine(RestAndRecover.Run(battleData));
				yield break;
			}
			else
				yield return battleManager.StartCoroutine(AIStates_old.AIMove(battleData));
		}

		public static IEnumerator AIMove(BattleData battleData)
		{
			yield return null;
		}

		public static void CheckActiveTrigger(BattleData battleData)
		{
			// 전투 시작시 활성화
			if (currentUnitAIData.activeTrigger.Contains(1))
			{
				currentUnitAIData.SetActive();
			}
			// 일정 페이즈부터 활성화
			else if (currentUnitAIData.activeTrigger.Contains(2))
			{
				if (battleData.currentPhase >= currentUnitAIData.activePhase)
					currentUnitAIData.SetActive();
			}
			// 자신 주위 일정 영역에 접근하면 활성화
			else if (currentUnitAIData.activeTrigger.Contains(3))
			{
				bool isThereAnotherSideUnit = false;
				
				// 자신을 기준으로 한 상대좌표
				List<Tile> aroundTiles = battleData.tileManager.GetTilesInRange(
												currentUnitAIData.rangeForm, 
												currentUnit.GetPosition() + currentUnitAIData.midPosition,
												currentUnitAIData.minReach, 
												currentUnitAIData.maxReach, 
												currentUnitAIData.width,
												currentUnit.GetDirection());
				List<Unit> aroundUnits = new List<Unit>();
				foreach (var tile in aroundTiles)
				{
					if (tile.IsUnitOnTile())
						aroundUnits.Add(tile.GetUnitOnTile());
				}
				if (aroundUnits.Contains(currentUnit))
					aroundUnits.Remove(currentUnit);

				isThereAnotherSideUnit = aroundUnits.Any(unit => unit.GetSide() != currentUnit.GetSide());

				if (isThereAnotherSideUnit)
					currentUnitAIData.SetActive();
			}
			// 맵 상의 특정 영역에 접근하면 활성화
			else if (currentUnitAIData.activeTrigger.Contains(4))
			{
				bool isThereAnotherSideUnit = false;
				
				// 절대좌표
				List<Tile> aroundTiles = battleData.tileManager.GetTilesInRange(
												currentUnitAIData.rangeForm, 
												currentUnitAIData.midPosition,
												currentUnitAIData.minReach, 
												currentUnitAIData.maxReach, 
												currentUnitAIData.width,
												currentUnit.GetDirection());
				List<Unit> aroundUnits = new List<Unit>();
				foreach (var tile in aroundTiles)
				{
					if (tile.IsUnitOnTile())
						aroundUnits.Add(tile.GetUnitOnTile());
				}
				if (aroundUnits.Contains(currentUnit))
					aroundUnits.Remove(currentUnit);

				isThereAnotherSideUnit = aroundUnits.Any(unit => unit.GetSide() != currentUnit.GetSide());

				if (isThereAnotherSideUnit)
					currentUnitAIData.SetActive();
			}
			// 자신을 대상으로 기술이 시전되면 활성화
			else if (currentUnitAIData.activeTrigger.Contains(5))
			{
				// 뭔가 기술의 영향을 받으면
				// SkillAndChainState.ApplySkill에서 체크
			}
		}
	}

    public class AIStates_old
	{
		public static IEnumerator AIStart(BattleData battleData)
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
			List<Tile> movableTiles = new List<Tile>();
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
            Tile destTile = battleData.tileManager.GetTile(destPosition);
			//List<Tile> destPath = movableTilesWithPath[destPosition].path;
			//Vector2 currentTilePos = currentUnit.GetPosition();
			//Vector2 distanceVector = destPosition - currentTilePos;
			//int distance = (int)Mathf.Abs(distanceVector.x) + (int)Mathf.Abs(distanceVector.y);
			int totalUseActivityPoint = movableTilesWithPath[destPosition].requireActivityPoint;

			// battleData.tileManager.DepaintTiles(movableTiles, TileColor.Blue);
			battleData.currentState = CurrentState.CheckDestination;

			//List<Tile> destTileList = destPath;
			//destTileList.Add(destTile);

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
		}

		public static IEnumerator AIAttack(BattleData battleData)
		{
			int selectedSkillIndex = 1;
			battleData.indexOfSelectedSkillByUser = selectedSkillIndex;
			Skill selectedSkill = battleData.SelectedSkill;

			int currentAP = battleData.selectedUnit.GetCurrentActivityPoint();
			int requireAP = battleData.SelectedSkill.GetRequireAP();

			bool enoughAP = currentAP >= requireAP;

			if (enoughAP) {
				return AIAttack(battleData, selectedSkillIndex);
			} else {
				return PassTurn();
			}
		}

		public static IEnumerator PassTurn()
		{
			yield return new WaitForSeconds(0.5f);
		}

		public static IEnumerator AIAttack(BattleData battleData, int selectedSkillIndex)
		{
			BattleManager battleManager = battleData.battleManager;
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
			//Direction beforeDirection = originalDirection;
			List<Tile> selectedTiles = new List<Tile>();
			Unit selectedUnit = battleData.selectedUnit;
			Skill selectedSkill = battleData.SelectedSkill;

			selectedTiles = battleData.tileManager.GetTilesInRange(selectedSkill.GetSecondRangeForm(),
														selectedUnit.GetPosition(),
														selectedSkill.GetSecondMinReach(),
														selectedSkill.GetSecondMaxReach(),
														selectedSkill.GetSecondWidth(),
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

			//BattleManager battleManager = battleData.battleManager;
			battleData.currentState = CurrentState.CheckApplyOrChain;

			List<Tile> tilesInSkillRange = GetTilesInSkillRange(battleData, selectedTile, selectedUnit);

			yield return SkillAndChainStates.ApplyChain(battleData, selectedTile, tilesInSkillRange, GetTilesInFirstRange(battleData));
			FocusUnit(battleData.selectedUnit);
			battleData.currentState = CurrentState.FocusToUnit;

			battleData.uiManager.ResetSkillNamePanelUI();
		}

		public static IEnumerator SelectSkillApplyPoint(BattleData battleData, Direction originalDirection)
		{
			//Direction beforeDirection = originalDirection;
			Unit selectedUnit = battleData.selectedUnit;

			while (battleData.currentState == CurrentState.SelectSkillApplyPoint)
			{
				Vector2 selectedUnitPos = battleData.selectedUnit.GetPosition();

				List<Tile> activeRange = new List<Tile>();
				Skill selectedSkill = battleData.SelectedSkill;
				activeRange = 
					battleData.tileManager.GetTilesInRange(selectedSkill.GetFirstRangeForm(),
														selectedUnitPos,
														selectedSkill.GetFirstMinReach(),
														selectedSkill.GetFirstMaxReach(),
														selectedSkill.GetFirstWidth(),
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

				//BattleManager battleManager = battleData.battleManager;
				battleData.currentState = CurrentState.CheckApplyOrChain;

				List<Tile> tilesInSkillRange = GetTilesInSkillRange(battleData, selectedTile, selectedUnit);

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

		private static List<Tile> GetTilesInSkillRange(BattleData battleData, Tile targetTile, Unit selectedUnit = null)
		{
				Skill selectedSkill = battleData.SelectedSkill;
				List<Tile> selectedTiles = battleData.tileManager.GetTilesInRange(selectedSkill.GetSecondRangeForm(),
																			targetTile.GetTilePos(),
																			selectedSkill.GetSecondMinReach(),
																			selectedSkill.GetSecondMaxReach(),
																			selectedSkill.GetSecondWidth(),
																			selectedUnit.GetDirection());
				if (selectedSkill.GetSkillType() == SkillType.Auto)
				{
					if (selectedUnit != null)
					{
						selectedTiles.Remove(battleData.tileManager.GetTile(selectedUnit.GetPosition()));
					}
					else
					{
						selectedTiles.Remove(targetTile);
					}
				}
				return selectedTiles;
		}

		private static List<Tile> GetTilesInFirstRange(BattleData battleData)
		{
			var firstRange = battleData.tileManager.GetTilesInRange(battleData.SelectedSkill.GetFirstRangeForm(),
																battleData.selectedUnit.GetPosition(),
																battleData.SelectedSkill.GetFirstMinReach(),
																battleData.SelectedSkill.GetFirstMaxReach(),
																battleData.SelectedSkill.GetFirstWidth(),
																battleData.selectedUnit.GetDirection());

			return firstRange;
		}

	}
}
