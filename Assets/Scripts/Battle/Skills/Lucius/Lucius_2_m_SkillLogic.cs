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
            foreach(Direction direction in Enum.GetValues(typeof(Direction))) {
                if ((int)direction < 4) {
                    Tile tile = tileManager.GetTile(casterPosition + Utility.ToVector2(direction));
                    if (tile == null || !tileManager.isTilePassable(tile))
                        count++;
                }
            }
            UnitStatusEffect alreadyAppliedStatusEffect = caster.StatusEffectList.Find(se => se.GetOriginSkill() == passiveSkill);
            if (alreadyAppliedStatusEffect == null) {
            List<UnitStatusEffect> statusEffects = StatusEffector.AttachStatusEffect(caster, passiveSkill, caster);
            if (statusEffects.Count >= 1)
                statusEffects[0].SetRemainStack(count);
            } else alreadyAppliedStatusEffect.SetRemainStack(count);
        }
    }
}
