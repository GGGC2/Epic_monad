using Battle.Damage;
using System.Collections.Generic;

namespace Battle.Skills {
    class Darkenir_0_1_SkillLogic : BasePassiveSkillLogic {
        public override bool TriggerStatusEffectAppliedToOwner(UnitStatusEffect statusEffect, Unit caster, Unit target) {
            UnitStatusEffect newStatusEffect = new UnitStatusEffect((UnitStatusEffect.FixedElement)statusEffect.fixedElem, caster, caster, statusEffect.GetOriginSkill());
            foreach(var actual in newStatusEffect.flexibleElem.actuals) {
                actual.amount /= 2;
                actual.remainAmount /= 2;
            }
            StatusEffector.AttachStatusEffect(caster, new List<UnitStatusEffect>() { newStatusEffect }, caster);
            return true;
        }
    }
}
