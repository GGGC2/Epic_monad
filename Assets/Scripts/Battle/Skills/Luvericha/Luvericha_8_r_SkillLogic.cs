
namespace Battle.Skills {
    class Luvericha_8_r_SkillLogic : BaseSkillLogic {
        public override bool TriggerStatusEffectApplied(StatusEffect statusEffect, Unit caster, Unit target) {
            if (caster == target)
                return false;
            return true;
        }
    }
}
