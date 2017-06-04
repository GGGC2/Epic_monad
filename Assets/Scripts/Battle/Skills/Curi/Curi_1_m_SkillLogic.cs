using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;

namespace Battle.Skills {
    public class Curi_1_m_SkillLogic : BaseSkillLogic {
        public override void TriggerStatusEffectsAtActionEnd(Unit target, StatusEffect statusEffect) { //'가연성 부착물' 스킬로직
            if (statusEffect.GetDisplayName() == "가연성 부착물") {
                Unit caster = statusEffect.GetCaster();
                Tile tileUnderUnit = target.GetTileUnderUnit();
                if (tileUnderUnit.GetTileElement() == Element.Fire) {
                    TileManager tileManager = MonoBehaviour.FindObjectOfType<TileManager>();
                    List<Tile> tileList = tileManager.GetTilesInRange(RangeForm.Diamond, target.GetPosition(), 0, 1, Direction.Left);
                    DamageCalculator.AttackDamage damage = new DamageCalculator.AttackDamage();
                    damage.resultDamage = statusEffect.GetAmount();

                    List<Unit> damagedUnitList = new List<Unit>();
                    damagedUnitList.Add(target);
                    foreach (Tile tile in tileList) {
                        if (tile.IsUnitOnTile()) {
                            Unit secondaryTarget = tile.GetUnitOnTile();
                            damagedUnitList.Add(secondaryTarget);
                        }
                    }
                    SkillInstanceData skillInstanceData = new SkillInstanceData(damage, statusEffect.GetOriginSkill(),
                    caster, damagedUnitList, target, damagedUnitList.Count);

                    foreach (var secondaryTarget in damagedUnitList) {
                        secondaryTarget.currentHealth -= (int)DamageCalculator.GetActualDamage(skillInstanceData, true);
                    }
                    statusEffect.DecreaseRemainStack();
                }
            }
        }
    }
}