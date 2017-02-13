using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enums;
using Battle.Skills;
using Battle.Damage;

namespace Battle.Turn
{
	public class SkillAndChainStates
	{
		private static IEnumerator UpdatePreviewAP(BattleData battleData)
		{
			while (true)
			{
				if (battleData.indexOfPreSelectedSkillByUser != 0)
				{
					Skill preSelectedSkill = battleData.PreSelectedSkill;
					int requireAP = preSelectedSkill.GetRequireAP();
					battleData.previewAPAction = new APAction(APAction.Action.Skill, requireAP);
				}
				else
				{
					battleData.previewAPAction = null;
				}
				battleData.uiManager.UpdateApBarUI(battleData, battleData.unitManager.GetAllUnits());
				yield return null;
			}
		}

		public static IEnumerator SelectSkillState(BattleData battleData)
		{
			while (battleData.currentState == CurrentState.SelectSkill)
			{
				battleData.uiManager.UpdateSkillInfo(battleData.selectedUnitObject);
				battleData.uiManager.CheckUsableSkill(battleData.selectedUnitObject);

				battleData.isWaitingUserInput = true;
				battleData.indexOfSeletedSkillByUser = 0;

				var update = UpdatePreviewAP(battleData);
				battleData.battleManager.StartCoroutine(update);
				yield return battleData.battleManager.StartCoroutine(EventTrigger.WaitOr(
					battleData.triggers.skillSelected,
					battleData.triggers.rightClicked,
					battleData.triggers.cancelClicked
				));
				battleData.battleManager.StopCoroutine(update);

				battleData.indexOfPreSelectedSkillByUser = 0;
				battleData.isWaitingUserInput = false;
				battleData.uiManager.DisableSkillUI();

				if (battleData.triggers.rightClicked.Triggered || battleData.triggers.cancelClicked.Triggered)
				{
					battleData.currentState = CurrentState.FocusToUnit;
					yield break;
				}

				BattleManager battleManager = battleData.battleManager;
				Skill selectedSkill = battleData.SelectedSkill;
				SkillType skillTypeOfSelectedSkill = selectedSkill.GetSkillType();
				if (skillTypeOfSelectedSkill == SkillType.Auto || 
					skillTypeOfSelectedSkill == SkillType.Self ||
					skillTypeOfSelectedSkill == SkillType.Route)
				{
					battleData.currentState = CurrentState.SelectSkillApplyDirection;
					yield return battleManager.StartCoroutine(SelectSkillApplyDirection(battleData, battleData.selectedUnitObject.GetComponent<Unit>().GetDirection()));
				}
				else
				{
					battleData.currentState = CurrentState.SelectSkillApplyPoint;
					yield return battleManager.StartCoroutine(SelectSkillApplyPoint(battleData, battleData.selectedUnitObject.GetComponent<Unit>().GetDirection()));
				}

				battleData.previewAPAction = null;
				battleData.uiManager.UpdateApBarUI(battleData, battleData.unitManager.GetAllUnits());

				yield return null;
			}
		}

		private static IEnumerator UpdateRangeSkillMouseDirection(BattleData battleData)
		{
			Unit selectedUnit = battleData.selectedUnitObject.GetComponent<Unit>();
			Skill selectedSkill = battleData.SelectedSkill;
			var selectedTiles = GetTilesInFirstRange(battleData);
			if (battleData.SelectedSkill.GetSkillType() == SkillType.Route)
			{
				selectedTiles = GetRouteTiles(selectedTiles);
			}

			battleData.tileManager.PaintTiles(selectedTiles, TileColor.Red);
			
			while (true)
			{
				Direction newDirection = Utility.GetMouseDirectionByUnit(battleData.selectedUnitObject);
				Direction beforeDirection = battleData.SelectedUnit.GetDirection();
				var selectedTilesByBeforeDirection = GetTilesInFirstRange(battleData, beforeDirection);

				if (battleData.SelectedSkill.GetSkillType() == SkillType.Route)
				{
					selectedTilesByBeforeDirection = GetRouteTiles(selectedTilesByBeforeDirection);
				}
				
				var selectedTilesByNewDirection = GetTilesInFirstRange(battleData, newDirection);
				if (battleData.SelectedSkill.GetSkillType() == SkillType.Route)
				{
					// 동일한 코드
					selectedTilesByNewDirection = GetRouteTiles(selectedTilesByNewDirection);
				}

				if (beforeDirection != newDirection)
				{
					battleData.tileManager.DepaintTiles(selectedTilesByBeforeDirection, TileColor.Red);

					beforeDirection = newDirection;
					battleData.SelectedUnit.SetDirection(newDirection);

					battleData.tileManager.PaintTiles(selectedTilesByNewDirection, TileColor.Red);
				}
				yield return null;
			}
		}

