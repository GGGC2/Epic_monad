using System.Collections.Generic;
using Battle.Damage;

namespace Battle.Skills {
    class Eugene_7_r_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerOnTurnStart(Unit caster, Unit turnStarter) {
            if(Utility.GetDistance(caster.GetPosition(), turnStarter.GetPosition()) <= 2) {
                StatusEffect.FixedElement fixedElement = passiveSkill.GetStatusEffectList().Find(se => se.display.displayName == "길잡이");
                StatusEffect statusEffectToAttach = new StatusEffect(fixedElement, caster, turnStarter, null, passiveSkill);
                turnStarter.GetStatusEffectList().Add(statusEffectToAttach);
            }
        }
        public override void TriggerStatusEffectsOnMove(Unit target, StatusEffect statusEffect) {
            if (!target.GetHasUsedSkillThisTurn())
                StatusEffector.AttachStatusEffect(statusEffect.GetCaster(), passiveSkill, target);
        }
    }
}
