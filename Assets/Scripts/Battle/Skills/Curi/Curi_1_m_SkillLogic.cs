using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;

namespace Battle.Skills {
    public class Curi_1_m_SkillLogic : BaseSkillLogic {
        public override IEnumerator TriggerStatusEffectAtActionEnd(Unit target, StatusEffect statusEffect) { //'가연성 부착물' 스킬로직
            if (statusEffect.GetDisplayName() == "가연성 부착물") {
                Unit caster = statusEffect.GetCaster();
                Tile tileUnderUnit = target.GetTileUnderUnit();
                if (tileUnderUnit.GetTileElement() == Element.Fire) {
                    TileManager tileManager = MonoBehaviour.FindObjectOfType<TileManager>();
                    List<Tile> tileList = tileManager.GetTilesInRange(RangeForm.Diamond, target.GetPosition(), 0, 1, 0, Direction.Left);
                    float damage = statusEffect.GetAmountOfType(StatusEffectType.Etc);

                    List<Unit> damagedUnitList = new List<Unit>();
                    foreach (Tile tile in tileList) {
                        if (tile.IsUnitOnTile()) {
                            Unit secondaryTarget = tile.GetUnitOnTile();
                            damagedUnitList.Add(secondaryTarget);
                        }
                    }
                    foreach (var secondaryTarget in damagedUnitList) {
                        //secondaryTarget.Damaged(skillInstanceData, true);
                        BattleManager battleManager = MonoBehaviour.FindObjectOfType<BattleManager>();
                        yield return battleManager.StartCoroutine(secondaryTarget.Damaged(damage, caster, 0, 0, true));
                    }
                }
            }
        }
    }
}