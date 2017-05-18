using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Enums;

namespace Battle.Skills { 
    public class Curi_2_m_SkillLogic : BasePassiveSkillLogic {
        public IEnumerator Trigger(Unit target, StatusEffect statusEffect) {
            Unit caster = statusEffect.GetCaster();
            Tile tileUnderUnit = target.GetTileUnderUnit();
            if (tileUnderUnit.GetTileElement() == Enums.Element.Fire) {
                DamageCalculator.AttackDamage damage = new DamageCalculator.AttackDamage();
                damage.resultDamage = 0.3f * caster.GetStat(Stat.Power);

                TileManager tileManager = MonoBehaviour.FindObjectOfType<TileManager>();
                List<Tile> tileList = tileManager.GetTilesInRange(RangeForm.Diamond, target.GetPosition(), 0, 1, Direction.Left);
                foreach(Tile tile in tileList) {
                    if(tile.IsUnitOnTile()) {
                        Unit secondaryTarget = tile.GetUnitOnTile();
                        SkillInstanceData skillInstanceData = new SkillInstanceData(damage, 
                            statusEffect.GetOriginSkill(), caster, secondaryTarget, 1);
                        yield return secondaryTarget.Damaged(skillInstanceData, true);
                    }
                }
            }
        }
    }
}
