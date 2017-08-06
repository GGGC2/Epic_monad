using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enums;

namespace Battle.Turn{
	public class AIUtil{
		public static Side GetEnemySide(Unit unit){
			//후에 제3세력 등장하면 조건이 바뀔 수 있음
			Side mySide = unit.GetSide ();
			Side enemySide;
			if (mySide == Side.Ally)
				enemySide = Side.Enemy;
			else
				enemySide = Side.Ally;
			return enemySide;
		}
			
		//AI 입장에서 다른 유닛이 적으로 인식되는지에 대한 함수이기 때문에 AI 부분에서만 써야 한다(은신 스킬 때문)
		public static bool IsEnemyToEachOther(Unit unit1,Unit unit2){
			//후에 제3세력 등장하면 조건이 바뀔 수 있음

			//두 유닛 중에 누가 '은신' 효과(아직 미구현인 그레네브 스킬 효과)를 갖고 있는지 여기에서 확인해서 적용해야 할 것 (AI에게 적으로 인식되지 않음)
			return unit1.GetSide () == GetEnemySide (unit2);
		}
		public static Vector2 FindNearestEnemy(List<Tile> movableTiles, List<Unit> units, Unit mainUnit){
			var positions = from tile in movableTiles
				from unit in units
					where IsEnemyToEachOther(unit, mainUnit)
				let distance = Vector2.Distance(tile.GetTilePos(), unit.GetPosition())
				orderby distance
				select tile.GetTilePos();

			List<Vector2> availablePositions = positions.ToList();
			if (availablePositions.Count > 0)
				return availablePositions[0];

			return mainUnit.GetPosition();
		}
		public static Tile FindNearestEnemyAttackableTile(Unit caster, ActiveSkill skill, Dictionary<Vector2, TileWithPath> movableTilesWithPath, BattleData battleData){
			Unit selectedUnit = battleData.selectedUnit;
			SkillType skillTypeOfSelectedSkill = skill.GetSkillType ();

			Dictionary<Vector2, TileWithPath> enemyAttackableTilesWithPath = new Dictionary<Vector2, TileWithPath> ();

			foreach (var pair in movableTilesWithPath) {
				Tile tile = pair.Value.tile;
				Tile attackAbleTile = AI.GetAttackableEnemyUnitTile (caster, tile, skill);

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
			var tilesHaveEnemy = from tile in activeTileRange
					where tile.GetUnitOnTile() != null
				let unit = tile.GetUnitOnTile()
					where IsEnemyToEachOther(unit, mainUnit)
				select tile;
			return tilesHaveEnemy.FirstOrDefault();
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

			Tile attackAbleTile = GetAttackableEnemyUnitTile (unit, currentTile, selectedSkill);

			//곧바로 공격 가능하면 이동하지 않는다
			if (attackAbleTile != null) {
				yield break;
			}

			Dictionary<Vector2, TileWithPath> movableTilesWithPath = PathFinder.CalculatePath(battleData.selectedUnit);
			List<Tile> movableTiles = new List<Tile>();
			foreach (KeyValuePair<Vector2, TileWithPath> movableTileWithPath in movableTilesWithPath)
			{
				movableTiles.Add(movableTileWithPath.Value.tile);
			}

			Tile destTile=AIUtil.FindNearestEnemyAttackableTile (unit, selectedSkill, movableTilesWithPath, battleData);
			//destTile이 null인 경우 : 이번 턴에 적을 공격 가능한 범위로 못 가는 경우. 그냥 가장 가까운 적을 향해 이동한다
			//null이 아니면 적을 공격 가능한 타일 중 도달경로의 총 AP 소모량이 가장 적은 곳으로 간다
			if (destTile == null) {
				Vector2 destPosition = AIUtil.FindNearestEnemy (movableTiles, battleData.unitManager.GetAllUnits (), battleData.selectedUnit);
				destTile = battleData.tileManager.GetTile(destPosition);
			}

			yield return MoveToTheTileAndChangeDirection (unit, destTile, movableTilesWithPath);

		}
		private static IEnumerator MoveToTheTileAndChangeDirection(Unit unit, Tile destTile, Dictionary<Vector2, TileWithPath> movableTilesWithPath){
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

				yield return Move(unit, destTile, finalDirection, totalUseAP);
			}

		}
		private static IEnumerator DecideSkillTargetAndUseSkill(Unit unit){
			while (true) {
				yield return battleManager.BeforeActCommonAct ();

				int selectedSkillIndex = 1;
				battleData.indexOfSelectedSkillByUser = selectedSkillIndex;
				ActiveSkill selectedSkill = battleData.SelectedSkill;

				Tile currTile = unit.GetTileUnderUnit ();
				Tile attackAbleTile = GetAttackableEnemyUnitTile (unit, currTile, selectedSkill);

				if (attackAbleTile == null) {
					yield break;
				}

				Tile targetTile = attackAbleTile;

				int currentAP = battleData.selectedUnit.GetCurrentActivityPoint ();
				int requireAP = unit.GetActualRequireSkillAP(battleData.SelectedSkill);
				bool enoughAP = currentAP >= requireAP;

				if (!enoughAP) {
					yield break;
				}

				//FIXME : 자기자신 위치에 쓰는 기술의 경우 방향 안 바뀌게 해야 함(후순위)
				Direction direction = Utility.GetDirectionToTarget(unit, targetTile.GetTilePos());

				List<Tile> tilesInSkillRange = GetTilesInSkillRange(targetTile, unit);
				//tilesInRealEffectRange는 투사체 스킬의 경우 경로상 유닛이 없으면 빈 List로 설정해야 하나 AI는 그 경우 아예 스킬을 쓰지 않므로 그런 경우가 없음
				List<Tile> tilesInRealEffectRange =  tilesInSkillRange;

				yield return UseSkill (unit, direction,targetTile);
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
		//FIXME : 경로의 totalUseAP를 패러미터로 받지 않고 여기서 계산하는 게 깔끔할 것 같은데...
		public static IEnumerator Move(Unit unit, Tile destTile, Direction finalDirection, int totalUseAP){
			unit.SetDirection (finalDirection);
			FocusToSelectedUnit ();
			yield return battleData.battleManager.StartCoroutine (MoveStates.MoveToTile (battleData, destTile, Direction.RightDown, totalUseAP));
		}
		public static IEnumerator UseSkill(Unit unit, Direction direction, Tile targetTile){
			unit.SetDirection (direction);
			FocusToSelectedUnit ();

			List<Tile> tilesInSkillRange = new List<Tile> ();
			tilesInSkillRange.Add (targetTile);
			List<Tile> tilesInRealEffectRange = tilesInSkillRange;
			yield return SkillAndChainStates.ApplyChain (battleData, targetTile, tilesInSkillRange, tilesInRealEffectRange, GetTilesInFirstRange ());

			FocusToSelectedUnit ();
			battleData.uiManager.ResetSkillNamePanelUI ();
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

		private static void FocusToSelectedUnit(){
			BattleManager.MoveCameraToUnit (battleData.selectedUnit);
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

				bool isThereAnotherSideUnit = aroundUnits.Any(anyUnit => AIUtil.IsEnemyToEachOther(anyUnit, unit));

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

				bool isThereAnotherSideUnit = aroundUnits.Any(anyUnit => AIUtil.IsEnemyToEachOther(anyUnit, unit));

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

		public static Tile GetAttackableEnemyUnitTile(Unit caster, Tile casterTile,ActiveSkill skill){
			Tile attackAbleTile;
			SkillType skillType = skill.GetSkillType();
			if (skillType != SkillType.Point)
				attackAbleTile = GetAttackableEnemyUnitTileOfDirectionSkill (caster, casterTile);
			else
				attackAbleTile = GetAttackableEnemyUnitTileOfPointSkill (caster, casterTile);
			return attackAbleTile;
		}

		public static Tile GetAttackableEnemyUnitTileOfDirectionSkill(Unit caster, Tile unitTile){
			List<Tile> selectedTiles = new List<Tile>();
			ActiveSkill selectedSkill = battleData.SelectedSkill;

			Tile castingTile = unitTile;

			//투사체 스킬이면 직선경로상에서 유닛이 가로막은 지점을 castingTile로 함. 범위 끝까지 가로막은 유닛이 없으면 범위 맨 끝 타일이 castingTile=null
			if (selectedSkill.GetSkillType() == SkillType.Route) {
				Vector2 currPos = caster.GetPosition ();
				//FIXME : 리스트로 만들어야 되는데.... 전체적으로 혼파망이라서 일단 이렇게 놔둠
				castingTile = GetRouteSkillCastingTile (currPos, selectedSkill, Direction.LeftUp);
				if(castingTile==null)
					castingTile = GetRouteSkillCastingTile (currPos, selectedSkill, Direction.LeftDown);
				if(castingTile==null)
					castingTile = GetRouteSkillCastingTile (currPos, selectedSkill, Direction.RightUp);
				if (castingTile == null)
					castingTile = GetRouteSkillCastingTile (currPos, selectedSkill, Direction.RightDown);
			}

			//FIXME : 4방향 다 체크해야 하는데...
			if (castingTile != null) {
				selectedTiles = battleData.tileManager.GetTilesInRange (selectedSkill.GetSecondRangeForm (),
					castingTile.GetTilePos (),
					selectedSkill.GetSecondMinReach (),
					selectedSkill.GetSecondMaxReach (),
					selectedSkill.GetSecondWidth (),
					caster.GetDirection ());
			}
			else {
				selectedTiles = new List<Tile> ();
			}

			Tile selectedTile = AIUtil.FindOtherSideUnitTile(selectedTiles, caster);

			return selectedTile;
		}
		public static Tile GetRouteSkillCastingTile(Vector2 unitPos, ActiveSkill routeSkill, Direction direction){				
			List<Tile> firstRange = battleData.tileManager.GetTilesInRange(routeSkill.GetFirstRangeForm(),
				unitPos,
				routeSkill.GetFirstMinReach(),
				routeSkill.GetFirstMaxReach(),
				routeSkill.GetFirstWidth(),
				direction);
			return SkillAndChainStates.GetRouteEnd(firstRange);
		}
		//  위 : 지정형 빼고 나머지 스킬
		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		//  아래 : 지정형 (Point) 스킬
		public static Tile GetAttackableEnemyUnitTileOfPointSkill(Unit caster, Tile unitTile){
			List<Tile> activeRange = new List<Tile>();
			ActiveSkill selectedSkill = battleData.SelectedSkill;

			//FIXME : 1차범위 설정 후에 2차범위까지 적용해야 하는데...
			activeRange = 
				battleData.tileManager.GetTilesInRange(selectedSkill.GetFirstRangeForm(),
					unitTile.GetTilePos(),
					selectedSkill.GetFirstMinReach(),
					selectedSkill.GetFirstMaxReach(),
					selectedSkill.GetFirstWidth(),
					caster.GetDirection());

			Tile selectedTile = AIUtil.FindOtherSideUnitTile(activeRange, caster);

			return selectedTile;
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

				Tile rightDownTile = AI.GetRouteSkillCastingTile (currPos, skill, Direction.RightDown);
				Tile leftUpTile = AI.GetRouteSkillCastingTile (currPos, skill, Direction.LeftUp);
				Tile rightUpTile = AI.GetRouteSkillCastingTile (currPos, skill, Direction.RightUp);
				Tile leftDownTile = AI.GetRouteSkillCastingTile (currPos, skill, Direction.LeftDown);

				if (IsTastyTile (rightDownTile)) {
					yield return AI.UseSkill (unit, Direction.RightDown, rightDownTile);
					continue;
				}
				if (IsTastyTile (leftUpTile)) {
					yield return AI.UseSkill (unit, Direction.LeftUp, leftUpTile);
					continue;
				}
				if (IsTastyTile (rightUpTile)) {
					yield return AI.UseSkill (unit, Direction.RightUp, rightUpTile);
					continue;
				}
				if (IsTastyTile (leftDownTile)) {
					yield return AI.UseSkill (unit, Direction.LeftDown, leftDownTile);
					continue;
				}
				if (IsDecentTile (rightDownTile)) {
					yield return AI.UseSkill (unit, Direction.RightDown, rightDownTile);
					continue;
				}
				if (IsDecentTile (leftUpTile)) {
					yield return AI.UseSkill (unit, Direction.LeftUp, leftUpTile);
					continue;
				}
				if (IsDecentTile (rightUpTile)) {
					yield return AI.UseSkill (unit, Direction.RightUp, rightUpTile);
					continue;
				}
				if (IsDecentTile (leftDownTile)) {
					yield return AI.UseSkill (unit, Direction.LeftDown, leftDownTile);
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

			while (currentAP - requireAP >= unit.GetStandardAP ()) {
				Vector2 nextPos = pos + battleData.tileManager.ToVector2 (Direction.RightDown);
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
				battleData.indexOfSelectedSkillByUser = selectedSkillIndex;
				ActiveSkill skill = battleData.SelectedSkill;

				Vector2 currPos = unit.GetPosition ();
				int currentAP = unit.GetCurrentActivityPoint ();

				//AP 부족하면 할 거 없으니 끝내고
				if (currentAP < unit.GetActualRequireSkillAP(skill))
					break;

				//현 타일에서 그레/비앙/달케 공격 가능할 시 공격
				Tile rightDownTile = AI.GetRouteSkillCastingTile (currPos, skill, Direction.RightDown);
				Tile leftUpTile = AI.GetRouteSkillCastingTile (currPos, skill, Direction.LeftUp);
				Tile rightUpTile = AI.GetRouteSkillCastingTile (currPos, skill, Direction.RightUp);
				Tile leftDownTile = AI.GetRouteSkillCastingTile (currPos, skill, Direction.LeftDown);
				if (IsTastyTile (rightDownTile)) {
					yield return AI.UseSkill (unit, Direction.RightDown, rightDownTile);
					continue;
				}
				if (IsTastyTile (leftUpTile)) {
					yield return AI.UseSkill (unit, Direction.LeftUp, leftUpTile);
					continue;
				}
				if (IsTastyTile (rightUpTile)) {
					yield return AI.UseSkill (unit, Direction.RightUp, rightUpTile);
					continue;
				}
				if (IsTastyTile (leftDownTile)) {
					yield return AI.UseSkill (unit, Direction.LeftDown, leftDownTile);
					continue;
				}

				//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
				//FIXME : 나중에 코드 제대로 수정할 계획

				if (currentAP < 3 + unit.GetActualRequireSkillAP(skill))
					break;

				//좌 타일로 한칸 움직이는 경우도 확인
				Vector2 movedPos = currPos + battleData.tileManager.ToVector2(Direction.RightUp);
				Tile movedTile = battleData.tileManager.GetTile (movedPos);
				if (!movedTile.IsUnitOnTile ()) {
					rightDownTile = AI.GetRouteSkillCastingTile (movedPos, skill, Direction.RightDown);
					leftUpTile = AI.GetRouteSkillCastingTile (movedPos, skill, Direction.LeftUp);

					if (IsTastyTile (rightDownTile)) {
						yield return AI.Move (unit, movedTile, Direction.RightUp, 3);
						yield return AI.UseSkill (unit, Direction.RightDown, rightDownTile);
						continue;
					}
					if (IsTastyTile (leftUpTile)) {
						yield return AI.Move (unit, movedTile, Direction.RightUp, 3);
						yield return AI.UseSkill (unit, Direction.LeftUp, leftUpTile);
						continue;
					}
				}
				//우 타일로 한칸
				movedPos = currPos + battleData.tileManager.ToVector2(Direction.LeftDown);
				movedTile = battleData.tileManager.GetTile (movedPos);
				if (!movedTile.IsUnitOnTile ()) {
					rightDownTile = AI.GetRouteSkillCastingTile (movedPos, skill, Direction.RightDown);
					leftUpTile = AI.GetRouteSkillCastingTile (movedPos, skill, Direction.LeftUp);
					if (IsTastyTile (rightDownTile)) {
						yield return AI.Move (unit, movedTile, Direction.LeftDown, 3);
						yield return AI.UseSkill (unit, Direction.RightDown, rightDownTile);
						continue;
					}
					if (IsTastyTile (leftUpTile)) {
						yield return AI.Move (unit, movedTile, Direction.LeftDown, 3);
						yield return AI.UseSkill (unit, Direction.LeftUp, leftUpTile);
						continue;
					}
				}

				if (currentAP < 8 + unit.GetActualRequireSkillAP(skill))
					break;

				//좌 타일로 두칸
				movedPos = currPos + battleData.tileManager.ToVector2(Direction.RightUp) * 2;
				movedTile = battleData.tileManager.GetTile (movedPos);
				if (!movedTile.IsUnitOnTile ()) {
					rightDownTile = AI.GetRouteSkillCastingTile (movedPos, skill, Direction.RightDown);
					leftUpTile = AI.GetRouteSkillCastingTile (movedPos, skill, Direction.LeftUp);
					if (IsTastyTile (rightDownTile)) {
						yield return AI.Move (unit, movedTile, Direction.RightUp, 8);
						yield return AI.UseSkill (unit, Direction.RightDown, rightDownTile);
						continue;
					}
					if (IsTastyTile (leftUpTile)) {
						yield return AI.Move (unit, movedTile, Direction.RightUp, 8);
						yield return AI.UseSkill (unit, Direction.LeftUp, leftUpTile);
						continue;
					}
				}
				//우 타일로 두칸
				movedPos = currPos + battleData.tileManager.ToVector2(Direction.LeftDown) * 2;
				movedTile = battleData.tileManager.GetTile (movedPos);
				if (!movedTile.IsUnitOnTile ()) {
					rightDownTile = AI.GetRouteSkillCastingTile (movedPos, skill, Direction.RightDown);
					leftUpTile = AI.GetRouteSkillCastingTile (movedPos, skill, Direction.LeftUp);;
					if (IsTastyTile (rightDownTile)) {
						yield return AI.Move (unit, movedTile, Direction.LeftDown, 8);
						yield return AI.UseSkill (unit, Direction.RightDown, rightDownTile);
						continue;
					}
					if (IsTastyTile (leftUpTile)) {
						yield return AI.Move (unit, movedTile, Direction.LeftDown, 8);
						yield return AI.UseSkill (unit, Direction.LeftUp, leftUpTile);
						continue;
					}
				}

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

				Tile barrierTile = AI.GetRouteSkillCastingTile (currPos, skill, Direction.RightDown);

				if (barrierTile == null) {
					yield return OnlyMove (unit);
					break;
				} else {
					if (currentAP < unit.GetActualRequireSkillAP (skill))
						break;
					yield return AI.UseSkill (unit, Direction.RightDown, barrierTile);
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
