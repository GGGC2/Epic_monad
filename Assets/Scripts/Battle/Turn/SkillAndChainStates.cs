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
		private static BattleData battleData;
		public static void SetBattleData(BattleData battleDataInstance){
			battleData=battleDataInstance;
		}

        private static IEnumerator UpdatePreviewAP( ) {
            while (true) {
                if (battleData.indexOfPreSelectedSkillByUser != 0) {
                    ActiveSkill preSelectedSkill = battleData.PreSelectedSkill;
                    int requireAP = preSelectedSkill.GetRequireAP();
                    battleData.previewAPAction = new APAction(APAction.Action.Skill, requireAP);
                }else 
                    battleData.previewAPAction = null;
                battleData.uiManager.UpdateApBarUI(battleData, battleData.unitManager.GetAllUnits());
                yield return null;
            }
        }

        public static IEnumerator SelectSkillState( ) {
            while (battleData.currentState == CurrentState.SelectSkill) {
                battleData.uiManager.SetSkillUI(battleData.selectedUnit);

                battleData.isWaitingUserInput = true;
                battleData.indexOfSelectedSkillByUser = 0;

                var update = UpdatePreviewAP();
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

                if (battleData.triggers.rightClicked.Triggered || battleData.triggers.cancelClicked.Triggered) {
                    battleData.currentState = CurrentState.FocusToUnit;
                    yield break;
                }

                BattleManager battleManager = battleData.battleManager;
                ActiveSkill selectedSkill = battleData.SelectedSkill;
                SkillType skillTypeOfSelectedSkill = selectedSkill.GetSkillType();
                if (skillTypeOfSelectedSkill == SkillType.Auto ||
                    skillTypeOfSelectedSkill == SkillType.Self ||
                    skillTypeOfSelectedSkill == SkillType.Route) {
                    battleData.currentState = CurrentState.SelectSkillApplyDirection;
                    yield return battleManager.StartCoroutine(SelectSkillApplyDirection(battleData.selectedUnit.GetDirection()));
                }
                else{
                    battleData.currentState = CurrentState.SelectSkillApplyPoint;
                    yield return battleManager.StartCoroutine(SelectSkillApplyPoint(battleData.selectedUnit.GetDirection()));
                }

                battleData.previewAPAction = null;
                battleData.uiManager.UpdateApBarUI(battleData, battleData.unitManager.GetAllUnits());

                yield return null;
            }
        }

        private static IEnumerator UpdateRangeSkillMouseDirection(Direction originalDirection) {
            Unit selectedUnit = battleData.selectedUnit;
			Vector2 unitPos = selectedUnit.GetPosition ();
            ActiveSkill selectedSkill = battleData.SelectedSkill;
			var selectedTiles = selectedSkill.GetTilesInFirstRange (unitPos, originalDirection);
            battleData.tileManager.PaintTiles(selectedTiles, TileColor.Red);

            while (true) {
                Direction newDirection = Utility.GetMouseDirectionByUnit(battleData.selectedUnit, originalDirection);
                Direction beforeDirection = battleData.selectedUnit.GetDirection();
				var selectedTilesByBeforeDirection = selectedSkill.GetTilesInFirstRange (unitPos, beforeDirection);
				var selectedTilesByNewDirection = selectedSkill.GetTilesInFirstRange (unitPos, newDirection);

                if (beforeDirection != newDirection) {
                    battleData.tileManager.DepaintTiles(selectedTilesByBeforeDirection, TileColor.Red);
                    beforeDirection = newDirection;
                    battleData.selectedUnit.SetDirection(newDirection);
                    battleData.tileManager.PaintTiles(selectedTilesByNewDirection, TileColor.Red);
                }
                yield return null;
            }
        }

        public static IEnumerator SelectSkillApplyDirection(Direction originalDirection) {
            Direction beforeDirection = originalDirection;
            Unit selectedUnit = battleData.selectedUnit;
            ActiveSkill selectedSkill = battleData.SelectedSkill;

            while (true) {
                battleData.isWaitingUserInput = true;
                //마우스 방향을 돌릴 때마다 그에 맞춰서 빨간 범위 표시도 업데이트하고 유닛 시선방향도 돌림
                var updateRedArea = UpdateRangeSkillMouseDirection(originalDirection);
                battleData.battleManager.StartCoroutine(updateRedArea);

				//지정형 스킬이 아니라면 4방향 화살표를 띄운다(본인 중심으로 4방향 대칭인 스킬도 화살표가 의미가 있는데, 스킬 시전 후의 시전자 시선 방향을 결정하기 때문)
                if(battleData.SelectedSkill.GetSkillType() != SkillType.Point)
                    battleData.uiManager.EnableSelectDirectionUI();

                yield return battleData.battleManager.StartCoroutine(EventTrigger.WaitOr(
                    battleData.triggers.rightClicked,
                    battleData.triggers.cancelClicked,
                    battleData.triggers.tileSelectedByUser,
                    battleData.triggers.directionSelectedByUser
                ));
                battleData.battleManager.StopCoroutine(updateRedArea);
                battleData.isWaitingUserInput = false;

                battleData.tileManager.DepaintAllTiles(TileColor.Red);

				//취소선택시->1. 4방향 화살표 제거 2. 유닛이 원래 방향을 바라보게 되돌림 3. currentState는 스킬을 고르는 단계로 돌려놓는다
                if (battleData.triggers.rightClicked.Triggered ||
                    battleData.triggers.cancelClicked.Triggered) {
                    battleData.uiManager.DisableSelectDirectionUI();
                    selectedUnit.SetDirection(originalDirection);
                    battleData.currentState = CurrentState.SelectSkill;
                    yield break;
                }
				//범위 내 타일이나 방향(사분면 중 하나)을 선택했을시->1. currentState는 즉시시전/연계대기를 선택하는 단계로 한다
				//2. 현재 선택된 타겟 타일에 즉시시전/연계대기를 할지 선택하게 한다. 단, 투사체 스킬이면 선택된 영역(경로) 중 맨 끝점을 타겟 타일로 한다.
                else{
                    BattleManager battleManager = battleData.battleManager;
                    battleData.currentState = CurrentState.CheckApplyOrChain;
					var targetTile = battleData.SelectedUnitTile;
					SkillLocation skillLocation = new SkillLocation (selectedUnit.GetTileUnderUnit (), targetTile, selectedUnit.GetDirection ());
					//투사체 스킬이면 선택된 영역(경로) 중 맨 끝점을 시전 타일로 한다.
					selectedSkill.SetRealTargetTileForSkillLocation (skillLocation);
					yield return battleManager.StartCoroutine(CheckApplyOrChain(new Casting(selectedUnit, selectedSkill, skillLocation), originalDirection));
                }
					
                if (battleData.currentState != CurrentState.SelectSkillApplyDirection) {
					//이제 취소 버튼 UI 없으니 이 줄은 필요없지 않나
                    battleData.uiManager.DisableCancelButtonUI();
                    
					yield break;
                }
            }
        }

        private static IEnumerator UpdatePointSkillMouseDirection(Direction originalDirection) {
            Direction beforeDirection = Utility.GetMouseDirectionByUnit(battleData.selectedUnit, originalDirection);
            while (true) {
                Direction newDirection = Utility.GetMouseDirectionByUnit(battleData.selectedUnit, originalDirection);
                if (beforeDirection != newDirection) {
                    beforeDirection = newDirection;
                    battleData.selectedUnit.SetDirection(newDirection);
                }
                yield return null;
            }
        }

        public static IEnumerator SelectSkillApplyPoint ( Direction originalDirection) {
            Direction beforeDirection = originalDirection;
            Unit selectedUnit = battleData.selectedUnit;

            if (battleData.currentState == CurrentState.SelectSkill) {
                battleData.uiManager.DisableCancelButtonUI();
                yield break;
            }

            while (battleData.currentState == CurrentState.SelectSkillApplyPoint) {
                Vector2 selectedUnitPos = battleData.selectedUnit.GetPosition();

                List<Tile> activeRange = new List<Tile>();
                ActiveSkill selectedSkill = battleData.SelectedSkill;
				activeRange = selectedSkill.GetTilesInFirstRange (selectedUnitPos, selectedUnit.GetDirection ());

                battleData.tileManager.PaintTiles(activeRange, TileColor.Red);
				battleData.tileManager.PreselectTiles (activeRange);
                battleData.uiManager.EnableCancelButtonUI();
                battleData.isWaitingUserInput = true;

                var update = UpdatePointSkillMouseDirection(originalDirection);
                battleData.battleManager.StartCoroutine(update);
                yield return battleData.battleManager.StartCoroutine(EventTrigger.WaitOr(
                    battleData.triggers.tileSelectedByUser,
                    battleData.triggers.rightClicked,
                    battleData.triggers.cancelClicked
                ));
                battleData.battleManager.StopCoroutine(update);
                battleData.isWaitingUserInput = false;
                battleData.uiManager.DisableCancelButtonUI();

                if (battleData.triggers.rightClicked.Triggered ||
                    battleData.triggers.cancelClicked.Triggered) {
                    selectedUnit.SetDirection(originalDirection);
                    battleData.tileManager.DepaintTiles(activeRange, TileColor.Red);
					battleData.tileManager.DepreselectAllTiles ();
                    battleData.currentState = CurrentState.SelectSkill;
                    battleData.isWaitingUserInput = false;
                    yield break;
                }

                battleData.tileManager.DepaintTiles(activeRange, TileColor.Red);
				battleData.tileManager.DepreselectAllTiles ();
                battleData.uiManager.DisableSkillUI();

                BattleManager battleManager = battleData.battleManager;
                battleData.currentState = CurrentState.CheckApplyOrChain;
				SkillLocation skillLocation = new SkillLocation (selectedUnitPos, battleData.SelectedTile, selectedUnit.GetDirection ());
				yield return battleManager.StartCoroutine(CheckApplyOrChain(new Casting(selectedUnit, selectedSkill, skillLocation), originalDirection));
            }
        }

		public static IEnumerator CheckApplyOrChain (Casting casting, Direction originalDirection) {
            while (battleData.currentState == CurrentState.CheckApplyOrChain) {
				Unit caster = casting.Caster;
				ActiveSkill skill = casting.Skill;
                BattleManager.MoveCameraToTile(casting.Location.TargetTile);

				//secondRange는 2차 범위(타겟 타일을 가지고 계산한 스킬 효과 범위) 내 타일들이며 빨갛게 칠한다
				List<Tile> secondRange = casting.SecondRange;
                battleData.tileManager.PaintTiles(secondRange, TileColor.Red);
				//realEffectRange는 실제로 효과나 데미지가 가해지는 영역으로, 일반적인 경우는 secondRange와 동일
				//투사체 스킬은 타겟 타일에 유닛이 없으면 아무 효과도 데미지도 없이 이펙트만 나오게 한다. 연계 발동은 안 되고 연계 대기는 된다
				List<Tile> realEffectRange = casting.RealEffectRange;

                //데미지 미리보기
				Dictionary<Unit, DamageCalculator.DamageInfo> allCalculatedTotalDamages = DisplayPreviewDamage(casting);

				bool isApplyPossible = skill.SkillLogic.CheckApplyPossibleToTargetTiles(caster, secondRange);
                bool isChainPossible = CheckChainPossible(casting);
				//참고로 아래 함수에서 즉시시전(Apply) 불가한 상황엔 연계대기(Chain) 버튼도 못 누르게 되어있다
                battleData.uiManager.EnableSkillCheckWaitButton(isApplyPossible, isChainPossible);
                battleData.uiManager.SetSkillCheckAP(casting);

                battleData.skillApplyCommand = SkillApplyCommand.Waiting;
                yield return battleData.battleManager.StartCoroutine(EventTrigger.WaitOr(
                    battleData.triggers.cancelClicked,
                    battleData.triggers.rightClicked,
                    battleData.triggers.skillApplyCommandChanged
                ));

				// 데미지 미리보기 해제.
				HidePreviewDamage(allCalculatedTotalDamages);
                battleData.tileManager.DepaintTiles(secondRange, TileColor.Red);

                if (battleData.triggers.rightClicked.Triggered ||
					battleData.triggers.cancelClicked.Triggered) {
                    BattleManager.MoveCameraToUnit(caster);
                    battleData.uiManager.DisableSkillCheckUI();
                    caster.SetDirection(originalDirection);
                    if (skill.GetSkillType() != SkillType.Point)
                        battleData.currentState = CurrentState.SelectSkillApplyDirection;
                    else
                        battleData.currentState = CurrentState.SelectSkillApplyPoint;
                    yield break;
                }

                BattleManager battleManager = battleData.battleManager;
                if (battleData.skillApplyCommand == SkillApplyCommand.Apply) {
                    battleData.skillApplyCommand = SkillApplyCommand.Waiting;
					yield return ApplyAllTriggeredChains(casting);
					BattleManager.MoveCameraToUnit(caster);
					battleData.currentState = CurrentState.FocusToUnit;
                }
				else if (battleData.skillApplyCommand == SkillApplyCommand.Chain) {
                    battleData.skillApplyCommand = SkillApplyCommand.Waiting;
                    battleData.currentState = CurrentState.ChainAndStandby;
					yield return battleManager.StartCoroutine(StandbyChain(casting));
                } else {
                    Debug.LogError("Invalid State");
                    yield return null;
                }
            }
            yield return null;
        }

		private static Dictionary<Unit, DamageCalculator.DamageInfo> DisplayPreviewDamage(Casting casting){
			//데미지 미리보기
			Dictionary<Unit, DamageCalculator.DamageInfo> allCalculatedTotalDamages = DamageCalculator.CalculateAllPreviewTotalDamages(battleData, casting);
			foreach (KeyValuePair<Unit, DamageCalculator.DamageInfo> kv in allCalculatedTotalDamages) {
				if(kv.Value.damage > 0) kv.Key.GetComponentInChildren<HealthViewer>().PreviewDamageAmount((int)kv.Value.damage);
				else kv.Key.GetComponentInChildren<HealthViewer>().PreviewRecoverAmount((int)(-kv.Value.damage));
			}
			return allCalculatedTotalDamages;
		}
		private static void HidePreviewDamage(Dictionary<Unit, DamageCalculator.DamageInfo> allCalculatedTotalDamages){
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
            foreach (var unit in battleData.unitManager.GetAllUnits()) {
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
                ActiveSkill originSkill = tileStatusEffect.GetOriginSkill();
                if (originSkill != null) {
                    if (!originSkill.SkillLogic.TriggerTileStatusEffectWhenUnitTryToChain(tileUnderCaster, tileStatusEffect)) {
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
            BattleManager battleManager = battleData.battleManager;
			Unit caster = casting.Caster;

			// 현재 시전으로 발동되는 모든 시전의 리스트(현재 시전 포함)를 받는다.
			// 연계발동 불가능한 스킬일 경우엔 ChainList.GetAllChainTriggered에서 현재 시전만 담은 리스트를 반환하므로 걍 그 스킬만 시전되고 끝난다
			List<Chain> allTriggeredChains = ChainList.GetAllChainTriggered (casting);
			int chainCombo = allTriggeredChains.Count;

			caster.PrintChainBonus (chainCombo);

			// 발동되는 모든 시전을 순서대로 실행
			foreach (var chain in allTriggeredChains) {
				Tile focusedTile = chain.SecondRange [0];
				BattleManager.MoveCameraToTile (focusedTile);
				battleData.currentState = CurrentState.ApplySkill;
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
			caster.UseActivityPoint(casting.RequireAP);

            // 스킬 쿨다운 기록
            if (skill.GetCooldown() > 0)
                caster.GetUsedSkillDict().Add(skill.GetName(), skill.GetCooldown());

            // 체인 목록에 추가.
			ChainList.AddChains(casting);
            battleData.indexOfSelectedSkillByUser = 0; // return to init value.
            yield return new WaitForSeconds(0.5f);

            BattleManager.MoveCameraToUnit(caster);
            battleData.currentState = CurrentState.Standby;
            battleData.alreadyMoved = false;
            BattleManager battleManager = battleData.battleManager;
            yield return battleManager.StartCoroutine(BattleManager.Standby()); // 이후 대기.
        }
    }
}
