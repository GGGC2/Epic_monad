using Battle.Damage;
using Enums;

namespace Battle.Skills {
    class Luvericha_3_m_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerOnStart(Unit caster) {
            StatusEffector.AttachStatusEffect(caster, passiveSkill, caster);
        }
        public override bool TriggerStatusEffectApplied(StatusEffect statusEffect, Unit caster, Unit target) {
            if(statusEffect.GetStatusEffectType() != StatusEffectType.Aura && caster == target) {
                return false;
            }
            Aura.TriggerOnApplied(statusEffect, caster, target);
            return true;
        }
        public override bool TriggerStatusEffectRemoved(StatusEffect statusEffect, Unit target) {
            Aura.TriggerOnRemoved(target, statusEffect);
            return true;
        }

    }
}
