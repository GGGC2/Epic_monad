using UnityEngine;
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

		// 1턴 내에 이동 후 공격 불가하거나 그럴 만한 가치가 없을 때 가장 가치있는 적을 향해 움직인다
		// 가치에는 거리도 적용되므로 너무 멀면 그쪽으로 가진 않는다
		public static Tile GetBestApproachWorthTile(Unit unit, List<ActiveSkill> skills, List<Vector2> goalArea){
			Tile unitTile = unit.GetTileUnderUnit ();
			Vector2 unitPos = unit.GetPosition ();
			int minAPUse = unit.MinAPUseForStandbyForAI ();


			TileWithPath bestFinalTileWithPath = new TileWithPath (unit.GetTileUnderUnit ());
			Tile bestTile = unit.GetTileUnderUnit ();

			if (goalArea.Count == 0) {
				float maxFinalReward = 0;
				Unit caster = unit;

				foreach (ActiveSkill skill in skills) {
					Dictionary<Vector2, TileWithPath> allPaths = PathFinder.CalculatePathsFromThisTileForAI (unit, unitTile, int.MaxValue, skill);

					foreach (var pair in allPaths) {
						TileWithPath tileWithPath = pair.Value;
						Tile tile = tileWithPath.tile;
						int requireAP = pair.Value.requireActivityPoint;

						Casting bestCastingOnThisTile = skill.GetBestCasting (caster, tile);

						if (bestCastingOnThisTile != null) {
							float singleCastingReward = skill.GetRewardByCasting (bestCastingOnThisTile);
							float reward = singleCastingReward / (float)Math.Sqrt (requireAP);

							if (reward > maxFinalReward) {
								maxFinalReward = reward;
								bestFinalTileWithPath = tileWithPath;
								AI.selectedSkill = skill;
							}
						}
					}
				}

				if (maxFinalReward == 0) {
					return bestTile;
				}

			} else {
				Debug.Log ("For goal");

				foreach (ActiveSkill skill in skills) {
					Dictionary<Vector2, TileWithPath> allGoalPaths = new Dictionary<Vector2, TileWithPath> ();
					Dictionary<Vector2, TileWithPath> allPaths = PathFinder.CalculatePathsFromThisTileForAI (unit, unitTile, int.MaxValue, skill);
					foreach (var pair in allPaths) {
						TileWithPath tileWithPath = pair.Value;
						Vector2 tilePos = tileWithPath.tile.GetTilePos ();
						if (goalArea.Contains (tilePos)) {
							allGoalPaths [pair.Key] = pair.Value;
						}
					}

					TileWithPath bestFinalTileWithPathForThisSkill = GetMinRequireAPTileWithPath (allGoalPaths);
					if (bestFinalTileWithPath.requireActivityPoint == 0 || bestFinalTileWithPathForThisSkill.requireActivityPoint < bestFinalTileWithPath.requireActivityPoint) {
						bestFinalTileWithPath = bestFinalTileWithPathForThisSkill;
						AI.selectedSkill = skill;
					}

					if(unit.GetCurrentActivityPoint() >= bestFinalTileWithPath.requireActivityPoint){
						bestTile=bestFinalTileWithPath.tile;
						return bestTile;
					}
				}
			}

			List<Tile> path = bestFinalTileWithPath.path;
			path.Add (bestFinalTileWithPath.tile);

			Dictionary<Vector2, TileWithPath> movableTilesWithPath = PathFinder.CalculateMovablePathsForAI(unit, AI.selectedSkill);

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
		public static Tile GetBestMovableTile(Unit caster, List<ActiveSkill> skills, int moveRestrainFactor, int minWorthReward){
			Tile bestTile = caster.GetTileUnderUnit ();
			int currAP = caster.GetCurrentActivityPoint ();
			float maxReward = 0;

			foreach (ActiveSkill skill in skills) {
				Dictionary<Vector2, TileWithPath> movableTilesWithPath = PathFinder.CalculateMovablePathsForAI(caster, skill);

				int skillRequireAP = caster.GetActualRequireSkillAP (skill);

				foreach (var pair in movableTilesWithPath) {
					Tile tile = pair.Value.tile;
					int requireAP = pair.Value.requireActivityPoint;
					int actualRemainAP = currAP - requireAP - moveRestrainFactor;
					int possibleSkillUseCount = actualRemainAP / skillRequireAP;

					if (possibleSkillUseCount <= 0)
						continue;

					float reward = 0;
					Casting bestCastingOnThisTile = skill.GetBestCasting (caster, tile);
					if (bestCastingOnThisTile != null) {
						float singleCastingReward = skill.GetRewardByCasting (bestCastingOnThisTile);
						reward = singleCastingReward * possibleSkillUseCount;

						if (reward > maxReward) {
							maxReward = reward;
							bestTile = tile;
							AI.selectedSkill = skill;
						}
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
		public static ActiveSkill selectedSkill; // 미리 선택한 스킬 기억용
		public void Initialize(Unit unit, AIData _AIData){
			this.unit = unit;
			this._AIData = _AIData;
		}

		enum State{ Dead, TurnStart, SkipTurn, EndTurn, MoveToBestCasting, CastingLoop, Approach, StandbyOrRest, Triana_Rest, ChildHolder, Burglar, Child }

		State state;

		public IEnumerator UnitTurn() {
            TutorialManager.Instance.RemoveSpriteAndMark();
            if (!_AIData.IsActive ()) {
				yield return Activate ();
			}
			if (!_AIData.IsActive ()) {
				ReduceAPToSkipTurn ();
				yield break;
			}

            LogManager.Instance.Record(new TurnStartLog(unit));
            yield return battleManager.StartUnitTurn (unit);

			state = State.TurnStart;

			if (BattleData.onTutorial) {
				while (state != State.EndTurn) {
					yield return ActByScenario (TutorialManager.Instance.GetNextAIScenario ());
				}
			} else {
				yield return DecideActionAndAct ();
			}

			selectedSkill = null;

			if (BattleData.currentState != CurrentState.Destroyed) {
				yield return battleManager.EndUnitTurn (unit);
			}
		}

		IEnumerator Activate(){
			CheckActiveTrigger();
			yield return LogManager.Instance.ExecuteLastEventLogAndConsequences();
		}

		IEnumerator DecideActionAndAct(){
			if (unit.GetNameEng ().Equals ("triana")) {
				state = State.StandbyOrRest;
			} else if (unit.GetNameEng ().Equals ("triana_Rest")) {
				state = State.Triana_Rest;
			} else if (unit.GetNameEng ().Equals ("child")) {
				state = State.Child;
			} else if (unit.GetNameEng ().Equals ("childHolder")) {
				state = State.ChildHolder;
			} else {
				state = State.MoveToBestCasting;
			}
			yield return FSM ();
		}

		IEnumerator FSM(){
			while (true) {
                yield return BattleManager.SlideCameraToPosition(unit.transform.position);
                if (state == State.Dead || state == State.EndTurn) {
					yield break;
				}
				Debug.Log("AI state : " + state.ToString ());
				yield return StartCoroutine (state.ToString ());
            }
		}

		IEnumerator MoveToBestCasting(){
			if (!unit.IsSkillUsePossibleState()) {
				state = State.Approach;
				yield break;
			}
			if (!unit.IsMovePossibleState()) {
				state = State.CastingLoop;
				yield break;
			}

			yield return battleManager.ToDoBeforeAction ();

			int minRewardWorthAttack = 0;
			if (unit.GetNameEng () == "kashasty_Escape") {
				minRewardWorthAttack = 400;
			}

			Tile destTile = AIUtil.GetBestMovableTile (unit, unit.GetSkillList(), 0, minRewardWorthAttack);

			if (destTile != null) {
				yield return MoveWithDestroyRoutine (destTile);
				state = State.CastingLoop;
			} else {
				state = State.Approach;
			}
		}

		IEnumerator MoveWithDestroyRoutine(Tile destTile){
			ActiveSkill skill = AI.selectedSkill;
			Tile startTile = unit.GetTileUnderUnit ();
			Dictionary<Vector2, TileWithPath> movableTilesWithPathWithDestroying = PathFinder.CalculateMovablePathsForAI (unit, skill);

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
			foreach (Tile tile in mustCheckEmptinessTiles) {
				if (tile.IsUnitOnTile ()) {
					yield return MoveToThePositionAndChangeDirection (prevTile.GetTilePos ());
					yield return DestroyObstacle (skill, tile);
				}
				prevTile = tile;
			}
			yield return MoveToThePositionAndChangeDirection (destTile.GetTilePos ());
		}

		IEnumerator DestroyObstacle(ActiveSkill skill, Tile obstacleTile){
			Tile currTile = unit.GetTileUnderUnit ();
			Vector2 currPos = currTile.GetTilePos ();
			Vector2 obstaclePos = obstacleTile.GetTilePos ();

			SkillLocation location;
			if (skill.GetSkillType () == SkillType.Route) {
				location = new SkillLocation (currTile, obstacleTile, Utility.VectorToDirection (obstaclePos - currPos));
			} else if (skill.GetSkillType () == SkillType.Point) {
				location = new SkillLocation (currTile, obstacleTile, Utility.GetDirectionToTarget (currPos, obstaclePos));
			} else {
				location = new SkillLocation (currTile, currTile, Utility.VectorToDirection (obstaclePos - currPos));
			}
			Casting casting = new Casting (unit, skill, location);

			while (obstacleTile.IsUnitOnTile ()) {
				Debug.Log ("destroying obstacle on the tile "+obstaclePos.x+" : "+obstaclePos.y);
				yield return battleManager.ToDoBeforeAction ();
				yield return UseSkill (casting);
			}
		}

		public IEnumerator MoveToThePositionAndChangeDirection(Vector2 destPos){
			Dictionary<Vector2, TileWithPath> movableTilesWithPath = PathFinder.CalculateMovablePaths(unit);
			if (movableTilesWithPath[destPos].path.Count > 0) {
				yield return Move (destPos, movableTilesWithPath);
			}
		}

		void PaintMovableTiles(){
			Dictionary<Vector2, TileWithPath> movableTilesWithPath = PathFinder.CalculateMovablePaths(unit);
			List<Tile> movableTiles = new List<Tile>();
			foreach (KeyValuePair<Vector2, TileWithPath> movableTileWithPath in movableTilesWithPath) {
				movableTiles.Add (movableTileWithPath.Value.tile);
			}
			LogManager.Instance.Record (new PaintTilesLog (movableTiles, TileColor.Blue));
		}

		IEnumerator Approach(){
			if (!unit.IsMovePossibleState()) {
				state = State.CastingLoop;
				yield break;
			}

			yield return battleManager.ToDoBeforeAction ();

			Tile destTile = AIUtil.GetBestApproachWorthTile (unit, unit.GetSkillList(), _AIData.goalArea);

			yield return MoveWithDestroyRoutine (destTile);
			state = State.StandbyOrRest;
			if (BattleData.selectedUnit.CheckReach ()) {
				state = State.Dead;
				BattleData.currentState = CurrentState.Destroyed;
			}
		}

		IEnumerator CastingLoop(){
			Tile currTile = unit.GetTileUnderUnit ();
			Casting casting = unit.GetBestCasting (currTile);

			if (casting == null) {
				if (!unit.IsMovePossibleState() || unit.GetNameEng() == "childHolder") {
					state = State.StandbyOrRest;
					yield break;
				} else {
					state = State.MoveToBestCasting;
					yield break;
				}
			}

			yield return battleManager.ToDoBeforeAction ();
			yield return UseSkill(casting);

			if (BattleManager.IsSelectedUnitRetreatOrDie ()) {
				state = State.Dead;
				BattleData.currentState = CurrentState.Destroyed;
				Debug.Log ("Current AI unit died");
			} else {
				state = State.MoveToBestCasting;
			}
		}

		IEnumerator StandbyOrRest(){
			yield return battleManager.ToDoBeforeAction ();
			if (unit.IsStandbyPossible ()) {
				yield return Standby ();
			} else {
			    yield return TakeRest ();
			}
			state = State.EndTurn;
		}

		IEnumerator Triana_Rest(){
			if (2 * unit.GetCurrentHealth () > unit.GetMaxHealth ()) {
				Dictionary<Vector2, TileWithPath> movableTilesWithPath = PathFinder.CalculateMovablePaths (unit);
				Tile tileNearYeong = Utility.GetNearestTileToUnit (BattleData.tileManager.GetAllTiles ().Keys.ToList(), movableTilesWithPath, "yeong");
				if (tileNearYeong != null) {
					yield return MoveToThePositionAndChangeDirection (tileNearYeong.GetTilePos ());
				}
				state = State.StandbyOrRest;
			} else {
				ActiveSkill skill1 = unit.GetSkillList () [0]; // 광폭화
				if (unit.IsThisSkillUsable (skill1)) {
					Casting casting = new Casting (unit, skill1, new SkillLocation (unit.GetTileUnderUnit (), unit.GetTileUnderUnit (), unit.GetDirection ()));
					yield return UseSkill (casting);
				} else {
					state = State.MoveToBestCasting;
				}
			}
			yield return null;
		}

		IEnumerator ChildHolder(){
			Vector2 holderPos = unit.GetPosition ();
			Unit nearChild = null;
			foreach(Direction direction in EnumUtil.directions){
				Tile tileNearHolder = BattleData.tileManager.GetTile (holderPos + Utility.ToVector2 (direction));
				if (tileNearHolder != null) {
					if (tileNearHolder.IsUnitOnTile ()) {
						Unit nearUnit = tileNearHolder.GetUnitOnTile ();
						if (nearUnit.GetNameEng () == "child") {
							nearChild = nearUnit;
						}
					}
				}
			}
			if (nearChild == null) {
				state = State.MoveToBestCasting;
			} else {
				
				Unit eren = BattleData.unitManager.GetAnUnit ("eren");

				if (eren != null) {
					Vector2 childPos = nearChild.GetPosition ();
					Dictionary<Vector2, TileWithPath> movableTilesWithPath = PathFinder.CalculateMovablePaths(unit);

					List<Vector2> range = new List<Vector2> ();
					foreach (Direction direction in EnumUtil.directions) {
						range.Add (childPos + Utility.ToVector2 (direction));
					}

					Tile destTile = Utility.GetFarthestTileToUnit (range, movableTilesWithPath, "eren");

					if (destTile != null) {
						yield return MoveToThePositionAndChangeDirection (destTile.GetTilePos ());
					}
					state = State.CastingLoop;

				}

			}
			yield return null;
		}

		IEnumerator Child(){
			Vector2 childPos = unit.GetPosition ();
			Unit nearEnemy = null;
			foreach(Direction direction in EnumUtil.directions){
				Tile tileNearChild = BattleData.tileManager.GetTile (childPos + Utility.ToVector2 (direction));
				if (tileNearChild != null) {
					if (tileNearChild.IsUnitOnTile ()) {
						Unit nearUnit = tileNearChild.GetUnitOnTile ();
						if (nearUnit.GetSide()==Side.Enemy) {
							nearEnemy = nearUnit;
						}
					}
				}
			}
			if (nearEnemy == null) {
				state = State.Approach;
			} else {
				state = State.StandbyOrRest;
			}
			yield return null;
		}

		IEnumerator Move(Vector2 destPos, Dictionary<Vector2, TileWithPath> path) {
            LogManager.Instance.Record(new MoveLog(unit, unit.GetTileUnderUnit().GetTilePos(), destPos));
            if (Configuration.NPCBehaviourDuration >= 0.05f) {
                PaintMovableTiles();
                LogManager.Instance.Record(new WaitForSecondsLog(Configuration.NPCBehaviourDuration));
                LogManager.Instance.Record(new DepaintTilesLog(TileColor.Blue));
            }
			MoveStates.MoveToTile (destPos, path);
            yield return LogManager.Instance.ExecuteLastEventLogAndConsequences();
        }
		IEnumerator UseSkill(Casting casting){
			yield return casting.Skill.AIUseSkill (casting);
        }
		IEnumerator Standby(){
			yield return null;
		}
		IEnumerator TakeRest() {
            LogManager.Instance.Record(new RestLog(unit));
			RestAndRecover.Run();
            yield return LogManager.Instance.ExecuteLastEventLogAndConsequences();
        }
		IEnumerator SkipTurn(){
			ReduceAPToSkipTurn ();
			state = State.EndTurn;
			yield return null;
		}
		void ReduceAPToSkipTurn(){
			unit.SetActivityPoint (unit.GetStandardAP () - 1);
		}

		IEnumerator ActByScenario(AIScenario scenario){
			if (scenario.act == AIScenario.ScenarioAct.UseSkill) {
				SkillLocation location = new SkillLocation (unit.GetPosition (), unit.GetPosition () + scenario.relativeTargetPos, scenario.direction);
				Casting casting = new Casting (unit, unit.GetSkillList () [scenario.skillIndex], location);
				yield return StartCoroutine (scenario.functionName, casting);
			} else if (scenario.act==AIScenario.ScenarioAct.Move) {
				yield return StartCoroutine (scenario.functionName, unit.GetPosition () + scenario.relativeTargetPos);
			} else {
				yield return StartCoroutine (scenario.functionName);
			}
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

			if (satisfyActiveCondition)
				_AIData.SetActive ();
		}
	}
}
