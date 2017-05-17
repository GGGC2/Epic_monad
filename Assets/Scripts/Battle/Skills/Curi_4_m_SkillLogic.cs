using UnityEngine;
using Enums;
using Battle.Damage;

namespace Battle.Skills {
    public class Curi_4_m_SkillLogic : BaseSkillLogic {
        public override bool TriggerStatusEffectApplied(StatusEffect statusEffect, Unit caster, Unit target) {
            if(target.GetTileUnderUnit().GetTileElement() != Element.Metal) {
                return false;
            }
            return true;
        }
    }
}
