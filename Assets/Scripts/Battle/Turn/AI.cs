﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enums;

namespace Battle.Turn{
	public class AIUtil{
		//AI 입장에서 다른 유닛이 적으로 인식되는지에 대한 함수이기 때문에 AI 부분에서만 써야 한다(은신 스킬 때문)
		//왼쪽 unit1에는 무조건 AI 유닛이 들어가야 하고(unit1의 입장에서  unit2를 보는 것임) unit2엔 아무 유닛이나 들어감
		public static bool IsSecondUnitEnemyToFirstUnit(Unit unit1,Unit unit2){
			//후에 제3세력 등장하면 조건이 바뀔 수 있음

			//unit2가 '은신' 효과를 갖고 있으면 AI인 unit1에게 적으로 인식되지 않는다
			return unit2.GetSide () == GetEnemySide (unit1) && !unit2.HasStatusEffect(StatusEffectType.Stealth);
		}
		private static Side GetEnemySide(Unit unit){
			//후에 제3세력 등장하면 조건이 바뀔 수 있음
			Side mySide = unit.GetSide ();
			Side enemySide;
			if (mySide == Side.Ally)
				enemySide = Side.Enemy;
			else
				enemySide = Side.Ally;
			return enemySide;
		}
		public static List<Tile> FindEnemyTilesInTheseTiles(List<Tile> tiles, Unit mainUnit){
			List<Tile> tilesHaveEnemy = (from tile in tiles
				where tile.GetUnitOnTile() != null
				let unit = tile.GetUnitOnTile()
				where IsSecondUnitEnemyToFirstUnit(mainUnit, unit)
				select tile).ToList();
			return tilesHaveEnemy;
		}

		// 1턴 내에 이동 후 공격 불가하거나 그럴 만한 가치가 없을 때 가장 가치있는 적을 향해 움직인다
		// 가치에는 거리도 적용되므로 너무 멀면 그쪽으로 가진 않는다
		public static Tile GetBestApproachWorthTile(Unit unit, ActiveSkill skill, Dictionary<Vector2, TileWithPath> movableTilesWithPath, List<Vector2> goalArea){
			Tile unitTile = unit.GetTileUnderUnit ();
			Vector2 unitPos = unit.GetPosition ();
			int minAPUse = unit.MinAPUseForStandbyForAI ();

			Dictionary<Vector2, TileWithPath> allPaths = PathFinder.CalculatePathsFromThisTileForAI (unit, unitTile, int.MaxValue, skill);
			TileWithPath bestFinalTileWithPath = new TileWithPath (unit.GetTileUnderUnit ());
			Tile bestTile = unit.GetTileUnderUnit ();

			if (goalArea.Count == 0) {
				float maxFinalReward = 0;
				Unit caster = unit;

				foreach (var pair in allPaths) {
					TileWithPath tileWithPath = pair.Value;
					Tile tile = tileWithPath.tile;
					int requireAP = pair.Value.requireActivityPoint;

					Casting bestCastingOnThisTile = skill.GetBestAttack (caster, tile);

					if (bestCastingOnThisTile != null) {
						float singleCastingReward = skill.GetRewardByCasting (bestCastingOnThisTile);
						float reward = singleCastingReward / (float)Math.Sqrt (requireAP);

						if (reward > maxFinalReward) {
							maxFinalReward = reward;
							bestFinalTileWithPath = tileWithPath;
						}
					}
				}
					
				if (maxFinalReward == 0) {
					return bestTile;
				}

			} else {
				Debug.Log ("For goal");
				Dictionary<Vector2, TileWithPath> allGoalPaths = new Dictionary<Vector2, TileWithPath> ();
				foreach (var pair in allPaths) {
					TileWithPath tileWithPath = pair.Value;
					Vector2 tilePos = tileWithPath.tile.GetTilePos ();
					if (goalArea.Contains (tilePos)) {
						allGoalPaths [pair.Key] = pair.Value;
					}
				}

				bestFinalTileWithPath = GetMinRequireAPTileWithPath (allGoalPaths);

				if(unit.GetCurrentActivityPoint() >= bestFinalTileWithPath.requireActivityPoint){
					bestTile=bestFinalTileWithPath.tile;
					return bestTile;
				}
			}

			List<Tile> path = bestFinalTileWithPath.path;
			path.Add (bestFinalTileWithPath.tile);

			foreach (Tile tile in path) {
				Vector2 tilePos = tile.GetTilePos ();
				if (!movableTilesWithPath.ContainsKey (tilePos)) {
					break;
				}
				if (movableTilesWithPath [tilePos].requireActivityPoint >= minAPUse) {
					bestTile = tile;
					break;
				}
			}

			return bestTile;
		}

