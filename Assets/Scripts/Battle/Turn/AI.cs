﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enums;

namespace Battle.Turn{
	public class AIUtil{
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
		public static Tile FindNearestEnemyAttackableTile(ActiveSkill skill, Dictionary<Vector2, TileWithPath> movableTilesWithPath, BattleData battleData){
			Unit selectedUnit = battleData.selectedUnit;
			Side otherSide = GetOtherSide (selectedUnit);
			SkillType skillTypeOfSelectedSkill = skill.GetSkillType ();

			Dictionary<Vector2, TileWithPath> enemyAttackableTilesWithPath = new Dictionary<Vector2, TileWithPath> ();

			foreach (var pair in movableTilesWithPath) {
				Tile tile = pair.Value.tile;
				Tile attackAbleTile = null;
				if (skillTypeOfSelectedSkill != SkillType.Point)
					attackAbleTile = AIStates_old.GetAttackableOtherSideUnitTileOfDirectionSkill (tile);
				else
					attackAbleTile = AIStates_old.GetAttackableOtherSideUnitTileOfPointSkill (tile);
				
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

		public static IEnumerator AIStart(BattleData battleData){
			currentUnit = battleData.selectedUnit;

			battleData.battleManager.StartUnitTurn (currentUnit);

			currentUnitAIData = currentUnit.GetComponent<AIData>();
			BattleManager battleManager = battleData.battleManager;

			if (!currentUnitAIData.IsActive())
				CheckActiveTrigger(battleData);
			
			if (currentUnit.HasStatusEffect(StatusEffectType.Faint) || !currentUnitAIData.IsActive())
			{
				Debug.Log (currentUnit.GetName () + " take rest(휴식) because of being deactivated(비활성화) or faint(기절)");
				yield return battleData.battleManager.StartCoroutine(RestAndRecover.Run(battleData));
				yield break;
			}
			else
				yield return battleManager.StartCoroutine(AIStates_old.AIMove());
		}

		public static IEnumerator AIMove(BattleData battleData){
			yield return null;
		}

		public static void CheckActiveTrigger(BattleData battleData){
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
							aroundUnits.Add(tile.GetUnitOnTile());
					});
				});

				if (aroundUnits.Contains(currentUnit))
					aroundUnits.Remove(currentUnit);

				bool isThereAnotherSideUnit = aroundUnits.Any(unit => unit.GetSide() != currentUnit.GetSide());

