using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enums;
using Battle.Skills;
using Battle.Damage;

namespace Battle.Turn {
    public class SkillAndChainStates {
        private static IEnumerator UpdatePreviewAP( ) {
            while (true) {
                if (BattleData.indexOfPreSelectedSkillByUser != 0) {
                    ActiveSkill preSelectedSkill = BattleData.PreSelectedSkill;
                    int requireAP = preSelectedSkill.GetRequireAP();
                    BattleData.previewAPAction = new APAction(APAction.Action.Skill, requireAP);
                }else 
                    BattleData.previewAPAction = null;
                BattleData.uiManager.UpdateApBarUI(BattleData.unitManager.GetAllUnits());
                yield return null;
            }
        }

        public static IEnumerator SelectSkillState( ){
            while (BattleData.currentState == CurrentState.SelectSkill){
                BattleManager battleManager = BattleData.battleManager;
                BattleData.uiManager.SetSkillUI(BattleData.selectedUnit);

                BattleData.isWaitingUserInput = true;
                BattleData.indexOfSelectedSkillByUser = 0;

                var update = UpdatePreviewAP();
                battleManager.StartCoroutine(update);

              	yield return battleManager.StartCoroutine(EventTrigger.WaitOr(
					BattleData.triggers.rightClicked,
					BattleData.triggers.cancelClicked,
					BattleData.triggers.skillSelected));

                BattleData.battleManager.StopCoroutine(update);

                BattleData.indexOfPreSelectedSkillByUser = 0;
                BattleData.isWaitingUserInput = false;
                BattleData.uiManager.DisableSkillUI();

                if (BattleData.triggers.rightClicked.Triggered || BattleData.triggers.cancelClicked.Triggered){
                    BattleData.currentState = CurrentState.FocusToUnit;
                    yield break;
                }

                ActiveSkill selectedSkill = BattleData.SelectedSkill;
                SkillType skillTypeOfSelectedSkill = selectedSkill.GetSkillType();
                if (skillTypeOfSelectedSkill == SkillType.Auto ||
                    skillTypeOfSelectedSkill == SkillType.Self ||
                    skillTypeOfSelectedSkill == SkillType.Route) {
                    BattleData.currentState = CurrentState.SelectSkillApplyDirection;
                    yield return battleManager.StartCoroutine(SelectSkillApplyDirection(BattleData.selectedUnit.GetDirection()));
                }
                else{
                    BattleData.currentState = CurrentState.SelectSkillApplyPoint;
                    yield return battleManager.StartCoroutine(SelectSkillApplyPoint(BattleData.selectedUnit.GetDirection()));
                }

                BattleData.previewAPAction = null;
                BattleData.uiManager.UpdateApBarUI(BattleData.unitManager.GetAllUnits());

                yield return null;
            }
        }

