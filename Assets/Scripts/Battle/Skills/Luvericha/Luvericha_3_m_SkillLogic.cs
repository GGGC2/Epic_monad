using Battle.Damage;
using Enums;

namespace Battle.Skills {
    class Luvericha_3_m_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerOnStart(Unit caster) {
            StatusEffector.AttachStatusEffect(caster, passiveSkill, caster);
        }
        public override bool TriggerStatusEffectApplied(UnitStatusEffect statusEffect, Unit caster, Unit target) {
            if(!statusEffect.IsOfType(StatusEffectType.Aura) && caster == target) {
                return false;
            }
            Aura.TriggerOnApplied(statusEffect, caster, target);
            return true;
        }
        public override bool TriggerStatusEffectRemoved(UnitStatusEffect statusEffect, Unit target) {
            Aura.TriggerOnRemoved(target, statusEffect);
            return true;
        }
    }
}
