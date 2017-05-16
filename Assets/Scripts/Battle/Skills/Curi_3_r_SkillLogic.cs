using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Enums;
using System.Linq;
using Battle.Damage;

namespace Battle.Skills {
    public class Curi_3_r_SkillLogic : BasePassiveSkillLogic {
        public override bool TriggerStatusEffectApplied(Unit unit, StatusEffect statusEffect) {
            if (unit.GetTileUnderUnit().GetTileElement() == Element.Metal) {
                StatusEffectType type = statusEffect.GetStatusEffectType();
                if (type == StatusEffectType.DefenseChange || type == StatusEffectType.ResistanceChange) {
                    return false;
                }
            }
            return true;
        }
    }
}