        private static IEnumerator UpdateRangeSkillMouseDirection(Direction originalDirection) {
            Unit selectedUnit = BattleData.selectedUnit;
			Tile targetTile = selectedUnit.GetTileUnderUnit ();
			Vector2 unitPos = selectedUnit.GetPosition ();
            ActiveSkill selectedSkill = BattleData.SelectedSkill;
			var selectedTiles = selectedSkill.GetTilesInFirstRange (unitPos, originalDirection);


			BattleData.tileManager.PaintTiles(selectedTiles, TileColor.Red);
			SkillLocation originalLocation = new SkillLocation (unitPos, targetTile, originalDirection);
			selectedSkill.SetRealTargetTileForSkillLocation (originalLocation);
			Casting originalCasting = new Casting (selectedUnit, selectedSkill, originalLocation);
			Unit caster = originalCasting.Caster;
			ActiveSkill skill = originalCasting.Skill;
			List<Tile> secondRange = originalCasting.SecondRange;
			BattleData.tileManager.PaintTiles(secondRange, TileColor.Purple);
			List<Tile> realEffectRange = originalCasting.RealEffectRange;
			DisplayPreviewDamage(originalCasting);


			allCalculatedTotalDamages = new Dictionary<Unit, DamageCalculator.DamageInfo> ();

            while (true) {
                Direction newDirection = Utility.GetMouseDirectionByUnit(BattleData.selectedUnit, originalDirection);
                Direction beforeDirection = BattleData.selectedUnit.GetDirection();
				var selectedTilesByBeforeDirection = selectedSkill.GetTilesInFirstRange (unitPos, beforeDirection);
				var selectedTilesByNewDirection = selectedSkill.GetTilesInFirstRange (unitPos, newDirection);

				if (beforeDirection != newDirection) {
					beforeDirection = newDirection;

					// 1차범위 표시 해제
					BattleData.tileManager.DepaintTiles(selectedTilesByBeforeDirection, TileColor.Red);
					// 2차범위 표시 해제
					BattleData.tileManager.DepaintAllTiles (TileColor.Purple);
					// 데미지 미리보기 해제.
					HidePreviewDamage();

					BattleData.selectedUnit.SetDirection(newDirection);
					BattleData.tileManager.PaintTiles(selectedTilesByNewDirection, TileColor.Red);

					SkillLocation newLocation = new SkillLocation (unitPos, targetTile, newDirection);
					selectedSkill.SetRealTargetTileForSkillLocation (newLocation);
					Casting newCasting = new Casting (selectedUnit, selectedSkill, newLocation);
					//secondRange는 2차 범위(타겟 타일을 가지고 계산한 스킬 효과 범위) 내 타일들이며 보라색으로(임시) 칠한다
					secondRange = newCasting.SecondRange;
					BattleData.tileManager.PaintTiles(secondRange, TileColor.Purple);
					realEffectRange = newCasting.RealEffectRange;
					//데미지 미리보기
					DisplayPreviewDamage(newCasting);
                }
                yield return null;
            }
        }

        public static IEnumerator SelectSkillApplyDirection(Direction originalDirection) {
            Direction beforeDirection = originalDirection;
            Unit selectedUnit = BattleData.selectedUnit;
            ActiveSkill selectedSkill = BattleData.SelectedSkill;

            while (true) {
                BattleData.isWaitingUserInput = true;
                //마우스 방향을 돌릴 때마다 그에 맞춰서 빨간 범위 표시도 업데이트하고 유닛 시선방향 돌리고 데미지 프리뷰와 2차범위 표시도 업데이트
                var updateRedArea = UpdateRangeSkillMouseDirection(originalDirection);
                BattleData.battleManager.StartCoroutine(updateRedArea);
                
				BattleData.uiManager.EnableSelectDirectionUI();

                yield return BattleData.battleManager.StartCoroutine(EventTrigger.WaitOr(
                    BattleData.triggers.rightClicked,
                    BattleData.triggers.cancelClicked,
                    BattleData.triggers.tileSelectedByUser,
                    BattleData.triggers.directionSelectedByUser
                ));
                BattleData.battleManager.StopCoroutine(updateRedArea);
                BattleData.isWaitingUserInput = false;

				BattleData.tileManager.DepaintAllTiles(TileColor.Red);	
				BattleData.tileManager.DepaintAllTiles(TileColor.Purple);
				HidePreviewDamage();

				//취소선택시->1. 4방향 화살표 제거 2. 유닛이 원래 방향을 바라보게 되돌림 3. currentState는 스킬을 고르는 단계로 돌려놓는다
                if (BattleData.triggers.rightClicked.Triggered ||
                    BattleData.triggers.cancelClicked.Triggered) {
                    BattleData.uiManager.DisableSelectDirectionUI();
                    selectedUnit.SetDirection(originalDirection);
                    BattleData.currentState = CurrentState.SelectSkill;
                    yield break;
                }
				//범위 내 타일이나 방향(사분면 중 하나)을 선택했을시->1. currentState는 즉시시전/연계대기를 선택하는 단계로 한다
				//2. 현재 선택된 타겟 타일에 즉시시전/연계대기를 할지 선택하게 한다. 단, 투사체 스킬이면 선택된 영역(경로) 중 맨 끝점을 타겟 타일로 한다.
                else{
                    BattleManager battleManager = BattleData.battleManager;
                    BattleData.currentState = CurrentState.CheckApplyOrChain;
					var targetTile = BattleData.SelectedUnitTile;
					SkillLocation skillLocation = new SkillLocation (selectedUnit.GetTileUnderUnit (), targetTile, selectedUnit.GetDirection ());
					//투사체 스킬이면 선택된 영역(경로) 중 맨 끝점을 시전 타일로 한다.
					selectedSkill.SetRealTargetTileForSkillLocation (skillLocation);
					yield return battleManager.StartCoroutine(CheckApplyOrChain(new Casting(selectedUnit, selectedSkill, skillLocation), originalDirection));
                }
					
                if (BattleData.currentState != CurrentState.SelectSkillApplyDirection) {
					//이제 취소 버튼 UI 없으니 이 줄은 필요없지 않나
                    BattleData.uiManager.DisableCancelButtonUI();
                    
					yield break;
                }
            }
        }