		// *주의 : ChainList.cs에서 같은 이름의 함수를 수정할 것!!
		public static List<GameObject> GetRouteTiles(List<GameObject> tiles)
		{
			List<GameObject> newRouteTiles = new List<GameObject>();
			foreach (var tile in tiles)
			{
				// 타일 단차에 의한 부분(미구현)
				// 즉시 탐색을 종료한다.
				// break;
				
				// 첫 유닛을 만난 경우
				// 이번 타일을 마지막으로 종료한다.
				newRouteTiles.Add(tile);
				if (tile.GetComponent<Tile>().IsUnitOnTile())
					break;
			}

			return newRouteTiles;
		}


		public static IEnumerator SelectSkillApplyDirection(BattleData battleData, Direction originalDirection)
		{
			Direction beforeDirection = originalDirection;
			Unit selectedUnit = battleData.selectedUnitObject.GetComponent<Unit>();
			Skill selectedSkill = battleData.SelectedSkill;

			while (true) {

				battleData.isWaitingUserInput = true;
				var update = UpdateRangeSkillMouseDirection(battleData);
				battleData.battleManager.StartCoroutine(update);
				yield return battleData.battleManager.StartCoroutine(EventTrigger.WaitOr(
					battleData.triggers.rightClicked,
					battleData.triggers.cancelClicked,
					battleData.triggers.selectedTileByUser
				));
				battleData.battleManager.StopCoroutine(update);
				battleData.isWaitingUserInput = false;

				List<GameObject> selectedTiles = new List<GameObject>();
				battleData.tileManager.DepaintAllTiles(TileColor.Red);

				if (battleData.triggers.rightClicked.Triggered ||
					battleData.triggers.cancelClicked.Triggered)
				{
					selectedUnit.SetDirection(originalDirection);
					battleData.currentState = CurrentState.SelectSkill;
					yield break;
				}

				if (battleData.triggers.selectedTileByUser.Triggered)
				{
					BattleManager battleManager = battleData.battleManager;
					battleData.currentState = CurrentState.CheckApplyOrChain;
					if (battleData.SelectedSkill.GetSkillType() == SkillType.Route)
					{
						var firstRange = GetTilesInFirstRange(battleData);
						var destTileAtRoute = GetRouteTiles(firstRange).Last().GetComponent<Tile>();
						yield return battleManager.StartCoroutine(CheckApplyOrChain(battleData, destTileAtRoute, originalDirection));
					}
					else
						yield return battleManager.StartCoroutine(CheckApplyOrChain(battleData, battleData.SelectedUnitTile, originalDirection));
				}

				if (battleData.currentState != CurrentState.SelectSkillApplyDirection)
				{
					battleData.uiManager.DisableCancelButtonUI();
					yield break;
				}
			}
		}

		private static IEnumerator UpdatePointSkillMouseDirection(BattleData battleData)
		{
			Direction beforeDirection = Utility.GetMouseDirectionByUnit(battleData.selectedUnitObject);
			while (true)
			{
				Direction newDirection = Utility.GetMouseDirectionByUnit(battleData.selectedUnitObject);
				if (beforeDirection != newDirection)
				{
					beforeDirection = newDirection;
					battleData.SelectedUnit.SetDirection(newDirection);
				}
				yield return null;
			}
		}

		public static IEnumerator SelectSkillApplyPoint(BattleData battleData, Direction originalDirection)
		{
			Direction beforeDirection = originalDirection;
			Unit selectedUnit = battleData.selectedUnitObject.GetComponent<Unit>();

			if (battleData.currentState == CurrentState.SelectSkill)
			{
				battleData.uiManager.DisableCancelButtonUI();
				yield break;
			}

			while (battleData.currentState == CurrentState.SelectSkillApplyPoint)
			{
				Vector2 selectedUnitPos = battleData.selectedUnitObject.GetComponent<Unit>().GetPosition();

				List<GameObject> activeRange = new List<GameObject>();
				Skill selectedSkill = battleData.SelectedSkill;
				activeRange = GetTilesInFirstRange(battleData);

				battleData.tileManager.PaintTiles(activeRange, TileColor.Red);
				battleData.uiManager.EnableCancelButtonUI();
				battleData.isWaitingUserInput = true;

				var update = UpdatePointSkillMouseDirection(battleData);
				battleData.battleManager.StartCoroutine(update);
				yield return battleData.battleManager.StartCoroutine(EventTrigger.WaitOr(
					battleData.triggers.selectedTileByUser,
					battleData.triggers.rightClicked,
					battleData.triggers.cancelClicked
				));
				battleData.battleManager.StopCoroutine(update);
				battleData.isWaitingUserInput = false;
				battleData.uiManager.DisableCancelButtonUI();

				if (battleData.triggers.rightClicked.Triggered ||
				    battleData.triggers.cancelClicked.Triggered)
				{
					selectedUnit.SetDirection(originalDirection);
					battleData.tileManager.DepaintTiles(activeRange, TileColor.Red);
					battleData.currentState = CurrentState.SelectSkill;
					battleData.isWaitingUserInput = false;
					yield break;
				}

				battleData.tileManager.DepaintTiles(activeRange, TileColor.Red);
				battleData.uiManager.DisableSkillUI();

				BattleManager battleManager = battleData.battleManager;
				battleData.currentState = CurrentState.CheckApplyOrChain;
				yield return battleManager.StartCoroutine(CheckApplyOrChain(battleData, battleData.SelectedTile, originalDirection));
			}
		}

