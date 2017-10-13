using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Enums;
using Battle.Damage;

namespace Battle.Skills {
    class Bianca_1_l_SkillLogic : BaseSkillLogic {
        public override bool TriggerStatusEffectApplied(UnitStatusEffect statusEffect, Unit caster, Unit target, List<Tile> targetTiles) {
            if(targetTiles.Count == 1)  return false;
            return true;
        }
        public override bool TriggerTileStatusEffectApplied(TileStatusEffect tileStatusEffect) {
            return Trap.TriggerOnApplied(tileStatusEffect);
        }
        public override void TriggerTileStatusEffectAtPhaseStart(TileStatusEffect tileStatusEffect) {
            Trap.ActivateTrap(tileStatusEffect);
        }
        public override void TriggerTileStatusEffectAtTurnStart(Unit turnStarter, TileStatusEffect tileStatusEffect) {
            Trap.TriggerAtTurnStart(tileStatusEffect, turnStarter);
        }
    }
}