				if (isThereAnotherSideUnit){
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

    public class AIStates_old{
		private static BattleData battleData;
		public static void SetBattleData(BattleData battleDataInstance){
			battleData = battleDataInstance;
		}
		private static IEnumerator KashastyAI(){
			BattleManager battleManager = battleData.battleManager;
			Unit unit = battleData.selectedUnit;
			Tile currentTile = unit.GetTileUnderUnit ();

			while (true) {
				int selectedSkillIndex = 1;
				battleData.indexOfSelectedSkillByUser = selectedSkillIndex;
				ActiveSkill selectedSkill = battleData.SelectedSkill;
				SkillType skillTypeOfSelectedSkill = selectedSkill.GetSkillType ();//더블 샷이니 경로형일 것임

				//행동하기 전마다 체크해야 할 사항들(중요!)
				yield return battleData.battleManager.StartCoroutine (BattleManager.UpdateRetreatAndDeadUnits (battleData, battleData.battleManager));
				yield return BattleManager.AtActionEnd (battleData);

				int currentAP = unit.GetCurrentActivityPoint ();
				Vector2 currPos = unit.GetPosition ();

				//속박/기절 안 걸린 상태
				if (BattleManager.Instance.IsMovePossibleState (battleData)) {

					//현 타일에서 그레/비앙/달케 공격 가능할 시 공격
					if (currentAP >= selectedSkill.GetRequireAP ()) {
						Tile rightDownTile = GetKashastyAttackRouteEnd (Direction.RightDown, unit, currPos);
						Tile leftUpTile = GetKashastyAttackRouteEnd (Direction.LeftUp, unit, currPos);
						Tile rightUpTile = GetKashastyAttackRouteEnd (Direction.RightUp, unit, currPos);
						Tile leftDownTile = GetKashastyAttackRouteEnd (Direction.LeftDown, unit, currPos);
						if (IsTastyTile (rightDownTile)) {
							yield return AISkillFuncPiece (unit, Direction.RightDown, rightDownTile);
							continue;
						}
						if (IsTastyTile (leftUpTile)) {
							yield return AISkillFuncPiece (unit, Direction.LeftUp, leftUpTile);
							continue;
						}
						if (IsTastyTile (rightUpTile)) {
							yield return AISkillFuncPiece (unit, Direction.RightUp, rightUpTile);
							continue;
						}
						if (IsTastyTile (leftDownTile)) {
							yield return AISkillFuncPiece (unit, Direction.LeftDown, leftDownTile);
							continue;
						}
					}
					//아래를 일반화해서 좌/우/좌좌/우우/좌좌좌/우우우 순서로 확인해야 함
					if (currentAP >= 3 + selectedSkill.GetRequireAP ()) {
						Vector2 leftPos = currPos + battleData.tileManager.ToVector2 (Direction.RightUp);
						Tile leftTile = battleData.tileManager.GetTile (leftPos);
						if (leftTile != null && leftTile.IsUnitOnTile ()) {
							Tile rightDownTile = GetKashastyAttackRouteEnd (Direction.RightDown, unit, leftPos);
							Tile leftUpTile = GetKashastyAttackRouteEnd (Direction.LeftUp, unit, leftPos);
							Tile rightUpTile = GetKashastyAttackRouteEnd (Direction.RightUp, unit, leftPos);
							Tile leftDownTile = GetKashastyAttackRouteEnd (Direction.LeftDown, unit, leftPos);
							if (IsTastyTile (rightDownTile)) {
								yield return AIMoveFuncPiece(leftTile, 3);
								yield return AISkillFuncPiece (unit, Direction.RightDown, rightDownTile);
								continue;
							}
							if (IsTastyTile (leftUpTile)) {
								yield return AIMoveFuncPiece(leftTile, 3);
								yield return AISkillFuncPiece (unit, Direction.LeftUp, leftUpTile);
								continue;
							}
							if (IsTastyTile (rightUpTile)) {
								yield return AIMoveFuncPiece(leftTile, 3);
								yield return AISkillFuncPiece (unit, Direction.RightUp, rightUpTile);
								continue;
							}
							if (IsTastyTile (leftDownTile)) {
								yield return AIMoveFuncPiece(leftTile, 3);
								yield return AISkillFuncPiece (unit, Direction.LeftDown, leftDownTile);
								continue;
							}
						}
					}

					Tile barrierTile = GetKashastyAttackRouteEnd(Direction.RightDown, unit, currPos);

					if (barrierTile == null) {
						int step = 0;
						int requireAP = 0;
						Vector2 pos = unit.GetPosition ();

						while ((!BattleManager.Instance.IsStandbyPossibleWithThisAP (unit, currentAP - requireAP)) || currentAP - requireAP >= unit.GetStandardAP ()) {
							pos += battleData.tileManager.ToVector2 (Direction.RightDown);
							Tile tile = battleData.tileManager.GetTile (pos);
							if (tile == null) {
								Debug.Log ("tile==null");
								break;
							}
							step++;
							requireAP += 3 + 2 * (step - 1);
							Debug.Log (step);
							Debug.Log (requireAP);
						}

						int totalUseAP = requireAP;
						Vector2 destPos = unit.GetPosition () + battleData.tileManager.ToVector2 (Direction.RightDown) * step;
						Tile destTile = battleData.tileManager.GetTile (destPos);
						battleData.currentState = CurrentState.CheckDestination;

						yield return AIMoveFuncPiece(destTile, totalUseAP);
						yield return PassTurn ();
						yield break;
					} else {
						if (currentAP < selectedSkill.GetRequireAP ())
							break;

						unit.SetDirection (Direction.RightDown);

						battleData.currentState = CurrentState.CheckApplyOrChain;

						List<Tile> tilesInSkillRange = new List<Tile> ();
						tilesInSkillRange.Add (barrierTile);
						List<Tile> tilesInRealEffectRange = tilesInSkillRange;

						yield return SkillAndChainStates.ApplyChain (battleData, barrierTile, tilesInSkillRange, tilesInRealEffectRange, GetTilesInFirstRange ());
						FocusUnit (battleData.selectedUnit);
						battleData.currentState = CurrentState.FocusToUnit;

						battleData.uiManager.ResetSkillNamePanelUI ();
					}
				}

				//속박/기절 걸린 상태
				else{
					//상하좌우에 쏠 수 있는 애가 있으면 쏜다. 우선순위는 그레네브=비앙카=달케니르 > 다른 모든 유닛(지형지물 포함)
					if (currentAP < selectedSkill.GetRequireAP ())
						break;

					Tile rightDownTile = GetKashastyAttackRouteEnd(Direction.RightDown, unit, currPos);
					Tile leftUpTile = GetKashastyAttackRouteEnd(Direction.LeftUp, unit, currPos);
					Tile rightUpTile = GetKashastyAttackRouteEnd(Direction.RightUp, unit, currPos);
					Tile leftDownTile = GetKashastyAttackRouteEnd(Direction.LeftDown, unit, currPos);

					if (IsTastyTile (rightDownTile)) {
						yield return AISkillFuncPiece (unit, Direction.RightDown, rightDownTile);
						continue;
					}
					if (IsTastyTile (leftUpTile)) {
						yield return AISkillFuncPiece (unit, Direction.LeftUp, leftUpTile);
						continue;
					}
					if (IsTastyTile (rightUpTile)) {
						yield return AISkillFuncPiece (unit, Direction.RightUp, rightUpTile);
						continue;
					}
					if (IsTastyTile (leftDownTile)) {
						yield return AISkillFuncPiece (unit, Direction.LeftDown, leftDownTile);
						continue;
					}
					if (IsDecentTile (rightDownTile)) {
						yield return AISkillFuncPiece (unit, Direction.RightDown, rightDownTile);
						continue;
					}
					if (IsDecentTile (leftUpTile)) {
						yield return AISkillFuncPiece (unit, Direction.LeftUp, leftUpTile);
						continue;
					}
					if (IsDecentTile (rightUpTile)) {
						yield return AISkillFuncPiece (unit, Direction.RightUp, rightUpTile);
						continue;
					}
					if (IsDecentTile (leftDownTile)) {
						yield return AISkillFuncPiece (unit, Direction.LeftDown, leftDownTile);
						continue;
					}
					battleData.currentState = CurrentState.RestAndRecover;
					yield return battleData.battleManager.StartCoroutine (RestAndRecover.Run (battleData));
					yield break;
				}
			}

			if (BattleManager.Instance.IsStandbyPossible (battleData) && unit.GetCurrentActivityPoint() < unit.GetStandardAP ()) {
				yield return PassTurn ();
			}
			else {
				battleData.currentState = CurrentState.RestAndRecover;
				yield return battleData.battleManager.StartCoroutine (RestAndRecover.Run (battleData));
			}

			yield break;
		}

		private static Tile GetKashastyAttackRouteEnd(Direction direction, Unit unit, Vector2 pos){
			List<Tile> frontEightTiles = battleData.tileManager.GetTilesInRange (RangeForm.Straight,
				pos,
				1,
				8,
				0,
				direction);
			Tile barrierTile = SkillAndChainStates.GetRouteEnd (frontEightTiles);
			return barrierTile;
		}
		private static bool IsTastyPCOnThatTile(Tile tile){
			if (tile == null)
				return false;
			if (tile.GetUnitOnTile () == null)
				return false;
			string unitCodeName = tile.GetUnitOnTile ().GetNameInCode ();
			return unitCodeName == "grenev" || unitCodeName == "darkenir" || unitCodeName == "bianca";
		}
		private static bool IsTastyTile(Tile tile){
			return tile != null && tile.IsUnitOnTile () && IsTastyPCOnThatTile (tile);
		}
		private static bool IsDecentTile(Tile tile){
			return tile != null && tile.IsUnitOnTile ();
		}
		private static IEnumerator AISkillFuncPiece(Unit unit, Direction direction, Tile targetTile){
			unit.SetDirection (direction);
			battleData.currentState = CurrentState.CheckApplyOrChain;
			List<Tile> tilesInSkillRange = new List<Tile> ();
			tilesInSkillRange.Add (targetTile);
			List<Tile> tilesInRealEffectRange = tilesInSkillRange;

			yield return SkillAndChainStates.ApplyChain (battleData, targetTile, tilesInSkillRange, tilesInRealEffectRange, GetTilesInFirstRange ());
			FocusUnit (unit);
			battleData.currentState = CurrentState.FocusToUnit;

			battleData.uiManager.ResetSkillNamePanelUI ();
		}
		private static IEnumerator AIMoveFuncPiece(Tile destTile,int totalUseAP){
			Debug.Log ("AIMoveFuncPiece");
			Camera.main.transform.position = new Vector3 (destTile.transform.position.x, destTile.transform.position.y, -10);
			battleData.currentState = CurrentState.MoveToTile;
			yield return battleData.battleManager.StartCoroutine (MoveStates.MoveToTile (battleData, destTile, Direction.RightDown, totalUseAP));
		}

		public static IEnumerator AIMove()
		{
			BattleManager battleManager = battleData.battleManager;
			Unit unit = battleData.selectedUnit;
			Tile currentTile = unit.GetTileUnderUnit ();

			if(unit.GetNameInCode() == "kashasty_Escape"){
				yield return KashastyAI();
				yield break;
			}
			
			//이동 전에 먼저 기술부터 정해야 한다... 기술 범위에 따라 어떻게 이동할지 아니면 이동 안 할지가 달라지므로
			//나중엔 여러 기술중에 선택해야겠지만 일단 지금은 AI 기술이 모두 하나뿐이니 그냥 첫번째걸로
			int selectedSkillIndex = 1;
			battleData.indexOfSelectedSkillByUser = selectedSkillIndex;
			ActiveSkill selectedSkill = battleData.SelectedSkill;

			SkillType skillTypeOfSelectedSkill = selectedSkill.GetSkillType ();

			Tile attackAbleTile;

			if (skillTypeOfSelectedSkill != SkillType.Point)
				attackAbleTile = GetAttackableOtherSideUnitTileOfDirectionSkill (currentTile);
			else
				attackAbleTile = GetAttackableOtherSideUnitTileOfPointSkill (currentTile);

			if (attackAbleTile != null) {
				yield return AIAct();
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

			yield return AIAct();
		}

		public static IEnumerator AIAct(){
			while (true) {
				//행동하기 전마다 체크해야 할 사항들(중요!)
				yield return battleData.battleManager.StartCoroutine(BattleManager.UpdateRetreatAndDeadUnits(battleData, battleData.battleManager));
				yield return BattleManager.AtActionEnd(battleData);

				int selectedSkillIndex = 1;
				battleData.indexOfSelectedSkillByUser = selectedSkillIndex;
				Unit selectedUnit = battleData.selectedUnit;
				ActiveSkill selectedSkill = battleData.SelectedSkill;

				if(selectedSkill == null){
					battleData.currentState = CurrentState.RestAndRecover;
					yield return battleData.battleManager.StartCoroutine (RestAndRecover.Run (battleData));
					yield break;
				}

				int currentAP = battleData.selectedUnit.GetCurrentActivityPoint ();
				int requireAP = battleData.SelectedSkill.GetRequireAP ();
				bool enoughAP = currentAP >= requireAP;
				bool notInNonskillableEffect = !(selectedUnit.HasStatusEffect(StatusEffectType.Silence) ||
					selectedUnit.HasStatusEffect(StatusEffectType.Faint));

				if (enoughAP && notInNonskillableEffect) {
					yield return AISkill (selectedSkillIndex);
				}
				else {
					if (BattleManager.Instance.IsStandbyPossible (battleData) && currentAP - requireAP < selectedUnit.GetStandardAP ()) {
						yield return PassTurn ();
					}
					else {
						battleData.currentState = CurrentState.RestAndRecover;
						yield return battleData.battleManager.StartCoroutine (RestAndRecover.Run (battleData));
					}
					yield break;
				}
			}
		}

		public static IEnumerator PassTurn()
		{
			yield return new WaitForSeconds(0.5f);
		}

		public static IEnumerator AISkill(int selectedSkillIndex)
		{
			BattleManager battleManager = battleData.battleManager;
			ActiveSkill selectedSkill = battleData.SelectedSkill;

			SkillType skillTypeOfSelectedSkill = selectedSkill.GetSkillType();
			if (skillTypeOfSelectedSkill != SkillType.Point)
			{
				battleData.currentState = CurrentState.SelectSkillApplyDirection;
				yield return battleManager.StartCoroutine(SelectSkillApplyDirection(battleData.selectedUnit.GetDirection()));
			}
			else
			{
				battleData.currentState = CurrentState.SelectSkillApplyPoint;
				yield return battleManager.StartCoroutine(SelectSkillApplyPoint(battleData.selectedUnit.GetDirection()));
			}

			battleData.previewAPAction = null;
			battleData.uiManager.UpdateApBarUI(battleData, battleData.unitManager.GetAllUnits());
		}

		public static IEnumerator SelectSkillApplyDirection(Direction originalDirection)
		{
			Unit selectedUnit = battleData.selectedUnit;
			Direction beforeDirection = selectedUnit.GetDirection ();
			Tile selectedTile = GetAttackableOtherSideUnitTileOfDirectionSkill(selectedUnit.GetTileUnderUnit());

			if (selectedTile == null)
			{
				Debug.Log(selectedUnit.GetName () + " cannot find unit for direction attack. " );
				// 아무것도 할 게 없을 경우 휴식
				battleData.currentState = CurrentState.RestAndRecover;
				yield return battleData.battleManager.StartCoroutine(RestAndRecover.Run(battleData));
				yield break;
			}

			Direction afterDirection = Utility.GetDirectionToTarget(selectedUnit, selectedTile.GetTilePos());
			selectedUnit.SetDirection (afterDirection);

			battleData.currentState = CurrentState.CheckApplyOrChain;

			List<Tile> tilesInSkillRange = GetTilesInSkillRange(selectedTile, selectedUnit);
			//tilesInRealEffectRange는 투사체 스킬의 경우 경로상 유닛이 없으면 빈 List로 설정해야 한다. 일단 AI 유닛 스킬엔 없으니 생략
			List<Tile> tilesInRealEffectRange =  tilesInSkillRange;

			yield return SkillAndChainStates.ApplyChain(battleData, selectedTile, tilesInSkillRange, tilesInRealEffectRange, GetTilesInFirstRange());
			FocusUnit(battleData.selectedUnit);
			battleData.currentState = CurrentState.FocusToUnit;

			battleData.uiManager.ResetSkillNamePanelUI();
		}
		public static Tile GetAttackableOtherSideUnitTileOfDirectionSkill(Tile unitTile){
			List<Tile> selectedTiles = new List<Tile>();
			Unit selectedUnit = battleData.selectedUnit;
			ActiveSkill selectedSkill = battleData.SelectedSkill;

			Tile castingTile = unitTile;

			//투사체 스킬이면 직선경로상에서 유닛이 가로막은 지점을 castingTile로 함. 범위 끝까지 가로막은 유닛이 없으면 범위 맨 끝 타일이 castingTile=null
			if (selectedSkill.GetSkillType() == SkillType.Route) {
				//FIXME : 리스트로 만들어야 되는데.... 전체적으로 혼파망이라서 일단 이렇게 놔둠
				castingTile = GetRouteSkillCastingTile ( selectedUnit, selectedSkill, Direction.LeftUp);
				if(castingTile==null)
					castingTile = GetRouteSkillCastingTile (selectedUnit, selectedSkill, Direction.LeftDown);
				if(castingTile==null)
					castingTile = GetRouteSkillCastingTile (selectedUnit, selectedSkill, Direction.RightUp);
				if (castingTile == null)
					castingTile = GetRouteSkillCastingTile ( selectedUnit, selectedSkill, Direction.RightDown);
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
		private static Tile GetRouteSkillCastingTile(Unit unit, ActiveSkill routeSkill, Direction direction){				
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

		public static IEnumerator SelectSkillApplyPoint(Direction originalDirection){

			Unit selectedUnit = battleData.selectedUnit;
			Direction beforeDirection = selectedUnit.GetDirection ();
			Tile selectedTile = GetAttackableOtherSideUnitTileOfPointSkill(selectedUnit.GetTileUnderUnit());

			if (selectedTile == null)
			{
				Debug.Log(selectedUnit.GetName () + " cannot find unit for point attack. " );
				// 아무것도 할 게 없을 경우 휴식
				battleData.currentState = CurrentState.RestAndRecover;
				yield return battleData.battleManager.StartCoroutine(RestAndRecover.Run(battleData));
				yield break;
			}

			Direction afterDirection = Utility.GetDirectionToTarget(selectedUnit, selectedTile.GetTilePos());
			selectedUnit.SetDirection (afterDirection);

			battleData.currentState = CurrentState.CheckApplyOrChain;

			List<Tile> tilesInSkillRange = GetTilesInSkillRange(selectedTile, selectedUnit);
			//tilesInRealEffectRange는 투사체 스킬의 경우 경로상 유닛이 없으면 빈 List로 설정해야 한다. 일단 AI 유닛 스킬엔 없으니 생략
			List<Tile> tilesInRealEffectRange =  tilesInSkillRange;

			yield return SkillAndChainStates.ApplyChain(battleData, selectedTile, tilesInSkillRange, tilesInRealEffectRange, GetTilesInFirstRange());
			FocusUnit(battleData.selectedUnit);
			battleData.currentState = CurrentState.FocusToUnit;

			battleData.uiManager.ResetSkillNamePanelUI();
		}
		public static Tile GetAttackableOtherSideUnitTileOfPointSkill(Tile unitTile){
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

		private static List<Tile> GetTilesInSkillRange(Tile targetTile, Unit selectedUnit = null)
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

		private static List<Tile> GetTilesInFirstRange()
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

// FIXME : 제작중이고 아직 안 쓰인다
public class AI{
	private static BattleData battleData;
	public static void SetBattleData(BattleData battleDataInstance){
		battleData = battleDataInstance;
	}
	private static BattleManager battleManager;
	public static void SetBattleManager(BattleManager battleManagerInstance){
		battleManager = battleManagerInstance;
	}

	public static IEnumerator UnitTurn(Unit unit){
		battleManager.StartUnitTurn (unit);

		yield return PrepareUnitActAndDecideActionAndAct(unit);

		battleManager.EndUnitTurn();
	}

	private static IEnumerator PrepareUnitActAndDecideActionAndAct(Unit unit){
		yield return battleManager.BeforeActCommonAct ();

		AIData unitAIData = unit.GetComponent<AIData>();

		if (!unitAIData.IsActive())
			CheckActiveTrigger(unit);

		if (!unitAIData.IsActive ()) {
			Debug.Log (unit.GetName () + " takes rest because of being deactivated");
			yield return battleData.battleManager.StartCoroutine (RestAndRecover.Run (battleData));
			yield break;
		}
		else {
			yield return DecideActionAndAct (unit);
		}
	}

	private static IEnumerator DecideActionAndAct(Unit unit){
		if (unit.HasStatusEffect(StatusEffectType.Faint))
		{
			yield return battleData.battleManager.StartCoroutine(RestAndRecover.Run(battleData));
			yield break;
		}
		yield return DecideMoveAndMove (unit);
		yield return DecideSkillAndUseSkill (unit);
		yield return DecideRestOrStandbyAndDoThat (unit);
	}

	private static IEnumerator DecideMoveAndMove(Unit unit){
		yield break;
	}
	private static IEnumerator DecideSkillAndUseSkill(Unit unit){
		yield break;
	}
	private static IEnumerator DecideRestOrStandbyAndDoThat(Unit unit){
		yield break;
	}

	private static void CheckActiveTrigger(Unit unit){
		bool satisfyActiveCondition = false;
		AIData unitAIData = unit.GetComponent<AIData> ();
		// 전투 시작시 활성화
		if (unitAIData.activeTriggers.Contains(1))
		{
			satisfyActiveCondition = true;
		}
		// 일정 페이즈부터 활성화
		else if (unitAIData.activeTriggers.Contains(2))
		{
			if (battleData.currentPhase >= unitAIData.activePhase) {
				Debug.Log (unit.GetName () + " is activated because enough phase passed");
				satisfyActiveCondition = true;
			}
		}
		// 자신 주위 일정 영역에 접근하면 활성화
		else if (unitAIData.activeTriggers.Contains(3))
		{
			// 자신을 기준으로 한 상대좌표
			List<List<Tile>> aroundTiles = unitAIData.trigger3Area;
			List<Unit> aroundUnits = new List<Unit>();

			aroundTiles.ForEach(eachArea => {
				eachArea.ForEach(tile => {
					if (tile.IsUnitOnTile())
						aroundUnits.Add(tile.GetUnitOnTile());
				});
			});

			if (aroundUnits.Contains(unit))
				aroundUnits.Remove(unit);

			bool isThereAnotherSideUnit = aroundUnits.Any(anyUnit => anyUnit.GetSide() != unit.GetSide());

			if (isThereAnotherSideUnit){
				Debug.Log (unit.GetName () + " is activated because its enemy came to nearby");
				satisfyActiveCondition = true;
			}
		}
		// 맵 상의 특정 영역에 접근하면 활성화
		else if (unitAIData.activeTriggers.Contains(4))
		{
			// 절대좌표
			List<List<Tile>> aroundTiles = unitAIData.trigger4Area;
			List<Unit> aroundUnits = new List<Unit>();

			aroundTiles.ForEach(eachArea => {
				eachArea.ForEach(tile => {
					if (tile.IsUnitOnTile())
					{
						aroundUnits.Add(tile.GetUnitOnTile());
					}
				});
			});

			if (aroundUnits.Contains(unit))
				aroundUnits.Remove(unit);

			bool isThereAnotherSideUnit = aroundUnits.Any(anyUnit => anyUnit.GetSide() != unit.GetSide());

			if (isThereAnotherSideUnit)
			{
				Debug.Log (unit.GetName () + " is activated because its enemy came to absolute position range");
				satisfyActiveCondition = true;
			}
		}
		// 자신을 대상으로 기술이 시전되면 활성화
		else if (unitAIData.activeTriggers.Contains(5))
		{
			// 뭔가 기술의 영향을 받으면
			// SkillAndChainState.ApplySkill에서 체크
		}
		if(satisfyActiveCondition)
			unitAIData.SetActive();
	}
}