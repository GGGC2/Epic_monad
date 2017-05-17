using UnityEngine;
using Enums;
using Battle.Damage;

namespace Battle.Skills {
    public class Curi_5_r_SkillLogic : BaseSkillLogic {
        public override bool TriggerStatusEffectApplied(StatusEffect statusEffect, Unit caster, Unit target) {
            if(target.GetTileUnderUnit().GetTileElement() == Element.Metal) {
                statusEffect.flexibleElem.display.remainPhase += 1;
            }
            return true;
        }
    }
}
