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

		// 1턴 내에 이동 후 공격 불가하거나 그럴 만한 가치가 없을 때 가장 가치있는 적을 향해 움직인다
		// 가치에는 거리도 적용되므로 너무 멀면 그쪽으로 가진 않는다
		public static Tile GetBestApproachWorthTile(Unit caster, ActiveSkill skill, Dictionary<Vector2, TileWithPath> movableTilesWithPath){
			Vector2 casterPos = caster.GetPosition ();
			int minAPUse = caster.MinAPUseForStandbyForAI ();
			Tile bestTile = caster.GetTileUnderUnit ();
			float maxReward = 0;

			List<Unit> enemies = UnitManager.Instance.GetEnemyUnitsToThisAIUnit (caster);

			Dictionary<Unit, float> enemiesWithReward = new Dictionary<Unit, float> ();
			foreach (Unit enemy in enemies) {
				float enemyReward = enemy.CalculatePredictReward (caster, skill);
				enemiesWithReward [enemy] = enemyReward;
			}

			foreach (var pair in movableTilesWithPath) {
				Tile tile = pair.Value.tile;
				int requireAP = pair.Value.requireActivityPoint;

				if (requireAP < minAPUse)
					continue;

				foreach (var enemy in enemies) {
					float approachReward = 0;
					// FIXME : 원래는 칸 거리가 아니라 필요 AP를 계산해야 하는데 임시로 이걸 넣어둔 거임
					int distance = Utility.GetDistance(tile.GetTilePos(), enemy.GetPosition());
					float enemyReward = enemiesWithReward [enemy];

					// 거리의 영향을 좀더 줄여야 함
					approachReward = (enemyReward / (distance+5) ) / requireAP;
					Debug.Log (tile.GetTilePos().x+" : "+tile.GetTilePos().y+" approach reward "+approachReward);

					if (approachReward > maxReward) {
						maxReward = approachReward;
						bestTile = tile;
					}
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

				if (currAP - requireAP - moveRestrainFactor <= 0)
					continue;

				float reward = 0;
				Casting bestCastingOnThisTile = skill.GetBestAttack (caster, tile);
				if (bestCastingOnThisTile != null) {
					float singleCastingReward = skill.GetRewardByCasting (bestCastingOnThisTile);
					reward = singleCastingReward * (currAP - requireAP - moveRestrainFactor) / skillRequireAP;

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

	public class AI{
		private static BattleManager battleManager;
		public static void SetBattleManager(BattleManager battleManagerInstance){
			battleManager = battleManagerInstance;
		}

		public static IEnumerator UnitTurn(Unit unit){
			CameraFocusToUnit(unit);

			battleManager.StartUnitTurn (unit);

			yield return CheckUnitIsActiveAndDecideActionAndAct(unit);

			battleManager.EndUnitTurn();
		}

		private static IEnumerator CheckUnitIsActiveAndDecideActionAndAct(Unit unit){
			AIData unitAIData = unit.GetComponent<AIData>();

			if (!unitAIData.IsActive()) {CheckActiveTrigger(unit);}

			if (!unitAIData.IsActive ()) {
				yield return battleManager.BeforeActCommonAct ();
				Debug.Log (unit.GetName () + " skips turn because of being deactivated");
				yield return SkipDeactivatedUnitTurn(unit);
				yield break;
			}
			if (unit.GetNameInCode () == "kashasty_Escape") {
				yield return AIKashasty.DecideActionAndAct (unit);
				yield break;
			}
			yield return DecideActionAndAct (unit);
		}

		private static IEnumerator DecideActionAndAct(Unit unit){
			//이동->기술->대기/휴식의 순서로 이동이나 기술사용은 안 할 수도 있다
			if(unit.IsMovePossibleState())
				yield return DecideMoveAndMove (unit);
			if (unit.IsSkillUsePossibleState ())
				yield return DecideSkillTargetAndUseSkill (unit);
			yield return DecideRestOrStandbyAndDoThat (unit);
		}

		private static IEnumerator DecideMoveAndMove(Unit unit){
			yield return battleManager.BeforeActCommonAct ();

			Tile currentTile = unit.GetTileUnderUnit ();

			//이동 전에 먼저 기술부터 정해야 한다... 기술 범위에 따라 어떻게 이동할지 아니면 이동 안 할지가 달라지므로
			//나중엔 여러 기술중에 선택해야겠지만 일단 지금은 AI 기술이 모두 하나뿐이니 그냥 첫번째걸로
			int selectedSkillIndex = 1;
			BattleData.indexOfSelectedSkillByUser = selectedSkillIndex;
			ActiveSkill selectedSkill = BattleData.SelectedSkill;

			Dictionary<Vector2, TileWithPath> movableTilesWithPath = PathFinder.CalculatePath(unit);

			yield return PaintMovableTiles(movableTilesWithPath);

			Tile destTile = AIUtil.GetBestMovableTile (unit, selectedSkill, movableTilesWithPath, 0);
			if (destTile != null) {
				yield return MoveToTheTileAndChangeDirection (unit, destTile, movableTilesWithPath);
			}
			else {
				destTile = AIUtil.GetBestApproachWorthTile(unit, selectedSkill, movableTilesWithPath);
				yield return MoveToTheTileAndChangeDirection (unit, destTile, movableTilesWithPath);
			}
		}
		public static IEnumerator PaintMovableTiles(Dictionary<Vector2, TileWithPath> movableTilesWithPath){
			List<Tile> movableTiles = new List<Tile>();
			foreach (KeyValuePair<Vector2, TileWithPath> movableTileWithPath in movableTilesWithPath)
				movableTiles.Add (movableTileWithPath.Value.tile);
			TileManager.Instance.PaintTiles (movableTiles, TileColor.Blue);
			yield return null;
		}
		public static IEnumerator MoveToTheTileAndChangeDirection(Unit unit, Tile destTile, Dictionary<Vector2, TileWithPath> movableTilesWithPath){
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

				yield return Move (unit, destTile, finalDirection, totalUseAP);
			}else {TileManager.Instance.DepaintAllTiles(TileColor.Blue);}
		}
		private static IEnumerator DecideSkillTargetAndUseSkill(Unit unit){
			while(true){
				yield return battleManager.BeforeActCommonAct ();
				
				if (unit == null) {break;}

				int selectedSkillIndex = 1;
				BattleData.indexOfSelectedSkillByUser = selectedSkillIndex;
				ActiveSkill skill = BattleData.SelectedSkill;

				if (!unit.HasEnoughAPToUseSkill(skill)) {yield break;}

				Tile currTile = unit.GetTileUnderUnit ();
				Casting casting = skill.GetBestAttack (unit, currTile);

				if (casting == null) {yield break;}
				
				yield return UseSkill(casting);
			}
		}
		public static IEnumerator DecideRestOrStandbyAndDoThat(Unit unit){
			yield return battleManager.BeforeActCommonAct ();
			if (unit.IsStandbyPossible ()) {
				yield return Standby (unit);
			}
			else {
				yield return TakeRest (unit);
			}
		}

		public static IEnumerator Move(Unit unit, Tile destTile, Direction finalDirection, int totalAPCost){
			CameraFocusToUnit(unit);
			yield return new WaitForSeconds (0.5f);
			TileManager.Instance.DepaintAllTiles (TileColor.Blue);
			yield return BattleData.battleManager.StartCoroutine (MoveStates.MoveToTile (destTile, finalDirection, totalAPCost));
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
				if (BattleData.currentPhase >= unitAIData.activePhase) {
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
		private static BattleManager battleManager;
		public static IEnumerator DecideActionAndAct(Unit unit){
			battleManager = BattleData.battleManager;
			bool movable = unit.IsMovePossibleState ();
			bool skillusable = unit.IsSkillUsePossibleState ();
			if (!movable && !skillusable) {
				//do nothing
			}
			else if (!movable && skillusable) {
				yield return OnlyAttack (unit);
			}
			else if (movable && !skillusable) {
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
				BattleData.indexOfSelectedSkillByUser = selectedSkillIndex;
				ActiveSkill skill = BattleData.SelectedSkill;

				Vector2 currPos = unit.GetPosition ();
				if (!unit.HasEnoughAPToUseSkill (skill))
					break;

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
				Tile nextTile = BattleData.tileManager.GetTile (nextPos);
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
			yield return AI.Move (unit, destTile, Direction.RightDown, totalUseAP);
		}
		private static IEnumerator FreeState(Unit unit){
			yield return TasteTastyTile (unit);
			yield return BreakAndEscape (unit);
		}
		private static IEnumerator TasteTastyTile(Unit unit){
			while (true) {
				yield return battleManager.BeforeActCommonAct ();

				int selectedSkillIndex = 1;
				BattleData.indexOfSelectedSkillByUser = selectedSkillIndex;
				ActiveSkill skill = BattleData.SelectedSkill;

				Vector2 currPos = unit.GetPosition ();
				Dictionary<Vector2, TileWithPath> movableTilesWithPath = PathFinder.CalculatePath(unit);
				AI.PaintMovableTiles(movableTilesWithPath);
				
				Tile destTile = AIUtil.GetBestMovableTile (unit, skill, movableTilesWithPath, unit.GetCurrentActivityPoint() - 24);

				if (TastyTileAttackDirectionOnThatPosition(destTile.GetTilePos(), skill) == null)
					yield break;

				yield return AI.MoveToTheTileAndChangeDirection (unit, destTile, movableTilesWithPath);

				while (true) {
					yield return battleManager.BeforeActCommonAct ();
					if (!unit.HasEnoughAPToUseSkill (skill))
						yield break;
					Tile currTile = unit.GetTileUnderUnit ();
					Casting casting = skill.GetBestAttack (unit, currTile);
					yield return AI.UseSkill (casting);
					if (TastyTileAttackDirectionOnThatPosition(destTile.GetTilePos(), skill) == null)
						break;
				}
			}
		}
		private static IEnumerator BreakAndEscape(Unit unit){
			while (true) {
				yield return battleManager.BeforeActCommonAct ();

				int currentAP = unit.GetCurrentActivityPoint ();
				Vector2 currPos = unit.GetPosition ();
				int selectedSkillIndex = 1;
				BattleData.indexOfSelectedSkillByUser = selectedSkillIndex;
				ActiveSkill skill = BattleData.SelectedSkill;

				Tile barrierTile = skill.GetRealTargetTileForAI (currPos, Direction.RightDown);

				if (barrierTile == null) {
					yield return OnlyMove (unit);
					break;
				}
				else {
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