		public static IEnumerator CheckApplyOrChain(BattleData battleData, Tile targetTile, Direction originalDirection)
		{
			while (battleData.currentState == CurrentState.CheckApplyOrChain)
			{
				FocusTile(targetTile);

				List<GameObject> tilesInSkillRange = GetTilesInSkillRange(battleData, targetTile);
				battleData.tileManager.PaintTiles(tilesInSkillRange, TileColor.Red);
				List<GameObject> firstRange = GetTilesInFirstRange(battleData);

				//데미지 미리보기
				Dictionary<GameObject, float> calculatedTotalDamage = DamageCalculator.CalculateTotalDamage(battleData, targetTile, tilesInSkillRange, firstRange);
				foreach (KeyValuePair<GameObject, float> kv in calculatedTotalDamage)
				{
					Debug.Log(kv.Key.GetComponent<Unit>().GetName() + " - Damage preview");
					kv.Key.GetComponentInChildren<HealthViewer>().PreviewDamageAmount((int)kv.Value);
				}

				bool isChainPossible = CheckChainPossible(battleData);
				battleData.uiManager.EnableSkillCheckChainButton(isChainPossible);
				Skill selectedSkill = battleData.SelectedSkill;
				battleData.uiManager.SetSkillCheckAP(battleData.selectedUnitObject, selectedSkill);

				battleData.skillApplyCommand = SkillApplyCommand.Waiting;
				yield return battleData.battleManager.StartCoroutine(EventTrigger.WaitOr(
					battleData.triggers.cancelClicked,
					battleData.triggers.rightClicked,
					battleData.triggers.skillApplyCommandChanged
				));
				battleData.tileManager.DepaintTiles(tilesInSkillRange, TileColor.Red);

				if (battleData.triggers.rightClicked.Triggered ||
				    battleData.triggers.cancelClicked.Triggered)
				{
					FocusUnit(battleData.SelectedUnit);
					battleData.uiManager.DisableSkillCheckUI();
					battleData.SelectedUnit.SetDirection(originalDirection);
					if (selectedSkill.GetSkillType() != SkillType.Point)
						battleData.currentState = CurrentState.SelectSkillApplyDirection;
					else
						battleData.currentState = CurrentState.SelectSkillApplyPoint;

					// 데미지 미리보기 해제.
					foreach (KeyValuePair<GameObject, float> kv in calculatedTotalDamage)
					{
						kv.Key.GetComponentInChildren<HealthViewer>().CancelPreview();
					}

					yield break;
				}

				BattleManager battleManager = battleData.battleManager;
				if (battleData.skillApplyCommand == SkillApplyCommand.Apply)
				{
					battleData.skillApplyCommand = SkillApplyCommand.Waiting;
					// 체인이 가능한 스킬일 경우. 체인 발동.
					// 왜 CheckChainPossible 안 쓴 거죠...? CheckChainPossible은 체인을 새로 만들때 체크
					// 여기서는 체인을 새로 만드는 게 아니라 기존에 쌓인 체인을 소모하는 코드
					if (selectedSkill.GetSkillApplyType() == SkillApplyType.DamageHealth
					 || selectedSkill.GetSkillApplyType() == SkillApplyType.Debuff)
					{
						yield return ApplyChain(battleData, targetTile, tilesInSkillRange, firstRange);
						FocusUnit(battleData.SelectedUnit);
						battleData.currentState = CurrentState.FocusToUnit;
						// 연계 정보 업데이트
						battleData.chainList = ChainList.RefreshChainInfo(battleData.chainList);
					}
					// 체인이 불가능한 스킬일 경우, 그냥 발동.
					else
					{
						battleData.currentState = CurrentState.ApplySkill;
						yield return battleManager.StartCoroutine(ApplySkill(battleData, tilesInSkillRange));
						// 연계 정보 업데이트
						battleData.chainList = ChainList.RefreshChainInfo(battleData.chainList);
					}
				}
				else if (battleData.skillApplyCommand == SkillApplyCommand.Chain)
				{
					battleData.skillApplyCommand = SkillApplyCommand.Waiting;
					battleData.currentState = CurrentState.ChainAndStandby;
					yield return battleManager.StartCoroutine(ChainAndStandby(battleData, targetTile, tilesInSkillRange, firstRange));
					// 연계 정보 업데이트. 여기선 안 해줘도 될 것 같은데 혹시 몰라서...
					battleData.chainList = ChainList.RefreshChainInfo(battleData.chainList);
				}
				else
				{
					Debug.LogError("Invalid State");
					yield return null;
				}
			}
			yield return null;
		}

		private static void FocusTile(Tile focusTile)
		{
			Camera.main.transform.position = new Vector3(focusTile.transform.position.x, focusTile.transform.position.y, -10);
		}

		private static void FocusUnit(Unit unit)
		{
			Camera.main.transform.position = new Vector3(unit.transform.position.x, unit.transform.position.y, -10);
		}

