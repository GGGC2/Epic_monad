using Enums;
using System.Collections.Generic;
using Battle.Damage;
using UnityEngine;

namespace Battle.Skills {
    class Sepia_1_m_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerOnActionEnd(Unit caster) {
            TileManager tileManager = MonoBehaviour.FindObjectOfType<TileManager>();
            Direction front = caster.GetDirection();
            List<Tile> tilesInRange = tileManager.GetTilesInRange(RangeForm.Straight, caster.GetPosition(), -2, -1, 1, front);
			List<StatusEffect> statusEffectList = caster.GetStatusEffectList();

            foreach(var tile in tilesInRange) {
                if(tile.IsUnitOnTile()) {
                    Unit unit = tile.GetUnitOnTile();
                    if(unit.GetSide() == Side.Ally) {
                        StatusEffector.AttachStatusEffect(caster, passiveSkill, caster);
                        return;
                    }
                }
            }
            StatusEffect statusEffect = statusEffectList.Find(x => x.GetOriginSkillName() == "신뢰의 끈");
            if (statusEffect != null)
                caster.RemoveStatusEffect(statusEffect);
        }
    }
}
