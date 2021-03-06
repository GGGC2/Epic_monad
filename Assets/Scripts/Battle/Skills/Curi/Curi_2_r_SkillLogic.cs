﻿using System.Collections.Generic;
using UnityEngine;
using Enums;

namespace Battle.Skills {
    public class Curi_2_r_SkillLogic : BaseSkillLogic {
        public override void ApplyAdditionalDamage(CastingApply castingApply) {
            int waterTileCount = 0;
            foreach(Tile tile in castingApply.GetRealEffectRange()) {
                if(tile.GetTileElement() == Element.Water)
                    waterTileCount++;
            }
            castingApply.GetDamage().relativeDamageBonus *= (1 + waterTileCount * 0.1f);
        }
        public override bool TriggerStatusEffectApplied(UnitStatusEffect statusEffect, Unit caster, Unit target, List<Tile> targetTiles) {
            bool isAllTileWater = true;
            foreach (Tile tile in targetTiles) {
                if (tile.GetTileElement() != Element.Water)
                    isAllTileWater = false;
            }
            if(!isAllTileWater) return false;
            return true;
        }
    }
}