		private static List<GameObject> GetTilesInFirstRange(BattleData battleData, Direction? direction = null)
		{
			Direction realDirection;
			if (direction.HasValue) {
				realDirection = direction.Value;
			} else {
				realDirection = battleData.SelectedUnit.GetDirection();
			}
			var firstRange = battleData.tileManager.GetTilesInRange(battleData.SelectedSkill.GetFirstRangeForm(),
																battleData.SelectedUnit.GetPosition(),
																battleData.SelectedSkill.GetFirstMinReach(),
																battleData.SelectedSkill.GetFirstMaxReach(),
																realDirection);

			return firstRange;
		}

		private static List<GameObject> GetTilesInSkillRange(BattleData battleData, Tile targetTile)
		{
				Skill selectedSkill = battleData.SelectedSkill;
				List<GameObject> selectedTiles = battleData.tileManager.GetTilesInRange(selectedSkill.GetSecondRangeForm(),
																			targetTile.GetTilePos(),
																			selectedSkill.GetSecondMinReach(),
																			selectedSkill.GetSecondMaxReach(),
																			battleData.selectedUnitObject.GetComponent<Unit>().GetDirection());
				if (selectedSkill.GetSkillType() == SkillType.Auto)
					selectedTiles.Remove(targetTile.gameObject);
				Debug.Log("NO. of tiles selected : "+selectedTiles.Count);
				return selectedTiles;
		}

		private static bool CheckChainPossible(BattleData battleData)
		{
			bool isPossible = false;

			// ap 조건으로 체크.
			int requireAP = battleData.selectedUnitObject.GetComponent<Unit>().GetActualRequireSkillAP(battleData.SelectedSkill);
			int remainAPAfterChain = battleData.selectedUnitObject.GetComponent<Unit>().GetCurrentActivityPoint() - requireAP;

			foreach (var unit in battleData.unitManager.GetAllUnits())
			{
				if ((unit != battleData.selectedUnitObject) &&
				(unit.GetComponent<Unit>().GetCurrentActivityPoint() > remainAPAfterChain))
				{
					isPossible = true;
				}
			}

			// 스킬 타입으로 체크. 공격스킬만 체인을 걸 수 있음.
			if (battleData.SelectedSkill.GetSkillApplyType() != SkillApplyType.DamageHealth
			 && battleData.SelectedSkill.GetSkillApplyType() != SkillApplyType.Debuff)
			{
				isPossible = false;
			}
			Debug.Log("Skill Apply Type : "+battleData.SelectedSkill.GetSkillApplyType());

			return isPossible;
		}

		
		public static IEnumerator ApplyChain(BattleData battleData, Tile targetTile, List<GameObject> tilesInSkillRange, List<GameObject> firstRange)
		{
			BattleManager battleManager = battleData.battleManager;
			// 자기 자신을 체인 리스트에 추가.
			ChainList.AddChains(battleData.selectedUnitObject, targetTile, tilesInSkillRange, battleData.SelectedSkill, firstRange);

			// 체인 체크, 순서대로 공격.
			List<ChainInfo> allVaildChainInfo = ChainList.GetAllChainInfoToTargetArea(battleData.selectedUnitObject, tilesInSkillRange);
			int chainCombo = allVaildChainInfo.Count;

			battleData.SelectedUnit.PrintChainBonus(chainCombo);

			foreach (var chainInfo in allVaildChainInfo)
			{
				GameObject focusedTile = chainInfo.GetTargetArea()[0];
				FocusTile(focusedTile.GetComponent<Tile>());
				battleData.currentState = CurrentState.ApplySkill;
				yield return battleManager.StartCoroutine(ApplySkill(battleData, chainInfo, chainCombo));
			}

			battleData.SelectedUnit.DisableChainText();
		}

		private static IEnumerator ChainAndStandby(BattleData battleData, Tile targetTile, List<GameObject> selectedTiles, List<GameObject> firstRange)
		{
			// 방향 돌리기.
			battleData.selectedUnitObject.GetComponent<Unit>().SetDirection(Utility.GetDirectionToTarget(battleData.selectedUnitObject, selectedTiles));

			int requireAP = SkillLogicFactory.Get(battleData.SelectedSkill).CalculateAP(battleData, selectedTiles);
			battleData.selectedUnitObject.GetComponent<Unit>().UseActivityPoint(requireAP);

			// 스킬 쿨다운 기록
			if (battleData.SelectedSkill.GetCooldown() > 0)
			{
				battleData.selectedUnitObject.GetComponent<Unit>().GetUsedSkillDict().Add(battleData.SelectedSkill.GetName(), battleData.SelectedSkill.GetCooldown());
			}

			// 체인 목록에 추가.
			ChainList.AddChains(battleData.selectedUnitObject, targetTile, selectedTiles, battleData.SelectedSkill, firstRange);
			battleData.indexOfSeletedSkillByUser = 0; // return to init value.
			yield return new WaitForSeconds(0.5f);

			Camera.main.transform.position = new Vector3(battleData.selectedUnitObject.transform.position.x, battleData.selectedUnitObject.transform.position.y, -10);
			battleData.currentState = CurrentState.Standby;
			battleData.alreadyMoved = false;
			BattleManager battleManager = battleData.battleManager;
			yield return battleManager.StartCoroutine(BattleManager.Standby()); // 이후 대기.
		}

