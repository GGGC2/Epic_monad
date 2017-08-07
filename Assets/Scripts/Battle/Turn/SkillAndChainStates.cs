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
                    battleData.currentState = CurrentState.SelectSkill;
                    battleData.isWaitingUserInput = false;
                    yield break;
                }

                battleData.tileManager.DepaintTiles(activeRange, TileColor.Red);
                battleData.uiManager.DisableSkillUI();

                BattleManager battleManager = battleData.battleManager;
                battleData.currentState = CurrentState.CheckApplyOrChain;
				SkillLocation skillLocation = new SkillLocation (selectedUnitPos, battleData.SelectedTile, selectedUnit.GetDirection ());
				yield return battleManager.StartCoroutine(CheckApplyOrChain(new Casting(selectedUnit, selectedSkill, skillLocation), originalDirection));
            }
        }

		public static IEnumerator CheckApplyOrChain ( Casting casting, Direction originalDirection) {
            while (battleData.currentState == CurrentState.CheckApplyOrChain) {
				Unit caster = casting.Caster;
				ActiveSkill skill = casting.Skill;
				SkillLocation location = casting.Location;
				Vector2 casterPos = caster.GetPosition ();
                BattleManager.MoveCameraToTile(location.TargetTile);

				//firstRange는 1차 범위(스킬 시전 가능 범위. 투사체의 경우 목표점까지의 경로) 내 타일들이고
				//secondRange는 2차 범위(타겟 타일을 가지고 계산한 스킬 효과 범위) 내 타일들임. secondRange가 현재 단계에서 중요
				List<Tile> firstRange = skill.GetTilesInFirstRange(casterPos, caster.GetDirection());
				List<Tile> secondRange = casting.SecondRange;
                battleData.tileManager.PaintTiles(secondRange, TileColor.Red);
				//realEffectRange는 실제로 효과나 데미지가 가해지는 영역으로, 일반적인 경우는 secondRange와 동일
				//투사체 스킬은 타겟 타일에 유닛이 없으면 아무 효과도 데미지도 없이 이펙트만 나오게 한다. 연계 발동은 안 되고 연계 대기는 된다
				List<Tile> realEffectRange = casting.RealEffectRange;

                //데미지 미리보기
                Debug.Log("/------- Damage preview -------\\");
				Dictionary<Unit, DamageCalculator.DamageInfo> calculatedTotalDamage = DamageCalculator.CalculatePreviewTotalDamage(battleData, casting);
                foreach (KeyValuePair<Unit, DamageCalculator.DamageInfo> kv in calculatedTotalDamage) {
                    if(kv.Value.damage > 0) kv.Key.GetComponentInChildren<HealthViewer>().PreviewDamageAmount((int)kv.Value.damage);
                    else kv.Key.GetComponentInChildren<HealthViewer>().PreviewRecoverAmount((int)(-kv.Value.damage));
                }
                Debug.Log("\\------- Damage preview -------/");

				bool isApplyPossible = skill.SkillLogic.CheckApplyPossible(caster, secondRange);
                bool isChainPossible = CheckChainPossible(casting);
                battleData.uiManager.EnableSkillCheckWaitButton(isApplyPossible, isChainPossible);
                battleData.uiManager.SetSkillCheckAP(casting);

                battleData.skillApplyCommand = SkillApplyCommand.Waiting;
                yield return battleData.battleManager.StartCoroutine(EventTrigger.WaitOr(
                    battleData.triggers.cancelClicked,
                    battleData.triggers.rightClicked,
                    battleData.triggers.skillApplyCommandChanged
                ));
                
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

                    // 데미지 미리보기 해제.
                    foreach (KeyValuePair<Unit, DamageCalculator.DamageInfo> kv in calculatedTotalDamage) {
                        kv.Key.GetComponentInChildren<HealthViewer>().CancelPreview();
                    }

                    yield break;
                }

                BattleManager battleManager = battleData.battleManager;
                if (battleData.skillApplyCommand == SkillApplyCommand.Apply) {
                    // 데미지 미리보기 해제.
                    foreach (KeyValuePair<Unit, DamageCalculator.DamageInfo> kv in calculatedTotalDamage) {
                        kv.Key.GetComponentInChildren<HealthViewer>().CancelPreview();
                    }

                    battleData.skillApplyCommand = SkillApplyCommand.Waiting;
                    // 체인이 가능한 스킬일 경우. 체인 발동.
                    // 왜 CheckChainPossible 안 쓴 거죠...? CheckChainPossible은 체인을 새로 만들때 체크
                    // 여기서는 체인을 새로 만드는 게 아니라 기존에 쌓인 체인을 소모하는 코드
					if (skill.IsChainable()) {
						yield return ApplyChain(casting);
                    }
                    // 체인이 불가능한 스킬일 경우, 그냥 발동.
                    else {
                        battleData.currentState = CurrentState.ApplySkill;
						yield return battleManager.StartCoroutine(casting.Cast(1));
					}

					BattleManager.MoveCameraToUnit(caster);
					battleData.currentState = CurrentState.FocusToUnit;
                }
				else if (battleData.skillApplyCommand == SkillApplyCommand.Chain) {
                    // 데미지 미리보기 해제.
                    foreach (KeyValuePair<Unit, DamageCalculator.DamageInfo> kv in calculatedTotalDamage) {
                        kv.Key.GetComponentInChildren<HealthViewer>().CancelPreview();
                    }

                    battleData.skillApplyCommand = SkillApplyCommand.Waiting;
                    battleData.currentState = CurrentState.ChainAndStandby;
					//투사체 스킬이고 타겟 타일에 유닛 없어도 연계 대기는 된다!(기획에서 결정된 사항)
                    yield return battleManager.StartCoroutine(ChainAndStandby(casting));
                } else {
                    Debug.LogError("Invalid State");
                    yield return null;
                }
            }
            yield return null;
        }

		private static bool CheckChainPossible ( Casting casting) {
			if (GameData.SceneData.stageNumber < Setting.chainOpenStage)
				return false;

			Unit caster = casting.Caster;
			ActiveSkill skill = casting.Skill;

			// 스킬 타입으로 체크. 공격/약화 스킬만 체인을 걸 수 있음.
			if (skill.IsChainable () == false)
				return false;

			// AP 조건 - 연계 대기해서 AP를 소모한 후에도 자신이 AP가 가장 높은 유닛일 경우 연계 불가
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


		public static IEnumerator ApplyChain ( Casting casting) {
            BattleManager battleManager = battleData.battleManager;
			Unit caster = casting.Caster;
			List<Tile> secondRange = casting.SecondRange;
			List<Tile> realEffectRange = casting.RealEffectRange;

			//실제 데미지/효과가 가해지지는 경우에만 연계가 발동
			if (realEffectRange.Count != 0) {
				// 자기 자신을 체인 리스트에 추가.
				ChainList.AddChains (casting);

				// 체인 체크, 순서대로 공격.
				List<ChainInfo> allVaildChainInfo = ChainList.GetAllChainInfoToTargetArea (caster, secondRange);
				int chainCombo = allVaildChainInfo.Count;

				caster.PrintChainBonus (chainCombo);

				foreach (var chainInfo in allVaildChainInfo) {
					Tile focusedTile = chainInfo.GetSecondRange () [0];
					BattleManager.MoveCameraToTile (focusedTile);
					battleData.currentState = CurrentState.ApplySkill;
					chainInfo.Caster.HideChainIcon ();
					yield return battleManager.StartCoroutine (chainInfo.Casting.Cast(chainCombo));
				}
				caster.DisableChainText ();
			}
			//실제 데미지/효과가 가해지지 않는 경우(이펙트만 나옴) 연계가 발동하지 않고 그냥 그 스킬만 발동시킨다
			else {
				yield return battleManager.StartCoroutine(casting.Cast(1));
			}
        }

		private static IEnumerator ChainAndStandby ( Casting casting) {
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