        private static IEnumerator UpdatePointSkillMouseDirection(Direction originalDirection) {
			Unit selectedUnit = BattleData.selectedUnit;
			ActiveSkill selectedSkill = BattleData.SelectedSkill;
			Vector2 unitPos = selectedUnit.GetPosition ();

			Tile previousTargetTile = null;
			TileManager.Instance.preSelectedMouseOverTile = null;
			Direction beforeDirection = Utility.GetMouseDirectionByUnit(BattleData.selectedUnit, originalDirection);

			allCalculatedTotalDamages = new Dictionary<Unit, DamageCalculator.DamageInfo> ();

			while (true) {
				Direction newDirection = Utility.GetMouseDirectionByUnit (BattleData.selectedUnit, originalDirection);
				if (beforeDirection != newDirection) {
					beforeDirection = newDirection;
					BattleData.selectedUnit.SetDirection (newDirection);
				}

				Tile newTargetTile = TileManager.Instance.preSelectedMouseOverTile;

				if (previousTargetTile != newTargetTile) {
					// 2차범위 표시 해제
					BattleData.tileManager.DepaintAllTiles (TileColor.Purple);
					// 데미지 미리보기 해제.
					HidePreviewDamage ();

					Tile targetTile = newTargetTile;
					if (targetTile != null) {
						SkillLocation newLocation = new SkillLocation (unitPos, targetTile, newDirection);
						Casting newCasting = new Casting (selectedUnit, selectedSkill, newLocation);

						Unit caster = newCasting.Caster;
						ActiveSkill skill = newCasting.Skill;
						//BattleManager.MoveCameraToTile(newCasting.Location.TargetTile);

						//secondRange는 2차 범위(타겟 타일을 가지고 계산한 스킬 효과 범위) 내 타일들이며 보라색으로(임시) 칠한다
						List<Tile> secondRange = newCasting.SecondRange;
						BattleData.tileManager.PaintTiles (secondRange, TileColor.Purple);
						//realEffectRange는 실제로 효과나 데미지가 가해지는 영역으로, 일반적인 경우는 secondRange와 동일
						//투사체 스킬은 타겟 타일에 유닛이 없으면 아무 효과도 데미지도 없이 이펙트만 나오게 한다. 연계 발동은 안 되고 연계 대기는 된다
						List<Tile> realEffectRange = newCasting.RealEffectRange;

						//데미지 미리보기
						DisplayPreviewDamage (newCasting);
					}

				}

                yield return null;
            }
        }