		// 체인 가능 스킬일 경우의 스킬 시전 코루틴. 체인 정보와 배수를 받는다.
		private static IEnumerator ApplySkill(BattleData battleData, ChainInfo chainInfo, int chainCombo)
		{
			GameObject unitObjectInChain = chainInfo.GetUnit();
			Unit unitInChain = unitObjectInChain.GetComponent<Unit>();
			Skill appliedSkill = chainInfo.GetSkill();
			Tile targetTile = chainInfo.GetCenterTile();
			List<GameObject> selectedTiles = chainInfo.GetTargetArea();

			// 시전 방향으로 유닛의 바라보는 방향을 돌림.
			if (appliedSkill.GetSkillType() != SkillType.Auto)
				unitInChain.SetDirection(Utility.GetDirectionToTarget(unitInChain.gameObject, selectedTiles));

			// 자신의 체인 정보 삭제.
			ChainList.RemoveChainsFromUnit(unitObjectInChain);

			BattleManager battleManager = battleData.battleManager;
			yield return battleManager.StartCoroutine(ApplySkillEffect(appliedSkill, unitInChain.gameObject, selectedTiles));

			List<Unit> targets = GetUnitsOnTiles(selectedTiles);

			foreach (var target in targets)
			{
				List<PassiveSkill> passiveSkills = target.GetLearnedPassiveSkillList();
				if (SkillLogicFactory.Get(passiveSkills).checkEvade()) {
					battleData.uiManager.AppendNotImplementedLog("EVASION SUCCESS");
					SkillLogicFactory.Get(passiveSkills).triggerEvasionEvent(battleData, target);
					continue;
				}

				if (appliedSkill.GetSkillApplyType() == SkillApplyType.DamageHealth)
				{
					yield return battleManager.StartCoroutine(ApplyDamage(battleData, unitInChain, target, appliedSkill, chainCombo, targets.Count, target == targets.Last()));
				}

				SkillLogicFactory.Get(appliedSkill).ActionInDamageRoutine(battleData, appliedSkill, unitInChain, targetTile, selectedTiles);

				// 기술의 상태이상은 기술이 적용된 후에 붙인다.
				if(appliedSkill.GetStatusEffectList().Count > 0)
				{
					StatusEffector.AttachStatusEffect(appliedSkill, target);
				}

				unitInChain.ActiveFalseAllBounsText();
			}

			int requireAP = battleData.selectedUnitObject.GetComponent<Unit>().GetActualRequireSkillAP(appliedSkill);
			if (unitInChain.gameObject == battleData.selectedUnitObject)
			{
				unitInChain.UseActivityPoint(requireAP); // 즉시시전 대상만 ap를 차감. 나머지는 선차감되었으므로 패스.
				// 스킬 쿨다운 기록
				if (appliedSkill.GetCooldown() > 0)
				{
					unitInChain.GetUsedSkillDict().Add(appliedSkill.GetName(), appliedSkill.GetCooldown());
				}
			}
			battleData.indexOfSeletedSkillByUser = 0; // return to init value.

			yield return new WaitForSeconds(0.5f);

			battleData.alreadyMoved = false;
		}

		private static IEnumerator ApplyDamage(BattleData battleData, Unit unitInChain, Unit target, Skill appliedSkill, int chainCombo, int targetCount, bool isLastTarget)
		{
			var passiveSkillsOfAttacker = unitInChain.GetLearnedPassiveSkillList();
			SkillLogicFactory.Get(passiveSkillsOfAttacker).triggerActiveSkillDamageApplied(
				unitInChain
			);

			var attackDamage = DamageCalculator.CalculateAttackDamage(battleData, target.gameObject, unitInChain.gameObject, appliedSkill, chainCombo, targetCount);
			if (attackDamage.directionBonus > 1f) unitInChain.PrintDirectionBonus(attackDamage.directionBonus);
			if (attackDamage.celestialBonus > 1f) unitInChain.PrintCelestialBonus();
			// '보너스'만 표시하려고 임시로 주석처리.
			// else if (celestialBonus == 0.8f) targetUnit.PrintCelestialBonus();
			if (attackDamage.chainBonus > 1f) unitInChain.PrintChainBonus(chainCombo);

			BattleManager battleManager = battleData.battleManager;
			// targetUnit이 반사 효과를 지니고 있을 경우 반사 대미지 코루틴 준비
			if (target.HasStatusEffect(StatusEffectType.Reflect))
			{
				float reflectAmount = DamageCalculator.CalculateReflectDamage(attackDamage.resultDamage, target);
				var reflectCoroutine = unitInChain.Damaged(target.GetUnitClass(), reflectAmount, appliedSkill.GetPenetration(), false, true);
				battleManager.StartCoroutine(reflectCoroutine);
			}

			var damageCoroutine = target.Damaged(unitInChain.GetUnitClass(), attackDamage.resultDamage, appliedSkill.GetPenetration(), false, true);
			if (isLastTarget)
			{
				yield return battleManager.StartCoroutine(damageCoroutine);
			}
			else
			{
				battleManager.StartCoroutine(damageCoroutine);
				yield return null;
			}
		}

