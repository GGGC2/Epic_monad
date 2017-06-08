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
                    Skill preSelectedSkill = battleData.PreSelectedSkill;
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
                battleData.uiManager.UpdateSkillInfo(battleData.selectedUnit);
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
                Skill selectedSkill = battleData.SelectedSkill;
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
            Skill selectedSkill = battleData.SelectedSkill;
            var selectedTiles = GetTilesInFirstRange(battleData);
            if (battleData.SelectedSkill.GetSkillType() == SkillType.Route) {
                selectedTiles = GetRouteTiles(selectedTiles);
            }

            battleData.tileManager.PaintTiles(selectedTiles, TileColor.Red);

            while (true) {
                Direction newDirection = Utility.GetMouseDirectionByUnit(battleData.selectedUnit);
                Direction beforeDirection = battleData.selectedUnit.GetDirection();
                var selectedTilesByBeforeDirection = GetTilesInFirstRange(battleData, beforeDirection);

                if (battleData.SelectedSkill.GetSkillType() == SkillType.Route) {
                    selectedTilesByBeforeDirection = GetRouteTiles(selectedTilesByBeforeDirection);
                }

                var selectedTilesByNewDirection = GetTilesInFirstRange(battleData, newDirection);
                if (battleData.SelectedSkill.GetSkillType() == SkillType.Route) {
                    // 동일한 코드
                    selectedTilesByNewDirection = GetRouteTiles(selectedTilesByNewDirection);
                }

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

                battleData.tileManager.DepaintAllTiles(TileColor.Red);

                if (battleData.triggers.rightClicked.Triggered ||
                    battleData.triggers.cancelClicked.Triggered) {
                    selectedUnit.SetDirection(originalDirection);
                    battleData.currentState = CurrentState.SelectSkill;
                    yield break;
                }

                if (battleData.triggers.selectedTileByUser.Triggered) {
                    BattleManager battleManager = battleData.battleManager;
                    battleData.currentState = CurrentState.CheckApplyOrChain;
                    if (battleData.SelectedSkill.GetSkillType() == SkillType.Route) {
                        var firstRange = GetTilesInFirstRange(battleData);
                        var destTileAtRoute = GetRouteTiles(firstRange).Last();
                        yield return battleManager.StartCoroutine(CheckApplyOrChain(battleData, destTileAtRoute, originalDirection));
                    } else
                        yield return battleManager.StartCoroutine(CheckApplyOrChain(battleData, battleData.SelectedUnitTile, originalDirection));
                }

                if (battleData.currentState != CurrentState.SelectSkillApplyDirection) {
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
                BattleManager.MoveCameraToTile(targetTile);

                List<Tile> tilesInSkillRange = GetTilesInSkillRange(battleData, targetTile);
                battleData.tileManager.PaintTiles(tilesInSkillRange, TileColor.Red);
                List<Tile> firstRange = GetTilesInFirstRange(battleData);

                //데미지 미리보기
                Dictionary<Unit, DamageCalculator.DamageInfo> calculatedTotalDamage = DamageCalculator.CalculateTotalDamage(battleData, targetTile, tilesInSkillRange, firstRange);
                foreach (KeyValuePair<Unit, DamageCalculator.DamageInfo> kv in calculatedTotalDamage) {
                    Debug.Log(kv.Key.GetName() + " - Damage preview");
                    kv.Key.GetComponentInChildren<HealthViewer>().PreviewDamageAmount((int)kv.Value.damage);
                }

                bool isChainPossible = CheckChainPossible(battleData);
                battleData.uiManager.EnableSkillCheckChainButton(isChainPossible);
                Skill selectedSkill = battleData.SelectedSkill;
                battleData.uiManager.SetSkillCheckAP(battleData.selectedUnit, selectedSkill);

                battleData.skillApplyCommand = SkillApplyCommand.Waiting;
                yield return battleData.battleManager.StartCoroutine(EventTrigger.WaitOr(
                    battleData.triggers.cancelClicked,
                    battleData.triggers.rightClicked,
                    battleData.triggers.skillApplyCommandChanged
                ));
                battleData.tileManager.DepaintTiles(tilesInSkillRange, TileColor.Red);

                if (battleData.triggers.rightClicked.Triggered ||
                    battleData.triggers.cancelClicked.Triggered) {
                    BattleManager.MoveCameraToUnit(battleData.selectedUnit);
                    battleData.uiManager.DisableSkillCheckUI();
                    battleData.selectedUnit.SetDirection(originalDirection);
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
                        yield return ApplyChain(battleData, targetTile, tilesInSkillRange, firstRange);
                        BattleManager.MoveCameraToUnit(battleData.selectedUnit);
                        battleData.currentState = CurrentState.FocusToUnit;
                        // 연계 정보 업데이트
                        battleData.chainList = ChainList.RefreshChainInfo(battleData.chainList);
                    }
                    // 체인이 불가능한 스킬일 경우, 그냥 발동.
                    else {
                        battleData.currentState = CurrentState.ApplySkill;
                        yield return battleManager.StartCoroutine(ApplySkill(battleData, battleData.selectedUnit, battleData.SelectedSkill, tilesInSkillRange, false, 1));
                        BattleManager.MoveCameraToUnit(battleData.selectedUnit);
                        battleData.currentState = CurrentState.FocusToUnit;
                        // 연계 정보 업데이트
                        battleData.chainList = ChainList.RefreshChainInfo(battleData.chainList);
                    }
                } else if (battleData.skillApplyCommand == SkillApplyCommand.Chain) {
                    // 데미지 미리보기 해제.
                    foreach (KeyValuePair<Unit, DamageCalculator.DamageInfo> kv in calculatedTotalDamage) {
                        kv.Key.GetComponentInChildren<HealthViewer>().CancelPreview();
                    }

                    battleData.skillApplyCommand = SkillApplyCommand.Waiting;
                    battleData.currentState = CurrentState.ChainAndStandby;
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

            return firstRange;
        }

        private static List<Tile> GetTilesInSkillRange(BattleData battleData, Tile targetTile) {
            Skill selectedSkill = battleData.SelectedSkill;
            List<Tile> selectedTiles = battleData.tileManager.GetTilesInRange(selectedSkill.GetSecondRangeForm(),
                                                                        targetTile.GetTilePos(),
                                                                        selectedSkill.GetSecondMinReach(),
                                                                        selectedSkill.GetSecondMaxReach(),
                                                                        selectedSkill.GetSecondWidth(),
                                                                        battleData.selectedUnit.GetDirection());
            if (selectedSkill.GetSkillType() == SkillType.Auto)
                selectedTiles.Remove(targetTile);
            Debug.Log("NO. of tiles selected : " + selectedTiles.Count);
            return selectedTiles;
        }

        private static bool CheckChainPossible(BattleData battleData) {
            bool isPossible = false;

            // ap 조건으로 체크.
            int requireAP = battleData.selectedUnit.GetActualRequireSkillAP(battleData.SelectedSkill);
            int remainAPAfterChain = battleData.selectedUnit.GetCurrentActivityPoint() - requireAP;

            foreach (var unit in battleData.unitManager.GetAllUnits()) {
                if ((unit != battleData.selectedUnit) &&
                (unit.GetCurrentActivityPoint() > remainAPAfterChain)) {
                    isPossible = true;
                }
            }

            // 스킬 타입으로 체크. 공격스킬만 체인을 걸 수 있음.
            if (battleData.SelectedSkill.GetSkillApplyType() != SkillApplyType.DamageHealth
             && battleData.SelectedSkill.GetSkillApplyType() != SkillApplyType.Debuff) {
                isPossible = false;
            }
            Debug.Log("Skill Apply Type : " + battleData.SelectedSkill.GetSkillApplyType());

            return isPossible;
        }


        public static IEnumerator ApplyChain(BattleData battleData, Tile targetTile, List<Tile> tilesInSkillRange, List<Tile> firstRange) {
            BattleManager battleManager = battleData.battleManager;
            // 자기 자신을 체인 리스트에 추가.
            ChainList.AddChains(battleData.selectedUnit, targetTile, tilesInSkillRange, battleData.SelectedSkill, firstRange);

            // 체인 체크, 순서대로 공격.
            List<ChainInfo> allVaildChainInfo = ChainList.GetAllChainInfoToTargetArea(battleData.selectedUnit, tilesInSkillRange);
            int chainCombo = allVaildChainInfo.Count;

            battleData.selectedUnit.PrintChainBonus(chainCombo);

            foreach (var chainInfo in allVaildChainInfo) {
                Tile focusedTile = chainInfo.GetTargetArea()[0];
                BattleManager.MoveCameraToTile(focusedTile);
                battleData.currentState = CurrentState.ApplySkill;
                yield return battleManager.StartCoroutine(ApplySkill(battleData, chainInfo.GetUnit(), chainInfo.GetSkill(), chainInfo.GetTargetArea(), true, chainCombo));
            }

            battleData.selectedUnit.DisableChainText();
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

        private static IEnumerator ApplySkill(BattleData battleData, Unit caster, Skill appliedSkill, List<Tile> selectedTiles, bool isChainable, int chainCombo) {
            BattleManager battleManager = battleData.battleManager;
            List<Unit> targets = GetUnitsOnTiles(selectedTiles);
            /*
            // 경로형 스킬의 경우 대상 범위 재지정
            if (appliedSkill.GetSkillType() == SkillType.Route)
                selectedTiles = ReselectTilesByRoute(battleData, selectedTiles);
            */
            // 시전 방향으로 유닛의 바라보는 방향을 돌림.
            if (appliedSkill.GetSkillType() != SkillType.Auto)
                caster.SetDirection(Utility.GetDirectionToTarget(caster, selectedTiles));

            if (isChainable)
                ChainList.RemoveChainsFromUnit(caster);

            yield return battleManager.StartCoroutine(ApplySkillEffect(appliedSkill, caster, selectedTiles));

            foreach (var target in targets) {
                if (!isChainable || !CheckEvasion(battleData, caster, target)) {
                    // 데미지 적용
                    if (appliedSkill.GetSkillApplyType() == SkillApplyType.DamageHealth) {
                        SkillInstanceData skillInstanceData = new SkillInstanceData(new DamageCalculator.AttackDamage(), appliedSkill, caster, targets, target, targets.Count);
                        yield return battleManager.StartCoroutine(ApplyDamage(skillInstanceData, battleData, chainCombo, target == targets.Last()));
                    }

                    // 효과 외의 부가 액션 (AP 감소 등)
                    SkillLogicFactory.Get(appliedSkill).ActionInDamageRoutine(battleData, appliedSkill, caster, selectedTiles);

                    // 기술의 상태이상은 기술이 적용된 후에 붙인다.
                    if (appliedSkill.GetStatusEffectList().Count > 0)
                        StatusEffector.AttachStatusEffect(caster, appliedSkill, target);
                }
                caster.ActiveFalseAllBonusText();
                // 사이사이에도 특성 발동 조건을 체크해준다.
                battleData.unitManager.TriggerPassiveSkillsAtActionEnd();
                battleData.unitManager.TriggerStatusEffectsAtActionEnd();
                battleData.unitManager.UpdateStatusEffectsAtActionEnd();
            }

            // 기술 사용 시 적용되는 특성
            List<PassiveSkill> passiveSkillsOfCaster = caster.GetLearnedPassiveSkillList();
            SkillLogicFactory.Get(passiveSkillsOfCaster).TriggerUsingSkill(caster, targets);

            if (caster == battleData.selectedUnit) {
                int requireAP = caster.GetActualRequireSkillAP(appliedSkill);
                caster.UseActivityPoint(requireAP); // 즉시시전 대상만 ap를 차감. 나머지는 선차감되었으므로 패스.
                // 스킬 쿨다운 기록
                if (appliedSkill.GetCooldown() > 0)
                    caster.GetUsedSkillDict().Add(appliedSkill.GetName(), appliedSkill.GetCooldown());
            }

            // 공격스킬 시전시 관련 효과중 1회용인 효과 제거 (공격할 경우 - 공격력 변화, 데미지 변화, 강타)
            if (isChainable) {
                List<StatusEffect> statusEffectsToRemove = caster.GetStatusEffectList().FindAll(x => (x.GetIsOnce() &&
                                                                                    (x.GetStatusEffectType() == StatusEffectType.PowerChange ||
                                                                                    x.GetStatusEffectType() == StatusEffectType.DamageChange ||
                                                                                    x.GetStatusEffectType() == StatusEffectType.Smite)));
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
            Skill appliedSkill = skillInstanceData.GetSkill();
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

            if (canReflect) {
                float reflectAmount = DamageCalculator.CalculateReflectDamage(attackDamage.resultDamage, target, unitInChain, damageType);

                DamageCalculator.AttackDamage reflectAttackDamage = new DamageCalculator.AttackDamage();
                reflectAttackDamage.resultDamage = reflectAmount;
                List<Unit> reflectTargetList = new List<Unit>();
                reflectTargetList.Add(unitInChain);
                SkillInstanceData reflectInstanceData = new SkillInstanceData(reflectAttackDamage, appliedSkill, target, 
                                                            reflectTargetList, unitInChain, 1);
                var reflectCoroutine = unitInChain.Damaged(reflectInstanceData, true);
                yield return battleManager.StartCoroutine(reflectCoroutine);
            }

            var damageCoroutine = target.Damaged(skillInstanceData, true);
            if (isLastTarget) {
                yield return battleManager.StartCoroutine(damageCoroutine);
            } else {
                battleManager.StartCoroutine(damageCoroutine);
                yield return null;
            }
        }

        private static List<Tile> ReselectTilesByRoute(BattleData battleData, List<Tile> selectedTiles) {
            List<Tile> reselectTiles = new List<Tile>();
            //단차 제외하고 구현.
            Vector2 startPos = battleData.selectedUnit.GetPosition();
            // Vector2 endPos = ; 

            return reselectTiles;
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

        private static IEnumerator ApplySkillEffect(Skill appliedSkill, Unit unit, List<Tile> selectedTiles) {
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
