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
		public static List<Tile> FindEnemyTilesInTheseTiles(List<Tile> tiles, Unit mainUnit){
			List<Tile> tilesHaveEnemy = (from tile in tiles
				where tile.GetUnitOnTile() != null
				let unit = tile.GetUnitOnTile()
				where IsSecondUnitEnemyToFirstUnit(mainUnit, unit)
				select tile).ToList();
			return tilesHaveEnemy;
		}
		public static Vector2 FindNearestEnemy(List<Tile> movableTiles, List<Unit> units, Unit mainUnit){
			var positions = from tile in movableTiles
				from anyUnit in units
					where IsSecondUnitEnemyToFirstUnit(mainUnit, anyUnit)
				let distance = Vector2.Distance(tile.GetTilePos(), anyUnit.GetPosition())
				orderby distance
				select tile.GetTilePos();

			List<Vector2> availablePositions = positions.ToList();
			if (availablePositions.Count > 0)
				return availablePositions[0];

			return mainUnit.GetPosition();
		}
		public static Tile FindNearestEnemyAttackableTile(Unit caster, ActiveSkill skill, Dictionary<Vector2, TileWithPath> movableTilesWithPath){
			Dictionary<Vector2, TileWithPath> enemyAttackableTilesWithPath = new Dictionary<Vector2, TileWithPath> ();
			foreach (var pair in movableTilesWithPath) {
				Tile movedTile = pair.Value.tile;
				bool attackAble = skill.IsAttackableOnTheTile (caster, movedTile);
				if (attackAble)
					enemyAttackableTilesWithPath [pair.Key] = pair.Value;
			}
			Tile nearestTile = GetMinRequireAPTile (enemyAttackableTilesWithPath);
			return nearestTile;
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

			yield return CheckUnitIsActiveAndDecideActionAndAct(unit);

			battleManager.EndUnitTurn();
		}

		private static IEnumerator CheckUnitIsActiveAndDecideActionAndAct(Unit unit){
			AIData unitAIData = unit.GetComponent<AIData>();

			if (!unitAIData.IsActive())
				CheckActiveTrigger(unit);

			if (!unitAIData.IsActive ()) {
				yield return battleManager.BeforeActCommonAct ();
				Debug.Log (unit.GetName () + " skips turn because of being deactivated");
				yield return SkipDeactivatedUnitTurn(unit);
				yield break;
			}
			if (unit.GetNameInCode () == "kashasty_Escape") {
				yield return AIKashasty.DecideActionAndAct (battleData, unit);
				yield break;
			}
			yield return DecideActionAndAct (unit);
		}

		private static IEnumerator DecideActionAndAct(Unit unit){
			//이동->기술->대기/휴식의 순서로 이동이나 기술사용은 안 할 수도 있다
			if(unit.IsMovePossibleState(battleData))
				yield return DecideMoveAndMove (unit);
			if (unit.IsSkillUsePossibleState (battleData))
				yield return DecideSkillTargetAndUseSkill (unit);
			yield return DecideRestOrStandbyAndDoThat (unit);
		}

		private static IEnumerator DecideMoveAndMove(Unit unit){
			yield return battleManager.BeforeActCommonAct ();

			Tile currentTile = unit.GetTileUnderUnit ();

			//이동 전에 먼저 기술부터 정해야 한다... 기술 범위에 따라 어떻게 이동할지 아니면 이동 안 할지가 달라지므로
			//나중엔 여러 기술중에 선택해야겠지만 일단 지금은 AI 기술이 모두 하나뿐이니 그냥 첫번째걸로
			int selectedSkillIndex = 1;
			battleData.indexOfSelectedSkillByUser = selectedSkillIndex;
			ActiveSkill selectedSkill = battleData.SelectedSkill;

			bool attackAble = selectedSkill.IsAttackableOnTheTile (unit, currentTile);

			//곧바로 공격 가능하면 이동하지 않는다
			if (attackAble) {
				yield break;
			}

			Dictionary<Vector2, TileWithPath> movableTilesWithPath = PathFinder.CalculatePath(unit);
			List<Tile> movableTiles = new List<Tile>();
			foreach (KeyValuePair<Vector2, TileWithPath> movableTileWithPath in movableTilesWithPath)
			{
				movableTiles.Add(movableTileWithPath.Value.tile);
			}

			Tile destTile=AIUtil.FindNearestEnemyAttackableTile (unit, selectedSkill, movableTilesWithPath);
			//destTile이 null이 아니면 적을 공격 가능한 타일 중 도달경로의 총 AP 소모량이 가장 적은 타일이 들어있는 것이다
			//destTile이 null : 이번 턴에 적을 공격 가능한 범위로 못 가는 경우. 그냥 가장 가까운 적을 향해 이동한다
			if (destTile == null) {
				Vector2 destPosition = AIUtil.FindNearestEnemy (movableTiles, battleData.unitManager.GetAllUnits (), unit);
				destTile = battleData.tileManager.GetTile(destPosition);
			}

			yield return MoveToTheTileAndChangeDirection (unit, destTile, movableTilesWithPath, movableTiles);

		}
		private static IEnumerator MoveToTheTileAndChangeDirection(Unit unit, Tile destTile, Dictionary<Vector2, TileWithPath> movableTilesWithPath, List<Tile> movableTiles){
			Vector2 destPos = destTile.GetTilePos ();
			TileWithPath pathToDestTile = movableTilesWithPath[destPos];

			//Count가 0이면 방향전환도 이동도 안 함
			if (pathToDestTile.path.Count > 0) {
				Tile prevLastTile = pathToDestTile.path.Last ();
				Vector2 prevLastTilePosition = prevLastTile.GetTilePos ();
				int totalUseAP = movableTilesWithPath [destPos].requireActivityPoint;

				// 이동했을때 볼 방향 설정
				Direction finalDirection;
				Vector2 delta = destPos - prevLastTilePosition;
				if (delta == new Vector2 (1, 0))
					finalDirection = Direction.RightDown;
				else if (delta == new Vector2 (-1, 0))
					finalDirection = Direction.LeftUp;
				else if (delta == new Vector2 (0, 1))
					finalDirection = Direction.RightUp;
				else // delta == new Vector2 (0, -1)
					finalDirection = Direction.LeftDown;

				yield return Move(unit, destTile, finalDirection, totalUseAP, movableTiles);
			}
		}
		private static IEnumerator DecideSkillTargetAndUseSkill(Unit unit){
			while (true) {
				yield return battleManager.BeforeActCommonAct ();

				if (unit == null)
					break;

				int selectedSkillIndex = 1;
				battleData.indexOfSelectedSkillByUser = selectedSkillIndex;
				ActiveSkill skill = battleData.SelectedSkill;
				SkillType skillType = skill.GetSkillType ();

				int currentAP = unit.GetCurrentActivityPoint ();
				int requireAP = unit.GetActualRequireSkillAP(skill);
				bool enoughAP = currentAP >= requireAP;
				if (!enoughAP) {
					yield break;
				}

				Tile currTile = unit.GetTileUnderUnit ();
				bool attackAble = skill.IsAttackableOnTheTile (unit, currTile);
				if (!attackAble) {
					yield break;
				}
				Casting casting = skill.GetBestAttack (unit, currTile);
				yield return UseSkill (casting);
			}
		}
		public static IEnumerator DecideRestOrStandbyAndDoThat(Unit unit){
			yield return battleManager.BeforeActCommonAct ();
			if (unit.IsStandbyPossible (battleData) && unit.GetCurrentActivityPoint() < unit.GetStandardAP ()) {
				yield return Standby (unit);
			}
			else {
				yield return TakeRest (unit);
			}
		}

		public static IEnumerator Move(Unit unit, Tile destTile, Direction finalDirection, int totalAPCost, List<Tile> movableTiles){
			unit.SetDirection (finalDirection);
			CameraFocusToUnit(unit);
			TileManager.Instance.PaintTiles (movableTiles, TileColor.Blue);
			yield return new WaitForSeconds (0.3f);
			TileManager.Instance.DepaintAllTiles (TileColor.Blue);
			yield return battleData.battleManager.StartCoroutine (MoveStates.MoveToTile (battleData, destTile, finalDirection, totalAPCost));
		}
		public static IEnumerator UseSkill(Casting casting){
			yield return casting.Skill.AIUseSkill (casting);
		}
		public static IEnumerator Standby(Unit unit){
			yield return new WaitForSeconds(0.2f);
		}
		public static IEnumerator TakeRest(Unit unit){
			yield return battleData.battleManager.StartCoroutine(RestAndRecover.Run(battleData));
		}
		private static IEnumerator SkipDeactivatedUnitTurn(Unit unit){
			unit.SetActivityPoint (unit.GetStandardAP () - 1);
			yield return new WaitForSeconds (0.05f);
		}

		private static void CameraFocusToUnit(Unit unit){
			BattleManager.MoveCameraToUnit (unit);
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

				bool isThereAnotherSideUnit = aroundUnits.Any(anyUnit => AIUtil.IsSecondUnitEnemyToFirstUnit(unit, anyUnit));

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

				bool isThereAnotherSideUnit = aroundUnits.Any(anyUnit => AIUtil.IsSecondUnitEnemyToFirstUnit(unit, anyUnit));

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

	public class AIKashasty{
		private static BattleData battleData;
		private static BattleManager battleManager;
		public static IEnumerator DecideActionAndAct(BattleData battleDataInstance, Unit unit){
			battleData = battleDataInstance;
			battleManager = battleData.battleManager;
			bool moveable = unit.IsMovePossibleState (battleData);
			bool skilluseable = unit.IsSkillUsePossibleState (battleData);
			if (!moveable && !skilluseable) {
				//do nothing
			}
			else if (!moveable && skilluseable) {
				yield return OnlyAttack (unit);
			}
			else if (moveable && !skilluseable) {
				yield return OnlyMove (unit);
			}
			else {
				yield return FreeState (unit);
			}
			yield return AI.DecideRestOrStandbyAndDoThat (unit);
		}
		private static IEnumerator OnlyAttack(Unit unit){
			while (true) {
				yield return battleManager.BeforeActCommonAct ();

				int selectedSkillIndex = 1;
				battleData.indexOfSelectedSkillByUser = selectedSkillIndex;
				ActiveSkill skill = battleData.SelectedSkill;

				Vector2 currPos = unit.GetPosition ();
				int currentAP = unit.GetCurrentActivityPoint ();

				//AP 부족하면 할 거 없으니 끝내고
				if (currentAP < unit.GetActualRequireSkillAP(skill))
					break;

				//상하좌우에 쏠 수 있는 애가 있으면 쏜다. 우선순위는 그레네브=비앙카=달케니르 > 다른 모든 유닛(지형지물 포함)

				//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
				//FIXME : 수정 계획...

				Vector2 casterPos = currPos;
				Tile casterTile = battleData.tileManager.GetTile (casterPos);
				Direction direction;
				Tile targetTile;

				direction = Direction.RightDown;
				targetTile = skill.GetRealTargetTileForAI (casterPos, direction);
				if (IsTastyTile (targetTile)) {
					yield return AI.UseSkill (new Casting(unit, skill, new SkillLocation (casterTile, targetTile, direction)));
					continue;
				}
				direction = Direction.LeftUp;
				targetTile = skill.GetRealTargetTileForAI (casterPos, direction);
				if (IsTastyTile (targetTile)) {
					yield return AI.UseSkill (new Casting(unit, skill, new SkillLocation (casterTile, targetTile, direction)));
					continue;
				}
				direction = Direction.RightUp;
				targetTile = skill.GetRealTargetTileForAI (casterPos, direction);
				if (IsTastyTile (targetTile)) {
					yield return AI.UseSkill (new Casting(unit, skill, new SkillLocation (casterTile, targetTile, direction)));
					continue;
				}
				direction = Direction.LeftDown;
				targetTile = skill.GetRealTargetTileForAI (casterPos, direction);
				if (IsTastyTile (targetTile)) {
					yield return AI.UseSkill (new Casting(unit, skill, new SkillLocation (casterTile, targetTile, direction)));
					continue;
				}

				direction = Direction.RightDown;
				targetTile = skill.GetRealTargetTileForAI (casterPos, direction);
				if (IsDecentTile (targetTile)) {
					yield return AI.UseSkill (new Casting(unit, skill, new SkillLocation (casterTile, targetTile, direction)));
					continue;
				}
				direction = Direction.LeftUp;
				targetTile = skill.GetRealTargetTileForAI (casterPos, direction);
				if (IsDecentTile (targetTile)) {
					yield return AI.UseSkill (new Casting(unit, skill, new SkillLocation (casterTile, targetTile, direction)));
					continue;
				}
				direction = Direction.RightUp;
				targetTile = skill.GetRealTargetTileForAI (casterPos, direction);
				if (IsDecentTile (targetTile)) {
					yield return AI.UseSkill (new Casting(unit, skill, new SkillLocation (casterTile, targetTile, direction)));
					continue;
				}
				direction = Direction.LeftDown;
				targetTile = skill.GetRealTargetTileForAI (casterPos, direction);
				if (IsDecentTile (targetTile)) {
					yield return AI.UseSkill (new Casting(unit, skill, new SkillLocation (casterTile, targetTile, direction)));
					continue;
				}

				//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

				yield break;
			}
		}
		private static IEnumerator OnlyMove(Unit unit){
			yield return battleManager.BeforeActCommonAct ();

			int step = 0;
			int currentAP = unit.GetCurrentActivityPoint ();
			int requireAP = 0;
			Vector2 pos = unit.GetPosition ();
			Tile tile = unit.GetTileUnderUnit ();
			List<Tile> movableTiles = new List<Tile> ();

			while (currentAP - requireAP >= unit.GetStandardAP ()) {
				movableTiles.Add (tile);
				Vector2 nextPos = pos + Utility.ToVector2 (Direction.RightDown);
				Tile nextTile = battleData.tileManager.GetTile (nextPos);
				if (nextTile == null || nextTile.IsUnitOnTile()) {
					break;
				}
				pos = nextPos;
				tile = nextTile;
				step++;
				//FIXME : 아래 AP 소모량 구하는 줄은 임시로 넣은 거고 나중에 고쳐야 됨 ㅋㅋ
				requireAP += 3 + 2 * (step - 1);
				Debug.Log (step);
				Debug.Log (requireAP);
			}

			int totalUseAP = requireAP;
			Tile destTile = tile;
			yield return AI.Move (unit, destTile, Direction.RightDown, totalUseAP, movableTiles);
		}
		private static IEnumerator FreeState(Unit unit){
			yield return TasteTastyTile (unit);
			yield return BreakAndEscape (unit);
		}
		private static IEnumerator TasteTastyTile(Unit unit){
			while (true) {
				yield return battleManager.BeforeActCommonAct ();

				int selectedSkillIndex = 1;
				battleData.indexOfSelectedSkillByUser = selectedSkillIndex;
				ActiveSkill skill = battleData.SelectedSkill;

				Vector2 currPos = unit.GetPosition ();
				int currentAP = unit.GetCurrentActivityPoint ();

				//AP 부족하면 할 거 없으니 끝내고
				if (currentAP < unit.GetActualRequireSkillAP(skill))
					break;

				//현 타일에서 그레/비앙/달케 공격 가능할 시 공격
				Vector2 casterPos = currPos;
				Tile casterTile = battleData.tileManager.GetTile (casterPos);
				Direction direction;
				Tile targetTile;

				direction = Direction.RightDown;
				targetTile = skill.GetRealTargetTileForAI (casterPos, direction);
				if (IsTastyTile (targetTile)) {
					yield return AI.UseSkill (new Casting(unit, skill, new SkillLocation (casterTile, targetTile, direction)));
					continue;
				}
				direction = Direction.LeftUp;
				targetTile = skill.GetRealTargetTileForAI (casterPos, direction);
				if (IsTastyTile (targetTile)) {
					yield return AI.UseSkill (new Casting(unit, skill, new SkillLocation (casterTile, targetTile, direction)));
					continue;
				}
				direction = Direction.RightUp;
				targetTile = skill.GetRealTargetTileForAI (casterPos, direction);
				if (IsTastyTile (targetTile)) {
					yield return AI.UseSkill (new Casting(unit, skill, new SkillLocation (casterTile, targetTile, direction)));
					continue;
				}
				direction = Direction.LeftDown;
				targetTile = skill.GetRealTargetTileForAI (casterPos, direction);
				if (IsTastyTile (targetTile)) {
					yield return AI.UseSkill (new Casting(unit, skill, new SkillLocation (casterTile, targetTile, direction)));
					continue;
				}

				//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
				//FIXME : 나중에 코드 제대로 수정할 계획
				/*
				if (currentAP < 3 + unit.GetActualRequireSkillAP(skill))
					break;

				//좌 타일로 한칸 움직이는 경우도 확인
				casterPos = currPos + Utility.ToVector2(Direction.RightUp);
				Tile movedTile = battleData.tileManager.GetTile (casterPos);
				if (!movedTile.IsUnitOnTile ()) {
					direction = Direction.RightDown;
					rightDownTile = skill.GetRealTargetTileForAI (casterPos, direction);
					direction = Direction.LeftUp;
					leftUpTile = skill.GetRealTargetTileForAI (casterPos, direction);

					if (IsTastyTile (rightDownTile)) {
						yield return AI.Move (unit, movedTile, Direction.RightUp, 3);
						yield return AI.UseSkill (unit,  skill, Direction.RightDown, rightDownTile);
						continue;
					}
					if (IsTastyTile (leftUpTile)) {
						yield return AI.Move (unit, movedTile, Direction.RightUp, 3);
						yield return AI.UseSkill (unit,  skill, Direction.LeftUp, leftUpTile);
						continue;
					}
				}
				//우 타일로 한칸
				casterPos = currPos + Utility.ToVector2(Direction.LeftDown);
				movedTile = battleData.tileManager.GetTile (casterPos);
				if (!movedTile.IsUnitOnTile ()) {
					direction = Direction.RightDown;
					rightDownTile = skill.GetRealTargetTileForAI (casterPos, direction);
					direction = Direction.LeftUp;
					leftUpTile = skill.GetRealTargetTileForAI (casterPos, direction);
					if (IsTastyTile (rightDownTile)) {
						yield return AI.Move (unit, movedTile, Direction.LeftDown, 3);
						yield return AI.UseSkill (unit,  skill, Direction.RightDown, rightDownTile);
						continue;
					}
					if (IsTastyTile (leftUpTile)) {
						yield return AI.Move (unit, movedTile, Direction.LeftDown, 3);
						yield return AI.UseSkill (unit,  skill, Direction.LeftUp, leftUpTile);
						continue;
					}
				}

				if (currentAP < 8 + unit.GetActualRequireSkillAP(skill))
					break;

				//좌 타일로 두칸
				casterPos = currPos + Utility.ToVector2(Direction.RightUp) * 2;
				movedTile = battleData.tileManager.GetTile (casterPos);
				if (!movedTile.IsUnitOnTile ()) {
					direction = Direction.RightDown;
					rightDownTile = skill.GetRealTargetTileForAI (casterPos, direction);
					direction = Direction.LeftUp;
					leftUpTile = skill.GetRealTargetTileForAI (casterPos, direction);
					if (IsTastyTile (rightDownTile)) {
						yield return AI.Move (unit, movedTile, Direction.RightUp, 8);
						yield return AI.UseSkill (unit,  skill, Direction.RightDown, rightDownTile);
						continue;
					}
					if (IsTastyTile (leftUpTile)) {
						yield return AI.Move (unit, movedTile, Direction.RightUp, 8);
						yield return AI.UseSkill (unit,  skill, Direction.LeftUp, leftUpTile);
						continue;
					}
				}
				//우 타일로 두칸
				casterPos = currPos + Utility.ToVector2(Direction.LeftDown) * 2;
				movedTile = battleData.tileManager.GetTile (casterPos);
				if (!movedTile.IsUnitOnTile ()) {
					direction = Direction.RightDown;
					rightDownTile = skill.GetRealTargetTileForAI (casterPos, direction);
					direction = Direction.LeftUp;
					leftUpTile = skill.GetRealTargetTileForAI (casterPos, direction);
					if (IsTastyTile (rightDownTile)) {
						yield return AI.Move (unit, movedTile, Direction.LeftDown, 8);
						yield return AI.UseSkill (unit,  skill, Direction.RightDown, rightDownTile);
						continue;
					}
					if (IsTastyTile (leftUpTile)) {
						yield return AI.Move (unit, movedTile, Direction.LeftDown, 8);
						yield return AI.UseSkill (unit,  skill, Direction.LeftUp, leftUpTile);
						continue;
					}
				}
				*/
				//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

				break;
			}
		}
		private static IEnumerator BreakAndEscape(Unit unit){
			while (true) {
				yield return battleManager.BeforeActCommonAct ();

				int currentAP = unit.GetCurrentActivityPoint ();
				Vector2 currPos = unit.GetPosition ();
				int selectedSkillIndex = 1;
				battleData.indexOfSelectedSkillByUser = selectedSkillIndex;
				ActiveSkill skill = battleData.SelectedSkill;

				Tile barrierTile = skill.GetRealTargetTileForAI (currPos, Direction.RightDown);

				if (barrierTile == null) {
					yield return OnlyMove (unit);
					break;
				} else {
					if (currentAP < unit.GetActualRequireSkillAP (skill))
						break;
					yield return AI.UseSkill (new Casting(unit,  skill, new SkillLocation(currPos, barrierTile, Direction.RightDown)));
				}
			}
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
