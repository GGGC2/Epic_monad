using UnityEngine;
using Enums;
using Battle.Damage;

namespace Battle.Skills {
    public class Curi_3_r_SkillLogic : BasePassiveSkillLogic {
        public override bool TriggerStatusEffectApplied(StatusEffect statusEffect, Unit caster, Unit target) {
            if (target.GetTileUnderUnit().GetTileElement() == Element.Metal) {
                StatusEffectType type = statusEffect.GetStatusEffectType();
                if (type == StatusEffectType.DefenseChange || type == StatusEffectType.ResistanceChange) {
                    return false;
                }
            }
            return true;
        }
    }
}