        public static IEnumerator SelectSkillApplyPoint (Direction originalDirection) {
            Direction beforeDirection = originalDirection;
            Unit selectedUnit = BattleData.selectedUnit;

            if (BattleData.currentState == CurrentState.SelectSkill) {
                BattleData.uiManager.DisableCancelButtonUI();
                yield break;
            }

            while (BattleData.currentState == CurrentState.SelectSkillApplyPoint) {
                Vector2 selectedUnitPos = BattleData.selectedUnit.GetPosition();

                List<Tile> activeRange = new List<Tile>();
                ActiveSkill selectedSkill = BattleData.SelectedSkill;
				activeRange = selectedSkill.GetTilesInFirstRange (selectedUnitPos, selectedUnit.GetDirection ());

                BattleData.tileManager.PaintTiles(activeRange, TileColor.Red);
				BattleData.tileManager.PreselectTiles (activeRange);
                BattleData.uiManager.EnableCancelButtonUI();
                BattleData.isWaitingUserInput = true;

                var update = UpdatePointSkillMouseDirection(originalDirection);
                BattleData.battleManager.StartCoroutine(update);
                yield return BattleData.battleManager.StartCoroutine(EventTrigger.WaitOr(
                    BattleData.triggers.tileSelectedByUser,
                    BattleData.triggers.rightClicked,
                    BattleData.triggers.cancelClicked
                ));
                BattleData.battleManager.StopCoroutine(update);
                BattleData.isWaitingUserInput = false;
                BattleData.uiManager.DisableCancelButtonUI();

				BattleData.tileManager.DepaintAllTiles(TileColor.Red);	
				BattleData.tileManager.DepaintAllTiles(TileColor.Purple);
				HidePreviewDamage();

                if (BattleData.triggers.rightClicked.Triggered ||
                    BattleData.triggers.cancelClicked.Triggered) {
                    selectedUnit.SetDirection(originalDirection);
					BattleData.tileManager.DepreselectAllTiles ();
                    BattleData.currentState = CurrentState.SelectSkill;
                    BattleData.isWaitingUserInput = false;
                    yield break;
                }

				BattleData.tileManager.DepreselectAllTiles ();
                BattleData.uiManager.DisableSkillUI();

                BattleManager battleManager = BattleData.battleManager;
                BattleData.currentState = CurrentState.CheckApplyOrChain;
				SkillLocation skillLocation = new SkillLocation (selectedUnitPos, BattleData.SelectedTile, selectedUnit.GetDirection ());
				yield return battleManager.StartCoroutine(CheckApplyOrChain(new Casting(selectedUnit, selectedSkill, skillLocation), originalDirection));
            }
        }

