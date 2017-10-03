using System.Collections.Generic;
using Enums;
using UnityEngine;
using Battle.Damage;
using System;

namespace Battle.Skills {
    class Lucius_2_m_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerOnActionEnd(Unit caster) {
            TileManager tileManager = TileManager.Instance;
            Vector2 casterPosition = caster.GetPosition();
            int count = 0;
            foreach (Direction direction in Enum.GetValues(typeof(Direction))) {
                Vector2 directionVector = Utility.ToVector2(direction);
                if (directionVector.magnitude == 1) {
                    Tile nearTile = tileManager.GetTile(casterPosition + directionVector);
                    if (!tileManager.isTilePassable(nearTile))
                        count++;
                }
            }

            StatusEffector.AttachStatusEffect(caster, passiveSkill, caster);
            StatusEffect alreadyAppliedStatusEffect = caster.StatusEffectList.Find(se => se.GetOriginSkill() == passiveSkill);
            alreadyAppliedStatusEffect.SetRemainStack(count);
        }
    }
}
