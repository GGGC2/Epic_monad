using System.Collections.Generic;

namespace Battle.Skills {
    class Eugene_3_m_SkillLogic : BaseSkillLogic {
        public override bool TriggerStatusEffectApplied(StatusEffect statusEffect, Unit caster, Unit target) {
            List<StatusEffect> statusEffectList = target.GetStatusEffectList();
            StatusEffect statusEffectToChange = statusEffectList.Find(se => (se.GetOriginSkillName() == "순백의 방패"
                                        && se.GetStatusEffectType() == Enums.StatusEffectType.Aura));
            if(statusEffectToChange != null) {
                target.RemoveStatusEffect(statusEffectToChange);
                return true;
            }
            else return false;
        }
    }
}