		private static List<GameObject> ReselectTilesByRoute(BattleData battleData, List<GameObject> selectedTiles)
		{
			List<GameObject> reselectTiles = new List<GameObject>();
			//단차 제외하고 구현.
			Vector2 startPos = battleData.selectedUnitObject.GetComponent<Unit>().GetPosition();
			// Vector2 endPos = ; 

			return reselectTiles;
		}

		private static List<Unit> GetUnitsOnTiles(List<GameObject> tiles)
		{
			List<Unit> units = new List<Unit>();
			foreach (var tileObject in tiles)
			{
				Tile tile = tileObject.GetComponent<Tile>();
				if (tile.IsUnitOnTile())
				{
					units.Add(tile.GetUnitOnTile().GetComponent<Unit>());
				}
			}
			return units;
		}

		// 체인 불가능 스킬일 경우의 스킬 시전 코루틴. 스킬 적용 범위만 받는다.
		private static IEnumerator ApplySkill(BattleData battleData, List<GameObject> selectedTiles)
		{
			Unit selectedUnit = battleData.selectedUnitObject.GetComponent<Unit>();
			Skill appliedSkill = battleData.SelectedSkill;
			BattleManager battleManager = battleData.battleManager;

			// 경로형 스킬의 경우 대상 범위 재지정
			if (appliedSkill.GetSkillType() == SkillType.Route)
				selectedTiles = ReselectTilesByRoute(battleData, selectedTiles);			
			
			// 시전 방향으로 유닛의 바라보는 방향을 돌림.
			if (appliedSkill.GetSkillType() != SkillType.Auto)
				selectedUnit.SetDirection(Utility.GetDirectionToTarget(selectedUnit.gameObject, selectedTiles));

			yield return battleManager.StartCoroutine(ApplySkillEffect(appliedSkill, battleData.selectedUnitObject, selectedTiles));

			List<Unit> targets = GetUnitsOnTiles(selectedTiles);

			foreach (var target in targets)
			{
				/* 팩토리로
				if (appliedSkill.GetSkillApplyType() == SkillApplyType.DamageAP)
				{
					// luvericha_l_30
					if (appliedSkill.GetName().Equals("에튀드:겨울바람"))
					{
						int apDamage = target.GetCurrentActivityPoint()*(int)appliedSkill.GetPowerFactor(Stat.None, appliedSkill.GetLevel());
						var damageCoroutine = target.Damaged(UnitClass.None, apDamage, 0.0f, false, false);
						battleManager.StartCoroutine(damageCoroutine);
					}
				}
				*/
				if (appliedSkill.GetSkillApplyType() == SkillApplyType.DamageHealth)
				{
					yield return battleManager.StartCoroutine(ApplyDamage(battleData, selectedUnit, target, appliedSkill, 1, targets.Count, target == targets.Last()));
				}

				/* 이것도 팩토리로
				if (appliedSkill.GetSkillApplyType() == SkillApplyType.HealHealth)
				{
					// 스킬 기본 계수 계산
					float actualAmount = 0.0f;
					foreach (var powerFactor in appliedSkill.GetPowerFactorDict().Keys)
					{
						Stat stat = (Stat)Enum.Parse(typeof(Stat), powerFactor);
						if (stat.Equals(Stat.UsedAP))
						{
							actualAmount += selectedUnit.GetActualRequireSkillAP(appliedSkill) * appliedSkill.GetPowerFactor(stat, appliedSkill.GetLevel());
						}
						else if (stat.Equals(Stat.None))
						{
							actualAmount += appliedSkill.GetPowerFactor(stat, appliedSkill.GetLevel());
						}
						else
						{
							actualAmount += selectedUnit.GetActualStat(stat) * appliedSkill.GetPowerFactor(stat, appliedSkill.GetLevel());
						}
					}

					var recoverAmount = (int) actualAmount;
					// luvericha_r_1 회복량 조건 체크
					if (appliedSkill.GetName().Equals("사랑의 기쁨") && target.GetNameInCode().Equals("luvericha"))
					{
						recoverAmount = recoverAmount / 2;
					}
					var recoverHealthCoroutine = target.RecoverHealth(recoverAmount);
					Debug.Log("recoverAmount : " + actualAmount);

					if (target == targets[targets.Count-1])
					{
						yield return battleManager.StartCoroutine(recoverHealthCoroutine);
					}
					else
					{
						battleManager.StartCoroutine(recoverHealthCoroutine);
					}

					Debug.Log("Apply " + recoverAmount + " heal to " + target.GetName());
				}
				
				if (appliedSkill.GetSkillApplyType() == SkillApplyType.HealAP)
				{
					// 스킬 기본 계수 계산
					float actualAmount = 0.0f;
					foreach (var powerFactor in appliedSkill.GetPowerFactorDict().Keys)
					{
						Stat stat = (Stat)Enum.Parse(typeof(Stat), powerFactor);
						if (stat.Equals(Stat.UsedAP))
						{
							actualAmount += selectedUnit.GetActualRequireSkillAP(appliedSkill) * appliedSkill.GetPowerFactor(stat, appliedSkill.GetLevel());
						}
						else if (stat.Equals(Stat.None))
						{
							actualAmount += appliedSkill.GetPowerFactor(stat, appliedSkill.GetLevel());
						}
						else
						{
							actualAmount += selectedUnit.GetActualStat(stat) * appliedSkill.GetPowerFactor(stat, appliedSkill.GetLevel());
						}
					}

					var recoverAmount = (int) actualAmount;
					var recoverAPCoroutine = target.RecoverAP(recoverAmount);
					Debug.Log("recoverAmount : " + actualAmount);

					if (target == targets[targets.Count-1])
					{
						yield return battleManager.StartCoroutine(recoverAPCoroutine);
					}
					else
					{
						battleManager.StartCoroutine(recoverAPCoroutine);
					}

					Debug.Log("Apply " + recoverAmount + " heal to " + target.GetName());
				}
				*/

				SkillLogicFactory.Get(appliedSkill).ActionInDamageRoutine(battleData, appliedSkill, selectedUnit, selectedTiles[0].GetComponent<Tile>(), selectedTiles);

				// 기술의 상태이상은 기술이 적용된 후에 붙인다.
				if(appliedSkill.GetStatusEffectList().Count > 0)
				{
					StatusEffector.AttachStatusEffect(appliedSkill, target);
				}
				
				selectedUnit.ActiveFalseAllBounsText();

				// 상태이상
				/*
				if (appliedSkill.GetSkillApplyType() == SkillApplyType.Buff)
				{
					if (appliedSkill.GetName().Equals("순간 재충전"))
					{
						target.GetUsedSkillDict().Clear();
					}
					else if(appliedSkill.GetStatusEffectList().Count > 0)
					{
						foreach (var statusEffect in appliedSkill.GetStatusEffectList())
						{
							// luvericha_m_1 조건 체크
							if (appliedSkill.GetName().Equals("교향곡:영웅") && target.GetNameInCode().Equals("luvericha")) continue;

							bool isInList = false;
							for (int i = 0; i < target.GetStatusEffectList().Count; i++)
							{
								if(statusEffect.IsSameStatusEffect(target.GetStatusEffectList()[i]))
								{
									isInList = true;
									target.GetStatusEffectList()[i].SetRemainPhase(statusEffect.GetRemainPhase());
									target.GetStatusEffectList()[i].SetRemainStack(statusEffect.GetRemainStack());
									if (statusEffect.IsOfType(StatusEffectType.Shield))
										target.GetStatusEffectList()[i].SetRemainAmount((int)(target.GetActualStat(statusEffect.GetAmountStat())*statusEffect.GetAmount(statusEffect.GetLevel())));
									break;
								}
							}
							if (!isInList)
							{
								if(statusEffect.IsOfType(StatusEffectType.Shield))
								{
									// luvericha_r_18 보호막 수치 계산
									if (appliedSkill.GetName().Equals("엘리제를 위하여"))
									{
										statusEffect.SetRemainAmount((int)(selectedUnit.GetActualStat(statusEffect.GetAmountStat())*statusEffect.GetAmount(statusEffect.GetLevel())));
									}
									// reina_r_30 보호막 수치 계산
									else if(appliedSkill.GetName().Equals("마법 보호막"))
									{
										statusEffect.SetRemainAmount((int)((float)selectedUnit.GetActualRequireSkillAP(appliedSkill) * statusEffect.GetAmount(statusEffect.GetLevel())));
									}
									else
									{
										statusEffect.SetRemainAmount((int)(target.GetActualStat(statusEffect.GetAmountStat())*statusEffect.GetAmount(statusEffect.GetLevel())));
									}
								}
								target.GetStatusEffectList().Add(statusEffect);
							}

							Debug.Log("Apply " + statusEffect.GetName() + " effect to " + target.name);
							Debug.Log("Amount : " + ((float)selectedUnit.GetActualRequireSkillAP(appliedSkill) * statusEffect.GetAmount(statusEffect.GetLevel())));
						}
					}
					yield return null;
				}
				// 스킬팩토리에서 처리 가능
				// luvericha_l_6 스킬 효과
				if (appliedSkill.GetName().Equals("정화된 밤"))
				{
					for (int i = 0; i < target.GetStatusEffectList().Count; i++)
					{
						if(target.GetStatusEffectList()[i].GetIsRemovable())
						{
							target.GetStatusEffectList().RemoveAt(i);
							break;
						}
					}
					
				}
				*/
			}

			// battleData.tileManager.DepaintTiles(selectedTiles, TileColor.Red);

			int requireAP = battleData.selectedUnitObject.GetComponent<Unit>().GetActualRequireSkillAP(appliedSkill);
			selectedUnit.UseActivityPoint(requireAP);
			// 스킬 쿨다운 기록
			if (appliedSkill.GetCooldown() > 0)
			{
				selectedUnit.GetUsedSkillDict().Add(appliedSkill.GetName(), appliedSkill.GetCooldown());
			}
			battleData.indexOfSeletedSkillByUser = 0; // return to init value.

			yield return new WaitForSeconds(0.5f);

			Camera.main.transform.position = new Vector3(battleData.selectedUnitObject.transform.position.x, battleData.selectedUnitObject.transform.position.y, -10);
			battleData.currentState = CurrentState.FocusToUnit;
			battleData.alreadyMoved = false;
		}