		// 1턴 내에 이동 후 공격하는 모든 경우 중 가장 가치있는 경우를 찾아서 이동 목적지 return
		public static Tile GetBestMovableTile(Unit caster, ActiveSkill skill, Dictionary<Vector2, TileWithPath> movableTilesWithPath, int moveRestrainFactor, int minWorthReward){
			Tile bestTile = caster.GetTileUnderUnit ();
			int currAP = caster.GetCurrentActivityPoint ();
			int skillRequireAP = caster.GetActualRequireSkillAP (skill);
			float maxReward = 0;

			foreach (var pair in movableTilesWithPath) {
				Tile tile = pair.Value.tile;
				int requireAP = pair.Value.requireActivityPoint;
				int actualRemainAP = currAP - requireAP - moveRestrainFactor;
				int possibleSkillUseCount = actualRemainAP / skillRequireAP;

				if (possibleSkillUseCount <= 0)
					continue;

				float reward = 0;
				Casting bestCastingOnThisTile = skill.GetBestAttack (caster, tile);
				if (bestCastingOnThisTile != null) {
					float singleCastingReward = skill.GetRewardByCasting (bestCastingOnThisTile);
					reward = singleCastingReward * possibleSkillUseCount;

					if (reward > maxReward) {
						maxReward = reward;
						bestTile = tile;
					}
				}
			}

			if (maxReward <= minWorthReward) {
				return null;
			} else {
				return bestTile;
			}
		}

		static TileWithPath GetMinRequireAPTileWithPath(Dictionary<Vector2, TileWithPath> movableTilesWithPath){
			if (movableTilesWithPath.Count > 0){
				TileWithPath nearestTileWithPath=null;
				int minRequireAP = Int32.MaxValue;
				foreach (var pair in movableTilesWithPath) {
					int requireAP = pair.Value.requireActivityPoint;
					if (requireAP < minRequireAP) {
						minRequireAP = requireAP;
						nearestTileWithPath = pair.Value;
					}
				}
				return nearestTileWithPath;
			}
			return null;
		}
	}

	public class AI : MonoBehaviour{
		private static BattleManager battleManager;
		public static void SetBattleManager(BattleManager battleManagerInstance){
			battleManager = battleManagerInstance;
		}

		Unit unit;
		AIData _AIData;
		public void Initialize(Unit unit, AIData _AIData){
			this.unit = unit;
			this._AIData = _AIData;
		}

		enum State{ Dead, SkipTurn, EndTurn, MoveToBestCasting, CastingLoop, Approach, StandbyOrRest }

		State state;

		public IEnumerator UnitTurn(){
			battleManager.StartUnitTurn (unit);

			yield return CheckUnitIsActiveAndDecideActionAndAct();

			if (state != State.Dead) {
				battleManager.EndUnitTurn ();
			}
		}

		IEnumerator CheckUnitIsActiveAndDecideActionAndAct(){
			if (!_AIData.IsActive()) {
				CheckActiveTrigger();
			}

			if (!_AIData.IsActive ()) {
				yield return battleManager.ToDoBeforeAction ();
				state = State.SkipTurn;
			} else if (unit.GetNameEng ().Equals ("triana")) {
				state = State.StandbyOrRest;
			} else {
				state = State.MoveToBestCasting;
			}
			yield return FSM ();
		}

		IEnumerator FSM(){
			while (true) {
				if (state == State.Dead || state == State.EndTurn) {
					yield break;
				}
				Debug.Log(state.ToString ());
				yield return StartCoroutine (state.ToString ());
			}
		}

