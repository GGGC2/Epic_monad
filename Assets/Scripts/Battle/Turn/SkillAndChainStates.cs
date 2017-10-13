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

			unitPreviewDict = new Dictionary<Unit, PreviewState>();

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

					List<Tile> realEffectRange = casting.RealEffectRange;
                    BattleData.tileManager.PaintTiles(realEffectRange, TileColor.Blue);
                    if (CheckApplyPossibleToTargetTiles(targetTile, casting))
						DisplayPreviewDamage (casting);
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
            LogManager logManager = LogManager.Instance;

            while (BattleData.currentState == CurrentState.SelectSkillApplyDirection) {
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
					
					if (!CheckApplyPossibleToTargetTiles(targetTile, casting))
						continue;
					
					if (BattleData.triggers.directionSelectedByUser.Triggered) {
						BattleData.currentState = CurrentState.ApplySkill;
                        logManager.Record(new CastLog(casting));
                        ApplyCasting (casting);
                    } else if (BattleData.triggers.directionLongSelectedByUser.Triggered) {
						if (CheckWaitChainPossible (casting)) {
							BattleData.currentState = CurrentState.WaitChain;
                            logManager.Record(new ChainLog(casting));
                            WaitChain (casting);
						}else{
							BattleData.currentState = CurrentState.ApplySkill;
                            logManager.Record(new CastLog(casting));
                            ApplyCasting (casting);
                        }
					}else if(BattleData.triggers.skillSelected.Triggered){
						yield return battleManager.StartCoroutine(SkillSelected());
					}
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

			unitPreviewDict = new Dictionary<Unit, PreviewState>();

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

                        if(CheckApplyPossibleToTargetTiles(targetTile, newCasting))
						    DisplayPreviewDamage (newCasting);
					}
				}

                yield return null;
            }
        }

        public static IEnumerator SelectSkillApplyPoint (Direction originalDirection) {
            Direction beforeDirection = originalDirection;
            Unit selectedUnit = BattleData.selectedUnit;
            LogManager logManager = LogManager.Instance;
            
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
                Tile targetTile = BattleData.SelectedTile;
				SkillLocation skillLocation = new SkillLocation (selectedUnitPos, targetTile, selectedUnit.GetDirection ());
				Casting casting = new Casting (selectedUnit, selectedSkill, skillLocation);

				if (!CheckApplyPossibleToTargetTiles(targetTile, casting))
					continue;

				if (BattleData.triggers.tileSelectedByUser.Triggered) {
					BattleData.currentState = CurrentState.ApplySkill;
                    logManager.Record(new CastLog(casting));
                    ApplyCasting (casting);
                } else if (BattleData.triggers.tileLongSelectedByUser.Triggered) {
					if (CheckWaitChainPossible (casting)) {
						BattleData.currentState = CurrentState.WaitChain;
                        logManager.Record(new ChainLog(casting));
                        WaitChain (casting);
					}else{
						BattleData.currentState = CurrentState.ApplySkill;
                        logManager.Record(new CastLog(casting));
                        ApplyCasting (casting);
                    }
				}else if(BattleData.triggers.skillSelected.Triggered) {
                    yield return BM.StartCoroutine(SkillSelected());
				}
            }
        }
        static bool CheckApplyPossibleToTargetTiles(Tile targetTile, Casting casting) {
            ActiveSkill skill = casting.Skill;
            if((!(skill.GetSkillType() == SkillType.Point) || casting.FirstRange.Contains(targetTile)) && skill.SkillLogic.CheckApplyPossibleToTargetTiles(casting))
                return true;
            else return false;
        }


        public class PreviewState {
            public Unit target;
            public float health;
            public float shield;
            public Vector2 position;
            public Direction direction;
            public PreviewState(Unit target) {
                this.target = target;
                position = target.GetPosition();
                direction = target.GetDirection();
                health = target.currentHealth;
                shield = target.GetRemainShield();
            }
            public void ApplyEffectLog(EffectLog log) {
                float damageAmount = 0;
                float shieldAmount = 0;
                if (log is HPChangeLog)     damageAmount  = -((HPChangeLog)log).amount;
                if (log is StatusEffectLog) shieldAmount = ((StatusEffectLog)log).GetShieldChangeAmount();
                if (log is PositionChangeLog)  position = ((PositionChangeLog)log).afterPos;
                if (shieldAmount < 0) {
                    damageAmount = -shieldAmount;
                    shieldAmount = 0;
                }
                Debug.Log(target.GetNameEng() + " " + health +" " + shield);
                if (damageAmount > 0) {
                    health += Math.Min(shield - damageAmount, 0);
                    health = Math.Max(health, 0);
                    shield = Math.Max(shield - damageAmount, 0);
                } else health = Math.Min(health - damageAmount, target.GetMaxHealth());
                if (shieldAmount > 0) shield += shieldAmount;
                Debug.Log(target.GetNameEng() + " " + health + " " + shield);
            }
        }
        
        static Dictionary<Unit, PreviewState> unitPreviewDict;
        static Dictionary<Unit, PreviewState> CullPreviewFromEventLog(EventLog eventLog) {
            Dictionary<Unit, PreviewState> result = new Dictionary<Unit, PreviewState>();
            foreach(var effectLog in eventLog.getEffectLogList()) {
                Unit unit = null;
                Vector2 position = new Vector2(0, 0);
                if (effectLog is HPChangeLog || effectLog is StatusEffectLog || effectLog is PositionChangeLog) {
                    if (effectLog is HPChangeLog)       unit = ((HPChangeLog)effectLog).unit;
                    if(effectLog is StatusEffectLog)    unit = ((StatusEffectLog)effectLog).GetOwner();
                    if(effectLog is PositionChangeLog)  unit = ((PositionChangeLog)effectLog).unit;
                    if (!result.ContainsKey(unit)) result.Add(unit, new PreviewState(unit));
                    result[unit].ApplyEffectLog(effectLog);
                }
            }
            return result;
        }
        
		static void DisplayPreviewDamage(Casting casting){
            //데미지 미리보기
            LogManager.Instance.Record(new CastLog(casting));                   // EventLog를 남긴다.
            ApplyCasting(casting);                                              // ApplyCasting한다. 로그만 남기므로 실제로 전투에 영향을 미치지 않는다.
            EventLog lastEventLog = LogManager.Instance.PopLastEventLog();      // 아까 남겼던 EventLog로 인해 생긴 로그들을 다 돌려받는다.
			unitPreviewDict = CullPreviewFromEventLog(lastEventLog);            // EventLog로부터 데미지와 연관된 부분만 추린다.
            foreach (KeyValuePair<Unit, PreviewState> kv in unitPreviewDict) {
                kv.Key.healthViewer.Preview(kv.Value.health, kv.Value.shield);
                kv.Key.SetAfterImageAt(kv.Value.position, kv.Value.direction);
            }
		}
		static void HidePreviewDamage(){
			// 데미지 미리보기 해제.
			foreach (KeyValuePair<Unit, PreviewState> kv in unitPreviewDict) {
                Unit unit = kv.Key;
                if (unit.GetComponentInChildren<HealthViewer>() != null) {
                    unit.GetComponentInChildren<HealthViewer>().CancelPreview();
                    unit.HideAfterImage();
                }
				unit.CheckAndHideObjectHealth();
			}
		}

		public static void ApplyCasting (Casting casting) {
            LogManager logManager = LogManager.Instance;
			Unit caster = casting.Caster;
			ActiveSkill skill = casting.Skill;
			BattleManager.MoveCameraToTile(casting.Location.TargetTile);

			BattleData.skillApplyCommand = SkillApplyCommand.Waiting;
			caster.UseActivityPoint (casting.RequireAP);
            if (skill.GetCooldown () > 0) {
				//caster.GetUsedSkillDict ().Add (skill.GetName (), skill.GetCooldown ());
                logManager.Record(new CoolDownLog(caster, skill.GetName(), skill.GetCooldown()));
            }
			ApplyAllTriggeredChains(casting);
        }

		public static void WaitChain (Casting casting) {
			Unit caster = casting.Caster;
			ActiveSkill skill = casting.Skill;
			SkillLocation location = casting.Location;
            LogManager logManager = LogManager.Instance;
            Direction direction = caster.GetDirection();

			caster.SetDirection(location.Direction);

			caster.UseActivityPoint (casting.RequireAP);
            if (skill.GetCooldown() > 0) {
                //caster.GetUsedSkillDict().Add(skill.GetName(), skill.GetCooldown());
                logManager.Record(new CoolDownLog(caster, skill.GetName(), skill.GetCooldown()));
            }

			// 체인 목록에 추가.
            logManager.Record(new AddChainLog(casting));
			//ChainList.AddChains(casting);
			BattleData.selectedSkill = null;
            logManager.Record(new WaitForSecondsLog(0.3f));
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
                if (originSkill is ActiveSkill) {
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

		public static void ApplyAllTriggeredChains (Casting casting) {
            BattleManager battleManager = BattleData.battleManager;
			Unit caster = casting.Caster;

			// 현재 시전으로 발동되는 모든 시전의 리스트(현재 시전 포함)를 받는다.
			// 연계발동 불가능한 스킬일 경우엔 ChainList.GetAllChainTriggered에서 현재 시전만 담은 리스트를 반환하므로 걍 그 스킬만 시전되고 끝난다
			List<Chain> allTriggeredChains = ChainList.GetAllChainTriggered (casting);
			int chainCombo = allTriggeredChains.Count;

			// 발동되는 모든 시전을 순서대로 실행
			foreach (var chain in allTriggeredChains) {
				if (chain.SecondRange.Count > 0) {
					Tile focusedTile = chain.SecondRange [0];
					LogManager.Instance.Record (new CameraMoveLog (focusedTile));
					BattleManager.MoveCameraToTile (focusedTile);
				}
				chain.Cast (chainCombo);
                LogManager.Instance.Record(new WaitForSecondsLog(0.3f));
                //BattleData.uiManager.chainBonusObj.SetActive(false);
                LogManager.Instance.Record(new PrintBonusTextLog("All", 0, false));
            }
        }
    }
}