using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Battle.Damage;

namespace Battle.Skills {
    class Curi_8_m_SkillLogic : BaseSkillLogic {
        public override float GetStatusEffectVar(StatusEffect statusEffect, int i, Unit unit, Unit owner) {
            return (float)owner.GetCurrentHealth(); // *0.08은 StatusEffect.CalculateAmount 메서드에서 적용됨.
        }
        public override void TriggerTileStatusEffectAtTurnEnd(Unit turnEnder, Tile tile, TileStatusEffect tileStatusEffect) {
            StatusEffector.AttachStatusEffect(tileStatusEffect.GetCaster(), skill, turnEnder);
        }
    }
}