		IEnumerator MoveToBestCasting(){
			if (!unit.IsMovePossibleState()) {
				state = State.CastingLoop;
				yield break;
			}

			yield return battleManager.ToDoBeforeAction ();

			//이동 전에 먼저 기술부터 정해야 한다... 기술 범위에 따라 어떻게 이동할지 아니면 이동 안 할지가 달라지므로
			//나중엔 여러 기술중에 선택해야겠지만 일단 지금은 AI 기술이 모두 하나뿐이니 그냥 첫번째걸로
			BattleData.selectedSkill = BattleData.selectedUnit.activeSkillList[0];
			ActiveSkill selectedSkill = BattleData.SelectedSkill;

			yield return PaintMovableTiles ();

			Dictionary<Vector2, TileWithPath> movableTilesWithPathWithDestroying = PathFinder.CalculateMovablePathsForAI(unit, selectedSkill);

			int minRewardWorthAttack = 0;

			if (unit.GetNameEng () == "kashasty_Escape") {
				minRewardWorthAttack = 400;
			}

			Tile destTile = AIUtil.GetBestMovableTile (unit, selectedSkill, movableTilesWithPathWithDestroying, 0, minRewardWorthAttack);

			if (destTile != null) {
				yield return MoveWithDestroyRoutine (selectedSkill, movableTilesWithPathWithDestroying, destTile);
				state = State.CastingLoop;
			} else {
				TileManager.Instance.DepaintAllTiles(TileColor.Blue);
				state = State.Approach;
				yield break;
			}
		}

		public static IEnumerator PaintMovableTiles(Dictionary<Vector2, TileWithPath> movableTilesWithPath){
			List<Tile> movableTiles = new List<Tile>();
			foreach (KeyValuePair<Vector2, TileWithPath> movableTileWithPath in movableTilesWithPath)
				movableTiles.Add (movableTileWithPath.Value.tile);
			TileManager.Instance.PaintTiles (movableTiles, TileColor.Blue);
			yield return null;
		}

		public IEnumerator MoveWithDestroyRoutine(ActiveSkill skill, Dictionary<Vector2, TileWithPath> movableTilesWithPathWithDestroying, Tile destTile){
			Tile startTile = unit.GetTileUnderUnit ();

			TileWithPath destTileWithPath = movableTilesWithPathWithDestroying [destTile.GetTilePos ()];
			List<Tile> destPath = destTileWithPath.path;
			int destRequireAP = destTileWithPath.requireActivityPoint;

			List<Tile> mustCheckEmptinessTiles = new List<Tile> ();
			foreach (Tile tile in destPath) {
				if (tile != startTile) {
					mustCheckEmptinessTiles.Add (tile);
				}
			}
			if (destTile != startTile) {
				mustCheckEmptinessTiles.Add (destTile);
			}

			Tile prevTile = unit.GetTileUnderUnit ();
			List<Tile> prevPath = new List<Tile> ();
			int prevRequireAP = 0;

			foreach (Tile tile in mustCheckEmptinessTiles) {
				if (tile.IsUnitOnTile ()) {
					TileWithPath intermediateTileWithPath = movableTilesWithPathWithDestroying [prevTile.GetTilePos ()];

					List<Tile> realPath = intermediateTileWithPath.path.Except (prevPath).ToList ();
					int realRequireAP = intermediateTileWithPath.requireActivityPoint - prevRequireAP;

					yield return MoveToTheTileAndChangeDirection (unit, prevTile, realPath, realRequireAP);

					prevPath = intermediateTileWithPath.path;
					prevRequireAP = intermediateTileWithPath.requireActivityPoint;

					yield return DestroyObstacle (skill, tile);
					prevRequireAP += obstacleDestroyCost;
					yield return PaintMovableTiles ();
				}
				prevTile = tile;
			}

			List<Tile> remainPath = destPath.Except (prevPath).ToList ();
			int remainRequireAP = destRequireAP - prevRequireAP;
			yield return MoveToTheTileAndChangeDirection (unit, destTile, remainPath, remainRequireAP);
		}

		int obstacleDestroyCost = 0;

