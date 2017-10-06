﻿using System.Collections.Generic;
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
            List<Tile> tilesInRange = tileManager.GetTilesInRange(RangeForm.Diamond, casterPosition, 1, 1, 0, Direction.Down);
            foreach (Tile tile in tilesInRange) {
                if (!tileManager.isTilePassable(tile))
                    count++;
            }

            StatusEffector.AttachStatusEffect(caster, passiveSkill, caster);
            StatusEffect alreadyAppliedStatusEffect = caster.StatusEffectList.Find(se => se.GetOriginSkill() == passiveSkill);
            alreadyAppliedStatusEffect.SetRemainStack(count);
        }
    }
}