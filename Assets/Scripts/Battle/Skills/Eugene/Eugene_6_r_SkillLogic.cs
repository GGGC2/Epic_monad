using Enums;
using Battle.Damage;

namespace Battle.Skills {
    class Eugene_6_r_SkillLogic : BasePassiveSkillLogic{
        public override void TriggerOnStart(Unit caster) {
            StatusEffector.AttachStatusEffect(caster, passiveSkill, caster);
        }
        public override bool TriggerStatusEffectApplied(StatusEffect statusEffect, Unit caster, Unit target) {
            if (statusEffect.GetOriginSkillName() == "야영 전문가" && statusEffect.IsOfType(StatusEffectType.Aura))
                Aura.TriggerOnApplied(statusEffect, caster, target);
            return true;
        }
        public override bool TriggerStatusEffectRemoved(StatusEffect statusEffect, Unit target) {
            if(statusEffect.GetOriginSkillName() == "야영 전문가" && statusEffect.IsOfType(StatusEffectType.Aura))
                Aura.TriggerOnRemoved(target, statusEffect);
            return true;
        }
        public override void TriggerStatusEffectsOnRest(Unit target, StatusEffect statusEffect) {
            if (!statusEffect.IsOfType(StatusEffectType.Aura)) {
                target.RemoveStatusEffect(statusEffect.GetCaster(), StatusEffectCategory.Debuff, 1);
            }
        }
    }
}
