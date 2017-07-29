using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Enums;

namespace Battle.Turn
{
	public class AIUtil
	{
		public static Side GetOtherSide(Unit unit){
			Side mySide = unit.GetSide ();
			Side otherSide;
			if (mySide == Side.Ally)
				otherSide = Side.Enemy;
			else
				otherSide = Side.Ally;
			return otherSide;
		}
		public static Vector2 FindNearestEnemy(List<Tile> movableTiles, List<Unit> units, Unit mainUnit)
		{
			Side otherSide = GetOtherSide (mainUnit);

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

		public static Tile FindOtherSideUnitTile(List<Tile> activeTileRange, Unit mainUnit)
		{
			Side otherSide = GetOtherSide (mainUnit);

			var tilesHaveEnemy = from tile in activeTileRange
								 where tile.GetUnitOnTile() != null
								 let unit = tile.GetUnitOnTile()
								 where unit.GetSide() == otherSide
								 select tile;

			return tilesHaveEnemy.FirstOrDefault();
		}
	}

	/*
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
    */

	// ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~
	//above class SpaghettiConLumache & below class OrchidBrain : legacy code
	// ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~

	/*
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
	*/

	// ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~
	//above class OrchidBrain : legacy code
	// ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~

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
				Debug.Log (currentUnit.GetName () + " take rest because of being faint or deactivated");
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
			bool satisfyActiveCondition = false;
			// 전투 시작시 활성화
			if (currentUnitAIData.activeTriggers.Contains(1))
			{
				satisfyActiveCondition = true;
			}
			// 일정 페이즈부터 활성화
			else if (currentUnitAIData.activeTriggers.Contains(2))
			{
				if (battleData.currentPhase >= currentUnitAIData.activePhase) {
					Debug.Log (currentUnit.GetName () + " is activated because enough phase passed");
					satisfyActiveCondition = true;
				}
			}
			// 자신 주위 일정 영역에 접근하면 활성화
			else if (currentUnitAIData.activeTriggers.Contains(3))
			{
				// 자신을 기준으로 한 상대좌표
				List<List<Tile>> aroundTiles = currentUnitAIData.trigger3Area;
				List<Unit> aroundUnits = new List<Unit>();

				aroundTiles.ForEach(eachArea => {
					eachArea.ForEach(tile => {
						if (tile.IsUnitOnTile())
						{
							aroundUnits.Add(tile.GetUnitOnTile());
						}
					});
				});

				if (aroundUnits.Contains(currentUnit))
					aroundUnits.Remove(currentUnit);

				bool isThereAnotherSideUnit = aroundUnits.Any(unit => unit.GetSide() != currentUnit.GetSide());

				if (isThereAnotherSideUnit)
				{
					Debug.Log (currentUnit.GetName () + " is activated because its enemy came to nearby");
					satisfyActiveCondition = true;
				}
			}
			// 맵 상의 특정 영역에 접근하면 활성화
			else if (currentUnitAIData.activeTriggers.Contains(4))
			{
				// 절대좌표
				List<List<Tile>> aroundTiles = currentUnitAIData.trigger4Area;
				List<Unit> aroundUnits = new List<Unit>();

				aroundTiles.ForEach(eachArea => {
					eachArea.ForEach(tile => {
						if (tile.IsUnitOnTile())
						{
							aroundUnits.Add(tile.GetUnitOnTile());
						}
					});
				});

				if (aroundUnits.Contains(currentUnit))
					aroundUnits.Remove(currentUnit);

				bool isThereAnotherSideUnit = aroundUnits.Any(unit => unit.GetSide() != currentUnit.GetSide());

				if (isThereAnotherSideUnit)
				{
					Debug.Log (currentUnit.GetName () + " is activated because its enemy came to absolute position range");
					satisfyActiveCondition = true;
				}
			}
			// 자신을 대상으로 기술이 시전되면 활성화
			else if (currentUnitAIData.activeTriggers.Contains(5))
			{
				// 뭔가 기술의 영향을 받으면
				// SkillAndChainState.ApplySkill에서 체크
			}
			if(satisfyActiveCondition)
				currentUnitAIData.SetActive();
		}
	}

    public class AIStates_old
	{
		public static IEnumerator AIMove(BattleData battleData)
		{
			BattleManager battleManager = battleData.battleManager;
			Unit unit = battleData.selectedUnit;
			Tile currentTile = unit.GetTileUnderUnit ();

			yield return battleManager.StartCoroutine(AIDIe(battleData));

			//이동 전에 먼저 기술부터 정해야 한다... 기술 범위에 따라 어떻게 이동할지 아니면 이동 안 할지가 달라지므로
			//나중엔 여러 기술중에 선택해야겠지만 일단 지금은 AI 기술이 모두 하나뿐이니 그냥 첫번째걸로
			int selectedSkillIndex = 1;
			battleData.indexOfSelectedSkillByUser = selectedSkillIndex;
			ActiveSkill selectedSkill = battleData.SelectedSkill;

			SkillType skillTypeOfSelectedSkill = selectedSkill.GetSkillType ();

			Tile attackAbleTile;

			if (skillTypeOfSelectedSkill == SkillType.Auto || skillTypeOfSelectedSkill == SkillType.Self)
			{
				attackAbleTile = GetAttackableOtherSideUnitTileOfDirectionSkill (battleData, currentTile);
				if (attackAbleTile != null) {
					battleData.currentState = CurrentState.SelectSkillApplyDirection;
					yield return battleManager.StartCoroutine (SelectSkillApplyDirection (battleData, battleData.selectedUnit.GetDirection ()));
					yield break;
				}
			}
			else
			{
				attackAbleTile = GetAttackableOtherSideUnitTileOfPointSkill (battleData, currentTile);
				if (attackAbleTile != null) {
					battleData.currentState = CurrentState.SelectSkillApplyPoint;
					yield return battleManager.StartCoroutine (SelectSkillApplyPoint (battleData, battleData.selectedUnit.GetDirection ()));
					yield  break;
				}
			}

			Dictionary<Vector2, TileWithPath> movableTilesWithPath = PathFinder.CalculatePath(battleData.selectedUnit);
			List<Tile> movableTiles = new List<Tile>();
			foreach (KeyValuePair<Vector2, TileWithPath> movableTileWithPath in movableTilesWithPath)
			{
				movableTiles.Add(movableTileWithPath.Value.tile);
			}

			battleData.uiManager.UpdateApBarUI(battleData, battleData.unitManager.GetAllUnits());

		    Unit currentUnit = battleData.selectedUnit;
		    Vector2 destPosition;
            // 현재 가장 가까운 적을 향해 움직임
			destPosition = AIUtil.FindNearestEnemy(movableTiles, battleData.unitManager.GetAllUnits(), battleData.selectedUnit);

            Tile destTile = battleData.tileManager.GetTile(destPosition);
			TileWithPath pathToDestTile = movableTilesWithPath[destPosition];

			if (pathToDestTile.path.Count > 0) {
				Tile prevLastTile = pathToDestTile.path.Last ();
				Vector2 prevLastTilePosition = prevLastTile.GetTilePos ();
				int totalUseActivityPoint = movableTilesWithPath [destPosition].requireActivityPoint;

				Direction destDirection;
				// 이동했을때 볼 방향 설정
				Vector2 delta = destPosition - prevLastTilePosition;
				if (delta == new Vector2 (1, 0))
					destDirection = Direction.RightDown;
				else if (delta == new Vector2 (-1, 0))
					destDirection = Direction.LeftUp;
				else if (delta == new Vector2 (0, 1))
					destDirection = Direction.RightUp;
				else // delta == new Vector2 (0, -1)
				destDirection = Direction.LeftDown;

				battleData.currentState = CurrentState.CheckDestination;

				// 카메라를 옮기고
				Camera.main.transform.position = new Vector3 (destTile.transform.position.x, destTile.transform.position.y, -10);

				battleData.currentState = CurrentState.MoveToTile;
				yield return battleManager.StartCoroutine (MoveStates.MoveToTile (battleData, destTile, destDirection, totalUseActivityPoint));
			}

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
			ActiveSkill selectedSkill = battleData.SelectedSkill;

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
			ActiveSkill selectedSkill = battleData.SelectedSkill;

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
			Unit selectedUnit = battleData.selectedUnit;
			Tile selectedTile = GetAttackableOtherSideUnitTileOfDirectionSkill(battleData, selectedUnit.GetTileUnderUnit());

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
			//tilesInRealEffectRange는 투사체 스킬의 경우 경로상 유닛이 없으면 빈 List로 설정해야 한다. 일단 AI 유닛 스킬엔 없으니 생략
			List<Tile> tilesInRealEffectRange =  tilesInSkillRange;

			yield return SkillAndChainStates.ApplyChain(battleData, selectedTile, tilesInSkillRange, tilesInRealEffectRange, GetTilesInFirstRange(battleData));
			FocusUnit(battleData.selectedUnit);
			battleData.currentState = CurrentState.FocusToUnit;

			battleData.uiManager.ResetSkillNamePanelUI();
		}
		public static Tile GetAttackableOtherSideUnitTileOfDirectionSkill(BattleData battleData, Tile unitTile){
			List<Tile> selectedTiles = new List<Tile>();
			Unit selectedUnit = battleData.selectedUnit;
			ActiveSkill selectedSkill = battleData.SelectedSkill;

			selectedTiles = battleData.tileManager.GetTilesInRange(selectedSkill.GetSecondRangeForm(),
				unitTile.GetTilePos(),
				selectedSkill.GetSecondMinReach(),
				selectedSkill.GetSecondMaxReach(),
				selectedSkill.GetSecondWidth(),
				selectedUnit.GetDirection());

			Tile selectedTile = AIUtil.FindOtherSideUnitTile(selectedTiles, battleData.selectedUnit);

			return selectedTile;
		}

		public static IEnumerator SelectSkillApplyPoint(BattleData battleData, Direction originalDirection)
		{
			//Direction beforeDirection = originalDirection;
			Unit selectedUnit = battleData.selectedUnit;
			ActiveSkill selectedSkill = battleData.SelectedSkill;

			while (battleData.currentState == CurrentState.SelectSkillApplyPoint)
			{
				Tile selectedTile = GetAttackableOtherSideUnitTileOfPointSkill (battleData, selectedUnit.GetTileUnderUnit ());

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
				//tilesInRealEffectRange는 투사체 스킬의 경우 경로상 유닛이 없으면 빈 List로 설정해야 한다. 일단 AI 유닛 스킬엔 없으니 생략
				List<Tile> tilesInRealEffectRange = tilesInSkillRange;

				yield return SkillAndChainStates.ApplyChain(battleData, selectedTile, tilesInSkillRange, tilesInRealEffectRange, GetTilesInFirstRange(battleData));
				FocusUnit(battleData.selectedUnit);
				battleData.currentState = CurrentState.FocusToUnit;
				battleData.uiManager.ResetSkillNamePanelUI();
			}
		}
		public static Tile GetAttackableOtherSideUnitTileOfPointSkill(BattleData battleData, Tile unitTile){
			Unit selectedUnit = battleData.selectedUnit;
			List<Tile> activeRange = new List<Tile>();
			ActiveSkill selectedSkill = battleData.SelectedSkill;
			activeRange = 
				battleData.tileManager.GetTilesInRange(selectedSkill.GetFirstRangeForm(),
					unitTile.GetTilePos(),
					selectedSkill.GetFirstMinReach(),
					selectedSkill.GetFirstMaxReach(),
					selectedSkill.GetFirstWidth(),
					battleData.selectedUnit.GetDirection());

			Tile selectedTile = AIUtil.FindOtherSideUnitTile(activeRange, battleData.selectedUnit);

			return selectedTile;
		}

		private static void FocusUnit(Unit unit)
		{
			Camera.main.transform.position = new Vector3(unit.transform.position.x, unit.transform.position.y, -10);
		}

		private static List<Tile> GetTilesInSkillRange(BattleData battleData, Tile targetTile, Unit selectedUnit = null)
		{
				ActiveSkill selectedSkill = battleData.SelectedSkill;
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
