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
		public static Tile GetBestApproachWorthTile(Unit unit, ActiveSkill skill, Dictionary<Vector2, TileWithPath> movableTilesWithPath){
			Tile unitTile = unit.GetTileUnderUnit ();
			Vector2 unitPos = unit.GetPosition ();
			int minAPUse = unit.MinAPUseForStandbyForAI ();

			Dictionary<Vector2, TileWithPath> allPaths = PathFinder.CalculatePathsFromThisTileForAI (unit, unitTile, int.MaxValue, skill);
			TileWithPath bestFinalTileWithPath = new TileWithPath (unit.GetTileUnderUnit ());
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

			Tile bestTile = unit.GetTileUnderUnit ();

			if (maxFinalReward == 0)
				return bestTile;

			List<Tile> path = bestFinalTileWithPath.path;

			foreach (Tile tile in path) {
				Vector2 tilePos = tile.GetTilePos ();
				if (!movableTilesWithPath.ContainsKey (tilePos))
					break;
				if (movableTilesWithPath [tilePos].requireActivityPoint >= minAPUse) {
					bestTile = tile;
					break;
				}
			}

			return bestTile;
		}

		// 1턴 내에 이동 후 공격하는 모든 경우 중 가장 가치있는 경우를 찾아서 이동 목적지 return
		public static Tile GetBestMovableTile(Unit caster, ActiveSkill skill, Dictionary<Vector2, TileWithPath> movableTilesWithPath, int moveRestrainFactor){
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
			if (maxReward == 0)
				return null;
			else
				return bestTile;
		}
		private static Tile GetMinRequireAPTile(Dictionary<Vector2, TileWithPath> movableTilesWithPath){
			if (movableTilesWithPath.Count > 0){
				Tile nearestTile=null;
				int minRequireAP = Int32.MaxValue;
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

			if(state != State.Dead)
				battleManager.EndUnitTurn();
		}

		IEnumerator CheckUnitIsActiveAndDecideActionAndAct(){
			/*
			if (unit.GetNameInCode () == "kashasty_Escape") {
				yield return AIKashasty.DecideActionAndAct (unit);
				yield break;
			}*/

			if (!_AIData.IsActive()) {
				CheckActiveTrigger();
			}

			if (!_AIData.IsActive ()) {
				yield return battleManager.ToDoBeforeAction ();
				state = State.SkipTurn;
			} else {
				state = State.MoveToBestCasting;
			}
			yield return FSM ();
		}

		IEnumerator FSM(){
			while (true) {
				if (state == State.Dead || state == State.EndTurn)
					yield break;
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
			int selectedSkillIndex = 1;
			BattleData.indexOfSelectedSkillByUser = selectedSkillIndex;
			ActiveSkill selectedSkill = BattleData.SelectedSkill;

			Dictionary<Vector2, TileWithPath> movableTilesWithPathForPainting = PathFinder.CalculateMovablePaths(unit);

			yield return PaintMovableTiles(movableTilesWithPathForPainting);

			Dictionary<Vector2, TileWithPath> movableTilesWithPathWithDestroying = PathFinder.CalculateMovablePathsForAI(unit, selectedSkill);

			Tile destTile = AIUtil.GetBestMovableTile (unit, selectedSkill, movableTilesWithPathWithDestroying, 0);

			if (destTile != null) {
				yield return MoveWithDestroyRoutine (unit, selectedSkill, movableTilesWithPathWithDestroying, destTile);
				state = State.CastingLoop;
			} else {
				TileManager.Instance.DepaintAllTiles(TileColor.Blue);
				state = State.Approach;
				yield break;
			}
		}

		public static int obstacleDestroyCost = 0;

		public static IEnumerator DestroyObstacle(Unit unit, ActiveSkill skill, Tile obstacleTile){
			obstacleDestroyCost = 0;

			Tile currTile = unit.GetTileUnderUnit ();
			Vector2 currPos = currTile.GetTilePos ();
			Vector2 obstaclePos = obstacleTile.GetTilePos ();

			SkillLocation location;
			if (skill.GetSkillType () != SkillType.Point) {
				location = new SkillLocation (currTile, currTile, Utility.VectorToDirection (obstaclePos - currPos));
			} else {
				location = new SkillLocation (currTile, obstacleTile, Utility.GetDirectionToTarget (currPos, obstaclePos));
			}
			Casting casting = new Casting (unit, skill, location);

			while (obstacleTile.IsUnitOnTile ()) {
				yield return battleManager.ToDoBeforeAction ();
				yield return UseSkill (casting);
				obstacleDestroyCost += unit.GetActualRequireSkillAP (skill);
			}
		}

		public static IEnumerator PaintMovableTiles(Dictionary<Vector2, TileWithPath> movableTilesWithPath){
			List<Tile> movableTiles = new List<Tile>();
			foreach (KeyValuePair<Vector2, TileWithPath> movableTileWithPath in movableTilesWithPath)
				movableTiles.Add (movableTileWithPath.Value.tile);
			TileManager.Instance.PaintTiles (movableTiles, TileColor.Blue);
			yield return null;
		}

		public static IEnumerator MoveWithDestroyRoutine(Unit unit, ActiveSkill skill, Dictionary<Vector2, TileWithPath> movableTilesWithPathWithDestroying, Tile destTile){
			TileWithPath destTileWithPath = movableTilesWithPathWithDestroying [destTile.GetTilePos ()];
			List<Tile> destPath = destTileWithPath.path;
			int destRequireAP = destTileWithPath.requireActivityPoint;
			destPath.Remove (unit.GetTileUnderUnit ());

			Tile prevTile = unit.GetTileUnderUnit ();
			List<Tile> prevPath = new List<Tile> ();
			int prevRequireAP = 0;
			foreach (Tile tile in destPath) {
				if (tile.IsUnitOnTile ()) {
					TileWithPath intermediateTileWithPath = movableTilesWithPathWithDestroying [prevTile.GetTilePos ()];

					List<Tile> realPath = intermediateTileWithPath.path.Except (prevPath).ToList ();
					int realRequireAP = intermediateTileWithPath.requireActivityPoint - prevRequireAP;

					yield return MoveToTheTileAndChangeDirection (unit, prevTile, realPath, realRequireAP);

					prevPath = intermediateTileWithPath.path;
					prevRequireAP = intermediateTileWithPath.requireActivityPoint;

					yield return DestroyObstacle (unit, skill, tile);
					prevRequireAP += obstacleDestroyCost;
				}
				prevTile = tile;
			}

			List<Tile> remainPath = destPath.Except (prevPath).ToList ();
			int remainRequireAP = destRequireAP - prevRequireAP;
			yield return MoveToTheTileAndChangeDirection (unit, destTile, remainPath, remainRequireAP);
		}

		public static IEnumerator MoveToTheTileAndChangeDirection(Unit unit, Tile destTile, List<Tile> path, int requireAP){
			Vector2 destPos = destTile.GetTilePos ();

			if (path.Count > 0) {
				Direction finalDirection = Utility.GetFinalDirectionOfPath (destTile, path, unit.GetDirection ());
				yield return Move (unit, destTile, finalDirection, requireAP, path.Count+1);
			}
			else { //Count가 0이면 방향전환도 이동도 안 함
				TileManager.Instance.DepaintAllTiles(TileColor.Blue);
			}
		}

		IEnumerator Approach(){
			if (!unit.IsMovePossibleState()) {
				state = State.CastingLoop;
				yield break;
			}

			yield return battleManager.ToDoBeforeAction ();

			//이동 전에 먼저 기술부터 정해야 한다... 기술 범위에 따라 어떻게 이동할지 아니면 이동 안 할지가 달라지므로
			//나중엔 여러 기술중에 선택해야겠지만 일단 지금은 AI 기술이 모두 하나뿐이니 그냥 첫번째걸로
			int selectedSkillIndex = 1;
			BattleData.indexOfSelectedSkillByUser = selectedSkillIndex;
			ActiveSkill skill = BattleData.SelectedSkill;

			Dictionary<Vector2, TileWithPath> movableTilesWithPathForPainting = PathFinder.CalculateMovablePaths(unit);
			yield return PaintMovableTiles(movableTilesWithPathForPainting);

			Dictionary<Vector2, TileWithPath> movableTilesWithPathWithDestroying = PathFinder.CalculateMovablePathsForAI (unit, skill);

			Tile destTile = AIUtil.GetBestApproachWorthTile(unit, skill, movableTilesWithPathWithDestroying);
			TileWithPath destTileWithPath = movableTilesWithPathWithDestroying [destTile.GetTilePos ()];

			yield return MoveWithDestroyRoutine (unit, skill, movableTilesWithPathWithDestroying, destTile);
			state = State.StandbyOrRest;
		}

		IEnumerator CastingLoop(){
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

				int selectedSkillIndex = 1;
				BattleData.indexOfSelectedSkillByUser = selectedSkillIndex;
				ActiveSkill skill = BattleData.SelectedSkill;

				if (!unit.HasEnoughAPToUseSkill(skill)) {
					state = State.StandbyOrRest;
					yield break;
				}

				Tile currTile = unit.GetTileUnderUnit ();
				Casting casting = skill.GetBestAttack (unit, currTile);

				if (casting == null) {
					state = State.MoveToBestCasting;
					yield break;
				}
				
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

		void CheckActiveTrigger(){
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

	public class AIKashasty{
		private static BattleManager battleManager;
		public static IEnumerator DecideActionAndAct(Unit unit){
			battleManager = BattleData.battleManager;
			bool movable = unit.IsMovePossibleState ();
			bool skillusable = unit.IsSkillUsePossibleState ();
			if (!movable && !skillusable) {
				//do nothing
			} else if (!movable && skillusable) {
				yield return OnlyAttack (unit);
			} else if (movable && !skillusable) {
				yield return OnlyMove (unit);
			} else {
				yield return FreeState (unit);
			}
			yield return unit.GetAI().StandbyOrRest ();
		}
		private static IEnumerator OnlyAttack(Unit unit){
			while(true) {
				yield return battleManager.ToDoBeforeAction ();

				int selectedSkillIndex = 1;
				BattleData.indexOfSelectedSkillByUser = selectedSkillIndex;
				ActiveSkill skill = BattleData.SelectedSkill;

				Vector2 currPos = unit.GetPosition ();
				if(!unit.HasEnoughAPToUseSkill(skill)) {break;}

				//상하좌우에 쏠 수 있는 애가 있으면 쏜다. 우선순위는 그레네브=비앙카=달케니르 > 다른 모든 유닛(지형지물 포함)
				Vector2 casterPos = currPos;
				Tile casterTile = BattleData.tileManager.GetTile (casterPos);
				Direction? attackDirection;
				Tile targetTile;

				attackDirection = TastyTileAttackDirectionOnThatPosition (casterPos, skill);
				if (attackDirection != null) {
					targetTile = skill.GetRealTargetTileForAI (casterPos, (Direction)attackDirection);
					yield return AI.UseSkill (new Casting (unit, skill, new SkillLocation (casterTile, targetTile, (Direction)attackDirection)));
					continue;
				}

				attackDirection = DecentTileAttackDirectionOnThatPosition (casterPos, skill);
				if (attackDirection != null) {
					targetTile = skill.GetRealTargetTileForAI (casterPos, (Direction)attackDirection);
					yield return AI.UseSkill (new Casting (unit, skill, new SkillLocation (casterTile, targetTile, (Direction)attackDirection)));
					continue;
				}

				break;
			}
		}
		private static IEnumerator OnlyMove(Unit unit){
			yield return battleManager.ToDoBeforeAction ();

			int step = 0;
			int currentAP = unit.GetCurrentActivityPoint ();
			int requireAP = 0;
			Vector2 pos = unit.GetPosition ();
			Tile tile = unit.GetTileUnderUnit ();
			List<Tile> movableTiles = new List<Tile> ();

			while (currentAP - requireAP >= unit.GetStandardAP ()) {
				movableTiles.Add (tile);
				Vector2 nextPos = pos + Utility.ToVector2 (Direction.RightDown);
				Tile nextTile = BattleData.tileManager.GetTile (nextPos);
				if (nextTile == null || nextTile.IsUnitOnTile()) {
					break;
				}
				requireAP += TileWithPath.NewTileMoveCost(nextTile, tile, step, unit);
				step++;
				pos = nextPos;
				tile = nextTile;
			}

			int totalUseAP = requireAP;
			Tile destTile = tile;
			yield return AI.Move (unit, destTile, Direction.RightDown, totalUseAP, step);
		}
		private static IEnumerator FreeState(Unit unit){
			yield return TasteTastyTile (unit);
			yield return BreakAndEscape (unit);
		}
		private static IEnumerator TasteTastyTile(Unit unit){
			while (true) {
				yield return battleManager.ToDoBeforeAction ();

				int selectedSkillIndex = 1;
				BattleData.indexOfSelectedSkillByUser = selectedSkillIndex;
				ActiveSkill skill = BattleData.SelectedSkill;
				int spareableAP = 24 + unit.GetActualRequireSkillAP (skill) * 2;

				Vector2 currPos = unit.GetPosition ();
				Dictionary<Vector2, TileWithPath> movableTilesWithPath = PathFinder.CalculateMovablePaths(unit);
				yield return AI.PaintMovableTiles(movableTilesWithPath);
				
				Tile destTile = AIUtil.GetBestMovableTile (unit, skill, movableTilesWithPath, unit.GetCurrentActivityPoint () - spareableAP);

				if (destTile == null || TastyTileAttackDirectionOnThatPosition(destTile.GetTilePos(), skill) == null)
					yield break;

				List<Tile> path = movableTilesWithPath [destTile.GetTilePos ()].path;
				int moveAP = movableTilesWithPath [destTile.GetTilePos ()].requireActivityPoint;
				yield return AI.MoveToTheTileAndChangeDirection (unit, destTile, path, moveAP);

				while (true) {
					if (!unit.HasEnoughAPToUseSkill (skill))
						yield break;
					yield return battleManager.ToDoBeforeAction ();
					Tile currTile = unit.GetTileUnderUnit ();
					if (TastyTileAttackDirectionOnThatPosition (currTile.GetTilePos (), skill) == null) {
						break;
					}
					Casting casting = skill.GetBestAttack (unit, currTile);
					yield return AI.UseSkill (casting);
				}
			}
		}
		private static IEnumerator BreakAndEscape(Unit unit){
			while (true) {
				yield return battleManager.ToDoBeforeAction ();

				int currentAP = unit.GetCurrentActivityPoint ();
				Vector2 currPos = unit.GetPosition ();
				int selectedSkillIndex = 1;
				BattleData.indexOfSelectedSkillByUser = selectedSkillIndex;
				ActiveSkill skill = BattleData.SelectedSkill;

				Tile barrierTile = skill.GetRealTargetTileForAI (currPos, Direction.RightDown);

				if (barrierTile == null) {
					yield return OnlyMove (unit);
					break;
				} else {
					if (!unit.HasEnoughAPToUseSkill (skill))
						break;
					yield return AI.UseSkill (new Casting(unit,  skill, new SkillLocation(currPos, barrierTile, Direction.RightDown)));
				}
			}
		}
		private static Direction? TastyTileAttackDirectionOnThatPosition(Vector2 casterPos, ActiveSkill skill){
			Tile casterTile = BattleData.tileManager.GetTile (casterPos);
			Direction? attackDirection = null;

			foreach(Direction direction in EnumUtil.directions){
				Tile targetTile = skill.GetRealTargetTileForAI (casterPos, direction);
				if (IsTastyTile (targetTile)) {
					attackDirection = direction;
					break;
				}
			}
			return attackDirection;
		}
		private static Direction? DecentTileAttackDirectionOnThatPosition(Vector2 casterPos, ActiveSkill skill){
			Tile casterTile = BattleData.tileManager.GetTile (casterPos);
			Direction? attackDirection = null;

			foreach(Direction direction in EnumUtil.directions){
				Tile targetTile = skill.GetRealTargetTileForAI (casterPos, direction);
				if (IsDecentTile (targetTile)) {
					attackDirection = direction;
					break;
				}
			}
			return attackDirection;
		}
		private static bool IsUnitOnThatTileTastyPC(Tile tile){
			string unitCodeName = tile.GetUnitOnTile ().GetNameInCode ();
			return unitCodeName == "grenev" || unitCodeName == "darkenir" || unitCodeName == "bianca";
		}
		private static bool IsTastyTile(Tile tile){
			return tile != null && tile.IsUnitOnTile () && IsUnitOnThatTileTastyPC (tile);
		}
		private static bool IsDecentTile(Tile tile){
			return tile != null && tile.IsUnitOnTile ();
		}
	}
}