		public static IEnumerator CheckApplyOrChain (Casting casting, Direction originalDirection) {
			/*
            while (BattleData.currentState == CurrentState.CheckApplyOrChain) {
				Unit caster = casting.Caster;
				ActiveSkill skill = casting.Skill;
                BattleManager.MoveCameraToTile(casting.Location.TargetTile);

				//secondRange는 2차 범위(타겟 타일을 가지고 계산한 스킬 효과 범위) 내 타일들이며 빨갛게 칠한다
				List<Tile> secondRange = casting.SecondRange;
                BattleData.tileManager.PaintTiles(secondRange, TileColor.Red);
				//realEffectRange는 실제로 효과나 데미지가 가해지는 영역으로, 일반적인 경우는 secondRange와 동일
				//투사체 스킬은 타겟 타일에 유닛이 없으면 아무 효과도 데미지도 없이 이펙트만 나오게 한다. 연계 발동은 안 되고 연계 대기는 된다
				List<Tile> realEffectRange = casting.RealEffectRange;

                //데미지 미리보기
				Dictionary<Unit, DamageCalculator.DamageInfo> allCalculatedTotalDamages = DisplayPreviewDamage(casting);

				bool isApplyPossible = skill.SkillLogic.CheckApplyPossibleToTargetTiles(caster, secondRange);
                bool isChainPossible = CheckChainPossible(casting);
                BattleData.uiManager.EnableSkillCheckWaitButton(isApplyPossible, isApplyPossible && isChainPossible);
                BattleData.uiManager.SetSkillCheckAP(casting);

                BattleData.skillApplyCommand = SkillApplyCommand.Waiting;
                yield return BattleData.battleManager.StartCoroutine(EventTrigger.WaitOr(
                    BattleData.triggers.cancelClicked,
                    BattleData.triggers.rightClicked,
                    BattleData.triggers.skillApplyCommandChanged
                ));

				// 데미지 미리보기 해제.
				HidePreviewDamage(allCalculatedTotalDamages);
                BattleData.tileManager.DepaintTiles(secondRange, TileColor.Red);

                if (BattleData.triggers.rightClicked.Triggered ||
					BattleData.triggers.cancelClicked.Triggered) {
                    BattleManager.MoveCameraToUnit(caster);
                    BattleData.uiManager.DisableSkillCheckUI();
                    caster.SetDirection(originalDirection);
                    if (skill.GetSkillType() != SkillType.Point)
                        BattleData.currentState = CurrentState.SelectSkillApplyDirection;
                    else
                        BattleData.currentState = CurrentState.SelectSkillApplyPoint;
                    yield break;
                }

                BattleManager battleManager = BattleData.battleManager;
                if (BattleData.skillApplyCommand == SkillApplyCommand.Apply) {
                    BattleData.skillApplyCommand = SkillApplyCommand.Waiting;
					caster.UseActivityPoint (casting.RequireAP);
					if (skill.GetCooldown() > 0)
						caster.GetUsedSkillDict().Add(skill.GetName(), skill.GetCooldown());
					yield return ApplyAllTriggeredChains(casting);
					BattleManager.MoveCameraToUnit(caster);
					BattleData.currentState = CurrentState.FocusToUnit;
                }
				else if (BattleData.skillApplyCommand == SkillApplyCommand.Chain) {
                    BattleData.skillApplyCommand = SkillApplyCommand.Waiting;
					BattleData.currentState = CurrentState.ChainAndStandby;
					caster.UseActivityPoint (casting.RequireAP);
					yield return battleManager.StartCoroutine(StandbyChain(casting));
                } else {
                    Debug.LogError("Invalid State");
                    yield return null;
                }
            }
            yield return null;
            */

			Unit caster = casting.Caster;
			ActiveSkill skill = casting.Skill;
			BattleManager.MoveCameraToTile(casting.Location.TargetTile);

			BattleData.skillApplyCommand = SkillApplyCommand.Waiting;
			caster.UseActivityPoint (casting.RequireAP);
			if (skill.GetCooldown() > 0)
				caster.GetUsedSkillDict().Add(skill.GetName(), skill.GetCooldown());
			yield return ApplyAllTriggeredChains(casting);
			BattleManager.MoveCameraToUnit(caster);
			BattleData.currentState = CurrentState.FocusToUnit;

        }

		static Dictionary<Unit, DamageCalculator.DamageInfo> allCalculatedTotalDamages;

		static void DisplayPreviewDamage(Casting casting){
			//데미지 미리보기
			allCalculatedTotalDamages = DamageCalculator.CalculateAllPreviewTotalDamages(casting);
			foreach (KeyValuePair<Unit, DamageCalculator.DamageInfo> kv in allCalculatedTotalDamages) {
				if(kv.Value.damage > 0) kv.Key.GetComponentInChildren<HealthViewer>().PreviewDamageAmount((int)kv.Value.damage);
				else kv.Key.GetComponentInChildren<HealthViewer>().PreviewRecoverAmount((int)(-kv.Value.damage));
			}
		}
		static void HidePreviewDamage(){
			// 데미지 미리보기 해제.
			foreach (KeyValuePair<Unit, DamageCalculator.DamageInfo> kv in allCalculatedTotalDamages) {
				kv.Key.GetComponentInChildren<HealthViewer>().CancelPreview();
			}
		}