		IEnumerator DestroyObstacle(ActiveSkill skill, Tile obstacleTile){
			Debug.Log ("Destory obstacle period");

			obstacleDestroyCost = 0;

			Tile currTile = unit.GetTileUnderUnit ();
			Vector2 currPos = currTile.GetTilePos ();
			Vector2 obstaclePos = obstacleTile.GetTilePos ();

			SkillLocation location;
			if (skill.GetSkillType () != SkillType.Point) {
				if (skill.GetSkillType () == SkillType.Route) {
					location = new SkillLocation (currTile, obstacleTile, Utility.VectorToDirection (obstaclePos - currPos));
				} else {
					location = new SkillLocation (currTile, currTile, Utility.VectorToDirection (obstaclePos - currPos));
				}
			} else {
				location = new SkillLocation (currTile, obstacleTile, Utility.GetDirectionToTarget (currPos, obstaclePos));
			}
			Casting casting = new Casting (unit, skill, location);

			while (obstacleTile.IsUnitOnTile ()) {
				Debug.Log ("destroying obstacle on the tile "+obstaclePos.x+" : "+obstaclePos.y);
				yield return battleManager.ToDoBeforeAction ();
				yield return UseSkill (casting);
				obstacleDestroyCost += unit.GetActualRequireSkillAP (skill);
			}
		}

		public static IEnumerator MoveToTheTileAndChangeDirection(Unit unit, Tile destTile, List<Tile> path, int requireAP){
			Vector2 destPos = destTile.GetTilePos ();

			if (path.Count > 0) {
				Direction finalDirection = Utility.GetFinalDirectionOfPath (destTile, path, unit.GetDirection ());
				yield return Move (unit, destTile, finalDirection, requireAP, path.Count + 1);
			} else {
				TileManager.Instance.DepaintAllTiles (TileColor.Blue);
			}
		}

		IEnumerator PaintMovableTiles(){
			Dictionary<Vector2, TileWithPath> movableTilesWithPathForPainting = PathFinder.CalculateMovablePaths(unit);
			yield return PaintMovableTiles(movableTilesWithPathForPainting);
		}

		IEnumerator Approach(){
			if (!unit.IsMovePossibleState()) {
				state = State.CastingLoop;
				yield break;
			}

			yield return battleManager.ToDoBeforeAction ();

			//이동 전에 먼저 기술부터 정해야 한다... 기술 범위에 따라 어떻게 이동할지 아니면 이동 안 할지가 달라지므로
			//나중엔 여러 기술중에 선택해야겠지만 일단 지금은 AI 기술이 모두 하나뿐이니 그냥 첫번째걸로
			BattleData.selectedSkill = BattleData.selectedUnit.activeSkillList[0];
			ActiveSkill skill = BattleData.SelectedSkill;

			yield return PaintMovableTiles ();

			Dictionary<Vector2, TileWithPath> movableTilesWithPathWithDestroying = PathFinder.CalculateMovablePathsForAI (unit, skill);

			Tile destTile = AIUtil.GetBestApproachWorthTile (unit, skill, movableTilesWithPathWithDestroying, _AIData.goalArea);
			TileWithPath destTileWithPath = movableTilesWithPathWithDestroying [destTile.GetTilePos ()];

			yield return MoveWithDestroyRoutine (skill, movableTilesWithPathWithDestroying, destTile);
			state = State.StandbyOrRest;
		}

		IEnumerator CastingLoop(){
			bool flag = false;

			while(true){
				if (BattleManager.IsSelectedUnitRetreatOrDie()) {
					state = State.Dead;
					Debug.Log ("Current AI unit died");
					yield break;
				}

				if (!unit.IsSkillUsePossibleState()) {
					state = State.StandbyOrRest;
					yield break;
				}

				yield return battleManager.ToDoBeforeAction ();

				BattleData.selectedSkill = BattleData.selectedUnit.activeSkillList[0];
				ActiveSkill skill = BattleData.SelectedSkill;

				if (!unit.HasEnoughAPToUseSkill(skill)) {
					state = State.StandbyOrRest;
					yield break;
				}

				Tile currTile = unit.GetTileUnderUnit ();
				Casting casting = skill.GetBestAttack (unit, currTile);

				if (casting == null) {
					if (flag) {
						state = State.MoveToBestCasting;
						yield break;
					} else {
						state = State.StandbyOrRest;
						yield break;
					}
				}

				flag = true;
				yield return UseSkill(casting);
			}
		}
		public IEnumerator StandbyOrRest(){
			yield return battleManager.ToDoBeforeAction ();
			if (unit.IsStandbyPossible ()) {
				yield return Standby (unit);
			} else {
				yield return TakeRest (unit);
			}
			state = State.EndTurn;
		}

