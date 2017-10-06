using UnityEngine;
using Enums;
using Battle.Damage;
using System;
using System.Collections.Generic;

namespace Battle.Skills {
    class Karldrich_2_r_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerOnActionEnd(Unit caster) {
            TileManager tileManager = TileManager.Instance;
            Vector2 casterPosition = caster.GetPosition();
            int count = 0;
            List<Tile> tileInRange = tileManager.GetTilesInRange(RangeForm.Diamond, casterPosition, 1, 2, 0, Direction.Down);
            foreach (Tile tile in tileInRange) {
                Unit unit = tile.GetUnitOnTile();
                if(unit != null && unit.GetSide() == Side.Ally)
                    count++;
            }

            StatusEffector.AttachStatusEffect(caster, passiveSkill, caster);
            StatusEffect alreadyAppliedStatusEffect = caster.StatusEffectList.Find(se => se.GetOriginSkill() == passiveSkill);
            alreadyAppliedStatusEffect.SetRemainStack(count);
        }
    }
}
