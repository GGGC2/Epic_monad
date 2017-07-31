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
        private static IEnumerator UpdatePreviewAP(BattleData battleData) {
            while (true) {
                if (battleData.indexOfPreSelectedSkillByUser != 0) {
                    ActiveSkill preSelectedSkill = battleData.PreSelectedSkill;
                    int requireAP = preSelectedSkill.GetRequireAP();
                    battleData.previewAPAction = new APAction(APAction.Action.Skill, requireAP);
                } else {
                    battleData.previewAPAction = null;
                }
                battleData.uiManager.UpdateApBarUI(battleData, battleData.unitManager.GetAllUnits());
                yield return null;
            }
        }

        public static IEnumerator SelectSkillState(BattleData battleData) {
            while (battleData.currentState == CurrentState.SelectSkill) {
                battleData.uiManager.SetSkillUI();
                battleData.uiManager.CheckUsableSkill(battleData.selectedUnit);

                battleData.isWaitingUserInput = true;
                battleData.indexOfSelectedSkillByUser = 0;

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
                    yield return battleManager.StartCoroutine(SelectSkillApplyDirection(battleData, battleData.selectedUnit.GetDirection()));
                } else {
                    battleData.currentState = CurrentState.SelectSkillApplyPoint;
                    yield return battleManager.StartCoroutine(SelectSkillApplyPoint(battleData, battleData.selectedUnit.GetDirection()));
                }

                battleData.previewAPAction = null;
                battleData.uiManager.UpdateApBarUI(battleData, battleData.unitManager.GetAllUnits());

                yield return null;
            }
        }

        private static IEnumerator UpdateRangeSkillMouseDirection(BattleData battleData) {
            Unit selectedUnit = battleData.selectedUnit;
            ActiveSkill selectedSkill = battleData.SelectedSkill;
            var selectedTiles = GetTilesInFirstRange(battleData);
            battleData.tileManager.PaintTiles(selectedTiles, TileColor.Red);

            while (true) {
                Direction newDirection = Utility.GetMouseDirectionByUnit(battleData.selectedUnit);
                Direction beforeDirection = battleData.selectedUnit.GetDirection();
                var selectedTilesByBeforeDirection = GetTilesInFirstRange(battleData, beforeDirection);
                var selectedTilesByNewDirection = GetTilesInFirstRange(battleData, newDirection);

                if (beforeDirection != newDirection) {
                    battleData.tileManager.DepaintTiles(selectedTilesByBeforeDirection, TileColor.Red);
                    beforeDirection = newDirection;
                    battleData.selectedUnit.SetDirection(newDirection);
                    battleData.tileManager.PaintTiles(selectedTilesByNewDirection, TileColor.Red);
                }
                yield return null;
            }
        }

        // *주의 : ChainList.cs에서 같은 이름의 함수를 수정할 것!!
        public static List<Tile> GetRouteTiles(List<Tile> tiles) {
            List<Tile> newRouteTiles = new List<Tile>();
            foreach (var tile in tiles) {
                // 타일 단차에 의한 부분(미구현)
                // 즉시 탐색을 종료한다.
                // break;

                // 첫 유닛을 만난 경우
                // 이번 타일을 마지막으로 종료한다.
                newRouteTiles.Add(tile);
                if (tile.IsUnitOnTile())
                    break;
            }

            return newRouteTiles;
        }


        public static IEnumerator SelectSkillApplyDirection(BattleData battleData, Direction originalDirection) {
            Direction beforeDirection = originalDirection;
            Unit selectedUnit = battleData.selectedUnit;
            ActiveSkill selectedSkill = battleData.SelectedSkill;

            while (true) {
                battleData.isWaitingUserInput = true;
                //마우스 방향을 돌릴 때마다 그에 맞춰서 빨간 범위 표시도 업데이트하고 유닛 시선방향도 돌림
                var updateRedArea = UpdateRangeSkillMouseDirection(battleData);
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
					var castingTile = battleData.SelectedUnitTile;
					//투사체 스킬이면 선택된 영역(경로) 중 맨 끝점을 시전 타일로 한다.
					if (battleData.SelectedSkill.GetSkillType() == SkillType.Route) {
						var firstRange = GetTilesInFirstRange(battleData);
						castingTile = GetRouteTiles(firstRange).Last();
					}
					yield return battleManager.StartCoroutine(CheckApplyOrChain(battleData, castingTile, originalDirection));
                }
					
                if (battleData.currentState != CurrentState.SelectSkillApplyDirection) {
					//이제 취소 버튼 UI 없으니 이 줄은 필요없지 않나
                    battleData.uiManager.DisableCancelButtonUI();
                    
					yield break;
                }
            }
        }

        private static IEnumerator UpdatePointSkillMouseDirection(BattleData battleData) {
            Direction beforeDirection = Utility.GetMouseDirectionByUnit(battleData.selectedUnit);
            while (true) {
                Direction newDirection = Utility.GetMouseDirectionByUnit(battleData.selectedUnit);
                if (beforeDirection != newDirection) {
                    beforeDirection = newDirection;
                    battleData.selectedUnit.SetDirection(newDirection);
                }
                yield return null;
            }
        }

        public static IEnumerator SelectSkillApplyPoint(BattleData battleData, Direction originalDirection) {
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
                activeRange = GetTilesInFirstRange(battleData);

                battleData.tileManager.PaintTiles(activeRange, TileColor.Red);
                battleData.uiManager.EnableCancelButtonUI();
                battleData.isWaitingUserInput = true;

                var update = UpdatePointSkillMouseDirection(battleData);
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
                yield return battleManager.StartCoroutine(CheckApplyOrChain(battleData, battleData.SelectedTile, originalDirection));
            }
        }

        public static IEnumerator CheckApplyOrChain(BattleData battleData, Tile targetTile, Direction originalDirection) {
            while (battleData.currentState == CurrentState.CheckApplyOrChain) {
                Unit caster = battleData.selectedUnit;
                BattleManager.MoveCameraToTile(targetTile);

				//firstRange는 1차 범위(스킬 시전 가능 범위. 투사체의 경우 목표점까지의 경로) 내 타일들이고
				//tilesInSkillRange는 2차 범위(타겟 타일을 가지고 계산한 스킬 효과 범위) 내 타일들임. tilesInSkillRange가 현재 단계에서 중요
				List<Tile> firstRange = GetTilesInFirstRange(battleData);
                List<Tile> tilesInSkillRange = GetTilesInSkillRange(battleData, targetTile);
                battleData.tileManager.PaintTiles(tilesInSkillRange, TileColor.Red);
				//tilesInRealEffectRange는 실제로 효과나 데미지가 가해지는 영역으로, 일반적인 경우는 tilesInSkillRange와 동일
				List<Tile> tilesInRealEffectRange = tilesInSkillRange;

				//투사체 스킬은 타겟 타일에 유닛이 없으면 아무 효과도 데미지도 없이 이펙트만 나오게 한다. 연계 발동은 안 되고 연계 대기는 된다
				if (battleData.SelectedSkill.GetSkillType() == SkillType.Route) {
					if (!targetTile.IsUnitOnTile ())
						tilesInRealEffectRange = new List<Tile>();
				}

                //데미지 미리보기
                Debug.Log("/------- Damage preview -------\\");
				Dictionary<Unit, DamageCalculator.DamageInfo> calculatedTotalDamage = DamageCalculator.CalculateTotalDamage(battleData, targetTile, tilesInRealEffectRange, firstRange);
                foreach (KeyValuePair<Unit, DamageCalculator.DamageInfo> kv in calculatedTotalDamage) {
                    if(kv.Value.damage > 0) kv.Key.GetComponentInChildren<HealthViewer>().PreviewDamageAmount((int)kv.Value.damage);
                    else kv.Key.GetComponentInChildren<HealthViewer>().PreviewRecoverAmount((int)(-kv.Value.damage));
                }
                Debug.Log("\\------- Damage preview -------/");

                bool isApplyPossible = SkillLogicFactory.Get(battleData.SelectedSkill).CheckApplyPossible(caster, tilesInSkillRange);
                bool isChainPossible = CheckChainPossible(battleData);
                battleData.uiManager.EnableSkillCheckWaitButton(isApplyPossible, isChainPossible);
                ActiveSkill selectedSkill = battleData.SelectedSkill;
                battleData.uiManager.SetSkillCheckAP(caster, selectedSkill);

                battleData.skillApplyCommand = SkillApplyCommand.Waiting;
                yield return battleData.battleManager.StartCoroutine(EventTrigger.WaitOr(
                    battleData.triggers.cancelClicked,
                    battleData.triggers.rightClicked,
                    battleData.triggers.skillApplyCommandChanged
                ));
                
                battleData.tileManager.DepaintTiles(tilesInSkillRange, TileColor.Red);

                if (battleData.triggers.rightClicked.Triggered ||
                    battleData.triggers.cancelClicked.Triggered) {
                    BattleManager.MoveCameraToUnit(caster);
                    battleData.uiManager.DisableSkillCheckUI();
                    caster.SetDirection(originalDirection);
                    if (selectedSkill.GetSkillType() != SkillType.Point)
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
                    if (selectedSkill.GetSkillApplyType() == SkillApplyType.DamageHealth
                     || selectedSkill.GetSkillApplyType() == SkillApplyType.Debuff) {
						yield return ApplyChain(battleData, targetTile, tilesInSkillRange, tilesInRealEffectRange, firstRange);
                    }
                    // 체인이 불가능한 스킬일 경우, 그냥 발동.
                    else {
                        battleData.currentState = CurrentState.ApplySkill;
						yield return battleManager.StartCoroutine(ApplySkill(battleData, caster, battleData.SelectedSkill, tilesInSkillRange, tilesInRealEffectRange, false, 1));
					}

					BattleManager.MoveCameraToUnit(caster);
					battleData.currentState = CurrentState.FocusToUnit;
					// 연계 정보 업데이트
					battleData.chainList = ChainList.RefreshChainInfo(battleData.chainList);
                }
				else if (battleData.skillApplyCommand == SkillApplyCommand.Chain) {
                    // 데미지 미리보기 해제.
                    foreach (KeyValuePair<Unit, DamageCalculator.DamageInfo> kv in calculatedTotalDamage) {
                        kv.Key.GetComponentInChildren<HealthViewer>().CancelPreview();
                    }

                    battleData.skillApplyCommand = SkillApplyCommand.Waiting;
                    battleData.currentState = CurrentState.ChainAndStandby;
					//투사체 스킬이고 타겟 타일에 유닛 없어도 연계 대기는 된다!(기획에서 결정된 사항)
                    yield return battleManager.StartCoroutine(ChainAndStandby(battleData, targetTile, tilesInSkillRange, firstRange));
                    // 연계 정보 업데이트. 여기선 안 해줘도 될 것 같은데 혹시 몰라서...
                    battleData.chainList = ChainList.RefreshChainInfo(battleData.chainList);
                } else {
                    Debug.LogError("Invalid State");
                    yield return null;
                }
            }
            yield return null;
        }

        private static List<Tile> GetTilesInFirstRange(BattleData battleData, Direction? direction = null) {
            Direction realDirection;
            if (direction.HasValue) {
                realDirection = direction.Value;
            } else {
                realDirection = battleData.selectedUnit.GetDirection();
            }
            var firstRange = battleData.tileManager.GetTilesInRange(battleData.SelectedSkill.GetFirstRangeForm(),
                                                                battleData.selectedUnit.GetPosition(),
                                                                battleData.SelectedSkill.GetFirstMinReach(),
                                                                battleData.SelectedSkill.GetFirstMaxReach(),
                                                                battleData.SelectedSkill.GetFirstWidth(),
                                                                realDirection);

			//투사체 스킬이면 직선경로상에서 유닛이 가로막은 지점까지를 1차 범위로 함. 범위 끝까지 가로막은 유닛이 없으면 직선 전체가 1차 범위
			if (battleData.SelectedSkill.GetSkillType() == SkillType.Route) {
				firstRange = GetRouteTiles(firstRange);
			}

            return firstRange;
        }

        private static List<Tile> GetTilesInSkillRange(BattleData battleData, Tile targetTile) {
            ActiveSkill selectedSkill = battleData.SelectedSkill;
            List<Tile> selectedTiles = battleData.tileManager.GetTilesInRange(selectedSkill.GetSecondRangeForm(),
                                                                        targetTile.GetTilePos(),
                                                                        selectedSkill.GetSecondMinReach(),
                                                                        selectedSkill.GetSecondMaxReach(),
                                                                        selectedSkill.GetSecondWidth(),
                                                                        battleData.selectedUnit.GetDirection());
            Debug.Log("NO. of tiles selected : " + selectedTiles.Count);
            return selectedTiles;
        }

        private static bool CheckChainPossible(BattleData battleData) {
			if (GameData.SceneData.stageNumber < Setting.ChainOpenStage)
				return false;

			bool isPossible = false;
            Unit caster = battleData.selectedUnit;

            // AP 조건으로 체크.
            int requireAP = caster.GetActualRequireSkillAP(battleData.SelectedSkill);
            int remainAPAfterChain = caster.GetCurrentActivityPoint() - requireAP;

            foreach (var unit in battleData.unitManager.GetAllUnits()) {
                if ((unit != caster) &&
                (unit.GetCurrentActivityPoint() > remainAPAfterChain)) {
                    isPossible = true;
                }
            }

            // 스킬 타입으로 체크. 공격스킬만 체인을 걸 수 있음.
            if (battleData.SelectedSkill.GetSkillApplyType() != SkillApplyType.DamageHealth
             && battleData.SelectedSkill.GetSkillApplyType() != SkillApplyType.Debuff) {
                isPossible = false;
            }
            
            Tile tileUnderCaster = caster.GetTileUnderUnit();
            foreach(var tileStatusEffect in tileUnderCaster.GetStatusEffectList()) {
                ActiveSkill originSkill = tileStatusEffect.GetOriginSkill();
                if (originSkill != null) {
                    if (!SkillLogicFactory.Get(originSkill).TriggerTileStatusEffectWhenUnitTryToChain(tileUnderCaster, tileStatusEffect)) {
                        isPossible = false;
                    }
                }
            }
            return isPossible;
        }


		public static IEnumerator ApplyChain(BattleData battleData, Tile targetTile, List<Tile> tilesInSkillRange, List<Tile> tilesInRealEffectRange, List<Tile> firstRange) {
            BattleManager battleManager = battleData.battleManager;

			//실제 데미지/효과가 가해지지는 경우에만 연계가 발동
			if (tilesInRealEffectRange.Count != 0) {
				// 자기 자신을 체인 리스트에 추가.
				ChainList.AddChains (battleData.selectedUnit, targetTile, tilesInSkillRange, battleData.SelectedSkill, firstRange);

				// 체인 체크, 순서대로 공격.
				List<ChainInfo> allVaildChainInfo = ChainList.GetAllChainInfoToTargetArea (battleData.selectedUnit, tilesInSkillRange);
				int chainCombo = allVaildChainInfo.Count;

				battleData.selectedUnit.PrintChainBonus (chainCombo);

				foreach (var chainInfo in allVaildChainInfo) {
					Tile focusedTile = chainInfo.GetTargetArea () [0];
					BattleManager.MoveCameraToTile (focusedTile);
					battleData.currentState = CurrentState.ApplySkill;
					chainInfo.GetUnit ().HideChainIcon ();
					yield return battleManager.StartCoroutine (ApplySkill (battleData, chainInfo.GetUnit (), chainInfo.GetSkill (), chainInfo.GetTargetArea (), chainInfo.GetTargetArea (), true, chainCombo));
				}
				battleData.selectedUnit.DisableChainText ();
			}
			//실제 데미지/효과가 가해지지 않는 경우(이펙트만 나옴) 연계가 발동하지 않고 그냥 그 스킬만 발동시킨다
			else {
				yield return battleManager.StartCoroutine(ApplySkill(battleData, battleData.selectedUnit, battleData.SelectedSkill, tilesInSkillRange, tilesInRealEffectRange, false, 1));
			}
        }

        private static IEnumerator ChainAndStandby(BattleData battleData, Tile targetTile, List<Tile> selectedTiles, List<Tile> firstRange) {
            // 방향 돌리기.
            battleData.selectedUnit.SetDirection(Utility.GetDirectionToTarget(battleData.selectedUnit, selectedTiles));

            int originAP = battleData.SelectedSkill.GetRequireAP();
            int requireAP = SkillLogicFactory.Get(battleData.SelectedSkill).CalculateAP(originAP, battleData.selectedUnit);
            battleData.selectedUnit.UseActivityPoint(requireAP);

            // 스킬 쿨다운 기록
            if (battleData.SelectedSkill.GetCooldown() > 0)
                battleData.selectedUnit.GetUsedSkillDict().Add(battleData.SelectedSkill.GetName(), battleData.SelectedSkill.GetCooldown());

            // 체인 목록에 추가.
            ChainList.AddChains(battleData.selectedUnit, targetTile, selectedTiles, battleData.SelectedSkill, firstRange);
            battleData.indexOfSelectedSkillByUser = 0; // return to init value.
            yield return new WaitForSeconds(0.5f);

            BattleManager.MoveCameraToUnit(battleData.selectedUnit);
            battleData.currentState = CurrentState.Standby;
            battleData.alreadyMoved = false;
            BattleManager battleManager = battleData.battleManager;
            yield return battleManager.StartCoroutine(BattleManager.Standby()); // 이후 대기.
        }

		private static IEnumerator ApplySkill(BattleData battleData, Unit caster, ActiveSkill appliedSkill, List<Tile> tilesInSkillRange, List<Tile> tilesInRealEffectRange, bool isChainable, int chainCombo) {
            BattleManager battleManager = battleData.battleManager;
			List<Unit> targets = GetUnitsOnTiles(tilesInRealEffectRange);
            List<PassiveSkill> passiveSkillsOfCaster = caster.GetLearnedPassiveSkillList();

			//tilesInSkillRange -> 스킬 이펙트용으로만 쓰인다(투사체가 아무 효과 없이 사라져도 이펙트가 날아갈 목표점은 있어야 하니까)
			//tilesInRealEffectRange -> 효과와 데미지 적용 등 모든 곳에 쓰이는 실제 범위

            if (isChainable)
                ChainList.RemoveChainsFromUnit(caster);

			//스킬 고유 효과음 재생
			if(appliedSkill.GetSoundEffectName () != null || appliedSkill.GetSoundEffectName () != "-")
				SoundManager.Instance.PlaySE (appliedSkill.GetSoundEffectName ());

			yield return battleManager.StartCoroutine(ApplySkillEffect(appliedSkill, caster, tilesInSkillRange));
            
			foreach (var tile in tilesInRealEffectRange) {
                if (tile.IsUnitOnTile()) {
                    Unit target = tile.GetUnitOnTile();

					// AI 유닛에게 뭔가 기술이 날아오면, 그 유닛이 활성화조건 5번(기술 날아온 순간 활성화)을 가지고 있는지 확인하고 맞으면 활성화시킨다
                    if (target.GetComponent<AIData>() != null) 
                        target.GetComponent<AIData>().SetActiveByExternalFactor();

                    if (!isChainable || !CheckEvasion(battleData, caster, target)) {
						SkillInstanceData skillInstanceData = new SkillInstanceData(new DamageCalculator.AttackDamage(), appliedSkill, caster, tilesInRealEffectRange, target, targets.Count);
                        // 데미지 적용
                        if (appliedSkill.GetSkillApplyType() == SkillApplyType.DamageHealth) {
                            yield return battleManager.StartCoroutine(ApplyDamage(skillInstanceData, battleData, chainCombo, target == targets.Last()));
                        } else {
                            DamageCalculator.CalculateAmountOtherThanAttackDamage(skillInstanceData);
                            float amount = skillInstanceData.GetDamage().resultDamage;
                            if (appliedSkill.GetSkillApplyType() == SkillApplyType.DamageAP) {
                                yield return battleManager.StartCoroutine(target.DamagedBySkill(skillInstanceData, false));
                                battleData.uiManager.UpdateApBarUI(battleData, battleData.unitManager.GetAllUnits());
                            } else if (appliedSkill.GetSkillApplyType() == SkillApplyType.HealHealth) {
                                yield return battleManager.StartCoroutine(target.RecoverHealth(amount));
                                yield return battleManager.StartCoroutine(SkillLogicFactory.Get(passiveSkillsOfCaster).TriggerApplyingHeal(skillInstanceData));
                            } else if (appliedSkill.GetSkillApplyType() == SkillApplyType.HealAP) {
                                yield return battleManager.StartCoroutine(target.RecoverActionPoint((int)amount));
                            }
                        }

                        // 효과 외의 부가 액션 (AP 감소 등)
                        yield return battleManager.StartCoroutine(SkillLogicFactory.Get(appliedSkill).ActionInDamageRoutine(skillInstanceData));

                        // 기술의 상태이상은 기술이 적용된 후에 붙인다.
                        if (appliedSkill.GetStatusEffectList().Count > 0) {
                            bool ignored = false;
                            foreach (var tileStatusEffect in tile.GetStatusEffectList()) {
                                ActiveSkill originSkill = tileStatusEffect.GetOriginSkill();
                                if (originSkill != null) {
                                    if (!SkillLogicFactory.Get(originSkill).TriggerTileStatusEffectWhenStatusEffectAppliedToUnit(skillInstanceData, tile, tileStatusEffect))
                                        ignored = true;
                                }
                            }
                            if(!ignored)
								StatusEffector.AttachStatusEffect(caster, appliedSkill, target, tilesInRealEffectRange);
                        }
                    }
                    caster.ActiveFalseAllBonusText();
                    // 사이사이에도 특성 발동 조건을 체크해준다.
                    battleData.unitManager.TriggerPassiveSkillsAtActionEnd();
                    yield return battleManager.StartCoroutine(battleData.unitManager.TriggerStatusEffectsAtActionEnd());
                    battleData.unitManager.UpdateStatusEffectsAtActionEnd();
                    battleData.tileManager.UpdateTileStatusEffectsAtActionEnd();

                    target.UpdateHealthViewer();
                }
                StatusEffector.AttachStatusEffect(caster, appliedSkill, tile);
            }

            // 기술 사용 시 적용되는 특성
            SkillLogicFactory.Get(passiveSkillsOfCaster).TriggerUsingSkill(caster, targets);
            foreach(var statusEffect in caster.GetStatusEffectList()) {
                PassiveSkill originPassiveSkill = statusEffect.GetOriginPassiveSkill();
                if(originPassiveSkill != null)
                    SkillLogicFactory.Get(originPassiveSkill).TriggerStatusEffectsOnUsingSkill(caster, targets, statusEffect);
            }
            caster.SetHasUsedSkillThisTurn(true);

            if (caster == battleData.selectedUnit) {
                int requireAP = caster.GetActualRequireSkillAP(appliedSkill);
                caster.UseActivityPoint(requireAP); // 즉시시전을 한 유닛만 AP를 차감. 나머지는 연계대기할 때 이미 차감되었으므로 패스.
                // 스킬 쿨다운 기록
                if (appliedSkill.GetCooldown() > 0)
                    caster.GetUsedSkillDict().Add(appliedSkill.GetName(), appliedSkill.GetCooldown());
            }

            // 공격스킬 시전시 관련 효과중 1회용인 효과 제거 (공격할 경우 - 공격력 변화, 데미지 변화, 강타)
            if (isChainable) {
                List<StatusEffect> statusEffectsToRemove = caster.GetStatusEffectList().FindAll(x => (x.GetIsOnce() &&
                                                                                    (x.IsOfType(StatusEffectType.PowerChange) ||
                                                                                    x.IsOfType(StatusEffectType.DamageChange) ||
                                                                                    x.IsOfType(StatusEffectType.Smite))));
                foreach(StatusEffect statusEffect in statusEffectsToRemove)
                    caster.RemoveStatusEffect(statusEffect);
            }
            battleData.indexOfSelectedSkillByUser = 0; // return to init value.

            yield return new WaitForSeconds(0.5f);
            battleData.alreadyMoved = false;
        }

        private static bool CheckEvasion(BattleData battleData, Unit caster, Unit target) {
            List<PassiveSkill> passiveSkillsOfTarget = target.GetLearnedPassiveSkillList();
            int totalEvasionChance = 0;
            totalEvasionChance = SkillLogicFactory.Get(passiveSkillsOfTarget).GetEvasionChance();

            int randomNumber = UnityEngine.Random.Range(0, 100);

            // 회피에 성공했는지 아닌지에 상관 없이 회피 효과 해제
            List<StatusEffect> statusEffectsToRemove =  caster.GetStatusEffectList().FindAll(x => x.IsOfType(StatusEffectType.EvasionChange));
            foreach(StatusEffect statusEffect in statusEffectsToRemove)
                caster.RemoveStatusEffect(statusEffect);

            if (totalEvasionChance > randomNumber) {
                battleData.uiManager.AppendNotImplementedLog("EVASION SUCCESS");
                // (타겟이) 회피 성공했을 경우 추가 효과
                SkillLogicFactory.Get(passiveSkillsOfTarget).TriggerOnEvasionEvent(battleData, caster, target);
                return true;
            } else return false;
        }

        private static IEnumerator ApplyDamage(SkillInstanceData skillInstanceData, BattleData battleData, int chainCombo, bool isLastTarget) {
            Unit unitInChain = skillInstanceData.GetCaster();
            Unit target = skillInstanceData.GetMainTarget();
            ActiveSkill appliedSkill = skillInstanceData.GetSkill();
            int targetCount = skillInstanceData.GetTargetCount();

            DamageCalculator.CalculateAttackDamage(skillInstanceData, chainCombo);
            DamageCalculator.AttackDamage attackDamage = skillInstanceData.GetDamage();

            if (attackDamage.attackDirection != DirectionCategory.Front) unitInChain.PrintDirectionBonus(attackDamage);
            if (attackDamage.celestialBonus != 1) unitInChain.PrintCelestialBonus(attackDamage.celestialBonus);
            if (attackDamage.chainBonus > 1) unitInChain.PrintChainBonus(chainCombo);
            if (attackDamage.heightBonus != 1) unitInChain.PrintHeightBonus(attackDamage.heightBonus);

            BattleManager battleManager = battleData.battleManager;
            // targetUnit이 반사 효과를 지니고 있을 경우 반사 대미지 코루틴 준비
            // fixme : 반사데미지는 다른 데미지 함수로 뺄 것! Damaged 함수 쓰면 원 공격자 스킬의 부가효과도 적용됨.
            UnitClass damageType = unitInChain.GetUnitClass();
            bool canReflect = target.HasStatusEffect(StatusEffectType.Reflect) ||
                                (target.HasStatusEffect(StatusEffectType.MagicReflect) && damageType == UnitClass.Magic) ||
                                (target.HasStatusEffect(StatusEffectType.MeleeReflect) && damageType == UnitClass.Melee);
            float reflectAmount = 0;
            if (canReflect) {
                reflectAmount = DamageCalculator.CalculateReflectDamage(attackDamage.resultDamage, target, unitInChain, damageType);
                attackDamage.resultDamage -= reflectAmount;
            }

            var damageCoroutine = target.DamagedBySkill(skillInstanceData, true);
            if (isLastTarget) {
                yield return battleManager.StartCoroutine(damageCoroutine);
            } else {
                battleManager.StartCoroutine(damageCoroutine);
                yield return null;
            }
            
            if(canReflect)  yield return battleManager.StartCoroutine(reflectDamage(unitInChain, target, reflectAmount));
        }
        private static IEnumerator reflectDamage(Unit caster, Unit target, float reflectAmount) {
            UnitClass damageType = caster.GetUnitClass();
            BattleManager battleManager = MonoBehaviour.FindObjectOfType<BattleManager>();
            yield return battleManager.StartCoroutine(caster.Damaged(reflectAmount, target, 0, 0, true, false));

            foreach (var statusEffect in target.GetStatusEffectList()) {
                bool canReflect = statusEffect.IsOfType(StatusEffectType.Reflect) ||
                                    (statusEffect.IsOfType(StatusEffectType.MagicReflect) && damageType == UnitClass.Magic) ||
                                    (statusEffect.IsOfType(StatusEffectType.MeleeReflect) && damageType == UnitClass.Melee);
                if (canReflect) {
                    if (statusEffect.GetOriginSkill() != null)
                        yield return battleManager.StartCoroutine(SkillLogicFactory.Get(statusEffect.GetOriginSkill()).
                                                TriggerStatusEffectAtReflection(target, statusEffect, caster));
                    if (statusEffect.GetIsOnce() == true)
                        target.RemoveStatusEffect(statusEffect);
                }
            }
        }

        private static List<Unit> GetUnitsOnTiles(List<Tile> tiles) {
            List<Unit> units = new List<Unit>();
            foreach (var tile in tiles) {
                if (tile.IsUnitOnTile()) {
                    units.Add(tile.GetUnitOnTile());
                }
            }
            return units;
        }

        private static IEnumerator ApplySkillEffect(ActiveSkill appliedSkill, Unit unit, List<Tile> selectedTiles) {
            string effectName = appliedSkill.GetEffectName();
            if (effectName == "-") {
                Debug.Log("There is no effect for " + appliedSkill.GetName());
                yield break;
            }

            EffectVisualType effectVisualType = appliedSkill.GetEffectVisualType();
            EffectMoveType effectMoveType = appliedSkill.GetEffectMoveType();

            if ((effectVisualType == EffectVisualType.Area) && (effectMoveType == EffectMoveType.Move)) {
                // 투사체, 범위형 이펙트.
                Vector3 startPos = unit.gameObject.transform.position;
                Vector3 endPos = new Vector3(0, 0, 0);
                foreach (var tile in selectedTiles) {
                    endPos += tile.gameObject.transform.position;
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
            } else if ((effectVisualType == EffectVisualType.Area) && (effectMoveType == EffectMoveType.NonMove)) {
                // 고정형, 범위형 이펙트.
                Vector3 targetPos = new Vector3(0, 0, 0);
                foreach (var tile in selectedTiles) {
                    targetPos += tile.transform.position;
                }
                targetPos = targetPos / (float)selectedTiles.Count;
                targetPos = targetPos - new Vector3(0, -0.5f, 5f); // 타일 축 -> 유닛 축으로 옮기기 위해 z축으로 5만큼 앞으로 빼준다.

                GameObject particlePrefab = Resources.Load("Particle/" + effectName) as GameObject;
                if (particlePrefab == null) {
                    Debug.LogError("Cannot load particle " + effectName);
                }
                GameObject particle = GameObject.Instantiate(particlePrefab) as GameObject;
                particle.transform.position = targetPos - new Vector3(0, -0.5f, 0.01f);
                yield return new WaitForSeconds(0.5f);
                GameObject.Destroy(particle, 0.5f);
                yield return null;
            } else if ((effectVisualType == EffectVisualType.Individual) && (effectMoveType == EffectMoveType.NonMove)) {
                // 고정형, 개별 대상 이펙트.
                List<Vector3> targetPosList = new List<Vector3>();
                foreach (var tileObject in selectedTiles) {
                    Tile tile = tileObject;
                    if (tile.IsUnitOnTile()) {
                        targetPosList.Add(tile.GetUnitOnTile().transform.position);
                    }
                }

                foreach (var targetPos in targetPosList) {
                    GameObject particle = GameObject.Instantiate(Resources.Load("Particle/" + effectName)) as GameObject;
                    particle.transform.position = targetPos - new Vector3(0, -0.5f, 0.01f);
                    GameObject.Destroy(particle, 0.5f + 0.3f); // 아랫줄에서의 지연시간을 고려한 값이어야 함.
                }

                if (targetPosList.Count == 0) // 대상이 없을 경우. 일단 가운데 이펙트를 띄운다.
                {
                    Vector3 midPos = new Vector3(0, 0, 0);
                    foreach (var tile in selectedTiles) {
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