		//연계'대기' 가능한 상태인가?
		private static bool CheckChainPossible (Casting casting) {
			if (GameData.SceneData.stageNumber < Setting.chainOpenStage)
				return false;

			Unit caster = casting.Caster;
			ActiveSkill skill = casting.Skill;

			// 공격/약화 타입 스킬만 연계대기 가능
			if (skill.IsChainable () == false)
				return false;

			// AP 조건 - 연계대기해서 AP를 소모한 후에도 자신이 AP가 가장 높은 유닛일 경우 연계대기 불가
			int requireAP = casting.RequireAP;
            int remainAPAfterChain = caster.GetCurrentActivityPoint() - requireAP;

			bool isAPConditionPossible = false;
            foreach (var unit in BattleData.unitManager.GetAllUnits()) {
                if ((unit != caster) &&
                (unit.GetCurrentActivityPoint() > remainAPAfterChain)) {
                    isAPConditionPossible = true;
                }
			}
			if (!isAPConditionPossible)
				return false;
            
			// 타일 조건 - 시전자가 있는 타일에 연계 대기 불가능 효과가 걸려있으면 연계대기 불가
			bool tileStatusConditionPossible = true;
            Tile tileUnderCaster = caster.GetTileUnderUnit();
            foreach(var tileStatusEffect in tileUnderCaster.GetStatusEffectList()) {
                Skill originSkill = tileStatusEffect.GetOriginSkill();
                if (originSkill.GetType() == typeof(ActiveSkill)) {
                    if (!((ActiveSkill)originSkill).SkillLogic.TriggerTileStatusEffectWhenUnitTryToChain(tileUnderCaster, tileStatusEffect)) {
						tileStatusConditionPossible = false;
                    }
                }
            }
			if (tileStatusConditionPossible)
				return true;
			else
				return false;
        }

		public static IEnumerator ApplyAllTriggeredChains (Casting casting) {
            BattleManager battleManager = BattleData.battleManager;
			Unit caster = casting.Caster;

			// 현재 시전으로 발동되는 모든 시전의 리스트(현재 시전 포함)를 받는다.
			// 연계발동 불가능한 스킬일 경우엔 ChainList.GetAllChainTriggered에서 현재 시전만 담은 리스트를 반환하므로 걍 그 스킬만 시전되고 끝난다
			List<Chain> allTriggeredChains = ChainList.GetAllChainTriggered (casting);
			int chainCombo = allTriggeredChains.Count;

			caster.PrintChainBonus (chainCombo);

			// 발동되는 모든 시전을 순서대로 실행
			foreach (var chain in allTriggeredChains) {
				if (chain.SecondRange.Count > 0) {
					Tile focusedTile = chain.SecondRange [0];
					BattleManager.MoveCameraToTile (focusedTile);
				}
				BattleData.currentState = CurrentState.ApplySkill;
				chain.Caster.HideChainIcon ();
				yield return battleManager.StartCoroutine (chain.Cast (chainCombo));
				caster.DisableChainText ();
			}
        }

		private static IEnumerator StandbyChain (Casting casting) {
			Unit caster = casting.Caster;
			ActiveSkill skill = casting.Skill;
			SkillLocation location = casting.Location;

			caster.SetDirection(location.Direction);

            // 스킬 쿨다운 기록
            if (skill.GetCooldown() > 0)
                caster.GetUsedSkillDict().Add(skill.GetName(), skill.GetCooldown());

            // 체인 목록에 추가.
			ChainList.AddChains(casting);
            BattleData.indexOfSelectedSkillByUser = 0; // return to init value.
            yield return new WaitForSeconds(0.5f);

            BattleManager.MoveCameraToUnit(caster);
            BattleData.currentState = CurrentState.Standby;
            BattleData.alreadyMoved = false;
            BattleManager battleManager = BattleData.battleManager;
            yield return battleManager.StartCoroutine(BattleManager.Standby()); // 이후 대기.
        }
    }
}
