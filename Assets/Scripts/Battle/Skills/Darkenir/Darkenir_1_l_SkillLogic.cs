﻿
namespace Battle.Skills {
    class Darkenir_1_l_SkillLogic : BasePassiveSkillLogic {
        public override bool TriggerStatusEffectAppliedToOwner(StatusEffect statusEffect, Unit caster, Unit target) {
            if(statusEffect.GetRemainPhase() == 1) {
                return false;
            }
            return true;
        }
    }
}