		public static IEnumerator Move(Unit unit, Tile destTile, Direction finalDirection, int totalAPCost, int tileCount){
			yield return new WaitForSeconds (0.5f);
			TileManager.Instance.DepaintAllTiles (TileColor.Blue);
			yield return BattleData.battleManager.StartCoroutine (MoveStates.MoveToTile (destTile, finalDirection, totalAPCost, tileCount));
		}
		public static IEnumerator UseSkill(Casting casting){
			yield return casting.Skill.AIUseSkill (casting);
		}
		public static IEnumerator Standby(Unit unit){
			yield return new WaitForSeconds(0.2f);
		}
		public static IEnumerator TakeRest(Unit unit){
			yield return BattleData.battleManager.StartCoroutine(RestAndRecover.Run());
		}
		IEnumerator SkipTurn(){
			unit.SetActivityPoint (unit.GetStandardAP () - 1);
			yield return new WaitForSeconds (0.05f);
			state = State.EndTurn;
		}

		static void CameraFocusToUnit(Unit unit){
			BattleManager.MoveCameraToUnit (unit);
		}

		public void CheckActiveTrigger(){
			bool satisfyActiveCondition = false;
			// 전투 시작시 활성화
			if (_AIData.activeTriggers.Contains(1)){
				satisfyActiveCondition = true;
			}
			// 일정 페이즈부터 활성화
			else if (_AIData.activeTriggers.Contains(2)){
				if (BattleData.currentPhase >= _AIData.activePhase) {
					Debug.Log (unit.GetNameKor () + " is activated because enough phase passed");
					satisfyActiveCondition = true;
				}
			}
			// 자신 주위 일정 영역에 접근하면 활성화
			else if (_AIData.activeTriggers.Contains(3)){
				// 자신을 기준으로 한 상대좌표
				List<List<Tile>> aroundTiles = _AIData.trigger3Area;
				List<Unit> aroundUnits = new List<Unit>();

				aroundTiles.ForEach(eachArea => {
					eachArea.ForEach(tile => {
						if (tile.IsUnitOnTile())
							aroundUnits.Add(tile.GetUnitOnTile());
					});
				});

				if (aroundUnits.Contains(unit))
					aroundUnits.Remove(unit);

				bool isThereAnotherSideUnit = aroundUnits.Any(anyUnit => AIUtil.IsSecondUnitEnemyToFirstUnit(unit, anyUnit));

				if (isThereAnotherSideUnit){
					Debug.Log (unit.GetNameKor () + " is activated because its enemy came to nearby");
					satisfyActiveCondition = true;
				}
			}
			// 맵 상의 특정 영역에 접근하면 활성화
			else if (_AIData.activeTriggers.Contains(4)){
				// 절대좌표
				List<List<Tile>> aroundTiles = _AIData.trigger4Area;
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

				bool isThereAnotherSideUnit = aroundUnits.Any(anyUnit => AIUtil.IsSecondUnitEnemyToFirstUnit(unit, anyUnit));

				if (isThereAnotherSideUnit)
				{
					Debug.Log (unit.GetNameKor () + " is activated because its enemy came to absolute position range");
					satisfyActiveCondition = true;
				}
			}
			// 자신을 대상으로 기술이 시전되면 활성화
			else if (_AIData.activeTriggers.Contains(5)){
				// 뭔가 기술의 영향을 받으면
				// SkillAndChainState.ApplySkill에서 체크하므로 여기선 할 일 없음
			}

			if (satisfyActiveCondition) {
				_AIData.SetActive ();
			}
		}
	}
}
