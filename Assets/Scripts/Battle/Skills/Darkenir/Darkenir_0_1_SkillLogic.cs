using Battle.Damage;
using System.Collections.Generic;

namespace Battle.Skills {
    class Darkenir_0_1_SkillLogic : BasePassiveSkillLogic {
        public override bool TriggerStatusEffectAppliedToOwner(StatusEffect statusEffect, Unit caster, Unit target) {
            StatusEffect newStatusEffect = new StatusEffect(statusEffect.fixedElem, caster, caster, statusEffect.GetOriginSkill(), statusEffect.GetOriginPassiveSkill());
            foreach(var actual in newStatusEffect.flexibleElem.actuals) {
                actual.amount /= 2;
                actual.remainAmount /= 2;
            }
            StatusEffector.AttachStatusEffect(caster, new List<StatusEffect>() { newStatusEffect }, caster);
            return true;
        }
    }
}