		private static IEnumerator ApplySkillEffect(Skill appliedSkill, GameObject unitObject, List<GameObject> selectedTiles)
		{
			string effectName = appliedSkill.GetEffectName();
			if (effectName == "-")
			{
				Debug.Log("There is no effect for " + appliedSkill.GetName());
				yield break;
			}

			EffectVisualType effectVisualType = appliedSkill.GetEffectVisualType();
			EffectMoveType effectMoveType = appliedSkill.GetEffectMoveType();

			if ((effectVisualType == EffectVisualType.Area) && (effectMoveType == EffectMoveType.Move))
			{
				// 투사체, 범위형 이펙트.
				Vector3 startPos = unitObject.transform.position;
				Vector3 endPos = new Vector3(0, 0, 0);
				foreach (var tile in selectedTiles)
				{
					endPos += tile.transform.position;
				}
				endPos = endPos / (float)selectedTiles.Count;

				GameObject particle = GameObject.Instantiate(Resources.Load("Particle/" + effectName)) as GameObject;
				particle.transform.position = startPos - new Vector3(0, -0.5f, 0.01f);
				yield return new WaitForSeconds(0.2f);
				// 타일 축 -> 유닛 축으로 옮기기 위해 z축으로 5만큼 앞으로 빼준다.
				// 유닛의 중앙 부분을 공격하기 위하여 y축으고 0.5 올린다.
				iTween.MoveTo(particle, endPos - new Vector3(0, 0, 0.01f) - new Vector3(0, -0.5f, 5f), 0.5f);
				yield return new WaitForSeconds(0.3f);
				GameObject.Destroy(particle, 0.5f);
				yield return null;
			}
			else if ((effectVisualType == EffectVisualType.Area) && (effectMoveType == EffectMoveType.NonMove))
			{
				// 고정형, 범위형 이펙트.
				Vector3 targetPos = new Vector3(0, 0, 0);
				foreach (var tile in selectedTiles)
				{
					targetPos += tile.transform.position;
				}
				targetPos = targetPos / (float)selectedTiles.Count;
				targetPos = targetPos - new Vector3(0, -0.5f, 5f); // 타일 축 -> 유닛 축으로 옮기기 위해 z축으로 5만큼 앞으로 빼준다.

				GameObject particlePrefab =  Resources.Load("Particle/" + effectName) as GameObject;
				if (particlePrefab == null)
				{
					Debug.LogError("Cannot load particle " + effectName);
				}
				GameObject particle = GameObject.Instantiate(particlePrefab) as GameObject;
				particle.transform.position = targetPos - new Vector3(0, -0.5f, 0.01f);
				yield return new WaitForSeconds(0.5f);
				GameObject.Destroy(particle, 0.5f);
				yield return null;
			}
			else if ((effectVisualType == EffectVisualType.Individual) && (effectMoveType == EffectMoveType.NonMove))
			{
				// 고정형, 개별 대상 이펙트.
				List<Vector3> targetPosList = new List<Vector3>();
				foreach (var tileObject in selectedTiles)
				{
					Tile tile = tileObject.GetComponent<Tile>();
					if (tile.IsUnitOnTile())
					{
						targetPosList.Add(tile.GetUnitOnTile().transform.position);
					}
				}

				foreach (var targetPos in targetPosList)
				{
					GameObject particle = GameObject.Instantiate(Resources.Load("Particle/" + effectName)) as GameObject;
					particle.transform.position = targetPos - new Vector3(0, -0.5f, 0.01f);
					GameObject.Destroy(particle, 0.5f + 0.3f); // 아랫줄에서의 지연시간을 고려한 값이어야 함.
				}
				if (targetPosList.Count == 0) // 대상이 없을 경우. 일단 가운데 이펙트를 띄운다.
				{
					Vector3 midPos = new Vector3(0, 0, 0);
					foreach (var tile in selectedTiles)
					{
						midPos += tile.transform.position;
					}
					midPos = midPos / (float)selectedTiles.Count;

					GameObject particle = GameObject.Instantiate(Resources.Load("Particle/" + effectName)) as GameObject;
					particle.transform.position = midPos - new Vector3(0, -0.5f, 0.01f);
					GameObject.Destroy(particle, 0.5f + 0.3f); // 아랫줄에서의 지연시간을 고려한 값이어야 함.
				}

				yield return new WaitForSeconds(0.5f);
			}
		}
	}
}
