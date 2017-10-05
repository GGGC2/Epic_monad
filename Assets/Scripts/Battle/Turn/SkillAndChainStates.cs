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
        private static IEnumerator UpdateRangeSkillMouseDirection() {
            Unit selectedUnit = BattleData.selectedUnit;
			Tile targetTile = selectedUnit.GetTileUnderUnit ();
			Vector2 unitPos = selectedUnit.GetPosition ();
            ActiveSkill selectedSkill = BattleData.selectedSkill;

			Direction? beforeDirection = null;
			Direction newDirection = selectedUnit.GetDirection ();

			allCalculatedTotalDamages = new Dictionary<Unit, DamageCalculator.DamageInfo> ();

			while (true) {
				if (beforeDirection != newDirection) {
					beforeDirection = newDirection;

					BattleData.tileManager.DepaintAllTiles (TileColor.Red);
					BattleData.tileManager.DepaintAllTiles (TileColor.Blue);
					HidePreviewDamage();

					BattleData.selectedUnit.SetDirection(newDirection);

					var selectedTilesByNewDirection = selectedSkill.GetTilesInFirstRange (unitPos, newDirection);
					BattleData.tileManager.PaintTiles(selectedTilesByNewDirection, TileColor.Red);

					SkillLocation location = new SkillLocation (unitPos, targetTile, newDirection);
					selectedSkill.SetRealTargetTileForSkillLocation (location);
					Casting casting = new Casting (selectedUnit, selectedSkill, location);
					List<Tile> secondRange = casting.SecondRange;
					BattleData.tileManager.PaintTiles(secondRange, TileColor.Blue);

					List<Tile> realEffectRange = casting.RealEffectRange;
					if (selectedSkill.SkillLogic.CheckApplyPossibleToTargetTiles (selectedUnit, realEffectRange)) {
						DisplayPreviewDamage (casting);
					}

				}
				yield return null;
				if(BattleData.selectedUnit != null){
					newDirection = Utility.GetMouseDirectionByUnit (BattleData.selectedUnit, selectedUnit.GetDirection());
				}
            }
        }

        public static IEnumerator SelectSkillApplyDirection(Direction originalDirection) {
            Direction beforeDirection = originalDirection;
            Unit selectedUnit = BattleData.selectedUnit;
            ActiveSkill selectedSkill = BattleData.selectedSkill;

            while (true) {
                BattleData.isWaitingUserInput = true;
                //마우스 방향을 돌릴 때마다 그에 맞춰서 빨간 범위 표시도 업데이트하고 유닛 시선방향 돌리고 데미지 프리뷰와 2차범위 표시도 업데이트
				var updateRedArea = UpdateRangeSkillMouseDirection();
                BattleData.battleManager.StartCoroutine(updateRedArea);
                
				BattleData.uiManager.EnableSelectDirectionUI();

                yield return BattleData.battleManager.StartCoroutine(EventTrigger.WaitOr(
                    BattleData.triggers.rightClicked, BattleData.triggers.cancelClicked, BattleData.triggers.skillSelected,
					BattleData.triggers.directionSelectedByUser, BattleData.triggers.directionLongSelectedByUser
                ));

                BattleData.battleManager.StopCoroutine(updateRedArea);
                BattleData.isWaitingUserInput = false;

				BattleData.tileManager.DepaintAllTiles(TileColor.Red);	
				BattleData.tileManager.DepaintAllTiles(TileColor.Blue);
				HidePreviewDamage();

				//취소선택시->1. 4방향 화살표 제거 2. 유닛이 원래 방향을 바라보게 되돌림 3. currentState는 스킬을 고르는 단계로 돌려놓는다
                if (BattleData.triggers.rightClicked.Triggered || BattleData.triggers.cancelClicked.Triggered){
                    BattleData.uiManager.DisableSelectDirectionUI();
                    selectedUnit.SetDirection(originalDirection);
                    BattleData.currentState = CurrentState.FocusToUnit;
                    yield break;
                }else{
					BattleManager battleManager = BattleData.battleManager;
					var targetTile = BattleData.SelectedUnitTile;
					SkillLocation skillLocation = new SkillLocation (selectedUnit.GetTileUnderUnit (), targetTile, selectedUnit.GetDirection ());
					//투사체 스킬이면 선택된 영역(경로) 중 맨 끝점을 시전 타일로 한다.
					selectedSkill.SetRealTargetTileForSkillLocation (skillLocation);
					Casting casting = new Casting (selectedUnit, selectedSkill, skillLocation);
					
					if (!selectedSkill.SkillLogic.CheckApplyPossibleToTargetTiles (casting.Caster, casting.RealEffectRange))
						continue;
					
					if (BattleData.triggers.directionSelectedByUser.Triggered) {
						BattleData.currentState = CurrentState.ApplySkill;
						yield return battleManager.StartCoroutine (ApplyCasting (casting));
					}else if (BattleData.triggers.directionLongSelectedByUser.Triggered) {
						if (CheckWaitChainPossible (casting)) {
							BattleData.currentState = CurrentState.WaitChain;
							yield return battleManager.StartCoroutine (WaitChain (casting));
						}else{
							BattleData.currentState = CurrentState.ApplySkill;
							yield return battleManager.StartCoroutine (ApplyCasting (casting));
						}
					}else if(BattleData.triggers.skillSelected.Triggered){
						battleManager.StartCoroutine(SkillSelected());
						yield break;
					}
                }

                if (BattleData.currentState != CurrentState.SelectSkillApplyDirection) {
					yield break;
                }
            }
        }

		public static IEnumerator SkillSelected(){
			ActiveSkill selectedSkill = BattleData.selectedSkill;
			UIManager.Instance.selectedUnitViewerUI.GetComponent<BattleUI.UnitViewer>().PreviewAp(BattleData.selectedUnit, selectedSkill.GetRequireAP());
            SkillType skillTypeOfSelectedSkill = selectedSkill.GetSkillType();
            if (skillTypeOfSelectedSkill == SkillType.Auto ||
                skillTypeOfSelectedSkill == SkillType.Self ||
                skillTypeOfSelectedSkill == SkillType.Route) {
                BattleData.currentState = CurrentState.SelectSkillApplyDirection;
                yield return BattleManager.Instance.StartCoroutine(SelectSkillApplyDirection(BattleData.selectedUnit.GetDirection()));
            }else{
                BattleData.currentState = CurrentState.SelectSkillApplyPoint;
                yield return BattleManager.Instance.StartCoroutine(SelectSkillApplyPoint(BattleData.selectedUnit.GetDirection()));
            }
		}
		
        private static IEnumerator UpdatePointSkillMouseDirection(Direction originalDirection) {
			Unit selectedUnit = BattleData.selectedUnit;
			ActiveSkill selectedSkill = BattleData.selectedSkill;
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
				if (newTargetTile != null && !newTargetTile.isMouseOver)
					newTargetTile = null;

				if (previousTargetTile != newTargetTile) {
					previousTargetTile = newTargetTile;

					BattleData.tileManager.DepaintAllTiles (TileColor.Blue);
					HidePreviewDamage ();

					Tile targetTile = newTargetTile;
					if (targetTile != null) {
						SkillLocation newLocation = new SkillLocation (unitPos, targetTile, newDirection);
						Casting newCasting = new Casting (selectedUnit, selectedSkill, newLocation);

						Unit caster = newCasting.Caster;
						ActiveSkill skill = newCasting.Skill;

						List<Tile> secondRange = newCasting.SecondRange;
						BattleData.tileManager.PaintTiles (secondRange, TileColor.Blue);
						List<Tile> realEffectRange = newCasting.RealEffectRange;

						DisplayPreviewDamage (newCasting);
					}
				}

                yield return null;
            }
        }

        public static IEnumerator SelectSkillApplyPoint (Direction originalDirection) {
            Direction beforeDirection = originalDirection;
            Unit selectedUnit = BattleData.selectedUnit;

            if (BattleData.currentState == CurrentState.FocusToUnit){
                yield break;
            }

            while (BattleData.currentState == CurrentState.SelectSkillApplyPoint) {
                Vector2 selectedUnitPos = BattleData.selectedUnit.GetPosition();

                List<Tile> activeRange = new List<Tile>();
                ActiveSkill selectedSkill = BattleData.selectedSkill;
				activeRange = selectedSkill.GetTilesInFirstRange (selectedUnitPos, selectedUnit.GetDirection ());

                BattleData.tileManager.PaintTiles(activeRange, TileColor.Red);
				BattleData.tileManager.PreselectTiles (activeRange);
                BattleData.isWaitingUserInput = true;

                var update = UpdatePointSkillMouseDirection(originalDirection);
                BattleData.battleManager.StartCoroutine(update);
                yield return BattleData.battleManager.StartCoroutine(EventTrigger.WaitOr(
					BattleData.triggers.tileSelectedByUser, BattleData.triggers.tileLongSelectedByUser,
                    BattleData.triggers.rightClicked, BattleData.triggers.cancelClicked, BattleData.triggers.skillSelected
                ));
                BattleData.battleManager.StopCoroutine(update);
                BattleData.isWaitingUserInput = false;

				BattleData.tileManager.DepaintAllTiles(TileColor.Red);	
				BattleData.tileManager.DepaintAllTiles(TileColor.Blue);
				HidePreviewDamage();

                if (BattleData.triggers.rightClicked.Triggered ||
                    BattleData.triggers.cancelClicked.Triggered) {
                    selectedUnit.SetDirection(originalDirection);
					BattleData.tileManager.DepreselectAllTiles ();
                    BattleData.currentState = CurrentState.FocusToUnit;
                    BattleData.isWaitingUserInput = false;
                    yield break;
                }

				BattleData.tileManager.DepreselectAllTiles ();
                BattleData.uiManager.DisableSkillUI();

                BattleManager BM = BattleData.battleManager;
				SkillLocation skillLocation = new SkillLocation (selectedUnitPos, BattleData.SelectedTile, selectedUnit.GetDirection ());
				Casting casting = new Casting (selectedUnit, selectedSkill, skillLocation);

				if (!selectedSkill.SkillLogic.CheckApplyPossibleToTargetTiles (casting.Caster, casting.RealEffectRange))
					continue;

				if (BattleData.triggers.tileSelectedByUser.Triggered) {
					BattleData.currentState = CurrentState.ApplySkill;
					yield return BM.StartCoroutine (ApplyCasting (casting));
				}else if (BattleData.triggers.tileLongSelectedByUser.Triggered) {
					if (CheckWaitChainPossible (casting)) {
						BattleData.currentState = CurrentState.WaitChain;
						yield return BM.StartCoroutine (WaitChain (casting));
					}else{
						BattleData.currentState = CurrentState.ApplySkill;
						yield return BM.StartCoroutine (ApplyCasting (casting));
					}
				}else if(BattleData.triggers.skillSelected.Triggered){
					BM.StartCoroutine(SkillSelected());
					//yield break;
				}
            }
        }


		static Dictionary<Unit, DamageCalculator.DamageInfo> allCalculatedTotalDamages;

		static void DisplayPreviewDamage(Casting casting){
			//데미지 미리보기
			allCalculatedTotalDamages = DamageCalculator.CalculateAllPreviewTotalDamages(casting);
			foreach (KeyValuePair<Unit, DamageCalculator.DamageInfo> kv in allCalculatedTotalDamages) {
				kv.Key.healthViewer.gameObject.SetActive(true);
				if(kv.Value.damage > 0) kv.Key.GetComponentInChildren<HealthViewer>().PreviewDamageAmount((int)kv.Value.damage);
				else kv.Key.GetComponentInChildren<HealthViewer>().PreviewRecoverAmount((int)(-kv.Value.damage));
			}
		}
		static void HidePreviewDamage(){
			// 데미지 미리보기 해제.
			foreach (KeyValuePair<Unit, DamageCalculator.DamageInfo> kv in allCalculatedTotalDamages) {
				if (kv.Key.GetComponentInChildren<HealthViewer> () != null) {
					kv.Key.GetComponentInChildren<HealthViewer> ().CancelPreview ();
				}
				kv.Key.CheckAndHideObjectHealth();
			}
		}

		public static IEnumerator ApplyCasting (Casting casting) {
            LogManager logManager = LogManager.Instance;
			Unit caster = casting.Caster;
			ActiveSkill skill = casting.Skill;
			BattleManager.MoveCameraToTile(casting.Location.TargetTile);

			BattleData.skillApplyCommand = SkillApplyCommand.Waiting;
            logManager.Record(new CastLog(casting));
			caster.UseActivityPoint (casting.RequireAP);
            if (skill.GetCooldown () > 0) {
				caster.GetUsedSkillDict ().Add (skill.GetName (), skill.GetCooldown ());
                logManager.Record(new CoolDownLog(caster, skill.GetName(), skill.GetCooldown()));
            }
			yield return ApplyAllTriggeredChains(casting);

			BattleManager.MoveCameraToUnit(caster);
            logManager.Record(new CameraMoveLog(caster.transform.position));
			BattleData.currentState = CurrentState.FocusToUnit;
        }

		public static IEnumerator WaitChain (Casting casting) {
			Unit caster = casting.Caster;
			ActiveSkill skill = casting.Skill;
			SkillLocation location = casting.Location;
            LogManager logManager = LogManager.Instance;
            Direction direction = caster.GetDirection();

            logManager.Record(new ChainLog(casting));
			caster.SetDirection(location.Direction);
            logManager.Record(new DirectionChangeLog(caster, direction, caster.GetDirection()));

			caster.UseActivityPoint (casting.RequireAP);
            if (skill.GetCooldown() > 0) {
                caster.GetUsedSkillDict().Add(skill.GetName(), skill.GetCooldown());
                logManager.Record(new CoolDownLog(caster, skill.GetName(), skill.GetCooldown()));
            }

			// 체인 목록에 추가.
			ChainList.AddChains(casting);
			BattleData.selectedSkill = null;
			yield return new WaitForSeconds(0.5f);

			BattleManager.MoveCameraToUnit(caster);
            logManager.Record(new CameraMoveLog(caster.transform.position));
			BattleData.currentState = CurrentState.Standby;
			yield return BattleData.battleManager.StartCoroutine(BattleManager.Standby()); // 이후 대기.
		}

		//연계'대기' 가능한 상태인가?
		private static bool CheckWaitChainPossible (Casting casting) {
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
                if (originSkill != null && originSkill.GetType() == typeof(ActiveSkill)) {
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

			BattleData.uiManager.PrintChainBonus (chainCombo);

			// 발동되는 모든 시전을 순서대로 실행
			foreach (var chain in allTriggeredChains) {
				if (chain.SecondRange.Count > 0) {
					Tile focusedTile = chain.SecondRange [0];
					BattleManager.MoveCameraToTile (focusedTile);
                    LogManager.Instance.Record(new CameraMoveLog(focusedTile.transform.position));
				}
				BattleData.currentState = CurrentState.ApplySkill;
				chain.Caster.HideChainIcon ();
				yield return battleManager.StartCoroutine (chain.Cast (chainCombo));
				BattleData.uiManager.chainBonusObj.SetActive(false);
			}

			yield return BattleData.battleManager.AtActionEnd();
        }
    }
}