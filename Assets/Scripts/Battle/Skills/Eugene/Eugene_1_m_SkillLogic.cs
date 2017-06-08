using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Enums;

namespace Battle.Skills {
    class Eugene_1_m_SkillLogic : BaseSkillLogic {
        public override bool TriggerStatusEffectApplied(StatusEffect statusEffect, Unit caster, Unit target) {
            Aura.TriggerOnApplied(statusEffect, caster, target);
            return true;
        }
        public override bool TriggerStatusEffectRemoved(StatusEffect statusEffect, Unit target) {
            Aura.TriggerOnRemoved(target, statusEffect);
            return true;
        }
    }
}
