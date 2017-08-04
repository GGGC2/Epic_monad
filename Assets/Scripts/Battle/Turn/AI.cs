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
		public static Vector2 FindNearestEnemy(List<Tile> movableTiles, List<Unit> units, Unit mainUnit){
			Side otherSide = GetOtherSide (mainUnit);

			var positions = from tile in movableTiles
					from unit in units
					where unit.GetSide() == otherSide
					let distance = Vector2.Distance(tile.GetTilePos(), unit.GetPosition())
					orderby distance
					select tile.GetTilePos();

			List<Vector2> availablePositions = positions.ToList();
			if (availablePositions.Count > 0)
				return availablePositions[0];

			return mainUnit.GetPosition();
		}
		public static Tile FindNearestEnemyAttackableTile(ActiveSkill skill, Dictionary<Vector2, TileWithPath> movableTilesWithPath, BattleData battleData)
		{
			Unit selectedUnit = battleData.selectedUnit;
			Side otherSide = GetOtherSide (selectedUnit);
			SkillType skillTypeOfSelectedSkill = skill.GetSkillType ();

			Dictionary<Vector2, TileWithPath> enemyAttackableTilesWithPath = new Dictionary<Vector2, TileWithPath> ();

			foreach (var pair in movableTilesWithPath) {
				Tile tile = pair.Value.tile;
				Tile attackAbleTile = null;
				if (skillTypeOfSelectedSkill != SkillType.Point)
					attackAbleTile = AIStates_old.GetAttackableOtherSideUnitTileOfDirectionSkill (battleData, tile);
				else
					attackAbleTile = AIStates_old.GetAttackableOtherSideUnitTileOfPointSkill (battleData, tile);
				
				if (attackAbleTile != null)
					enemyAttackableTilesWithPath [pair.Key] = pair.Value;
			}

			Tile nearestTile = GetMinRequireAPTile (enemyAttackableTilesWithPath);
			return nearestTile;
		}
		private static Tile GetMinRequireAPTile(Dictionary<Vector2, TileWithPath> movableTilesWithPath){
			if (movableTilesWithPath.Count > 0)
			{
				Tile nearestTile=null;
				int minRequireAP = 99999;
				foreach (var pair in movableTilesWithPath) {
					int requireAP = pair.Value.requireActivityPoint;
					if (requireAP < minRequireAP) {
						minRequireAP = requireAP;
						nearestTile = pair.Value.tile;
					}
				}
				return nearestTile;
			}
			return null;
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

	public class AIStates
	{
		public static Unit currentUnit;
		public static AIData currentUnitAIData;

		public static IEnumerator AIStart(BattleData battleData)
		{
			currentUnit = battleData.selectedUnit;

			battleData.uiManager.SetSelectedUnitViewerUI(currentUnit);
			currentUnit.SetActive();

			currentUnitAIData = currentUnit.GetComponent<AIData>();
			BattleManager battleManager = battleData.battleManager;
			// if (currentUnit.isBoss)
			// 보스 전용 AI
			// else
			// 기절 & 활성화되었는지 체크
			if (!currentUnitAIData.IsActive())
				CheckActiveTrigger(battleData);
			
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

			yield return battleManager.StartCoroutine(AIDie(battleData));

			//이동 전에 먼저 기술부터 정해야 한다... 기술 범위에 따라 어떻게 이동할지 아니면 이동 안 할지가 달라지므로
			//나중엔 여러 기술중에 선택해야겠지만 일단 지금은 AI 기술이 모두 하나뿐이니 그냥 첫번째걸로
			int selectedSkillIndex = 1;
			battleData.indexOfSelectedSkillByUser = selectedSkillIndex;
			ActiveSkill selectedSkill = battleData.SelectedSkill;

			SkillType skillTypeOfSelectedSkill = selectedSkill.GetSkillType ();

			Tile attackAbleTile;

			if (skillTypeOfSelectedSkill != SkillType.Point)
				attackAbleTile = GetAttackableOtherSideUnitTileOfDirectionSkill (battleData, currentTile);
			else
				attackAbleTile = GetAttackableOtherSideUnitTileOfPointSkill (battleData, currentTile);

			if (attackAbleTile != null) {
				yield return AIAct(battleData);
				yield break;
			}

			Dictionary<Vector2, TileWithPath> movableTilesWithPath = PathFinder.CalculatePath(battleData.selectedUnit);
			List<Tile> movableTiles = new List<Tile>();
			foreach (KeyValuePair<Vector2, TileWithPath> movableTileWithPath in movableTilesWithPath)
			{
				movableTiles.Add(movableTileWithPath.Value.tile);
			}

			battleData.uiManager.UpdateApBarUI(battleData, battleData.unitManager.GetAllUnits());

		    Unit currentUnit = battleData.selectedUnit;

			Tile destTile=AIUtil.FindNearestEnemyAttackableTile (selectedSkill, movableTilesWithPath, battleData);
			Vector2 destPosition;
			if (destTile == null) {
				destPosition = AIUtil.FindNearestEnemy (movableTiles, battleData.unitManager.GetAllUnits (), battleData.selectedUnit);
				destTile = battleData.tileManager.GetTile(destPosition);
			}
			else{
				destPosition = destTile.GetTilePos();
			}
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

			yield return AIAct(battleData);
		}

		public static IEnumerator AIDie(BattleData battleData)
		{
			BattleManager battleManager = battleData.battleManager;
			battleData.retreatUnits = battleData.unitManager.GetRetreatUnits();
			battleData.deadUnits = battleData.unitManager.GetDeadUnits();

			yield return battleManager.StartCoroutine(BattleManager.DestroyRetreatUnits(battleData));
			yield return battleManager.StartCoroutine(BattleManager.DestroyDeadUnits(battleData));
		}

		public static IEnumerator AIAct(BattleData battleData){
			while (true) {
				//행동하기 전마다 체크해야 할 사항들(중요!)
				yield return battleData.battleManager.StartCoroutine(BattleManager.UpdateRetreatAndDeadUnits(battleData, battleData.battleManager));
				yield return BattleManager.AtActionEnd(battleData);

				int selectedSkillIndex = 1;
				battleData.indexOfSelectedSkillByUser = selectedSkillIndex;
				ActiveSkill selectedSkill = battleData.SelectedSkill;

				int currentAP = battleData.selectedUnit.GetCurrentActivityPoint ();
				int requireAP = battleData.SelectedSkill.GetRequireAP ();
				bool enoughAP = currentAP >= requireAP;

				if (enoughAP) {
					yield return AISkill (battleData, selectedSkillIndex);
				}
				else {
					yield return PassTurn ();
					yield break;
				}
			}
		}

		public static IEnumerator PassTurn()
		{
			yield return new WaitForSeconds(0.5f);
		}

		public static IEnumerator AISkill(BattleData battleData, int selectedSkillIndex)
		{
			BattleManager battleManager = battleData.battleManager;
			ActiveSkill selectedSkill = battleData.SelectedSkill;

			SkillType skillTypeOfSelectedSkill = selectedSkill.GetSkillType();
			if (skillTypeOfSelectedSkill != SkillType.Point)
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
			Unit selectedUnit = battleData.selectedUnit;
			Direction beforeDirection = selectedUnit.GetDirection ();
			Tile selectedTile = GetAttackableOtherSideUnitTileOfDirectionSkill(battleData, selectedUnit.GetTileUnderUnit());

			if (selectedTile == null)
			{
				Debug.Log(selectedUnit.GetNameInCode () + " cannot find unit for direction attack. " );
				// 아무것도 할 게 없을 경우 휴식
				battleData.currentState = CurrentState.RestAndRecover;
				yield return battleData.battleManager.StartCoroutine(RestAndRecover.Run(battleData));
				yield break;
			}

			Direction afterDirection = Utility.GetDirectionToTarget(selectedUnit, selectedTile.GetTilePos());
			selectedUnit.SetDirection (afterDirection);

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

			Tile castingTile = unitTile;

			//투사체 스킬이면 직선경로상에서 유닛이 가로막은 지점을 castingTile로 함. 범위 끝까지 가로막은 유닛이 없으면 범위 맨 끝 타일이 castingTile=null
			if (selectedSkill.GetSkillType() == SkillType.Route) {
				//FIXME : 리스트로 만들어야 되는데.... 전체적으로 혼파망이라서 일단 이렇게 놔둠
				castingTile = GetRouteSkillCastingTile (battleData, selectedUnit, selectedSkill, Direction.LeftUp);
				if(castingTile==null)
					castingTile = GetRouteSkillCastingTile (battleData, selectedUnit, selectedSkill, Direction.LeftDown);
				if(castingTile==null)
					castingTile = GetRouteSkillCastingTile (battleData, selectedUnit, selectedSkill, Direction.RightUp);
				if (castingTile == null)
					castingTile = GetRouteSkillCastingTile (battleData, selectedUnit, selectedSkill, Direction.RightDown);
			}

			if (castingTile != null) {
				selectedTiles = battleData.tileManager.GetTilesInRange (selectedSkill.GetSecondRangeForm (),
					castingTile.GetTilePos (),
					selectedSkill.GetSecondMinReach (),
					selectedSkill.GetSecondMaxReach (),
					selectedSkill.GetSecondWidth (),
					selectedUnit.GetDirection ());
			}
			else {
				selectedTiles = new List<Tile> ();
			}

			Tile selectedTile = AIUtil.FindOtherSideUnitTile(selectedTiles, battleData.selectedUnit);

			return selectedTile;
		}
		private static Tile GetRouteSkillCastingTile(BattleData battleData, Unit unit, ActiveSkill routeSkill, Direction direction){				
			List<Tile> firstRange = battleData.tileManager.GetTilesInRange(routeSkill.GetFirstRangeForm(),
				unit.GetPosition(),
				routeSkill.GetFirstMinReach(),
				routeSkill.GetFirstMaxReach(),
				routeSkill.GetFirstWidth(),
				direction);
			return SkillAndChainStates.GetRouteEnd(firstRange);
		}

		//  위 : 지정형 빼고 나머지 스킬
		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		//  아래 : 지정형 (Point) 스킬

		public static IEnumerator SelectSkillApplyPoint(BattleData battleData, Direction originalDirection){

			Unit selectedUnit = battleData.selectedUnit;
			Direction beforeDirection = selectedUnit.GetDirection ();
			Tile selectedTile = GetAttackableOtherSideUnitTileOfPointSkill(battleData, selectedUnit.GetTileUnderUnit());

			if (selectedTile == null)
			{
				Debug.Log(selectedUnit.GetNameInCode () + " cannot find unit for point attack. " );
				// 아무것도 할 게 없을 경우 휴식
				battleData.currentState = CurrentState.RestAndRecover;
				yield return battleData.battleManager.StartCoroutine(RestAndRecover.Run(battleData));
				yield break;
			}

			Direction afterDirection = Utility.GetDirectionToTarget(selectedUnit, selectedTile.GetTilePos());
			selectedUnit.SetDirection (afterDirection);

			battleData.currentState = CurrentState.CheckApplyOrChain;

			List<Tile> tilesInSkillRange = GetTilesInSkillRange(battleData, selectedTile, selectedUnit);
			//tilesInRealEffectRange는 투사체 스킬의 경우 경로상 유닛이 없으면 빈 List로 설정해야 한다. 일단 AI 유닛 스킬엔 없으니 생략
			List<Tile> tilesInRealEffectRange =  tilesInSkillRange;

			yield return SkillAndChainStates.ApplyChain(battleData, selectedTile, tilesInSkillRange, tilesInRealEffectRange, GetTilesInFirstRange(battleData));
			FocusUnit(battleData.selectedUnit);
			battleData.currentState = CurrentState.FocusToUnit;

			battleData.uiManager.ResetSkillNamePanelUI();

			/*
			//Direction beforeDirection = originalDirection;
			Unit selectedUnit = battleData.selectedUnit;
			ActiveSkill selectedSkill = battleData.SelectedSkill;

			while (battleData.currentState == CurrentState.SelectSkillApplyPoint)
			{
				Tile selectedTile = GetAttackableOtherSideUnitTileOfPointSkill (battleData, selectedUnit.GetTileUnderUnit ());

				if (selectedTile == null){
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
			*/
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
