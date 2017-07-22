using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Enums;
using Battle.Damage;

namespace Battle.Skills {
    class Bianca_1_l_SkillLogic : BaseSkillLogic {
        public override bool TriggerStatusEffectApplied(StatusEffect statusEffect, Unit caster, Unit target, List<Tile> targetTiles) {
            if(targetTiles.Count == 1)  return false;
            return true;
        }
        public override bool TriggerTileStatusEffectApplied(TileStatusEffect tileStatusEffect, Unit caster, Tile targetTile) {
            return Trap.TriggerOnApplied(tileStatusEffect, targetTile);
        }
        public override bool TriggerTileStatusEffectRemoved(Tile tile, TileStatusEffect tileStatusEffect) {
            if(!tileStatusEffect.IsOfType(StatusEffectType.Trap)) {   //활성화 대기 중이었던 tileStatusEffect가 사라지면
                Trap.ActivateTrap(tileStatusEffect, tile);
            }
            return true;
        }
        public override void TriggerTileStatusEffectAtTurnStart(Unit turnStarter, Tile tile, TileStatusEffect tileStatusEffect) {
            Trap.TriggerAtTurnStart(tileStatusEffect, tile, turnStarter);
        }

    }
}
