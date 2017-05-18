using Enums;

namespace Battle.Skills {
    public class Curi_7_r_SkillLogic : BaseSkillLogic {
        public override bool TriggerStatusEffectApplied(StatusEffect statusEffect, Unit caster, Unit target) {
            if ((target.currentHealth <= (int)(target.GetMaxHealth()) * 0.4f) && (target.GetUnitClass() == UnitClass.Magic)){
                return true;
            }
            return false;
        }
        public override void ApplyAdditionalDamage(SkillInstanceData skillInstanceData) {
            if(skillInstanceData.getTarget().GetUnitClass() != UnitClass.Magic) {
                skillInstanceData.getDamage().relativeDamageBonus = 0;
            }
        }
    }
}