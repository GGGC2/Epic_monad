using Enums;
using Battle.Damage;

namespace Battle.Skills {
    class Eugene_6_r_SkillLogic : BasePassiveSkillLogic{
        public override void TriggerOnStart(Unit caster) {
            StatusEffector.AttachStatusEffect(caster, passiveSkill, caster);
        }
        public override bool TriggerStatusEffectApplied(StatusEffect statusEffect, Unit caster, Unit target) {
            Aura.TriggerOnApplied(statusEffect, caster, target);
            return true;
        }
        public override bool TriggerStatusEffectRemoved(StatusEffect statusEffect, Unit target) {
            Aura.TriggerOnRemoved(target, statusEffect);
            return true;
        }
        public override void TriggerStatusEffectsOnRest(Unit target, StatusEffect statusEffect) {
            if(!statusEffect.IsOfType(StatusEffectType.Aura))
                target.RemoveStatusEffect(StatusEffectCategory.Debuff, 1);
        }
    }
}
