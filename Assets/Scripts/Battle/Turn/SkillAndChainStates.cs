﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Enums;

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
					int requireAP = preSelectedSkill.GetRequireAP(preSelectedSkill.GetLevel());
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
				if (skillTypeOfSelectedSkill == SkillType.Auto || skillTypeOfSelectedSkill == SkillType.Self)
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
			while (true)
			{
				Direction newDirection = Utility.GetMouseDirectionByUnit(battleData.selectedUnitObject);
				Direction beforeDirection = battleData.SelectedUnit.GetDirection();
				var selectedTiles = battleData.tileManager.GetTilesInRange(battleData.SelectedSkill.GetSecondRangeForm(),
																battleData.SelectedUnit.GetPosition(),
																battleData.SelectedSkill.GetSecondMinReach(),
																battleData.SelectedSkill.GetSecondMaxReach(),
																battleData.SelectedUnit.GetDirection());

				if (beforeDirection != newDirection)
				{
					battleData.tileManager.DepaintTiles(selectedTiles, TileColor.Red);

					beforeDirection = newDirection;
					battleData.SelectedUnit.SetDirection(newDirection);

					battleData.tileManager.PaintTiles(selectedTiles, TileColor.Red);
				}
				yield return null;
			}
		}

		public static IEnumerator SelectSkillApplyDirection(BattleData battleData, Direction originalDirection)
		{
			Direction beforeDirection = originalDirection;
			List<GameObject> selectedTiles = new List<GameObject>();
			Unit selectedUnit = battleData.selectedUnitObject.GetComponent<Unit>();
			Skill selectedSkill = battleData.SelectedSkill;

			battleData.isWaitingUserInput = true;
			battleData.isPreSeletedTileByUser = false;

			if (battleData.currentState == CurrentState.SelectSkill)
			{
				battleData.uiManager.DisableCancelButtonUI();
				yield break;
			}

			if (battleData.currentState == CurrentState.SelectSkillApplyDirection)
			{
				selectedTiles = battleData.tileManager.GetTilesInRange(selectedSkill.GetSecondRangeForm(),
															selectedUnit.GetPosition(),
															selectedSkill.GetSecondMinReach(),
															selectedSkill.GetSecondMaxReach(),
															selectedUnit.GetDirection());

				battleData.tileManager.PaintTiles(selectedTiles, TileColor.Red);
			}

			var update = UpdateRangeSkillMouseDirection(battleData);
			battleData.battleManager.StartCoroutine(update);
			yield return battleData.battleManager.StartCoroutine(EventTrigger.WaitOr(
				battleData.triggers.rightClicked,
				battleData.triggers.cancelClicked,
				battleData.triggers.selectedTileByUser
			));
			battleData.battleManager.StopCoroutine(update);

			battleData.isPreSeletedTileByUser = false;
			battleData.isWaitingUserInput = false;
			battleData.uiManager.DisableCancelButtonUI();
			selectedTiles = battleData.tileManager.GetTilesInRange(selectedSkill.GetSecondRangeForm(),
											selectedUnit.GetPosition(),
											selectedSkill.GetSecondMinReach(),
											selectedSkill.GetSecondMaxReach(),
											selectedUnit.GetDirection());
			battleData.tileManager.DepaintTiles(selectedTiles, TileColor.Red);

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
				yield return battleManager.StartCoroutine(CheckApplyOrChain(battleData, battleData.SelectedUnitTile, originalDirection));
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
				activeRange = battleData.tileManager.GetTilesInRange(selectedSkill.GetFirstRangeForm(),
														selectedUnitPos,
														selectedSkill.GetFirstMinReach(),
														selectedSkill.GetFirstMaxReach(),
														battleData.selectedUnitObject.GetComponent<Unit>().GetDirection());
				battleData.tileManager.PaintTiles(activeRange, TileColor.Red);

				battleData.uiManager.EnableCancelButtonUI();

				battleData.isWaitingUserInput = true;
				battleData.isPreSeletedTileByUser = false;

				var update = UpdatePointSkillMouseDirection(battleData);
				battleData.battleManager.StartCoroutine(update);
				yield return battleData.battleManager.StartCoroutine(EventTrigger.WaitOr(
					battleData.triggers.selectedTileByUser,
					battleData.triggers.rightClicked,
					battleData.triggers.cancelClicked
				));
				battleData.battleManager.StopCoroutine(update);
				battleData.isPreSeletedTileByUser = false;
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

				// 타겟팅 스킬을 타겟이 없는 장소에 지정했을 경우 적용되지 않도록 예외처리 필요 - 대부분의 스킬은 논타겟팅. 추후 보강.
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

				//데미지 미리보기
				Dictionary<GameObject, float> calculatedTotalDamage = CalculateTotalDamage(battleData, tilesInSkillRange);
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
					if (selectedSkill.GetSkillType() == SkillType.Auto)
						battleData.currentState = CurrentState.SelectSkill;
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
					// 왜 CheckChainPossible 안 쓴 거죠...? 일단 발표 끝나고 고칠랭
					if (selectedSkill.GetSkillApplyType() == SkillApplyType.DamageHealth
					 || selectedSkill.GetSkillApplyType() == SkillApplyType.Debuff)
					{
						yield return ApplyChain(battleData, tilesInSkillRange);
						FocusUnit(battleData.SelectedUnit);
						battleData.currentState = CurrentState.FocusToUnit;
					}
					// 체인이 불가능한 스킬일 경우, 그냥 발동.
					else
					{
						battleData.currentState = CurrentState.ApplySkill;
						yield return battleManager.StartCoroutine(ApplySkill(battleData, tilesInSkillRange));
					}
				}
				else if (battleData.skillApplyCommand == SkillApplyCommand.Chain)
				{
					battleData.skillApplyCommand = SkillApplyCommand.Waiting;
					battleData.currentState = CurrentState.ChainAndStandby;
					yield return battleManager.StartCoroutine(ChainAndStandby(battleData, tilesInSkillRange));
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

		public static Dictionary<GameObject, float> CalculateTotalDamage(BattleData battleData, List<GameObject> tilesInSkillRange)
		{
			// List<ChainInfo> tempChainList = new List<ChainInfo>();
			Dictionary<GameObject, float> damageList = new Dictionary<GameObject, float>();

			ChainList.AddChains(battleData.selectedUnitObject, tilesInSkillRange, battleData.indexOfSeletedSkillByUser);

			List<ChainInfo> allVaildChainInfo = ChainList.GetAllChainInfoToTargetArea(battleData.selectedUnitObject, tilesInSkillRange);
			int chainCombo = allVaildChainInfo.Count;

			foreach (var chainInfo in allVaildChainInfo)
			{
				CalculateDamageOfEachSkill(battleData, chainInfo, chainCombo, damageList);
			}

			ChainList.RemoveChainsFromUnit(battleData.selectedUnitObject);

			return damageList;
		}

		static void CalculateDamageOfEachSkill(BattleData battleData, ChainInfo chainInfo, int chainCombo, Dictionary<GameObject, float> damageList)
		{
			GameObject unitObjectInChain = chainInfo.GetUnit();
			Unit unitInChain = unitObjectInChain.GetComponent<Unit>();
			Skill appliedSkill = unitInChain.GetSkillList()[chainInfo.GetSkillIndex() - 1];
			List<GameObject> selectedTiles = chainInfo.GetTargetArea();

			Direction oldDirection = unitInChain.GetDirection();

			// 시전 방향으로 유닛의 바라보는 방향을 돌림.
			if (appliedSkill.GetSkillType() != SkillType.Auto)
				unitInChain.SetDirection(Utility.GetDirectionToTarget(unitInChain.gameObject, selectedTiles));

			List<GameObject> targets = new List<GameObject>();

			foreach (var tileObject in selectedTiles)
			{
				Tile tile = tileObject.GetComponent<Tile>();
				if (tile.IsUnitOnTile())
				{
					targets.Add(tile.GetUnitOnTile());
				}
			}

			foreach (var target in targets)
			{
				Unit targetUnit = target.GetComponent<Unit>();
				// 방향 체크.
				Utility.GetDegreeAtAttack(unitObjectInChain, target);
				BattleManager battleManager = battleData.battleManager;
				if (appliedSkill.GetSkillApplyType() == SkillApplyType.DamageHealth)
				{
					// sisterna_r_6의 타일 속성 판정
					if (appliedSkill.GetName().Equals("전자기학"))
					{
						Element tileElement = battleData.tileManager.GetTile(targetUnit.GetPosition()).GetComponent<Tile>().GetTileElement();
						if(!tileElement.Equals(Element.Metal) && !tileElement.Equals(Element.Water)) continue;
					}

					// 스킬 기본 대미지 계산
					float baseDamage = 0.0f;
					foreach (var powerFactor in appliedSkill.GetPowerFactorDict().Keys)
					{
						Stat stat = (Stat)Enum.Parse(typeof(Stat), powerFactor);
						if (stat.Equals(Stat.UsedAP))
						{
							baseDamage += unitInChain.GetActualRequireSkillAP(appliedSkill) * appliedSkill.GetPowerFactor(stat, appliedSkill.GetLevel());
						}
						else if (stat.Equals(Stat.None))
						{
							baseDamage += appliedSkill.GetPowerFactor(stat, appliedSkill.GetLevel());
						}
						else
						{
							baseDamage += unitInChain.GetActualStat(stat) * appliedSkill.GetPowerFactor(stat, appliedSkill.GetLevel());
						}
					}

					// 방향 보너스.
					float directionBonus = Utility.GetDirectionBonus(unitObjectInChain, target);
					// if (directionBonus > 1f) unitInChain.PrintDirectionBonus(directionBonus);

					// 천체속성 보너스.
					float celestialBonus = Utility.GetCelestialBonus(unitObjectInChain, target);
					// if (celestialBonus > 1f) unitInChain.PrintCelestialBonus();
					// else if (celestialBonus == 0.8f) targetUnit.PrintCelestialBonus();

					// 체인 보너스.
					float chainBonus = battleData.GetChainDamageFactorFromChainCombo(chainCombo);
					// if (directionBonus > 1f) unitInChain.PrintDirectionBonus(chainCombo);

					// 강타 효과에 의한 대미지 추가
					int smiteAmount = 0;
					if (unitInChain.HasStatusEffect(StatusEffectType.Smite))
					{
						smiteAmount = (int) unitInChain.GetActualEffect((float)smiteAmount, StatusEffectType.Smite);
					}

					var damageAmount = (baseDamage * directionBonus * celestialBonus * chainBonus) + (float) smiteAmount;
					Debug.Log("baseDamage : " + baseDamage);
					Debug.Log("directionBonus : " + directionBonus);
					Debug.Log("celestialBonus : " + celestialBonus);
					Debug.Log("chainBonus : " + chainBonus);
					Debug.Log("smiteAmount : " + smiteAmount);

					// reina_m_12 속성 보너스
					if (appliedSkill.GetName().Equals("불의 파동") && targetUnit.GetElement().Equals(Element.Plant))
					{
						float[] elementDamageBonus = new float[]{1.1f, 1.2f, 1.3f, 1.4f, 1.5f};
						damageAmount = damageAmount * elementDamageBonus[0];
					}

					// yeong_l_18 대상 숫자 보너스
					if (appliedSkill.GetName().Equals("섬광 찌르기") && targets.Count > 1)
					{
						float targetNumberBonus = (float)targets.Count*0.1f + 1.0f;
						damageAmount = damageAmount * targetNumberBonus;
					}

					//유닛과 유닛의 데미지를 Dictionary에 추가.
					float actualDamage = targetUnit.GetActualDamage(unitInChain.GetUnitClass(), damageAmount, appliedSkill.GetPenetration(appliedSkill.GetLevel()), false, true);
					if (!damageList.ContainsKey(targetUnit.gameObject))
					{
						damageList.Add(targetUnit.gameObject, actualDamage);
					}
					else
					{
						float totalDamage = damageList[targetUnit.gameObject];
						totalDamage += actualDamage;
						damageList.Remove(targetUnit.gameObject);
						damageList.Add(targetUnit.gameObject, totalDamage);
					}

					// targetUnit이 반사 효과를 지니고 있을 경우 반사 대미지 코루틴 준비
					// 반사 미적용.

					Debug.Log("Apply " + damageAmount + " damage to " + targetUnit.GetName() + "\n" +
								"ChainCombo : " + chainCombo);
				}
			}

			unitInChain.SetDirection(oldDirection);
		}

		public static IEnumerator ApplyChain(BattleData battleData, List<GameObject> tilesInSkillRange)
		{
			BattleManager battleManager = battleData.battleManager;
			// 자기 자신을 체인 리스트에 추가.
			ChainList.AddChains(battleData.selectedUnitObject, tilesInSkillRange, battleData.indexOfSeletedSkillByUser);
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

		private static IEnumerator ChainAndStandby(BattleData battleData, List<GameObject> selectedTiles)
		{
			battleData.tileManager.DepaintTiles(selectedTiles, TileColor.Red);

			// 방향 돌리기.
			battleData.selectedUnitObject.GetComponent<Unit>().SetDirection(Utility.GetDirectionToTarget(battleData.selectedUnitObject, selectedTiles));

			// sisterna_m_30 대상 AP 선 계산
			if(battleData.SelectedSkill.GetName().Equals("조화진동"))
			{
				int enemyCurrentAP = selectedTiles[0].GetComponent<Unit>().GetCurrentActivityPoint();
				int requireAP = Math.Min(enemyCurrentAP, battleData.selectedUnitObject.GetComponent<Unit>().GetActualRequireSkillAP(battleData.SelectedSkill));
				battleData.selectedUnitObject.GetComponent<Unit>().UseActivityPoint(requireAP);
			}
			else
			{
				// 기타 스킬 AP 사용 선차감
				int requireAP = battleData.selectedUnitObject.GetComponent<Unit>().GetActualRequireSkillAP(battleData.SelectedSkill);
				battleData.selectedUnitObject.GetComponent<Unit>().UseActivityPoint(requireAP);
			}
			// 스킬 쿨다운 기록
			if (battleData.SelectedSkill.GetCooldown(battleData.SelectedSkill.GetLevel()) > 0)
			{
				battleData.selectedUnitObject.GetComponent<Unit>().GetUsedSkillDict().Add(battleData.SelectedSkill.GetName(), battleData.SelectedSkill.GetCooldown(battleData.SelectedSkill.GetLevel()));
			}

			// 체인 목록에 추가.
			ChainList.AddChains(battleData.selectedUnitObject, selectedTiles, battleData.indexOfSeletedSkillByUser);
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
			Skill appliedSkill = unitInChain.GetSkillList()[chainInfo.GetSkillIndex() - 1];
			List<GameObject> selectedTiles = chainInfo.GetTargetArea();

			// 시전 방향으로 유닛의 바라보는 방향을 돌림.
			if (appliedSkill.GetSkillType() != SkillType.Auto)
				unitInChain.SetDirection(Utility.GetDirectionToTarget(unitInChain.gameObject, selectedTiles));

			// 자신의 체인 정보 삭제.
			ChainList.RemoveChainsFromUnit(unitObjectInChain);

			BattleManager battleManager = battleData.battleManager;
			// 이펙트 임시로 비활성화.
			yield return battleManager.StartCoroutine(ApplySkillEffect(appliedSkill, unitInChain.gameObject, selectedTiles));

			List<GameObject> targets = new List<GameObject>();

			foreach (var tileObject in selectedTiles)
			{
				Tile tile = tileObject.GetComponent<Tile>();
				if (tile.IsUnitOnTile())
				{
					targets.Add(tile.GetUnitOnTile());
				}
			}

			foreach (var target in targets)
			{
				Unit targetUnit = target.GetComponent<Unit>();
				Debug.Log("Total target No : "+targets.Count+" Current Target : "+targetUnit.GetName());
				// 방향 체크.
				Utility.GetDegreeAtAttack(unitObjectInChain, target);
				if (appliedSkill.GetSkillApplyType() == SkillApplyType.DamageHealth)
				{
					// sisterna_r_6의 타일 속성 판정
					if (appliedSkill.GetName().Equals("전자기학"))
					{
						Element tileElement = battleData.tileManager.GetTile(targetUnit.GetPosition()).GetComponent<Tile>().GetTileElement();
						if(!tileElement.Equals(Element.Metal) && !tileElement.Equals(Element.Water)) continue;
					}
					// 스킬 기본 대미지 계산
					float baseDamage = 0.0f;
					foreach (var powerFactor in appliedSkill.GetPowerFactorDict().Keys)
					{
						Stat stat = (Stat)Enum.Parse(typeof(Stat), powerFactor);
						if (stat.Equals(Stat.UsedAP))
						{
							baseDamage += unitInChain.GetActualRequireSkillAP(appliedSkill) * appliedSkill.GetPowerFactor(stat, appliedSkill.GetLevel());
						}
						else if (stat.Equals(Stat.None))
						{
							baseDamage += appliedSkill.GetPowerFactor(stat, appliedSkill.GetLevel());
						}
						else
						{
							baseDamage += unitInChain.GetActualStat(stat) * appliedSkill.GetPowerFactor(stat, appliedSkill.GetLevel());
						}
					}

					// 방향 보너스.
					float directionBonus = Utility.GetDirectionBonus(unitObjectInChain, target);
					if (directionBonus > 1f) unitInChain.PrintDirectionBonus(directionBonus);

					// 천체속성 보너스.
					float celestialBonus = Utility.GetCelestialBonus(unitObjectInChain, target);
					if (celestialBonus > 1f) unitInChain.PrintCelestialBonus();
					// '보너스'만 표시하려고 임시로 주석처리.
					// else if (celestialBonus == 0.8f) targetUnit.PrintCelestialBonus();

					// 체인 보너스.
					float chainBonus = battleData.GetChainDamageFactorFromChainCombo(chainCombo);
					if (chainBonus > 1f) unitInChain.PrintChainBonus(chainCombo);

					// 강타 효과에 의한 대미지 추가
					int smiteAmount = 0;
					if (unitInChain.HasStatusEffect(StatusEffectType.Smite))
					{
						smiteAmount = (int) unitInChain.GetActualEffect((float)smiteAmount, StatusEffectType.Smite);
					}

					var damageAmount = (baseDamage * directionBonus * celestialBonus * chainBonus) + (float) smiteAmount;
					Debug.Log("baseDamage : " + baseDamage);
					Debug.Log("directionBonus : " + directionBonus);
					Debug.Log("celestialBonus : " + celestialBonus);
					Debug.Log("chainBonus : " + chainBonus);
					Debug.Log("smiteAmount : " + smiteAmount);

					// reina_m_12 속성 보너스
					if (appliedSkill.GetName().Equals("불의 파동") && targetUnit.GetElement().Equals(Element.Plant))
					{
						float[] elementDamageBonus = new float[]{1.1f, 1.2f, 1.3f, 1.4f, 1.5f};
						damageAmount = damageAmount * elementDamageBonus[0];
					}

					// yeong_l_18 대상 숫자 보너스
					if (appliedSkill.GetName().Equals("섬광 찌르기") && targets.Count > 1)
					{
						float targetNumberBonus = (float)targets.Count*0.1f + 1.0f;
						damageAmount = damageAmount * targetNumberBonus;
					}

					var damageCoroutine = targetUnit.Damaged(unitInChain.GetUnitClass(), damageAmount, appliedSkill.GetPenetration(appliedSkill.GetLevel()), false, true);

					// targetUnit이 반사 효과를 지니고 있을 경우 반사 대미지 코루틴 준비
					if (targetUnit.HasStatusEffect(StatusEffectType.Reflect))
					{
						float reflectAmount = damageAmount;
						foreach (var statusEffect in targetUnit.GetStatusEffectList())
						{
							if (statusEffect.IsOfType(StatusEffectType.Reflect))
							{
								reflectAmount = reflectAmount * statusEffect.GetDegree(statusEffect.GetLevel());
								break;
							}
						}

						var reflectCoroutine = unitInChain.Damaged(targetUnit.GetUnitClass(), reflectAmount, appliedSkill.GetPenetration(appliedSkill.GetLevel()), false, true);
						battleManager.StartCoroutine(reflectCoroutine);
					}

					// 상태이상 적용
					if(appliedSkill.GetStatusEffectList().Count > 0)
					{
						foreach (var statusEffect in appliedSkill.GetStatusEffectList())
						{
							bool isInList = false;
							for (int i = 0; i < targetUnit.GetStatusEffectList().Count; i++)
							{
								if(statusEffect.IsSameStatusEffect(targetUnit.GetStatusEffectList()[i]) && !statusEffect.GetIsStackable())
								{
									isInList = true;
									targetUnit.GetStatusEffectList()[i].SetRemainPhase(statusEffect.GetRemainPhase());
									targetUnit.GetStatusEffectList()[i].SetRemainStack(statusEffect.GetRemainStack());
									break;
								}
							}
							if (!isInList) targetUnit.GetStatusEffectList().Add(statusEffect);
							Debug.Log("Apply " + statusEffect.GetName() + " effect to " + targetUnit.name);
						}
					}

					if (target == targets[targets.Count-1])
					{
						yield return battleManager.StartCoroutine(damageCoroutine);
					}
					else
					{
						battleManager.StartCoroutine(damageCoroutine);
					}

					// eren_l_30 회복
					if (appliedSkill.GetName().Equals("생명력 흡수"))
					{
						int finalDamage = (int)CalculateTotalDamage(battleData, selectedTiles)[target];
						int targetCurrentHealth = targetUnit.GetCurrentHealth();
						float[] recoverFactor = new float[5] {0.3f, 0.35f, 0.4f, 0.45f, 0.5f};
						int recoverAmount = (int) ((float) finalDamage * recoverFactor[appliedSkill.GetLevel()-1]);
						if (targetCurrentHealth == 0) recoverAmount *= 2;
						var recoverCoroutine = unitInChain.RecoverHealth(recoverAmount);
						battleManager.StartCoroutine(recoverCoroutine);
					}

					Debug.Log("Apply " + damageAmount + " damage to " + targetUnit.GetName() + "\n" +
								"ChainCombo : " + chainCombo);
				}

				else if (appliedSkill.GetSkillApplyType() == SkillApplyType.Debuff)
				{
					Debug.Log("Using skill "+appliedSkill.GetName());
					// 상태이상 적용
					if(appliedSkill.GetStatusEffectList().Count > 0)
					{
						foreach (var statusEffect in appliedSkill.GetStatusEffectList())
						{
							bool isInList = false;
							for (int i = 0; i < targetUnit.GetStatusEffectList().Count; i++)
							{
								if(statusEffect.IsSameStatusEffect(targetUnit.GetStatusEffectList()[i]) && !statusEffect.GetIsStackable())
								{
									isInList = true;
									targetUnit.GetStatusEffectList()[i].SetRemainPhase(statusEffect.GetRemainPhase());
									targetUnit.GetStatusEffectList()[i].SetRemainStack(statusEffect.GetRemainStack());
									break;
								}
							}
							if (!isInList)
							{
								if (statusEffect.GetName().Equals("상자에 든 고양이")) 
									statusEffect.SetRemainAmount((int)(unitInChain.GetActualStat(Stat.Power)*statusEffect.GetAmount(appliedSkill.GetLevel())));
								targetUnit.GetStatusEffectList().Add(statusEffect);
							}
							Debug.Log("Apply " + statusEffect.GetName() + " effect to " + targetUnit.name);
						}
					}

					yield return null;
				}

				unitInChain.ActiveFalseAllBounsText();
			}

			int requireAP = battleData.selectedUnitObject.GetComponent<Unit>().GetActualRequireSkillAP(appliedSkill);
			if (unitInChain.gameObject == battleData.selectedUnitObject)
			{
				unitInChain.UseActivityPoint(requireAP); // 즉시시전 대상만 ap를 차감. 나머지는 선차감되었으므로 패스.
				// 스킬 쿨다운 기록
				if (appliedSkill.GetCooldown(appliedSkill.GetLevel()) > 0)
				{
					unitInChain.GetUsedSkillDict().Add(appliedSkill.GetName(), appliedSkill.GetCooldown(appliedSkill.GetLevel()));
				}
			}
			battleData.indexOfSeletedSkillByUser = 0; // return to init value.

			yield return new WaitForSeconds(0.5f);

			battleData.alreadyMoved = false;
		}

		// 체인 불가능 스킬일 경우의 스킬 시전 코루틴. 스킬 적용 범위만 받는다.
		private static IEnumerator ApplySkill(BattleData battleData, List<GameObject> selectedTiles)
		{
			Unit selectedUnit = battleData.selectedUnitObject.GetComponent<Unit>();
			Skill appliedSkill = battleData.SelectedSkill;
			BattleManager battleManager = battleData.battleManager;

			// 시전 방향으로 유닛의 바라보는 방향을 돌림.
			if (appliedSkill.GetSkillType() != SkillType.Auto)
				selectedUnit.SetDirection(Utility.GetDirectionToTarget(selectedUnit.gameObject, selectedTiles));

			// 이펙트 임시로 비활성화.
			yield return battleManager.StartCoroutine(ApplySkillEffect(appliedSkill, battleData.selectedUnitObject, selectedTiles));

			List<GameObject> targets = new List<GameObject>();

			foreach (var tileObject in selectedTiles)
			{
				Tile tile = tileObject.GetComponent<Tile>();
				if (tile.IsUnitOnTile())
				{
					targets.Add(tile.GetUnitOnTile());
				}
			}

			foreach (var target in targets)
			{
				Unit targetUnit = target.GetComponent<Unit>();

				if (appliedSkill.GetSkillApplyType() == SkillApplyType.DamageAP)
				{
					// kashasti_l_12
					if(appliedSkill.GetName().Equals("이매진 불릿"))
					{
						float[] apDamage = new float[5] {32.0f, 44.0f, 57.0f, 69.0f, 81.0f};
						var damageCoroutine = targetUnit.Damaged(UnitClass.None, apDamage[appliedSkill.GetLevel()-1], appliedSkill.GetPenetration(appliedSkill.GetLevel()), false, false);
						battleManager.StartCoroutine(damageCoroutine);
					}
					// luvericha_l_30
					else if (appliedSkill.GetName().Equals("에튀드:겨울바람"))
					{
						int apDamage = targetUnit.GetCurrentActivityPoint()*(int)appliedSkill.GetPowerFactor(Stat.None, appliedSkill.GetLevel());
						var damageCoroutine = targetUnit.Damaged(UnitClass.None, apDamage, 0.0f, false, false);
						battleManager.StartCoroutine(damageCoroutine);
					}
				}

				else if (appliedSkill.GetSkillApplyType() == SkillApplyType.HealHealth)
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
					if (appliedSkill.GetName().Equals("사랑의 기쁨") && targetUnit.GetNameInCode().Equals("luvericha"))
					{
						recoverAmount = recoverAmount / 2;
					}
					var recoverHealthCoroutine = targetUnit.RecoverHealth(recoverAmount);
					Debug.Log("recoverAmount : " + actualAmount);

					// 상태이상 적용
					if(appliedSkill.GetStatusEffectList().Count > 0)
					{
						foreach (var statusEffect in appliedSkill.GetStatusEffectList())
						{
							bool isInList = false;
							for (int i = 0; i < targetUnit.GetStatusEffectList().Count; i++)
							{
								if(statusEffect.IsSameStatusEffect(targetUnit.GetStatusEffectList()[i]) && !statusEffect.GetIsStackable())
								{
									isInList = true;
									targetUnit.GetStatusEffectList()[i].SetRemainPhase(statusEffect.GetRemainPhase());
									targetUnit.GetStatusEffectList()[i].SetRemainStack(statusEffect.GetRemainStack());
									if (statusEffect.IsOfType(StatusEffectType.Shield))
										targetUnit.GetStatusEffectList()[i].SetRemainAmount((int)(targetUnit.GetActualStat(statusEffect.GetAmountStat())*statusEffect.GetAmount(statusEffect.GetLevel())));
									break;
								}
							}
							if (!isInList)
							{
								targetUnit.GetStatusEffectList().Add(statusEffect);
								if (statusEffect.IsOfType(StatusEffectType.Shield))
								{
									targetUnit.GetStatusEffectList()[targetUnit.GetStatusEffectList().Count].SetRemainAmount
									((int)(targetUnit.GetActualStat(statusEffect.GetAmountStat())*statusEffect.GetAmount(statusEffect.GetLevel())));
								}
							}
							Debug.Log("Apply " + statusEffect.GetName() + " effect to " + targetUnit.name);
						}
					}

					if (target == targets[targets.Count-1])
					{
						yield return battleManager.StartCoroutine(recoverHealthCoroutine);
					}
					else
					{
						battleManager.StartCoroutine(recoverHealthCoroutine);
					}

					Debug.Log("Apply " + recoverAmount + " heal to " + targetUnit.GetName());
				}
				else if (appliedSkill.GetSkillApplyType() == SkillApplyType.HealAP)
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
					var recoverAPCoroutine = targetUnit.RecoverAP(recoverAmount);
					Debug.Log("recoverAmount : " + actualAmount);

					// 상태이상 적용
					if(appliedSkill.GetStatusEffectList().Count > 0)
					{
						foreach (var statusEffect in appliedSkill.GetStatusEffectList())
						{
							bool isInList = false;
							for (int i = 0; i < targetUnit.GetStatusEffectList().Count; i++)
							{
								if(statusEffect.IsSameStatusEffect(targetUnit.GetStatusEffectList()[i]) && !statusEffect.GetIsStackable())
								{
									isInList = true;
									targetUnit.GetStatusEffectList()[i].SetRemainPhase(statusEffect.GetRemainPhase());
									targetUnit.GetStatusEffectList()[i].SetRemainStack(statusEffect.GetRemainStack());
									if (statusEffect.IsOfType(StatusEffectType.Shield))
										targetUnit.GetStatusEffectList()[i].SetRemainAmount((int)(targetUnit.GetActualStat(statusEffect.GetAmountStat())*statusEffect.GetAmount(statusEffect.GetLevel())));
									break;
								}
							}
							if (!isInList)
							{
								if (statusEffect.IsOfType(StatusEffectType.Shield))
								{
									targetUnit.GetStatusEffectList()[targetUnit.GetStatusEffectList().Count].SetRemainAmount
									((int)(targetUnit.GetActualStat(statusEffect.GetAmountStat())*statusEffect.GetAmount(statusEffect.GetLevel())));
								}
								targetUnit.GetStatusEffectList().Add(statusEffect);
							}
							Debug.Log("Apply " + statusEffect.GetName() + " effect to " + targetUnit.name);
						}
					}

					if (target == targets[targets.Count-1])
					{
						yield return battleManager.StartCoroutine(recoverAPCoroutine);
					}
					else
					{
						battleManager.StartCoroutine(recoverAPCoroutine);
					}

					Debug.Log("Apply " + recoverAmount + " heal to " + targetUnit.GetName());
				}
				else if (appliedSkill.GetSkillApplyType() == SkillApplyType.Buff)
				{
					if (appliedSkill.GetName().Equals("순간 재충전"))
					{
						targetUnit.GetUsedSkillDict().Clear();
					}
					else if(appliedSkill.GetStatusEffectList().Count > 0)
					{
						foreach (var statusEffect in appliedSkill.GetStatusEffectList())
						{
							// luvericha_m_1 조건 체크
							if (appliedSkill.GetName().Equals("교향곡:영웅") && targetUnit.GetNameInCode().Equals("luvericha")) continue;

							bool isInList = false;
							for (int i = 0; i < targetUnit.GetStatusEffectList().Count; i++)
							{
								if(statusEffect.IsSameStatusEffect(targetUnit.GetStatusEffectList()[i]))
								{
									isInList = true;
									targetUnit.GetStatusEffectList()[i].SetRemainPhase(statusEffect.GetRemainPhase());
									targetUnit.GetStatusEffectList()[i].SetRemainStack(statusEffect.GetRemainStack());
									if (statusEffect.IsOfType(StatusEffectType.Shield))
										targetUnit.GetStatusEffectList()[i].SetRemainAmount((int)(targetUnit.GetActualStat(statusEffect.GetAmountStat())*statusEffect.GetAmount(statusEffect.GetLevel())));
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
										statusEffect.SetRemainAmount((int)(targetUnit.GetActualStat(statusEffect.GetAmountStat())*statusEffect.GetAmount(statusEffect.GetLevel())));
									}
								}
								targetUnit.GetStatusEffectList().Add(statusEffect);
							}

							Debug.Log("Apply " + statusEffect.GetName() + " effect to " + targetUnit.name);
							Debug.Log("Amount : " + ((float)selectedUnit.GetActualRequireSkillAP(appliedSkill) * statusEffect.GetAmount(statusEffect.GetLevel())));
						}
					}
					yield return null;
				}
				// luvericha_l_6 스킬 효과
				else if (appliedSkill.GetName().Equals("정화된 밤"))
				{
					for (int i = 0; i < targetUnit.GetStatusEffectList().Count; i++)
					{
						if(targetUnit.GetStatusEffectList()[i].GetIsRemovable())
						{
							targetUnit.GetStatusEffectList().RemoveAt(i);
							break;
						}
					}
					
				}
			}

			battleData.tileManager.DepaintTiles(selectedTiles, TileColor.Red);

			int requireAP = battleData.selectedUnitObject.GetComponent<Unit>().GetActualRequireSkillAP(appliedSkill);
			selectedUnit.UseActivityPoint(requireAP);
			// 스킬 쿨다운 기록
			if (appliedSkill.GetCooldown(appliedSkill.GetLevel()) > 0)
			{
				selectedUnit.GetUsedSkillDict().Add(appliedSkill.GetName(), appliedSkill.GetCooldown(appliedSkill.GetLevel()));
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
